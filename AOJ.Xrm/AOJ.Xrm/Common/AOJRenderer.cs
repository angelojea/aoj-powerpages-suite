using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.SessionState;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Web.Mvc.Liquid;
using static Adxstudio.Xrm.Web.Mvc.Html.LiquidExtensions;
using Hash = DotLiquid.Hash;
using Context = DotLiquid.Context;
using Adxstudio.Xrm.Web.UI.WebControls;
using Adxstudio.Xrm;
using System.Linq;
using Adxstudio.Xrm.Web.Mvc;
using Adxstudio.Xrm.Cms;
using AOJ.Configuration;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using System.Collections.Specialized;

namespace AOJ.Xrm.Common
{
    public class AOJRenderer
    {
        private readonly IOrganizationService _client;
        private readonly Guid _websiteId;
        private readonly Guid _userId;

        public AOJRenderer(IOrganizationService client, Guid websiteId, Guid userId)
        {
            _client = client;
            _websiteId = websiteId;
            _userId = userId;
            MockHttpContext();
        }

        public string RenderEntityList(Guid id)
        {
            AojConfigurationManager.Service = _client;
            AojConfigurationManager.Website = _client.Retrieve("mspp_website", _websiteId, new ColumnSet(true));
            AojConfigurationManager.User = _client.Retrieve("contact", _userId, new ColumnSet(true));

            var page = new MockPage();

            var list = new EntityList(_client.Retrieve("mspp_entitylist", id, new ColumnSet(true)).ToEntityReference());
            var listView = list.AOJCreateChildControls(page);
            page.AddEntityList(listView);

            using (StringWriter stringWriter = new StringWriter())
            {
                // Create an HtmlTextWriter to render the control
                using (HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter))
                {
                    page.RenderControl(htmlWriter);

                    // Output the rendered HTML
                    string renderedHtml = stringWriter.ToString();
                    return renderedHtml;
                }
            }
        }

