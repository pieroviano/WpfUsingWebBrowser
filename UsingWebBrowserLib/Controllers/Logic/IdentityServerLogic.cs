using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UsingWebBrowserLib.Model;

namespace UsingWebBrowserLib.Controllers.Logic
{
    public class IdentityServerLogic
    {
        public static async Task CallApi(string accessToken, Delegate messageBoxShow)
        {
            var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var response = await client.GetAsync(MainWindowModel.Instance.ApiServerUrl);
            if (!response.IsSuccessStatusCode)
            {
                messageBoxShow.DynamicInvoke(response.StatusCode.ToString());
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                messageBoxShow.DynamicInvoke(JArray.Parse(content).ToString());
            }
        }

        public static Dictionary<string, string> GetAuthorization()
        {
            var newSlotData =
                (Dictionary<string, string>) Thread.GetData(Thread.GetNamedDataSlot("Authorization"));
            return newSlotData;
        }

        public static void SetAuthorization(object data)
        {
            Thread.SetData(
                Thread.GetNamedDataSlot("Authorization"),
                data);
        }
    }
}