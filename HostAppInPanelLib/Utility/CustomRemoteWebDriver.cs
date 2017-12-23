using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;

namespace HostAppInPanelLib.Utility
{
    public class CustomRemoteWebDriver : RemoteWebDriver
    {
        public static bool NewSession = false;
        public static string CapPath = @"c:\automation\sessionCap";
        public static string SessiondIdPath = @"c:\automation\sessionid";

        public CustomRemoteWebDriver(Uri remoteAddress, DesiredCapabilities desiredCapabilities)
            : base(remoteAddress, desiredCapabilities)
        {
        }

        /// <summary>
        /// Executes a command with this driver .
        /// Store for the name property.
        /// A  value representing the command to execute.
        /// A  containing the names and values of the parameters of the command.
        /// A  containing information about the success or failure of the command and any data returned by the command.
        /// </summary>
        /// <param name="driverCommandToExecute">A <see cref="T:OpenQA.Selenium.Remote.DriverCommand" /> value representing the command to execute.</param>
        /// <param name="parameters">A <see cref="T:System.Collections.Generic.Dictionary`2" /> containing the names and values of the parameters of the command.</param>
        /// <returns>
        /// A <see cref="T:OpenQA.Selenium.Remote.Response" /> containing information about the success or failure of the command and any data returned by the command.
        /// </returns>
        protected override Response Execute(string driverCommandToExecute, Dictionary<string, object> parameters)
        {
            if (driverCommandToExecute == DriverCommand.NewSession)
            {
                if (!NewSession)
                {

                    var sidText = File.ReadAllText(SessiondIdPath);


                    return new Response
                    {
                        SessionId = sidText,

                    };
                }
                else
                {
                    var response = base.Execute(driverCommandToExecute, parameters);

                    File.WriteAllText(SessiondIdPath, response.SessionId);
                    return response;
                }
            }
            else
            {
                var response = base.Execute(driverCommandToExecute, parameters);
                return response;
            }
        }
    }
}
