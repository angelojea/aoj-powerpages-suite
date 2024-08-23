using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xrm.Client
{
    public class AojConfigurationManager
    {
        public static int LanguageCode = 1033;

        public static IOrganizationService Service { get; set; }

        public static Entity Website { get; set; }

        public static Entity User { get; set; }
    }
}
