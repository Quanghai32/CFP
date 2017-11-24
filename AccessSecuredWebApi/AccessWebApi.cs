using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;

namespace AccessSecuredWebApi
{
    public class AccessWebApi
    {
        public string apiBaseUri { get; set; } // The address of website: http://localhost:58310
        public string UserName { get; set; } //Default is "CFP"
        public string PassWord { get; set; } //Default is "Cfp0123!"
        public string AccessToken { get; set; }

        public void SetAccountInfo(string userName, string password)
        {
            this.UserName = userName;
            this.PassWord = password;
        }

        public object GetApiCall(string requestPath, List<object> lstListParams = null)
        {
            var listParams = lstListParams.Select(l => l.ToString()).ToList();
            var test = GetRequestWithQueryParams(this.AccessToken, apiBaseUri, requestPath, listParams);
            return test;
        }

        public object GetJsonObjectValueFromList(string json, int Id, string property)
        {
            //Test Deserialize json => To List of JObject
            List<JObject> result = JsonConvert.DeserializeObject<List<JObject>>(json);

            //Get Desired JObject
            JObject test = result[Id];
            var test1 = test.GetValue(property);
            //Return
            return test1;
        }

        public object GetJsonObjectValue(string json, string property)
        {
            //Test Deserialize json => To List of JObject
            JObject result = JsonConvert.DeserializeObject<JObject>(json);

            //Get Desired JObject
            var test = result.GetValue(property);
            //Return
            return test;
        }

        public bool GetAccessToken()
        {
            //Test connection is OK or not
            var isConnect = this.TestServerConnection(2000);

            if (isConnect == true) //Connection OK
            {
                //var accessToken = await GetAPIToken("Admin", "Admin0123!", "http://localhost:58310");
                //var test = await GetRequest(accessToken, "http://localhost:58310", "/api/TestCruds");
                var accessToken = GetAPIToken();
                this.AccessToken = accessToken;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TestServerConnection(int TimeOutMs = 3000)
        {
            bool result = false;

            int Tick1 = MyLibrary.clsApiFunc.GetTickCount();

            while ((result == false) && (MyLibrary.clsApiFunc.GetTickCount() - Tick1 < TimeOutMs))
            {
                try
                {
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(apiBaseUri);
                    myRequest.Timeout = TimeOutMs;
                    myRequest.Method = "HEAD"; //Do not download all website contents

                    using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                    {
                        result = response.StatusCode == HttpStatusCode.OK;
                    }
                }
                catch (Exception ex) //Default is about 2 second only?
                {
                    string errMsg = ex.Message;
                    result = false;
                }
            }

            //
            return result;
        }

        public async Task<bool> TestServerConnectionAsync(int TimeOutMs)
        {
            bool result = false;

            int Tick1 = MyLibrary.clsApiFunc.GetTickCount();

            while ((result == false) && (MyLibrary.clsApiFunc.GetTickCount() - Tick1 < TimeOutMs))
            {
                try
                {
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(apiBaseUri);
                    myRequest.Timeout = TimeOutMs;
                    myRequest.Method = "HEAD"; //Do not download all website contents

                    //using (HttpWebResponse response = await (HttpWebResponse)myRequest.GetResponseAsync())
                    //{
                    //    result = response.StatusCode == HttpStatusCode.OK;
                    //}

                    var res = await myRequest.GetResponseAsync();
                    HttpWebResponse response = (HttpWebResponse)res;
                    result = response.StatusCode == HttpStatusCode.OK;
                }
                catch (Exception ex) //Default is about 2 second only?
                {
                    string errMsg = ex.Message;
                    result = false;
                }
            }

            //
            return result;
        }

        private string GetAPIToken()
        {
            using (var client = new HttpClient())
            {
                //setup client
                client.BaseAddress = new Uri(apiBaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("grant_type", "password"),
                     new KeyValuePair<string, string>("username", UserName),
                     new KeyValuePair<string, string>("password", PassWord),
                     new KeyValuePair<string, string>("scope", "openid email profile roles"),
                 });

                //send request
                HttpResponseMessage responseMessage = client.PostAsync("/connect/token", formContent).Result;

                //get access token from response body
                var responseJson = responseMessage.Content.ReadAsStringAsync().Result;
                var jObject = JObject.Parse(responseJson);
                var test = jObject.GetValue("access_token").ToString();
                return test;
            }
        }

        private string GetRequest(string token, string apiBaseUri, string requestPath)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //UriBuilder uriBuilder = new UriBuilder(apiBaseUri);
                    //uriBuilder.Query = "name=abc&pass=123";
                    //var test = uriBuilder.Uri;


                    //setup client
                    client.BaseAddress = new Uri(apiBaseUri);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                    //make request
                    HttpResponseMessage response = client.GetAsync(requestPath).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    return responseString;
                }
            }
            catch(Exception ex)
            {
                return null;
            }
        }


        private string GetRequestWithQueryParams(string token, string apiBaseUri, string requestPath, List<string> queryParams)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //Cal Query string
                    UriBuilder uriBuilder = new UriBuilder(apiBaseUri);
                    //uriBuilder.Query = "name=abc&pass=123";
                    //var test = uriBuilder.Uri;
                    string paramString = "";

                    for(int i=0;i<queryParams.Count;i++)
                    {
                        //paramString = paramString + queryParams[i].key + "=" + queryParams[i].value;
                        //if (i != queryParams.Count - 1) paramString = paramString + "&";
                        if(i%2==0) //Key
                        {
                            paramString = paramString + queryParams[i] + "=";
                        }
                        else //value
                        {
                            paramString = paramString + queryParams[i];
                            if (i != queryParams.Count - 1) paramString = paramString + "&";
                        }
                    }
                    uriBuilder.Query = paramString;


                    //setup client
                    client.BaseAddress = new Uri(apiBaseUri);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                    //make request
                    HttpResponseMessage response = client.GetAsync(requestPath + uriBuilder.Query).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;

                    //var responseString = client.GetStringAsync(uriBuilder.Uri).Result;


                    return responseString;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //Constructor
        //public AccessWebApi()
        //{
        //    this.UserName = "CFP";
        //    this.PassWord = "Cfp0123!";
        //}

        public AccessWebApi(string _apiBaseUri, string _UserName, string _PassWord)
        {
            this.apiBaseUri = _apiBaseUri;
            this.UserName = _UserName;
            this.PassWord = _PassWord;
        }
    }
}
