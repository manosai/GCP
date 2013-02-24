using System.Net;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Web; 
using System.Runtime.Serialization; 
using System.Runtime.Serialization.Json;

namespace GoogleCloudPrint
{

    public class GoogleCloudPrint
    {
        static void Main(string[] args)
        {
          
            var cloudPrint = new GoogleCloudPrint();
            cloudPrint.UserName = "mano.eerabathini@gmail.com";
            cloudPrint.Password = "******";
            byte[]data = File.ReadAllBytes("C:/Users/manosaie/Desktop/sample.pdf");
            Console.WriteLine(data.Length); 
            cloudPrint.PrintDocument("8m9tozfwqmm4sjcyq4sibq", "Test Page", data); 
            Console.WriteLine("Press something to close:");
            Console.ReadKey(); 
        }
        public GoogleCloudPrint()
        {
            Source = "basewebtek-youreontime-v1";
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Source { get; set; }

        public CloudPrintJob PrintDocument(string printerId, string title, byte[]document)
        {
            
            try
            {
                string authCode;
                if (!Authorize(out authCode))
                {
                    return new CloudPrintJob() { success = false };
                }

                 string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x"); 

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://www.google.com/cloudprint/submit?output=json"); 
            webRequest.ContentType = "multipart/form-data; boundary=" + boundary; 
            webRequest.Method = "POST"; 
            webRequest.UserAgent = "Agility WMS"; 
            webRequest.SendChunked = false; 

            webRequest.Headers.Add("X-CloudPrint-Proxy", Source); 
            webRequest.Headers.Add("Authorization", "GoogleLogin auth=" + authCode); 

            // Build Contents for Post 
            string header = string.Format("--{0}", boundary); 
            string footer = header + "--"; 

            StringBuilder contents = new StringBuilder(); 

            // title 
            contents.AppendLine(header); 
            contents.AppendLine("Content-Disposition: form-data; name= \"title\""); 
            contents.AppendLine(); 
            contents.AppendLine(title); 

            // capabilities 
            contents.AppendLine(header); 
            contents.AppendLine("Content-Disposition: form-data; name= \"capabilities\""); 
            contents.AppendLine(); 
            contents.AppendLine(""); 

            // printerid 
            contents.AppendLine(header); 
            contents.AppendLine("Content-Disposition: form-data; name= \"printerid\""); 
            contents.AppendLine(); 
            contents.AppendLine(printerId); 

            // contentType 
            contents.AppendLine(header); 
            contents.AppendLine("Content-Disposition: form-data; name= \"contentType\""); 
            contents.AppendLine(); 
            contents.AppendLine("dataUrl"); 

           
            // content 
            contents.AppendLine(header); 
            contents.AppendLine("Content-Disposition: form-data; name= \"content\""); 
            contents.AppendLine(); 
            contents.AppendLine("data:application/pdf;base64," + Convert.ToBase64String(document)); 

            // Footer 
            contents.AppendLine(footer); 

            // This is sent to the Post 
            byte[] data = Encoding.ASCII.GetBytes(contents.ToString()); 

            webRequest.ContentLength = data.Length; 

            Stream stream = webRequest.GetRequestStream(); 
            stream.Write(data, 0, data.Length); 
            stream.Close(); 

            WebResponse webResponse = webRequest.GetResponse(); 
            string responseContent = new 
            StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            Console.WriteLine(responseContent); 

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CloudPrintJob)); 
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(responseContent)); 

            webResponse.Close(); 
            webRequest = null; 
            webResponse = null; 

            CloudPrintJob printJob = serializer.ReadObject(ms) as CloudPrintJob;
            return printJob;
            }
            catch (Exception ex)
            {
                return new CloudPrintJob() { success = false, message = ex.Message };
            }
        }

        public CloudPrinters Printers
        {
            get
            {
                var printers = new CloudPrinters();

                string authCode;
                if (!Authorize(out authCode))
                    return new CloudPrinters() { success = false };

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.google.com/cloudprint/search?output=json");
                    request.Method = "POST";

                    request.Headers.Add("X-CloudPrint-Proxy", Source);
                    request.Headers.Add("Authorization", "GoogleLogin auth=" + authCode);

                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = 0;

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string responseContent = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CloudPrinters));
                    MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(responseContent));
                    printers = serializer.ReadObject(ms) as CloudPrinters;

                    return printers;
                }
                catch (Exception)
                {
                    return printers;
                }
            }
        }

        private bool Authorize(out string authCode)
        {
            bool result = false;
            authCode = "";

            string queryString = String.Format("https://www.google.com/accounts/ClientLogin?accountType=HOSTED_OR_GOOGLE&Email={0}&Passwd={1}&service=cloudprint&source={2}", UserName, Password, Source);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(queryString);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseContent = new StreamReader(response.GetResponseStream()).ReadToEnd();

            string[] split = responseContent.Split('\n');
            foreach (string s in split)
            {
                string[] nvsplit = s.Split('=');
                if (nvsplit.Length == 2)
                {
                    if (nvsplit[0] == "Auth")
                    {
                        authCode = nvsplit[1];
                        result = true;
                    }
                }
            }

            return result;
        }
    }

    [DataContract]
    public class CloudPrintJob
    {
        [DataMember]
        public bool success { get; set; }

        [DataMember]
        public string message { get; set; }
    }

    [DataContract]
    public class CloudPrinters
    {
        [DataMember]
        public bool success { get; set; }

        [DataMember]
        public List<CloudPrinter> printers { get; set; }
    }

    [DataContract]
    public class CloudPrinter
    {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string description { get; set; }

        [DataMember]
        public string proxy { get; set; }

        [DataMember]
        public string status { get; set; }

        [DataMember]
        public string capsHash { get; set; }

        [DataMember]
        public string createTime { get; set; }

        [DataMember]
        public string updateTime { get; set; }

        [DataMember]
        public string accessTime { get; set; }

        [DataMember]
        public bool confirmed { get; set; }

        [DataMember]
        public int numberOfDocuments { get; set; }

        [DataMember]
        public int numberOfPages { get; set; }
    }
}