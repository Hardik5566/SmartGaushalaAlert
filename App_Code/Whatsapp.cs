using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Reminder.App_Code
{
    internal class Whatsapp
    {
        private const string API_BASE_URL = "https://whatsapp.hlgroups.in";
        private const string API_KEY = "my_secret_key_123"; // Your working API key
        private const string DEVICE_ID = "919558001712"; // Your device ID

        /// <summary>
        /// Send bulk message to multiple numbers
        /// </summary>
        public static void send_bulk(string mobileNumbers, string message)
        {
            try
            {
                // FIX: Turn off Expect100Continue. Postman doesn't use it, and many servers reject it with a 403.
                ServicePointManager.Expect100Continue = false;

                // Added TLS 1.3 support along with TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)12288;

                string url = API_BASE_URL + "/api/send-bulk";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";

                // Decodes compressed responses safely 
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                // Headers carefully matched to browser/Postman standards
                request.ContentType = "application/json";
                request.Accept = "application/json, text/plain, */*";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                request.Headers.Add("x-api-key", API_KEY);
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                request.Headers.Add("Cache-Control", "no-cache");

                request.KeepAlive = true;
                request.Timeout = 30000; // 30 seconds

                // Build JSON payload
                string jsonBody = "{\"deviceId\":\"" + DEVICE_ID + "\",\"numbers\":\"" + mobileNumbers + "\",\"message\":\"" + EscapeJson(message) + "\"}";

                Console.WriteLine("Request URL: " + url);
                Console.WriteLine("Request Body: " + jsonBody);

                byte[] data = Encoding.UTF8.GetBytes(jsonBody);
                request.ContentLength = data.Length;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string result = reader.ReadToEnd();
                        Console.WriteLine("Response: " + result);
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("WebException: " + ex.Message);
                if (ex.Response != null)
                {
                    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string errorResponse = reader.ReadToEnd();
                        Console.WriteLine("Error Response: " + errorResponse);
                        Console.WriteLine("Status Code: " + ((HttpWebResponse)ex.Response).StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Send bulk message with image
        /// </summary>
        public static void send_bulk_with_image(string mobileNumbers, string message, string imageUrl)
        {
            try
            {
                // FIX: Turn off Expect100Continue here as well
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)12288;

                string url = API_BASE_URL + "/api/send-bulk";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";

                // Decodes compressed responses safely
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                // Headers carefully matched to browser/Postman standards
                request.ContentType = "application/json";
                request.Accept = "application/json, text/plain, */*";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                request.Headers.Add("x-api-key", API_KEY);
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                request.Headers.Add("Cache-Control", "no-cache");

                request.KeepAlive = true;
                request.Timeout = 30000;

                // Build JSON payload with image
                string jsonBody = "{\"deviceId\":\"" + DEVICE_ID + "\",\"numbers\":\"" + mobileNumbers + "\",\"message\":\"" + EscapeJson(message) + "\",\"imageUrl\":\"" + imageUrl + "\"}";

                Console.WriteLine("Request URL: " + url);
                Console.WriteLine("Request Body: " + jsonBody);

                byte[] data = Encoding.UTF8.GetBytes(jsonBody);
                request.ContentLength = data.Length;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string result = reader.ReadToEnd();
                        Console.WriteLine("Response: " + result);
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("WebException: " + ex.Message);
                if (ex.Response != null)
                {
                    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string errorResponse = reader.ReadToEnd();
                        Console.WriteLine("Error Response: " + errorResponse);
                        Console.WriteLine("Status Code: " + ((HttpWebResponse)ex.Response).StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Escape special characters for JSON
        /// </summary>
        private static string EscapeJson(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return input.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t");
        }

        /// <summary>
        /// Backward compatible method
        /// </summary>
        public static void send_text(string mobile_no, string message)
        {
            send_bulk(mobile_no, message);
        }
    }


}
