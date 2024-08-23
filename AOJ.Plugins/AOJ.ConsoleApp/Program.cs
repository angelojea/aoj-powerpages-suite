using Adxstudio.Xrm;
using Adxstudio.Xrm.Services;
using Adxstudio.Xrm.Web.Mvc.Liquid;
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
using System.Web.UI;
using System.Xml;
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

            // Sample XML Configuration as a string
            //            string xmlConfig = @"
            //<configuration>

            // <configSections>
            //  <section name=""microsoft.xrm.client"" type=""Microsoft.Xrm.Client.Configuration.CrmSection, Microsoft.Xrm.Client""/>
            //  <section name=""microsoft.xrm.portal"" type=""Microsoft.Xrm.Portal.Configuration.PortalCrmSection, Microsoft.Xrm.Portal""/>
            // </configSections>

            // <connectionStrings>
            //  <add name=""Xrm"" connectionString=""ServiceUri=...; Domain=...; Username=...; Password=...""/>
            // </connectionStrings>

            // <microsoft.xrm.client>
            //  <contexts>
            //   <add name=""Xrm"" type=""Xrm.XrmServiceContext, Xrm""/>
            //  </contexts>
            // </microsoft.xrm.client>

            // <microsoft.xrm.portal>
            //  <portals>
            //   <add name=""Xrm""/>
            //  </portals>
            // </microsoft.xrm.portal>

            // <location path=""Services/Cms.svc"">
            //  <system.web>
            //   <authorization>
            //    <allow roles=""My Portal Administrators""/>
            //    <deny users=""*""/>
            //   </authorization>
            //  </system.web>
            // </location>

            //</configuration>";
            //            using (var stringReader = new StringReader(xmlConfig))
            //            using (var xmlReader = XmlReader.Create(stringReader))
            //            {
            //                var configMap = new ExeConfigurationFileMap(); // Not mapped to any file
            //                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            //                var appSettingsSection = (AppSettingsSection)config.GetSection("appSettings");

            //                if (appSettingsSection == null)
            //                {
            //                    appSettingsSection = new AppSettingsSection();
            //                    config.Sections.Add("appSettings", appSettingsSection);
            //                }
            //                while (xmlReader.Read())
            //                {
            //                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "add")
            //                    {
            //                        string key = xmlReader.GetAttribute("key");
            //                        string value = xmlReader.GetAttribute("value");

            //                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            //                        {
            //                            appSettingsSection.Settings.Add(key, value);
            //                        }
            //                    }
            //                }
            //            }
            
            using (var client = new CrmServiceClient(
                $@"AuthenticationType=ClientSecret; url={url}; ClientId={clientId}; ClientSecret={clientSecret};"))
            {
                AojConfigurationManager.Service = client;
                var name = "Xrm";
                var portal = PortalCrmConfigurationManager.CreatePortalContext(name);

                var svc = new OrganizationServiceContext(client);
                var portalLiquidContext = new PortalLiquidContext(client);

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

                //foreach (var factory in _globalVariableFactories)
                //{
                //	globals[factory.Key] = factory.Value(html);
                //}

                var environment = new LiquidEnvironment(globals, new Hash
                {
                    //{ "file_system", new CompositeFileSystem(
                    //    new EmbeddedResourceFileSystem(typeof(LiquidExtensions).Assembly, "Adxstudio.Xrm.Liquid")) },
                    { "portalLiquidContext", portalLiquidContext }
                });
                var localVariables = Hash.FromDictionary(environment.Globals);
                var registers = Hash.FromDictionary(environment.Registers);
                var context = new Context(new List<Hash> { localVariables }, new Hash(), registers, false);

                var entityForm = client.Retrieve("mspp_entityform", Guid.Parse("e0ce2b35-875e-ef11-bfe3-000d3a56777a"), new ColumnSet(true));
                var control = new LiquidServerControl();

                var form = control.InitEntityForm(entityForm.Id);
                var sourceDef = form.GetEntitySourceDefinition(svc, entityForm);
                var view = form.RenderForm(svc, entityForm, System.Web.UI.WebControls.FormViewMode.Insert, sourceDef);

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