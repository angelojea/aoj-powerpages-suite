using Adxstudio.Xrm;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Web.Mvc.Html;
using Adxstudio.Xrm.Web.Mvc.Liquid;
using Adxstudio.Xrm.Web.UI.CrmEntityListView;
using Adxstudio.Xrm.Web.UI.WebControls;
using DotLiquid;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using static Adxstudio.Xrm.Web.Mvc.Html.LiquidExtensions;

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
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://test.com", ""), new HttpResponse(new StringWriter()));
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


                using (StringWriter stringWriter = new StringWriter())
                {
                    // Create an HtmlTextWriter to render the control
                    using (HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter))
                    {
                        view.RenderControl(htmlWriter);

                        // Output the rendered HTML
                        string renderedHtml = stringWriter.ToString();
                        Console.WriteLine(renderedHtml);
                    }
                }

                Console.WriteLine(LiquidRenderer.Liquid(@"{% assign name = ""Birl"" %} {{name}}", context));
            }
        }
    }
}