        public string RenderEntityForm(Guid id)
        {
            AojConfigurationManager.Service = _client;
            AojConfigurationManager.Website = _client.Retrieve("mspp_website", _websiteId, new ColumnSet(true));
            AojConfigurationManager.User = _client.Retrieve("contact", _userId, new ColumnSet(true));

            var control = new LiquidServerControl();
            var page = new MockPage();

            var form = control.InitEntityForm(id);
            var formView = form.AOJCreateChildControls(page);
            page.AddForm(formView);

            using (StringWriter stringWriter = new StringWriter())
            {
                // Create an HtmlTextWriter to render the control
                using (HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter))
                {
                    page.RenderControl(htmlWriter);

                    // Output the rendered HTML
                    string renderedHtml = stringWriter.ToString();
                    return renderedHtml;
                }
            }
        }

        public string RenderLiquid(string liquid, Entity contextEntity = null)
        {
            AojConfigurationManager.Service = _client;
            AojConfigurationManager.Website = _client.Retrieve("mspp_website", _websiteId, new ColumnSet(true));
            AojConfigurationManager.User = _client.Retrieve("contact", _userId, new ColumnSet(true));

            var context = new OrganizationServiceContext(_client);
            var portalContext = new PortalContext(context,
                AojConfigurationManager.Website,
                AojConfigurationManager.User,
                //Context Entity
                contextEntity != null ? contextEntity : AojConfigurationManager.User
             );
            AojConfigurationManager.ViewContext = Adxstudio.Xrm.AOJ.GetMockHtmlHelper(portalContext);

            var portalViewContext = new PortalViewContext(new PortalContextDataAdapterDependencies(portalContext));

            var portalLiquidContext = new PortalLiquidContext(_client, portalViewContext);


            var blogDependencies = new Adxstudio.Xrm.Blogs.PortalConfigurationDataAdapterDependencies(portalContext);
            var knowledgeDependencies = new Adxstudio.Xrm.KnowledgeArticles.PortalConfigurationDataAdapterDependencies();
            var contextDrop = new PortalViewContextDrop(portalLiquidContext);

            //TODO: implement
            var requestDrop = new RequestDrop(
                portalLiquidContext,
                new MockHttpRequest()
            );

            var globals = new Hash
            {
                { "context", contextDrop },
                { "entities", new EntitiesDrop(portalLiquidContext) },
                { "now", DateTime.UtcNow },
                { "params",  requestDrop == null ? null : requestDrop.Params },
                { "request", requestDrop },
                { "settings", new SettingsDrop(portalViewContext.Settings) },
                { "sharepoint", new SharePointDrop(portalLiquidContext) },
				//{ "sitemap", siteMapDrop },
				//{ "sitemarkers", new SiteMarkersDrop(portalLiquidContext, portalViewContext.SiteMarkers) },
				{ "snippets", new SnippetsDrop(portalLiquidContext, portalViewContext.Snippets) },
                { "user", contextDrop.User },
                { "weblinks", new WebLinkSetsDrop(portalLiquidContext, portalViewContext.WebLinks) },
				//{ "ads", new AdsDrop(portalLiquidContext, portalViewContext.Ads) },
				//{ "polls", new PollsDrop(portalLiquidContext, portalViewContext.Polls) },
				//{ "forums", new ForumsDrop(portalLiquidContext, forumDependencies) },
				//{ "events", new EventsDrop(portalLiquidContext, forumDependencies) },
				{ "blogs", new BlogsDrop(portalLiquidContext, blogDependencies) },
                { "website", contextDrop.Website },
                { "resx", new ResourceManagerDrop(portalLiquidContext) },
                { "knowledge", new KnowledgeDrop(portalLiquidContext, knowledgeDependencies) },
                { "uniqueId", new UniqueDrop() }
            };

            var environment = new LiquidEnvironment(globals, new Hash
                {
                    { "portalLiquidContext", portalLiquidContext }
                });
            var localVariables = Hash.FromDictionary(environment.Globals);
            var registers = Hash.FromDictionary(environment.Registers);
            var dotContext = new Context(new List<Hash> { localVariables }, new Hash(), registers, false);
            return LiquidRenderer.Liquid(liquid, dotContext);
        }

        public void MockHttpContext()
        {
            var httpRequest = new HttpRequest("", "http://tempuri.org", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            HttpContext.Current = httpContext;

            // Add cookies if needed
            var sessionContainer = new HttpSessionStateContainer("ASP.NET_SessionId", new SessionStateItemCollection(),
            new HttpStaticObjectsCollection(), 10, true, HttpCookieMode.UseUri, SessionStateMode.InProc, false);

            HttpContext.Current.Request.Browser = new MockHttpBrowserCapabilities();
            HttpContext.Current.Request.Cookies.Add(new HttpCookie("ASP.ARRAffinity", "any"));
            HttpContext.Current.Request.Cookies.Add(new HttpCookie("ASP.ARRAffinitySameSite", "any"));
            HttpContext.Current.Request.Cookies.Add(new HttpCookie("ASP.NET_SessionId", sessionContainer.SessionID));


            // Mock User if needed
            HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
                new System.Security.Principal.GenericIdentity("Usuario"),
                new[] { "admin", "customizer" });
        }

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

    internal class MockPage : Page
    {
        private HttpContext _context;

        internal void HandleChildControls(Control control)
        {
            control.Page = this;
            control.ID = Guid.NewGuid().ToString();
            if (control.Controls.Count > 0)
            {
                foreach (var child in control.Controls.Cast<Control>().ToList())
                {
                    HandleChildControls(child);
                }
            }
        }

        public void AddEntityList(CompositeControl list)
        {
            HandleChildControls(list);
            list.ID = "EntityListControl";
            Controls.Add(list);
            var rawHtml = new LiteralControl($"<script>{Helpers.EntityListScript()}</script>");
            Controls.Add(rawHtml);
        }

        public void AddForm(CompositeControl form)
        {
            var htmlForm = new HtmlForm();
            htmlForm.Attributes["runat"] = "server";
            htmlForm.Controls.Add(form);

            HandleChildControls(htmlForm);
            Controls.Add(htmlForm);
        }

        public override bool EnableEventValidation
        {
            get => false;
            set { }
        }
    }

    internal class MockHttpRequest : HttpRequestBase
    {
        private Uri _url;
        public MockHttpRequest() : base()
        {
            _url = new Uri("http://aoj-powerpages.com?test=Biiiiirl");
        }
        public override Uri Url => _url;
        public override string RawUrl => _url.Host;
        public override NameValueCollection Params => new NameValueCollection() { { "test", "Biiirl" } };
    }
}
