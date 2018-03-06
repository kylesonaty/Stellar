using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Stellar
{
    internal static class HttpRequestHelper
    {
        private static HttpClient client = new HttpClient();

        internal async static Task<HttpResponseMessage> ExecuteResourceRequest(string verb, string url, string key, string queryPath, string resourceType, string resourceValue, string body = "", bool isQuery = false, bool upsert = false)
        {
            try
            {
                var uri = new Uri(new Uri(url), queryPath);
                var utcDate = DateTime.UtcNow.ToString("r");
                var authHeader = CreateAuthorizationSignature(utcDate, verb, resourceType, resourceValue, key, "master", "1.0");
                using (var requestMessage = new HttpRequestMessage())
                {
                    requestMessage.Headers.Add("authorization", authHeader);
                    requestMessage.Headers.Add("x-ms-date", utcDate);
                    requestMessage.Headers.Add("x-ms-version", "2015-12-16");
                    requestMessage.Headers.Add("Accept", "application/json");

                    if (upsert)
                        requestMessage.Headers.Add("x-ms-documentdb-is-upsert", "True");

                    if (isQuery)
                        requestMessage.Headers.Add("x-ms-documentdb-isquery", "true");
                    
                    requestMessage.RequestUri = uri;
                    switch (verb.ToLower())
                    {
                        case "delete":
                            requestMessage.Method = HttpMethod.Delete;
                            break;
                        case "get":
                            requestMessage.Method = HttpMethod.Get;
                            break;
                        case "put":
                            requestMessage.Method = HttpMethod.Put;
                            StringContent stringContent = new StringContent(body);
                            requestMessage.Content = stringContent;
                            break;
                        case "post":
                            requestMessage.Method = HttpMethod.Post;
                            StringContent cont;
                            if (!isQuery)
                            {
                                cont = new StringContent(body);
                            }
                            else
                            {
                                cont = new StringContent(body, Encoding.ASCII, "application/query+json");
                                cont.Headers.ContentType.CharSet = "";
                            }
                            requestMessage.Content = cont;
                            break;
                        default:
                            throw new ArgumentException("Unknown VERB: " + verb + ", recognized verbs are: get, post, delete and put");
                    }
                    HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                    return responseMessage;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }    
        }
        
        internal static string CreateAuthorizationSignature(string utcDate, string verb, string resourceType, string resourceValue, string key, string keyType, string tokenVersion)
        {
            try
            {
                var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(key) };

                verb = verb ?? "";
                resourceType = resourceType ?? "";
                resourceValue = resourceValue ?? "";

                string payLoad = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n{4}\n",
                    verb.ToLowerInvariant(),
                    resourceType.ToLowerInvariant(),
                    resourceValue,
                    utcDate.ToLowerInvariant(),
                    ""
                );
                
                var hashPayLoad = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
                string signature = Convert.ToBase64String(hashPayLoad);
                return Uri.EscapeDataString(String.Format(System.Globalization.CultureInfo.InvariantCulture, "type=" + keyType + "&ver=" + tokenVersion + "&sig=" + signature));
            }
            catch (Exception ex)
            { 
                Trace.TraceError(ex.ToString());
                throw;
            }
        }
    }
}