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
            string url = "http://127.0.0.1:8000/rest_api_test/";
            var request = (HttpWebRequest)WebRequest.Create(url);

            string response = string.Empty;
            string data = "{ \"id\": \"101\", \"name\" : \"Alex\" }";

            //// request setting

            request.Method = "POST";
            request.Headers.Add("X-CSRFToken", "hg8IWiQkLdI3VQO11L4daMgsyPezyoOidripNTGnOn1jFZrLj97cByeqvIIPqX22"); // 헤더 추가 방법
            request.ContentType = "application/json";
            request.Timeout = 10 * 1000;
            //request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = data.Length;

            ////// Data Stream setting
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            request.ContentLength = bytes.Length;

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }

            //// POST Request & Response
            string responseText = string.Empty;
            using (HttpWebResponse res = (HttpWebResponse)request.GetResponse())
            {
                HttpStatusCode status = res.StatusCode;
                Stream response_stream = res.GetResponseStream();
                using (StreamReader read_stream = new StreamReader(response_stream))
                {
                    response = read_stream.ReadToEnd();
                }
            }
            Console.WriteLine(response);
            Console.ReadLine();

            //var postData = "username=" + Uri.EscapeDataString("myUser");
            //postData += "&password=" + Uri.EscapeDataString("myPassword");
            //var data = Encoding.ASCII.GetBytes(postData);

            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = data.Length;

            //using (var stream = request.GetRequestStream())
            //{
            //    stream.Write(data, 0, data.Length);
            //}

            //var response = (HttpWebResponse)request.GetResponse();
            //// 응답 Stream 읽기           
            //Stream stReadData = response.GetResponseStream();
            //StreamReader srReadData = new StreamReader(stReadData, Encoding.Default);
            //// 응답 Stream -> 응답 String 변환  
            //string strResult = srReadData.ReadToEnd();
            //Console.WriteLine(strResult);
            //Console.ReadLine();
        }
    }
}
