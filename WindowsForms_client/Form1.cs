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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace WindowsForms_client
{

    enum postFuncType
    {
        Auth,
        Plate,
        OTPReqeust
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
        string tokenVal = null;
        const int BUFFER_SIZE = 4096;  // 4 KB
        //const string serverURL = "http://localhost:8080";
        const string serverURL = "https://localhost:8443";
        //const string serverURL = "http://10.58.6.244:8080/db/";
        Form2 otpInput = null;
        public string receivedOTPNumber;
        System.Windows.Forms.Timer loginWating = null;
        bool bLoginPASS = false;
        //https://10.58.6.244:8443/db  
        //https://10.58.6.244:8080/db
        class ResponseAPI
        {
            public string id { get; set; }
            public string plateNum { get; set; }

        }
        public class receivedJSON
        {
            public string code;
            public string msg;
            public string token;

        }
        public class receivedPlateInfo
        {
            public string licensenumber; //#1
            public string licensestatus; //#2
            public string licenseexpdate; //#3
            public string ownername;//#4
            public string ownerbirthday; //#5
            public string owneraddress;//#6
            public string ownercity;//#7
            public string vhemake;//#8
            public string vhemodel; // #9
            public string vhecolor; //#10


        }
        public class queryAccount
        {
            public string userid;
            public string password;
        }
        public class queryPlate
        {
            public string plateNum;
            public string id;
        }
        public class queryOTP
        {
            public string userid;
            public string otpKey;
        }
        class Hash
        {
            public static string getHashSha256(string text)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                SHA256Managed hashstring = new SHA256Managed();
                byte[] hash = hashstring.ComputeHash(bytes);
                string hashString = string.Empty;
                foreach (byte x in hash)
                {
                    hashString += String.Format("{0:x2}", x);
                }
                return hashString;
            }
        }
        public MainFrm()
        {

            InitializeComponent();
            loginWating = new System.Windows.Forms.Timer();
            loginWating.Tick += LoginWating_Tick;
            loginWating.Interval = 50;
            this.Location = new Point(0, 0);
            //AllocConsole();
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            setCertification();
        }

        private void LoginWating_Tick(object sender, EventArgs e)
        {
            progressBar1.Increment(1);
            if (progressBar1.Value >= progressBar1.Maximum)
                loginWating.Stop();

        }
        private static X509Certificate2 GetCertificateFromStore(string certName)
        {

            // Get the certificate store for the current user.
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly);

                // Place all certificates in an X509Certificate2Collection object.
                X509Certificate2Collection certCollection = store.Certificates;
                // If using a certificate with a trusted root you do not need to FindByTimeValid, instead:
                // currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, true);
                //X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                X509Certificate2Collection signingCert = certCollection.Find(X509FindType.FindBySubjectDistinguishedName, certName, true);
                if (signingCert.Count == 0)
                    return null;
                // Return the first certificate in the collection, has the right name and is current.
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }
        private void setCertification()
        {
            clientHandler = new WebRequestHandler();
            //string crt = Path.Combine(System.Windows.Forms.Application.StartupPath, "client.pfx");
            //X509Certificate2 modCert = new X509Certificate2(crt,"qwe123..");
            string certName = "CN=CLIENT, OU=CMU22_DEF, O=LGE, L=PGH, S=PA, C=US";
            X509Certificate2 cert = GetCertificateFromStore(certName);
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadWrite);
            //store.Add(modCert);
            clientHandler.ClientCertificates.Add(cert);
            clientHandler.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
        }


        private Task<HttpResponseMessage> HttpPostMessageSecure(postFuncType type, string plateNum = null, string userid = null, string password = null, int timeout = 300, string optNum = null)
        {
            HttpResponseMessage response = null;
            try
            {

                string urlAddress = serverURL;
                switch (type)
                {
                    case postFuncType.Auth:
                        urlAddress += "/auth/login";
                        break;
                    case postFuncType.Plate:
                        urlAddress += "/testJSON";
                        break;
                    case postFuncType.OTPReqeust:
                        urlAddress += "/auth/otp-check";
                        break;
                    default:
                        break;
                }
                ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                HttpClient _httpClient = new HttpClient(clientHandler);
                _httpClient.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                _httpClient.BaseAddress = new Uri(urlAddress);
                //tokenVal = "eyJhbGciOiJIUzI1NiJ9.eyJ1c2VybmFtZSI6ImFhYSIsImlhdCI6MTY1NjM5ODE0NSwiZXhwIjoxNjU2NDg0NTQ1fQ.H5e0L_3jLRvi-ShNEy4uQi3eG75DNsuudmJXtqRI9Gs";
                string bearer = "Bearer " + tokenVal;
                _httpClient.DefaultRequestHeaders.Add("Authorization", bearer);
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //Request
                queryAccount accountInfo = null;
                queryPlate plateInfo = null;
                queryOTP OTPInfo = null;
                switch (type)
                {
                    case postFuncType.Auth:
                        accountInfo = new queryAccount();
                        accountInfo.userid = userid;
                        accountInfo.password = password;
                        response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, accountInfo).Result;
                        break;
                    case postFuncType.Plate:
                        plateInfo = new queryPlate();
                        plateInfo.id = "user";
                        plateInfo.plateNum = plateNum;
                        response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, plateInfo).Result;
                        break;
                    case postFuncType.OTPReqeust:
                        OTPInfo = new queryOTP();
                        OTPInfo.userid = userid;
                        OTPInfo.otpKey = receivedOTPNumber;
                        response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, OTPInfo).Result;
                        break;
                    default:
                        break;

                }
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //response 
                    var contents = response.Content.ReadAsStringAsync();
                    receivedJSON recevJson = null;
                    this.Invoke(new Action(delegate ()
                    {
                        TXT_DESC.AppendText(contents.Result.ToString());

                    }));
                    switch (type)
                    {
                        case postFuncType.Auth:
                            recevJson = response.Content.ReadAsAsync<receivedJSON>().Result;
                            if (recevJson.code == "200")
                                bLoginPASS = true;
                            break;
                        case postFuncType.Plate:
                            var recevJsonPlate = response.Content.ReadAsAsync<receivedPlateInfo>().Result;
                            this.Invoke(new Action(delegate ()
                            {
                                dataGridView1.Rows.Add(recevJsonPlate.licensenumber,
                                    recevJsonPlate.licensestatus, recevJsonPlate.licenseexpdate,
                                    recevJsonPlate.ownername, recevJsonPlate.ownerbirthday, recevJsonPlate.owneraddress,
                                    recevJsonPlate.ownercity, recevJsonPlate.vhemake, recevJsonPlate.vhemodel, recevJsonPlate.vhecolor);

                            }));
                            break;
                        case postFuncType.OTPReqeust:
                            recevJson = response.Content.ReadAsAsync<receivedJSON>().Result;
                            if (recevJson.code == "200")
                            {
                                tokenVal = recevJson.token;
                                this.Invoke(new Action(delegate ()
                                {
                                    TXT_ID.Enabled = false;
                                    TXT_PW.Enabled = false;
                                    BTN_LOGIN.Enabled = false;
                                }));
                                MessageBox.Show("Success to login!!!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                loadALPR();
                            }
                            else if(recevJson.code == "900")
                            {
                                MessageBox.Show("Your OTP number is wrong!!!\nPlease confirm your OTP number","OTP Number",MessageBoxButtons.OK,MessageBoxIcon.Information);
                            }

                            break;
                        default:
                            break;

                    }



                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

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
            clientHandler.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequired;
            clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            //httpClient = gcnew HttpClient(clientHandler);
            //HttpContent ^ httpContent = gcnew ByteArrayContent(state->postBody);
            //httpContent->Headers->ContentType = gcnew MediaTypeHeaderValue("application/octet-stream");
            //resultTask = httpClient->PostAsync(state->httpRequest, httpContent);


        }
        private void Form1_Load(object sender, EventArgs e)
        {

            producer = new Thread(new ThreadStart(listenALPR));
            producer.Start();

        }
        //private bool postdata(postFuncType type, string plateNum = null, string userid = null, string password = null, int timeout = 300, string optNum = null)
        //{

        //    bool result = false;
        //    string urlAddress = serverURL;
        //    try
        //    {
        //        switch (type)
        //        {
        //            case postFuncType.Auth:
        //                urlAddress += "/auth/login";
        //                break;
        //            case postFuncType.Plate:
        //                urlAddress += "/testJSON";
        //                break;
        //            case postFuncType.OTPReqeust:
        //                urlAddress += "/auth/otp-check";
        //                break;
        //            default:
        //                break;
        //        }
        //        var request = (HttpWebRequest)WebRequest.Create(urlAddress);
        //        request.ContentType = "application/json";
        //        string response = string.Empty;
        //        request.Credentials = CredentialCache.DefaultCredentials;
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
        //        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        //        string data = null;
        //        if (type == postFuncType.Auth)
        //        {

        //            //string hsPW = Hash.getHashSha256(password);
        //            //data = string.Format("\"userid\": \"{0}\", \"password\": \"{1}\"", userid, hsPW);
        //            data = string.Format("\"userid\": \"{0}\", \"password\": \"{1}\"", userid, password);

        //            data = "{" + data + "}";
        //        }
        //        else if (type == postFuncType.Plate)
        //        {
        //            string bearer = "Bearer " + tokenVal;
        //            request.Headers.Add("Authorization", bearer);
        //            data = string.Format("\"id\": \"user\", \"plateNum\": \"{0}\"", plateNum);
        //            data = "{" + data + "}";
        //            //data = "{ \"id\": \"user\", \"plateNum\": \"LBV615\" }";
        //        }
        //        else if (type == postFuncType.OTPReqeust)
        //        {
        //            data = string.Format("\"userid\": \"{0}\", \"otpKey\": \"{1}\"", userid, receivedOTPNumber);
        //            data = "{" + data + "}";

        //        }
        //        else
        //        {
        //            data = "{ \"id\": \"user\", \"plateNum\": \"LBV6157\" }";
        //        }

        //        // request setting
        //        request.Method = "POST";
        //        request.ContentType = "application/json";
        //        request.Timeout = timeout;
        //        //request.ContentType = "application/x-www-form-urlencoded";


        //        // Data Stream setting
        //        byte[] bytes = Encoding.ASCII.GetBytes(data);
        //        request.ContentLength = bytes.Length;

        //        // POST Request
        //        using (Stream reqStream = request.GetRequestStream())
        //        {
        //            reqStream.Write(bytes, 0, bytes.Length);
        //        }

        //        // Response
        //        string responseText = string.Empty;

        //        using (HttpWebResponse res = (HttpWebResponse)request.GetResponse())
        //        {

        //            HttpStatusCode status = res.StatusCode;
        //            if (status == HttpStatusCode.OK)
        //            {
        //                Stream response_stream = res.GetResponseStream();
        //                using (StreamReader read_stream = new StreamReader(response_stream))
        //                {
        //                    response = read_stream.ReadToEnd();
        //                }
        //                this.Invoke(new Action(delegate ()
        //                {
        //                    TXT_DESC.AppendText(response);
        //                    receivedJSON obj = JsonConvert.DeserializeObject<receivedJSON>(response);
        //                    switch (type)
        //                    {
        //                        case postFuncType.Auth:
        //                            if (obj.code == "200")
        //                            {
        //                                bLoginPASS = true;
        //                            }
        //                            break;
        //                        case postFuncType.Plate:
        //                            break;
        //                        case postFuncType.OTPReqeust:
        //                            break;
        //                        default:
        //                            break;
        //                    }


        //                    if (obj.code == "200")
        //                    {

        //                    }
        //                    //TXT_DESC.AppendText(rootObject.code);
        //                    //TXT_DESC.AppendText(rootObject.msg);
        //                    //TXT_DESC.AppendText(rootObject.token);
        //                    if (!response.Contains("@@@@"))
        //                    {
        //                        string[] infos = response.Split('\n');
        //                        if (infos.Length > 0)
        //                        {
        //                            dataGridView1.Rows.Add(infos);

        //                        }
        //                    }


        //                }));
        //            }

        //        }

        //        Console.WriteLine(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = false;
        //        MessageBox.Show(ex.ToString());
        //    }
        //    return result;


        //}
        void listenALPR()
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
                if (platenum.Length > 0)
                {
                    this.Invoke(new Action(delegate ()
                    {
                        TXT_DESC.AppendText("Plate Number: " + platenum[0] + Environment.NewLine);
                    }));
                    Task.Run(() =>
                    {
                        if (platenum[0].Length >= 6)
                            HttpPostMessageSecure(postFuncType.Plate, platenum[0]);
                    });


                }


            }

            PipeNative.FlushFileBuffers(hPipe);
            PipeNative.DisconnectNamedPipe(hPipe);
            PipeNative.CloseHandle(hPipe);
            //Console.ReadKey();
        }


        // Encrypt a file using a public key.
        private static void EncryptFile(string inFile, RSA rsaPublicKey)
        {
            using (Aes aes = Aes.Create())
            {
                // Create instance of Aes for
                // symetric encryption of the data.
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                using (ICryptoTransform transform = aes.CreateEncryptor())
                {
                    RSAPKCS1KeyExchangeFormatter keyFormatter = new RSAPKCS1KeyExchangeFormatter(rsaPublicKey);
                    byte[] keyEncrypted = keyFormatter.CreateKeyExchange(aes.Key, aes.GetType());

                    // Create byte arrays to contain
                    // the length values of the key and IV.
                    byte[] LenK = new byte[4];
                    byte[] LenIV = new byte[4];

                    int lKey = keyEncrypted.Length;
                    LenK = BitConverter.GetBytes(lKey);
                    int lIV = aes.IV.Length;
                    LenIV = BitConverter.GetBytes(lIV);

                    // Write the following to the FileStream
                    // for the encrypted file (outFs):
                    // - length of the key
                    // - length of the IV
                    // - ecrypted key
                    // - the IV
                    // - the encrypted cipher content
                    string encrFolder = Path.GetDirectoryName(inFile);
                    int startFileName = inFile.LastIndexOf("\\") + 1;
                    // Change the file's extension to ".enc"
                    string outFile = encrFolder + "\\" + inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";
                    //Directory.CreateDirectory(encrFolder);

                    using (FileStream outFs = new FileStream(outFile, FileMode.CreateNew))
                    {

                        outFs.Write(LenK, 0, 4);
                        outFs.Write(LenIV, 0, 4);
                        outFs.Write(keyEncrypted, 0, lKey);
                        outFs.Write(aes.IV, 0, lIV);

                        // Now write the cipher text using
                        // a CryptoStream for encrypting.
                        using (CryptoStream outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                        {

                            // By encrypting a chunk at
                            // a time, you can save memory
                            // and accommodate large files.
                            int count = 0;

                            // blockSizeBytes can be any arbitrary size.
                            int blockSizeBytes = aes.BlockSize / 8;
                            byte[] data = new byte[blockSizeBytes];
                            int bytesRead = 0;

                            using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                            {
                                do
                                {
                                    count = inFs.Read(data, 0, blockSizeBytes);
                                    outStreamEncrypted.Write(data, 0, count);
                                    bytesRead += count;
                                }
                                while (count > 0);
                                inFs.Close();
                            }
                            outStreamEncrypted.FlushFinalBlock();
                            outStreamEncrypted.Close();
                        }
                        outFs.Close();
                        File.Delete(inFile);
                    }
                }
            }
        }


        // Decrypt a file using a private key.
        private static void DecryptFile(string inFile, RSA rsaPrivateKey)
        {

            // Create instance of Aes for
            // symetric decryption of the data.
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;

                // Create byte arrays to get the length of
                // the encrypted key and IV.
                // These values were stored as 4 bytes each
                // at the beginning of the encrypted package.
                byte[] LenK = new byte[4];
                byte[] LenIV = new byte[4];

                string decrFolder = Path.GetDirectoryName(inFile);
                int startFileName = inFile.LastIndexOf("\\") + 1;

                // Construct the file name for the decrypted file.
                string outFile = decrFolder + "\\" + Path.GetFileNameWithoutExtension(inFile) + ".csv";
                // Use FileStream objects to read the encrypted
                // file (inFs) and save the decrypted file (outFs).
                using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                {

                    inFs.Seek(0, SeekOrigin.Begin);
                    inFs.Seek(0, SeekOrigin.Begin);
                    inFs.Read(LenK, 0, 3);
                    inFs.Seek(4, SeekOrigin.Begin);
                    inFs.Read(LenIV, 0, 3);

                    // Convert the lengths to integer values.
                    int lenK = BitConverter.ToInt32(LenK, 0);
                    int lenIV = BitConverter.ToInt32(LenIV, 0);

                    // Determine the start position of
                    // the cipher text (startC)
                    // and its length(lenC).
                    int startC = lenK + lenIV + 8;
                    int lenC = (int)inFs.Length - startC;

                    // Create the byte arrays for
                    // the encrypted Aes key,
                    // the IV, and the cipher text.
                    byte[] KeyEncrypted = new byte[lenK];
                    byte[] IV = new byte[lenIV];

                    // Extract the key and IV
                    // starting from index 8
                    // after the length values.
                    inFs.Seek(8, SeekOrigin.Begin);
                    inFs.Read(KeyEncrypted, 0, lenK);
                    inFs.Seek(8 + lenK, SeekOrigin.Begin);
                    inFs.Read(IV, 0, lenIV);
                    Directory.CreateDirectory(decrFolder);
                    // Use RSA
                    // to decrypt the Aes key.
                    byte[] KeyDecrypted = rsaPrivateKey.Decrypt(KeyEncrypted, RSAEncryptionPadding.Pkcs1);

                    // Decrypt the key.
                    using (ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV))
                    {

                        // Decrypt the cipher text from
                        // from the FileSteam of the encrypted
                        // file (inFs) into the FileStream
                        // for the decrypted file (outFs).
                        using (FileStream outFs = new FileStream(outFile, FileMode.Create))
                        {

                            int count = 0;

                            int blockSizeBytes = aes.BlockSize / 8;
                            byte[] data = new byte[blockSizeBytes];

                            // By decrypting a chunk a time,
                            // you can save memory and
                            // accommodate large files.

                            // Start at the beginning
                            // of the cipher text.
                            inFs.Seek(startC, SeekOrigin.Begin);
                            using (CryptoStream outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                            {
                                do
                                {
                                    count = inFs.Read(data, 0, blockSizeBytes);
                                    outStreamDecrypted.Write(data, 0, count);
                                }
                                while (count > 0);

                                outStreamDecrypted.FlushFinalBlock();
                                outStreamDecrypted.Close();
                            }
                            outFs.Close();
                        }
                        inFs.Close();
                        File.Delete(inFile);
                    }
                }
            }
        }

        private void menu_fileEncrypt()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV (*.csv)|*.csv";
            bool fileError = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string certName = "CN=CLIENT, OU=CMU22_DEF, O=LGE, L=PGH, S=PA, C=US";
                    // Get the certifcate to use to encrypt the key.
                    X509Certificate2 cert = GetCertificateFromStore(certName);
                    if (cert == null)
                    {
                        TXT_DESC.AppendText("Certificate not found.\n");
                        return;
                    }

                    // Encrypt the file using the public key from the certificate.
                    EncryptFile(ofd.FileName, (RSA)cert.PublicKey.Key);
                    MessageBox.Show("Success to encrypt file!!!","Encrypt File",MessageBoxButtons.OK,MessageBoxIcon.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    fileError = true;
                }
                
            }
        }
        private void menu_fileDecrypt()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "enc (*.enc)|*.enc";
            bool fileError = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string certName = "CN=CLIENT, OU=CMU22_DEF, O=LGE, L=PGH, S=PA, C=US";
                    // Get the certifcate to use to encrypt the key.
                    X509Certificate2 cert = GetCertificateFromStore(certName);
                    if (cert == null)
                    {
                        TXT_DESC.AppendText("Certificate not found.\n");
                        return;
                    }

                    // Encrypt the file using the public key from the certificate.
                    DecryptFile(ofd.FileName, cert.GetRSAPrivateKey());
                    MessageBox.Show("Success to decrypt file!!!", "Decrypt File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    fileError = true;
                }

            }
        }
        private void menu_savetoFile()
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
                            MessageBox.Show("Data Exported Successfully !!!", "Save File",MessageBoxButtons.OK,MessageBoxIcon.Information);
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
        private void menu_Logout()
        {

            this.Invoke(new Action(delegate ()
            {
                BTN_LOGIN.Enabled = true;
                TXT_ID.Enabled = true;
                TXT_PW.Enabled = true;
                progressBar1.Value = 0;
                TXT_DESC.Clear();
                TXT_ID.Clear();
                TXT_PW.Clear();
                tokenVal = "";
                panel2.Visible = false;
                panel1.Visible = true;
            }));
          

        }
        private void menuToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            menuToolStripMenuItem.HideDropDown();
            if (e.ClickedItem.Text.ToUpper() == "SAVE")
            {
                menu_savetoFile();
            }
            else if (e.ClickedItem.Text.ToUpper() == "LOGOUT")
            {
                menu_Logout();
                MessageBox.Show("Success to logout!!!","Logout",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            else if (e.ClickedItem.Text.ToUpper() == "FILE ENCRYPT")
            {
                menu_fileEncrypt();
            }
            else if (e.ClickedItem.Text.ToUpper() == "FILE DECRYPT")
            {
                menu_fileDecrypt();
            }
           

        }

        private async void BTN_LOGIN_Click(object sender, EventArgs e)
        {

            this.Invoke(new Action(delegate ()
            {
                this.progressBar1.Value = 0;
                this.progressBar1.Maximum = 150;
            }));
            loginWating.Start();
            await Task.Run(() =>
            {
                //1. ID/PW넣고 서버에 송신
                HttpPostMessageSecure(postFuncType.Auth, null, TXT_ID.Text, TXT_PW.Text, 20000);
            });

            this.Invoke(new Action(delegate ()
            {
                loginWating.Stop();
                this.progressBar1.Value = progressBar1.Maximum;

            }));

            if (bLoginPASS)
            {
                if (otpInput == null)
                {
                    //개인 계정을 통해 OTP번호 확인 후 기입 후 송신
                    otpInput = new Form2(receivedOTPNumber);
                    otpInput.Owner = this;
                    if (otpInput.ShowDialog() == DialogResult.OK)
                    {
                        //Request OTP
                        await Task.Run(() =>
                        {
                            HttpPostMessageSecure(postFuncType.OTPReqeust, null, TXT_ID.Text, null, 10000, receivedOTPNumber);
                        });

                    }
                }
                else
                {
                    if (otpInput.ShowDialog() == DialogResult.OK)
                    {
                        await Task.Run(() =>
                        {
                            HttpPostMessageSecure(postFuncType.OTPReqeust, null, TXT_ID.Text, null, 10, receivedOTPNumber);

                        });
                    }
                }
            }
            else
            {
                MessageBox.Show("Log-In Failed\nPlease check your ID and password", "Log-In", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            bLoginPASS = false;



        }
        private void loadALPR()
        {
            this.Invoke(new Action(delegate ()
            {
                panel2.Visible = true;
               
            }));
      
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
                    SetParent(process.MainWindowHandle, panel2.Handle);
                    MoveWindow(process.MainWindowHandle, 0, 0, panel2.Width, panel2.Height, true);
                }
            }
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            producer.Abort();

        }
        private Task ProcessData()
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    this.Invoke(new Action(async delegate ()
                    {
                        // Task.Delay(10);
                        progressBar1.Increment(1);
                    }));



                }

            });

        }
    }
}
