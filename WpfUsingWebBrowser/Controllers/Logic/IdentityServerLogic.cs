using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using WpfUsingWebBrowser.Model;

namespace WpfUsingWebBrowser.Controllers.Logic
{
    public class IdentityServerLogic
    {
        public static async Task CallApi(string accessToken)
        {
            var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var response = await client.GetAsync(MainWindowModel.ApiServerUrl);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(response.StatusCode.ToString());
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                MessageBox.Show(JArray.Parse(content).ToString());
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