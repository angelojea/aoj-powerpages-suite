using Adxstudio.Xrm;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.Mvc.Liquid;
using Adxstudio.Xrm.Web.Mvc.Liquid.Tags;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebControls;
using DotLiquid;
using Microsoft.ClearScript.V8;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.Caching;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using static Adxstudio.Xrm.Data.PaginatedList;
using static Adxstudio.Xrm.Web.Mvc.Html.LiquidExtensions;
using Context = DotLiquid.Context;
using Hash = DotLiquid.Hash;
using Page = System.Web.UI.Page;

namespace AOJ.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "https://orgd53f2c17.crm.dynamics.com/";
            var clientId = "af246865-349f-45d9-bedf-018f05fd38a4";
            var clientSecret = "Qka8Q~ZjcDiKiLiKTsVtYaGEXPRCgFq.d3hzDbio";

            using (var client = new CrmServiceClient(
                $@"AuthenticationType=ClientSecret; url={url}; ClientId={clientId}; ClientSecret={clientSecret};"))
            {
                MockHttpContext();
                AojConfigurationManager.Service = client;
                AojConfigurationManager.Website = client.Retrieve("mspp_website", Guid.Parse("ce496a9b-0b4b-4d8f-8f0e-7e08c21c5715"), new ColumnSet(true));
                AojConfigurationManager.User = client.Retrieve("contact", Guid.Parse("2ba29921-1b5c-ef11-bfe2-000d3a56777a"), new ColumnSet(true));
                var name = "Xrm";
                var portal = PortalCrmConfigurationManager.CreatePortalContext(name);
                var portalLiquidContext = new PortalLiquidContext(client);

                AojConfigurationManager.ViewContext = Adxstudio.Xrm.AOJ.GetMockHtmlHelper(portal);

                //var blogDependencies = new Adxstudio.Xrm.Blogs.PortalConfigurationDataAdapterDependencies(svc, portalContext);
                //var knowledgeDependencies = new Adxstudio.Xrm.KnowledgeArticles.PortalConfigurationDataAdapterDependencies();
                //var contextDrop = new PortalViewContextDrop(portalLiquidContext);
                //var requestDrop = RequestDrop.FromHtmlHelper(portalLiquidContext, null);

                var globals = new Hash
            {
                //{ "context", contextDrop },
                { "entities", new EntitiesDrop(portalLiquidContext) },
                { "now", DateTime.UtcNow },
				//{ "params", requestDrop == null ? null : requestDrop.Params },
				//{ "request", requestDrop },
				//{ "settings", new SettingsDrop(portalViewContext.Settings) },
				{ "sharepoint", new SharePointDrop(portalLiquidContext) },
				//{ "sitemap", siteMapDrop },
				//{ "sitemarkers", new SiteMarkersDrop(portalLiquidContext, portalViewContext.SiteMarkers) },
				//{ "snippets", new SnippetsDrop(portalLiquidContext, portalViewContext.Snippets) },
				//{ "user", contextDrop.User },
				//{ "weblinks", new WebLinkSetsDrop(portalLiquidContext, portalViewContext.WebLinks) },
				//{ "ads", new AdsDrop(portalLiquidContext, portalViewContext.Ads) },
				//{ "polls", new PollsDrop(portalLiquidContext, portalViewContext.Polls) },
				//{ "forums", new ForumsDrop(portalLiquidContext, forumDependencies) },
				//{ "events", new EventsDrop(portalLiquidContext, forumDependencies) },
				//{ "blogs", new BlogsDrop(portalLiquidContext, blogDependencies) },
				//{ "website", contextDrop.Website },
				{ "resx", new ResourceManagerDrop(portalLiquidContext) },
				//{ "knowledge", new KnowledgeDrop(portalLiquidContext, knowledgeDependencies) },
				{ "uniqueId", new UniqueDrop() }
            };

                var environment = new LiquidEnvironment(globals, new Hash
                {
                    { "portalLiquidContext", portalLiquidContext }
                });
                var localVariables = Hash.FromDictionary(environment.Globals);
                var registers = Hash.FromDictionary(environment.Registers);
                var context = new Context(new List<Hash> { localVariables }, new Hash(), registers, false);

                var control = new LiquidServerControl();

                var form = control.InitEntityForm(Guid.Parse("e0ce2b35-875e-ef11-bfe3-000d3a56777a"));
                var view = form.AOJCreateChildControls();

                var page = new MockPage(view);

                using (StringWriter stringWriter = new StringWriter())
                {
                    // Create an HtmlTextWriter to render the control
                    using (HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter))
                    {
                        //page.ProcessRequest(HttpContext.Current);
                        page.RenderControl(htmlWriter);

                        // Output the rendered HTML
                        string renderedHtml = stringWriter.ToString();
                        Console.WriteLine(renderedHtml);
                    }
                }

                Console.WriteLine(LiquidRenderer.Liquid(@"{% assign name = ""Birl"" %} {{name}}", context));
            }
        }

        public static void MockHttpContext()
        {
            var httpRequest = new HttpRequest("", "http://tempuri.org", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            HttpContext.Current = httpContext;

            // Optionally, set up more properties like QueryString, Form, etc.
            //HttpContext.Current.Session.Add()
            //HttpContext.Current.Session["MySessionItem"] = "SessionValue";

            // Add cookies if needed
            var sessionContainer = new HttpSessionStateContainer("ASP.NET_SessionId", new SessionStateItemCollection(),
            new HttpStaticObjectsCollection(), 10, true, HttpCookieMode.UseUri, SessionStateMode.InProc, false);

            HttpContext.Current.Request.Browser = new MockHttpBrowserCapabilities();
            HttpContext.Current.Request.Cookies.Add(new HttpCookie("ASP.ARRAffinity", "any"));
            HttpContext.Current.Request.Cookies.Add(new HttpCookie("ASP.ARRAffinitySameSite", "any"));
            HttpContext.Current.Request.Cookies.Add(new HttpCookie("ASP.NET_SessionId", sessionContainer.SessionID));


            // Mock User if needed
            HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
                new System.Security.Principal.GenericIdentity("12bb9eca-270c-4d37-be42-35bc0f325c11"),
                new[] { "admin", "customizer" });
        }

        internal class MockHttpBrowserCapabilities : HttpBrowserCapabilities
        {
            public MockHttpBrowserCapabilities()
            {
                Capabilities = new Dictionary<string, string>();
                Capabilities["Browser"] = "Chrome";
                Capabilities["Version"] = "99.0";
                Capabilities["Platform"] = "WinNT";
                Capabilities["IsMobileDevice"] = "false";
                Capabilities["MajorVersion"] = "99";
                Capabilities["MinorVersion"] = "0";
                Capabilities["Frames"] = "true";
                Capabilities["Tables"] = "true";
                Capabilities["Cookies"] = "true";
                Capabilities["JavaScript"] = "true";
                Capabilities["VBScript"] = "false";
                Capabilities["EcmaScriptVersion"] = "3.0";
                Capabilities["SupportsCss"] = "true";
                Capabilities["PreferredRenderingMime"] = "text/html";
                Capabilities["PreferredRenderingType"] = "html32";
                Capabilities["PreferredRequestEncoding"] = "utf-8";
                Capabilities["PreferredResponseEncoding"] = "utf-8";
                Capabilities["SupportsXmlHttp"] = "true";
                Capabilities["SupportsCallback"] = "true";
                Capabilities["SupportsRedirectWithCookie"] = "true";
                Capabilities["SupportsEmptyStringInCookieValue"] = "true";
                Capabilities["beta"] = "true";
                Capabilities["crawler"] = "true";
                Capabilities["aol"] = "true";
                Capabilities["win16"] = "true";
                Capabilities["win32"] = "true";
                Capabilities["frames"] = "true";
                Capabilities["tables"] = "true";
                Capabilities["cookies"] = "true";
                Capabilities["vbscript"] = "true";
                Capabilities["javascript"] = "true";
                Capabilities["javaapplets"] = "true";
                Capabilities["activexcontrols"] = "true";
                Capabilities["backgroundsounds"] = "true";
                Capabilities["cdf"] = "true";
            }
        }

        public class MockPage : Page
        {
            internal void SetPageOfChildControls(System.Web.UI.Control control)
            {
                control.Page = this;
                control.ID = Guid.NewGuid().ToString();
                if (control.Controls.Count > 0)
                {
                    foreach (System.Web.UI.Control child in control.Controls)
                    {
                        SetPageOfChildControls(child);
                    }
                }
            }

            public MockPage(CompositeControl view)
            {
                SetPageOfChildControls(view);
                Controls.Add(view);
            }

            public override void VerifyRenderingInServerForm(System.Web.UI.Control control)
            {
                return;
            }

            public override bool EnableEventValidation {
                get => false;
                set { }
            }
        }
    }
}