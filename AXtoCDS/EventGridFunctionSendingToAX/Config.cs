using System;
using System.Collections.Generic;
using System.Text;

namespace EventGridFunctionSendingToAX
{
    public partial class Config
    {
        public static Config Default { get { return Config.OneBox; } }

        public static Config OneBox = new Config()
        {
            UriString = "https://serbusdemo-devdab83280c3478b63devaos.cloudax.dynamics.com/",
            UserName = "",
            // Insert the correct password here for the actual test.
            Password = "",

            ActiveDirectoryResource = "https://serbusdemo-devdab83280c3478b63devaos.cloudax.dynamics.com",
            ActiveDirectoryTenant = "https://login.windows.net/microsoft.com",
            ActiveDirectoryClientAppId = "0ebd8b83-a5bb-4ef2-9f4d-bb759253b191",
            // Insert here the application secret when authenticate with AAD by the application
            ActiveDirectoryClientAppSecret = "RevrL2vQpri2+7HgiPUs5lGZ0wdLyd+AX0QdQYC3jmw=",

            // Change TLS version of HTTP request from the client here
            // Ex: TLSVersion = "1.2"
            // Leave it empty if want to use the default version
            TLSVersion = "",
        };

        public string TLSVersion { get; set; }
        public string UriString { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ActiveDirectoryResource { get; set; }
        public String ActiveDirectoryTenant { get; set; }
        public String ActiveDirectoryClientAppId { get; set; }
        public string ActiveDirectoryClientAppSecret { get; set; }
    }
}
