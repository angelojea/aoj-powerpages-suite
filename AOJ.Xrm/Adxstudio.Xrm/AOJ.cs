using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Portal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Adxstudio.Xrm.Web.Mvc.Html.EntityExtensions;
using Adxstudio.Xrm.Cms;
using System.Web.Routing;

namespace Adxstudio.Xrm
{
    public static class AOJ
    {
        public static HtmlHelper GetMockHtmlHelper(IPortalContext portal)
        {
            var controllerContext = new ControllerContext(new RequestContext(), new MockController());
            var portalViewContext = new PortalViewContext(new PortalContextDataAdapterDependencies(portal));

            var htmlHelper = new HtmlHelper(new ViewContext(controllerContext, new MockView(), new ViewDataDictionary(), new TempDataDictionary(), new StringWriter())
            {
                ViewData = new ViewDataDictionary
                {
                    { PortalExtensions.PortalViewContextKey, portalViewContext }
                }
            }, new ViewPage());

            htmlHelper.ViewData[PortalExtensions.PortalViewContextKey] = portalViewContext;

            return htmlHelper;
        }
    }
}
