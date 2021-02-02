using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using PX.Data;

namespace PX.Objects.FS.RouteOtimizer
{
    public class RouteOptimizerClient
    {

        /// <summary>
        /// Optimizes a set of waypoints so that they are best allocated to a set of vehicles taking into 
        /// account the following complex constraints:
        /// 1 - vehicle limited capacity and working time-window;
        /// 2 - waypoint service time, delivery time-window and priority;
        /// </summary>
        /// <param name="requestBody">Set of waypoints and vechicle config</param>
        /// <param name="ApiKey">The API key to use for the client connection</param>
        /// <returns></returns>
        public SingleDayOptimizationOutput getSingleDayOptimization(string url, string apiKey ,SingleDayOptimizationInput requestBody)
        {
            SingleDayOptimizationOutput responseObject = new SingleDayOptimizationOutput();

            if (url != null)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.Trim() + "?key=" + apiKey);

                string jsonData = JsonConvert.SerializeObject(requestBody);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = jsonData.Length;
                using (Stream webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                {
                    requestWriter.Write(jsonData);
                }

                try
                {
                    WebResponse webResponse = request.GetResponse();
                    using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                    using (StreamReader responseReader = new StreamReader(webStream))
                    {
                        string response = responseReader.ReadToEnd();

                        responseObject = JsonConvert.DeserializeObject<SingleDayOptimizationOutput>(response);
                    }
                }
                catch (WebException ex)
                {
                    using (Stream webStream = ex.Response.GetResponseStream() ?? Stream.Null)
                    using (StreamReader responseReader = new StreamReader(webStream))
                    {
                        string response = responseReader.ReadToEnd();

                        Error errorResponse = JsonConvert.DeserializeObject<Error>(response);

                        string errorMessage;

                        switch (errorResponse.errorCode) 
                        {
                            case "-100":
                                errorMessage = TX.Error.WORKWAVE_INTERNAL_SERVER_ERROR;
                                break;
                            case "-200":
                                errorMessage = TX.Error.WORKWAVE_UNKNOWN_API_KEY;
                                break;
                            case "-400":
                                errorMessage = TX.Error.WORKWAVE_KEY_CONSTRAINT_VIOLATED;
                                break;
                            case "-500":
                                errorMessage = TX.Error.WORKWAVE_KEY_DAILY_LIMIT_EXCEEDED;
                                break;
                            case "-600":
                                errorMessage = TX.Error.WORKWAVE_KEY_EXPIRED;
                                break;
                            case "-900":
                                errorMessage = TX.Error.WORKWAVE_TOO_MANY_REQUESTS;
                                break;
                            case "-901":
                                errorMessage = TX.Error.WORKWAVE_TOO_MANY_CONCURRENT_REQUESTS;
                                break;
                            case "-1000":
                                errorMessage = TX.Error.WORKWAVE_MALFORMED_REQUEST;
                            break;
                            default:
                                errorMessage = errorResponse.errorDescription;
                            break;
                        }

                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(errorMessage));
                    }
                }
            }

            return responseObject;
        }
    }
}