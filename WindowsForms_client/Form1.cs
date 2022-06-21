using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
namespace WindowsForms_client
{
   
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
      

        const int BUFFER_SIZE = 4096;  // 4 KB

            
        public Form1()
        {
            InitializeComponent();
            this.Location = new Point(0, 0);
            //AllocConsole();
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread producer = new Thread(new ThreadStart(listenToSNMP));
            producer.Start();
            //producer.Join();   // Join both threads with no timeout
            
            //string[] temp = { "1", "2" ,"3", "4", "5", "6", "7", "8", "9", "10", "11"};
            //dataGridView1.Rows.Add(temp);

        }
        void listenToSNMP()
        {
            bool bResult;
            /////////////////////////////////////////////////////////////////////
            // Create a named pipe.
            // Prepare the pipe name
            ////////////////////////////////////////////////////////////////////
            String strPipeName = String.Format(@"\\{0}\pipe\{1}",
                ".",                // Server name
                "IPC_DATA_CHANNEL"        // Pipe name
                );

            // Prepare the security attributes

            IntPtr pSa = IntPtr.Zero;   // NULL
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();

            SECURITY_DESCRIPTOR sd;
            SecurityNative.InitializeSecurityDescriptor(out sd, 1);
            // DACL is set as NULL to allow all access to the object.
            SecurityNative.SetSecurityDescriptorDacl(ref sd, true, IntPtr.Zero, false);
            sa.lpSecurityDescriptor = Marshal.AllocHGlobal(Marshal.SizeOf(
                typeof(SECURITY_DESCRIPTOR)));
            Marshal.StructureToPtr(sd, sa.lpSecurityDescriptor, false);
            sa.bInheritHandle = false;              // Not inheritable
            sa.nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));

            pSa = Marshal.AllocHGlobal(sa.nLength);
            Marshal.StructureToPtr(sa, pSa, false);

            // Create the named pipe.
            IntPtr hPipe = PipeNative.CreateNamedPipe(
                strPipeName,                        // The unique pipe name.
                PipeOpenMode.PIPE_ACCESS_DUPLEX,    // The pipe is bi-directional
                PipeMode.PIPE_TYPE_MESSAGE |        // Message type pipe 
                PipeMode.PIPE_READMODE_MESSAGE |    // Message-read mode 
                PipeMode.PIPE_WAIT,                 // Blocking mode is on
                PipeNative.PIPE_UNLIMITED_INSTANCES,// Max server instances
                BUFFER_SIZE,                        // Output buffer size
                BUFFER_SIZE,                        // Input buffer size
                PipeNative.NMPWAIT_USE_DEFAULT_WAIT,// Time-out interval
                pSa                                 // Pipe security attributes
                );

            if (hPipe.ToInt32() == PipeNative.INVALID_HANDLE_VALUE)
            {
                Console.WriteLine("Unable to create named pipe {0} w/err 0x{1:X}",
                    strPipeName, PipeNative.GetLastError());
                return;
            }
            Console.WriteLine("The named pipe, {0}, is created.", strPipeName);


            /////////////////////////////////////////////////////////////////////
            // Wait for the client to connect.
            // 

            Console.WriteLine("Waiting for the client's connection...");

            bool bConnected = PipeNative.ConnectNamedPipe(hPipe, IntPtr.Zero) ?
                true : PipeNative.GetLastError() == PipeNative.ERROR_PIPE_CONNECTED;

            if (!bConnected)
            {
                Console.WriteLine(
                    "Error occurred while connecting to the client: 0x{0:X}",
                    PipeNative.GetLastError());
                PipeNative.CloseHandle(hPipe);      // Close the pipe handle.
                return;
            }
            // A byte buffer of BUFFER_SIZE bytes. The buffer should be big 
            // enough for ONE request from a client.

            while (true)
            {
                string strMessage;
                byte[] bRequest = new byte[BUFFER_SIZE];    // Client -> Server
                uint cbBytesRead, cbRequestBytes;
                // Receive one message from the pipe.
                cbRequestBytes = BUFFER_SIZE;
                bResult = PipeNative.ReadFile(      // Read from the pipe.
                    hPipe,                          // Handle of the pipe
                    bRequest,                       // Buffer to receive data
                    cbRequestBytes,                 // Size of buffer in bytes
                    out cbBytesRead,                // Number of bytes read
                    IntPtr.Zero);                   // Not overlapped I/O

                if (!bResult/*Failed*/ || cbBytesRead == 0/*Finished*/)
                {
                    Environment.Exit(0);
                }
                    

                // Unicode-encode the byte array and trim all the '\0' chars at 
                // the end.
                strMessage = Encoding.Unicode.GetString(bRequest).TrimEnd('\0');
                Console.WriteLine("Receives {0} bytes; Message: \"{1}\"",
                    cbBytesRead, strMessage);
                string[] temp = strMessage.Split('\n');
                if(temp.Length > 0)
                    dataGridView1.Rows.Add(temp);

            }

            PipeNative.FlushFileBuffers(hPipe);
            PipeNative.DisconnectNamedPipe(hPipe);
            PipeNative.CloseHandle(hPipe);
            Console.ReadKey();
        }

      

        private void savetoFile()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "Output.csv";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            int columnCount = dataGridView1.Columns.Count;
                            string columnNames = "";
                            string[] outputCsv = new string[dataGridView1.Rows.Count + 1];
                            for (int i = 0; i < columnCount; i++)
                            {
                                columnNames += dataGridView1.Columns[i].HeaderText.ToString() + ",";
                            }
                            outputCsv[0] += columnNames;
                            
                            for (int i = 1; (i - 1) < dataGridView1.Rows.Count; i++)
                            {
                                for (int j = 0; j < columnCount; j++)
                                {
                                    outputCsv[i] += dataGridView1.Rows[i - 1].Cells[j].Value.ToString() + ",";
                                }
                            }

                            File.WriteAllLines(sfd.FileName, outputCsv, Encoding.UTF8);
                            MessageBox.Show("Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void menuToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text.ToUpper() == "SAVE")
                savetoFile();

            menuToolStripMenuItem.HideDropDown();
            
        }
    }
}
