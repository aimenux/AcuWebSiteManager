using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace PX.Commerce.Shopify.API.REST
{
    public class RestException : Exception
    {
        public string ResponceMessage;
		public string ResponceStatusCode;
        public string ResponceContent;
		protected readonly List<string> _errors;

		public string ErrorMessage => base.Message;

        public RestException(IRestResponse response)
        : base(GetErrorMessage(response.ErrorMessage, response.StatusCode.ToString(), response.Content, GetErrorData(response)))
        {
			ResponceMessage = response.ErrorMessage;
			ResponceStatusCode = response.StatusCode.ToString();
            ResponceContent = response.Content;

            _errors = GetErrorData(response) ?? new List<string>();
        }

		public RestException(string message) : base(message)
		{
			ResponceMessage = message;
			ResponceStatusCode = string.Empty;
			ResponceContent = string.Empty;
			_errors = new List<string>() { message };
		}

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var error in _errors)
            {
                yield return error;
            }
        }

		public override string ToString()
		{
			return GetErrorMessage(ResponceMessage, ResponceStatusCode, ResponceContent, _errors);
		}
		public static String GetErrorMessage(String message, String statusCode, String content, List<string> restErrors)
		{
			StringBuilder sb = new StringBuilder();

			//Returned Errors
			bool errorsPersist = false;
			for (int i = 0; i < restErrors.Count; i++)
			{
				errorsPersist = true;
				sb.AppendFormat("Error on {0};", restErrors[i]);
				sb.AppendLine();
			}
			//if no errors parsed, display rough Content
			if (!errorsPersist)
			{
				//Content
				if (!String.IsNullOrEmpty(content))
				{
					sb.AppendLine($"Content:  {content}");
					sb.AppendLine();
				}
				sb.Append(ShopifyApiStatusCodes.GetCodeMessage(statusCode));
			}
			
			return sb.ToString();
		}

		public static List<string> GetErrorData(IRestResponse response)
		{
			if (response.StatusCode == HttpStatusCode.OK ||
				response.StatusCode == HttpStatusCode.Created ||
				response.StatusCode == HttpStatusCode.Accepted ||
				response.StatusCode == HttpStatusCode.NoContent)
			{
				return null;
			}

			List<string> errorList = new List<string>();
			if (string.IsNullOrWhiteSpace(response.Content)) return errorList;

			DeserializeJson(JToken.Parse(response.Content), string.Empty, ref errorList);
			return errorList;
		}

		private static void DeserializeJson(JToken content, string name, ref List<string> errorList)
		{
			if (content != null)
			{
				switch (content.Type)
				{
					case JTokenType.Object when content.HasValues:
						{
							foreach (var item in content.Children())
							{
								DeserializeJson(item, name, ref errorList);
							}
							break;
						}
					case JTokenType.Array when ((JArray)content)?.Count > 0:
						{
							foreach (var arr in ((JArray)content).Children())
							{
								DeserializeJson(arr, name, ref errorList);
							}
							break;
						}
					case JTokenType.Property:
						{
							var pContent = (JProperty)content;
							DeserializeJson(pContent?.Value, pContent?.Name ?? name, ref errorList);
							break;
						}
					default:
						{
							var value = ((JValue)content)?.ToString();
							if (!string.IsNullOrWhiteSpace(value))
							{
								errorList.Add(string.IsNullOrWhiteSpace(name) ? value : $"{name} : {value}");
							}
							break;
						}
				}
			}

		}
	}
}
