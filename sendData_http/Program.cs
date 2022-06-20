using System;
using System.IO;
using System.Net;
using System.Text;

namespace sendData_http
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string url = "http://127.0.0.1:8000/polls";
            var request = (HttpWebRequest)WebRequest.Create(url);

            var postData = "username=" + Uri.EscapeDataString("myUser");
            //postData += "&password=" + Uri.EscapeDataString("myPassword");
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "GET";
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = data.Length;

            //using (var stream = request.GetRequestStream())
            //{
            //    stream.Write(data, 0, data.Length);
            //}

            var response = (HttpWebResponse)request.GetResponse();
            // 응답 Stream 읽기           
            Stream stReadData = response.GetResponseStream();
            StreamReader srReadData = new StreamReader(stReadData, Encoding.Default);
            // 응답 Stream -> 응답 String 변환  
            string strResult = srReadData.ReadToEnd();
            Console.WriteLine(strResult);
            Console.ReadLine();
        }
    }
}
