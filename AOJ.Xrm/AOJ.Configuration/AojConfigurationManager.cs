using Microsoft.Xrm.Sdk;
using System.Web.Mvc;

namespace AOJ.Configuration
{
    public class AojConfigurationManager
    {
        public static int LanguageCode = 1033;

        public static IOrganizationService Service { get; set; }
        public static HtmlHelper ViewContext { get; set; }

        public static Entity Website { get; set; }

        public static Entity User { get; set; }
    }
}
