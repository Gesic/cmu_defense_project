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
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Net.Http;

namespace WindowsForms_client
{

    enum postFuncType
    {
        Auth,
        Plate
    };
    public partial class MainFrm : Form
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        WebRequestHandler clientHandler;
        Thread producer;
        const int BUFFER_SIZE = 4096;  // 4 KB
        const string serverURL = "https://10.58.6.244:8443/test/";
        //const string serverURL = "http://10.58.6.244:8080/db/";

        //https://10.58.6.244:8443/db  
        //https://10.58.6.244:8080/db
        class ResponseAPI
        {
            public string id { get; set; }
            public string plateNum { get; set; }

        }

        public MainFrm()
        {
            
            InitializeComponent();
            this.Location = new Point(0, 0);
            //AllocConsole();
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private Task<HttpResponseMessage> HttpComm()
        {
            HttpClient _httpClient = new HttpClient(clientHandler);
            _httpClient.Timeout = new TimeSpan(0, 0, 5);
            _httpClient.BaseAddress = new Uri(serverURL);
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var request = new
            {
                id = "user",
                plateNum = "LBV6157",

            };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, request).Result;
            var contents = response.Content.ReadAsStringAsync();
            TXT_DESC.AppendText(contents.Result.ToString());
            string[] infos = contents.Result.Split('\n');
            if (infos.Length > 0)
            {
                dataGridView1.Rows.Add(infos);

            }

            //var testResult = response.Content.ReadAsAsync<ResponseAPI>().Result;
            return Task.FromResult(response);
        }
        private void addCertification()
        {
            
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            clientHandler = new WebRequestHandler();
            string crt = Path.Combine(System.Windows.Forms.Application.StartupPath, "client.pfx");
            X509Certificate2 modCert = new X509Certificate2(crt, "qwe123..");
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
           
            store.Open(OpenFlags.ReadWrite);
            store.Add(modCert);

         
            clientHandler.ClientCertificates.Add(modCert);
            clientHandler.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            //httpClient = gcnew HttpClient(clientHandler);
            //HttpContent ^ httpContent = gcnew ByteArrayContent(state->postBody);
            //httpContent->Headers->ContentType = gcnew MediaTypeHeaderValue("application/octet-stream");
            //resultTask = httpClient->PostAsync(state->httpRequest, httpContent);

            
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            addCertification();
            await HttpComm();
            producer = new Thread(new ThreadStart(listenToSNMP));
            producer.Start();
            //producer.Join();   // Join both threads with no timeout
        
           

        }
        private async void postdata(postFuncType type, string plateNum = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(serverURL);
                request.ContentType = "application/json";
                string response = string.Empty;
                request.Credentials = CredentialCache.DefaultCredentials;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                string data = null;
                if (type == postFuncType.Auth)
                {
                    data = "{ \"id\": \"user\", \"plateNum\": \"LBV6157\" }";
                }
                else if (type == postFuncType.Plate)
                {
                    data = string.Format("\"id\": \"user\", \"plateNum\": \"{0}\"", plateNum);
                    data = "{" + data + "}";
                    //data = "{ \"id\": \"user\", \"plateNum\": \"LBV615\" }";
                }
                else
                {
                    data = "{ \"id\": \"user\", \"plateNum\": \"LBV6157\" }";
                }

                // request setting
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 300;
                //request.ContentType = "application/x-www-form-urlencoded";
                //request.Headers.Add("Authorization", "BASIC SGVsbG8=");

                // Data Stream setting
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                request.ContentLength = bytes.Length;

                // POST Request
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                }

                // Response
                string responseText = string.Empty;
                using (HttpWebResponse res = (HttpWebResponse)request.GetResponse())
                {
                    HttpStatusCode status = res.StatusCode;
                    if (status == HttpStatusCode.OK)
                    {
                        Stream response_stream = res.GetResponseStream();
                        using (StreamReader read_stream = new StreamReader(response_stream))
                        {
                            response = read_stream.ReadToEnd();
                        }
                        this.Invoke(new Action(delegate ()
                        {
                            TXT_DESC.AppendText(response);
                            if(!response.Contains("@@@@"))
                            {
                                string[] infos = response.Split('\n');
                                if (infos.Length > 0)
                                {
                                    dataGridView1.Rows.Add(infos);

                                }
                            }
                     
                           
                        }));
                    }

                }

                Console.WriteLine(response);
            }
            catch(Exception ex)
            {

            }

            
            
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
                    producer.Abort();
                    Application.ExitThread();
                    Environment.Exit(0);
                }
                    

                // Unicode-encode the byte array and trim all the '\0' chars at 
                // the end.
                strMessage = Encoding.Unicode.GetString(bRequest).TrimEnd('\0');
                Console.WriteLine("Receives {0} bytes; Message: \"{1}\"",
                    cbBytesRead, strMessage);
                string[] platenum = strMessage.Split('\n');
                if(platenum.Length > 0)
                {
                    this.Invoke(new Action(delegate () 
                    {
                        if (platenum.Length == 1)
                        {
                            //TXT_DESC.AppendText("Plate Number: " + temp[0] + Environment.NewLine);
                            if (platenum[0].Length > 4)
                                postdata(postFuncType.Plate, platenum[0]);
                        }
                            
                        else
                            dataGridView1.Rows.Add(platenum);
                    }));
           
                }
                  

            }

            PipeNative.FlushFileBuffers(hPipe);
            PipeNative.DisconnectNamedPipe(hPipe);
            PipeNative.CloseHandle(hPipe);
            //Console.ReadKey();
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
          
            Application.ExitThread();
            Process[] processList = Process.GetProcessesByName("lgdemo_w");

            if (processList.Length > 0)
            {
                processList[0].Kill();
            }
            Environment.Exit(0);
           
        }

        private void menuToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text.ToUpper() == "SAVE")
                savetoFile();

            menuToolStripMenuItem.HideDropDown();
            
        }

        private void BTN_LOGIN_Click(object sender, EventArgs e)
        {
            //string[] temp = { "1", "2" ,"3", "4", "5", "6", "7", "8", "9", "10", "11"};
            //dataGridView1.Rows.Add(temp);
            using (var process = new Process())
            {
                process.StartInfo.FileName = "lgdemo_w.exe";
                //process.StartInfo.Arguments = "/?";
                process.StartInfo.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                if (File.Exists(Path.Combine(process.StartInfo.WorkingDirectory, "lgdemo_w.exe")))
                {
                    process.Start();
                    IntPtr ptr = IntPtr.Zero;
                    while ((ptr = process.MainWindowHandle) == IntPtr.Zero) ;
                    SetParent(process.MainWindowHandle, panel1.Handle);
                    MoveWindow(process.MainWindowHandle, 0, 0, panel1.Width, panel1.Height, true);
                }
            }

        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            producer.Abort();
           
        }
    }
}
