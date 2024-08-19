using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOJ.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "https://orgd53f2c17.crm.dynamics.com/";
            var user = "admin@M365x20845073.onmicrosoft.com";
            var pwd = "2e~S=^~L+I2Ij5b9f8";

            using (var svc = new CrmServiceClient(
                $@"AuthenticationType=Office365; url={url}; UserName={user}; Password={pwd};"))
            {
            }
        }
    }
}
