using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using FodboldApp.Model;
using System.Net;
using System.IO;

namespace FodboldServerLogic
{
    //sends notifications
    class RESTTest
    {

        public static void Run(string Message)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            request.Headers.Add("authorization", "Basic OWQ4ODY1ZjUtOGQzNi00ZDU4LWJmNGUtOTRmMWYxNGRkY2Mz");

            byte[] byteArray = Encoding.UTF8.GetBytes("{"
                                                    + "\"app_id\": \"84ec0128-74a1-40f9-89b1-35e35da35acd\","
                                                    + "\"contents\": {\"en\": \"" + Message + "\"},"
                                                    + "\"included_segments\": [\"Live Match Subscription\"]}");

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
        }
    }
}
