/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using System.Web;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Crm.Sdk.Messages;
	using Adxstudio.Xrm.Cms;
	using Configuration;
	using Web;
    using global::AOJ.Configuration;

    /// <summary>
    /// Class holding context language information for a portal.
    /// </summary>
    public class ContextLanguageInfo : IDisposable
	{
		/// <summary>
		/// Name of the cookie used to store portal language info.
		/// </summary>
		internal static readonly string ContextLanguageCookieName = "ContextLanguageCode";

		/// <summary>
		/// Name of the site setting of whether language code should be displayed in URL.
		/// </summary>
		private static readonly string SiteSettingNameDisplayLanguageCodeInUrl = "MultiLanguage/DisplayLanguageCodeInURL";

		/// <summary>
		/// Read-write lock for the activeWebsiteLanguages singleton.
		/// </summary>
		private static readonly ReaderWriterLockSlim ActiveWebsiteLanguagesLock = new ReaderWriterLockSlim();

		/// <summary>
		/// Read timeout for the ActiveWebsiteLanguagesLock.
		/// </summary>
		private static readonly TimeSpan ActiveWebsiteLanguagesLockReadTimeout = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Write timeout for the ActiveWebsiteLanguagesLock.
		/// </summary>
		private static readonly TimeSpan ActiveWebsiteLanguagesLockWriteTimeout = TimeSpan.FromSeconds(15);

		/// <summary>
		/// LCIDS that Portal currently does not support (RTL)
		/// </summary>
		private static readonly List<int> UnsupportedLcids = new List<int> { 1025, 1037 };

		/// <summary>
		/// Singleton object to contain the website languages.
		/// All access to this object MUST be done through ActiveWebsiteLanguages getter or one of the InitializeActiveWebsiteLanguages methods
		/// to ensure adherence to the read-write lock.
		/// </summary>
		private static IEnumerable<IWebsiteLanguage> activeWebsiteLanguages;

		/// <summary>
		/// Gets the site setting for whether Language Code should be displayed in the url.
		/// </summary>
		public static bool DisplayLanguageCodeInUrl
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets all the active website languages (published and draft).
		/// Since the underlying activeWebsiteLanguages singleton is read-write lock protected, a clone (instead of original IEnumerable) is returned.
		/// Note this value will not be available (null or empty list is returned) before ContextLanguageInfo object has been constructed,
		/// for example in SiteMarkerRoute.GetRouteData().
		/// </summary>
		public IEnumerable<IWebsiteLanguage> ActiveWebsiteLanguages
		{
			get { return GetActiveWebsiteLanguages(); }
		}

		/// <summary>
		/// Gets the language that the portal should currently be displayed in. If using a legacy CRM organization that
		/// does not have multi-language (i.e. no WebsiteLanguage entities exist) then null will be returned.
		/// </summary>
		public IWebsiteLanguage ContextLanguage { get; private set; }

		/// <summary>
		/// Gets user preferred language (from contact's profile)
		/// </summary>
		public IWebsiteLanguage UserPreferredLanguage { get; private set; }

		/// <summary>
		/// Gets default web site language
		/// </summary>
		public IWebsiteLanguage DefaultLanguage { get; private set; }

		/// <summary>
		/// Gets whether the request URL had a valid language code in it.
		/// </summary>
		public bool RequestUrlHasLanguageCode { get; private set; }

		/// <summary>
		/// Gets the absolute path of the current context request with any language code prefix stripped out.
		/// ex: http://contoso.com/en-US/contact-us -> /contact-us
		/// </summary>
		public string AbsolutePathWithoutLanguageCode { get; private set; }

		/// <summary>
		/// Gets the query string of the current request URL.
		/// </summary>
		private Uri ContextUrl { get; set; }

		/// <summary>
		/// Gets whether the CRM instance represented by the ContentMap has been updated to support multi-language.
		/// </summary>
		public bool IsCrmMultiLanguageEnabled
		{
			get { return ContextLanguage != null && ContextLanguage.PortalLanguageId != Guid.Empty; }
		}

		/// <summary>
		/// Checks if a CRM website has multi-language feature available.
		/// This static method is intended to be used to check for multi-language before PortalContext is available.
		/// </summary>
		/// <param name="website">adx_website entity to check.</param>
		/// <returns>Whether multi-language is available in the CRM website.</returns>
		public static bool IsCrmMultiLanguageEnabledInWebsite(Entity website)
		{
			website.AssertEntityName("adx_website");
			return website.Attributes.ContainsKey("adx_defaultlanguage");
		}

		/// <summary>
		/// Sets the current thread's CurrentCulture and CurrentUICulture to the given LCID.
		/// </summary>
		/// <param name="lcid">LCID of the culture to set.</param>
		public static void SetCultureInfo(int lcid)
		{
			// Note: this will only set the culture for this request. 
			// System.Globalization.CultureInfo.DefaultThreadCurrentCulture will set the culture for the entire application which would incorrectly propogate to all other browser sessions as well.
			var culture = GetCulture(lcid);
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}

		/// <summary>
		/// Sets the current thread's CurrentCulture and CurrentUICulture to the given code.
		/// </summary>
		/// <param name="code">Code of the culture to set.</param>
		public static void SetCultureInfo(string code)
		{
			// Note: this will only set the culture for this request. 
			// System.Globalization.CultureInfo.DefaultThreadCurrentCulture will set the culture for the entire application which would incorrectly propogate to all other browser sessions as well.
			var culture = GetCulture(code);
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}

		/// <summary>
		/// Returns .NET culture by portal language
		/// </summary>
		/// <param name="lcid">Language LCID.</param>
		/// <returns>CultureInfo object.</returns>
		public static CultureInfo GetCulture(int lcid)
		{
			if (lcid == 0) // In some upper flows lcid was nullable and converted to 0 in case of nulls
			{
				return CultureInfo.CurrentCulture;
			}
			try
			{
				return new CultureInfo(lcid);
			}
			catch (CultureNotFoundException)
			{
				return CultureInfo.CurrentCulture;
			}
		}

		/// <summary>
		/// Returns .NET culture by portal language
		/// </summary>
		/// <param name="code">Language code.</param>
		/// <returns>CultureInfo object.</returns>
		public static CultureInfo GetCulture(string code)
		{
			if (string.IsNullOrWhiteSpace(code)) // In some upper flows lcid was nullable and converted to 0 in case of nulls
			{
				return CultureInfo.CurrentCulture;
			}
			try
			{
				return new CultureInfo(code);
			}
			catch (CultureNotFoundException)
			{
				return CultureInfo.CurrentCulture;
			}
		}

		/// <summary>
		/// Attempt to resolve what the context language should be from just an HttpContext. This method is intended to be called
		/// before PortalContext is available. Will check in order:
		/// 1. URL, 2. Cookie, 3. Website default language.
		/// Authenticated User won't be available until PortalContext happens.
		/// </summary>
		/// <param name="context">Http context.</param>
		/// <returns>Context language.</returns>
		public static IWebsiteLanguage ResolveContextLanguage(HttpContextBase context)
		{
			// Whenever this is called, we can only get the code from the url, the query parameter, or the cookie (in that order)
			IWebsiteLanguage contextLanguage = null;
			var activeLanguages = GetActiveWebsiteLanguages();
			var languagesList = activeLanguages == null ? Enumerable.Empty<IWebsiteLanguage>().ToList() : activeLanguages.ToList();

			// if a language doesn't have a Portal Language GUID then MLP is disabled
			if (languagesList.Count == 1 && languagesList[0].PortalLanguageId == Guid.Empty)
			{
				return languagesList[0];
			}

			bool pathHasLanguageCode;
			string absolutePathWithoutLanguageCode;
			ExtractLanguageCode(context.Request.Url.PathAndQuery, languagesList, out pathHasLanguageCode, out contextLanguage, out absolutePathWithoutLanguageCode);
			if (contextLanguage != null)
			{
				return contextLanguage;
			}

			string languageCode;
			if (CheckQueryParamForLanguage(context, out languageCode))
			{
				contextLanguage = FindWebsiteLanguage(languagesList, languageCode, false);
				if (contextLanguage != null)
				{
					return contextLanguage;
				}
			}

			string cookieLanguageCode;
			if (TryReadLanguageCodeFromCookie(context, out cookieLanguageCode))
			{
				contextLanguage = languagesList.FirstOrDefault(l => string.Equals(l.Code, cookieLanguageCode));
				if (contextLanguage != null)
				{
					return contextLanguage;
				}
			}

			return null;
		}

		/// <summary> Finds a CRM supported LCID from a given lcid. </summary>
		/// <param name="lcid"> The lcid. </param>
		/// <returns> The lcid </returns>
		public static int ResolveCultureLcid(int lcid)
		{
            if (AojConfigurationManager.LanguageCode > 0)
            {
				return AojConfigurationManager.LanguageCode;
            }

			IDictionary<EntityReference, EntityNode> portalLanguages;
			PortalLanguageNode matchLanguage = null;

			return matchLanguage?.CrmLanguage ?? lcid;
		}

		/// <summary> Resolves a non-CRM supported language code against a CRM supported code in a query parameter </summary>
		/// <param name="code"> The requested code </param>
		/// <param name="match"> The CRM language code </param>
		/// <returns> True if the requested code has a different matching CRM language code </returns>
		public static bool ResolveCultureCode(string code, out string match)
		{
			match = string.Empty;
			var websiteLanguages = GetActiveWebsiteLanguages().ToArray();
			var reqLanguage = FindWebsiteLanguage(websiteLanguages, code, false);
			if (reqLanguage == null && !TryGetLanguageFromMapping(websiteLanguages, code, out reqLanguage))
			{
				return false;
			}

			try
			{
				var tempCulture = new CultureInfo(reqLanguage.CrmLcid);
				if (string.Equals(code, tempCulture.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					return false;
				}

				match = tempCulture.Name;
				return true;
			}
			catch (CultureNotFoundException)
			{
				ADXTrace.TraceWarning(TraceCategory.Application,
					string.Format(
						"No language code could be determined from CRM LCID:{0}, Portal/User LCID:{1}, Code:{2}, PortalLanguage:{3}",
						reqLanguage.CrmLcid, reqLanguage.Lcid, reqLanguage.Code, reqLanguage.PortalLanguageId));

				return false;
			}
		}

		/// <summary>
		/// Retrieves a website language that matches a given language code base on mapping setting
		/// </summary>
		/// <param name="websiteLanguages">list of portal supported languages</param>
		/// <param name="code">the language code.</param>
		/// <param name="language">the website language.</param>
		/// <returns>true if a language mapping is successful, false otherwise</returns>
		public static bool TryGetLanguageFromMapping(IEnumerable<IWebsiteLanguage> websiteLanguages, string code, out IWebsiteLanguage language)
		{
			language = null;
			return true;
		}

		/// <summary>
		/// Check the query parameter for language codes in the Request
		/// </summary>
		/// <param name="context">current http context</param>
		/// <param name="code">langauge code</param>
		/// <returns>true if found, false otherwise</returns>
		private static bool CheckQueryParamForLanguage(HttpContextBase context, out string code)
		{
			code = context.Request.QueryString["lang"];
			return !string.IsNullOrWhiteSpace(code);
		}

		/// <summary>
		/// Strips out the language code prefix from a given absolute path and returns the result. If path doesn't have language code prefix, then nothing will be done.
		/// This version of the method is only intended to be used in cases where PortalContext isn't available yet (ex: SiteMarkerRoute.GetRouteData).
		/// </summary>
		/// <param name="context">Http context. context.Request.Url.AbsolutePath is what will be processed.</param>
		/// <param name="pathHasLanguageCode">Returns whether the given path had a language code in it.</param>
		/// <param name="websiteLanguage">List of website languages.</param>
		/// <returns>Absolute path with language code prefix stripped out.</returns>
		public static string StripLanguageCodeFromAbsolutePath(HttpContextBase context, out bool pathHasLanguageCode, out IWebsiteLanguage websiteLanguage)
		{
			string absolutePathWithoutLanguageCode;
			IEnumerable<IWebsiteLanguage> activeLanguages = BuildActiveWebsiteLanguages(context);
			ExtractLanguageCode(context.Request.Url.PathAndQuery, activeLanguages, out pathHasLanguageCode, out websiteLanguage, out absolutePathWithoutLanguageCode);
			return absolutePathWithoutLanguageCode;
		}

		/// <summary>
		/// Checks if a given url path contains a language code (case-insensitive).
		/// </summary>
		/// <param name="path">Path to check, must have a leading forward-slash. If the given path contains a querystring, it will be preserved.</param>
		/// <param name="activeLanguages">Context website languages.</param>
		/// <param name="pathHasLanguageCode">Whether the given path had a language code in it.</param>
		/// <param name="language">The active website language referenced by the URL's language code.</param>
		/// <param name="absolutePathWithoutLanguageCode">Absolute path with language code prefix stripped out.</param>
		private static void ExtractLanguageCode(string path, IEnumerable<IWebsiteLanguage> activeLanguages, out bool pathHasLanguageCode, out IWebsiteLanguage language, out string absolutePathWithoutLanguageCode)
		{
			// Check in case the given absolute path has a query string
			int questionMarkIndex = path.IndexOf("?");
			string query;
			if (questionMarkIndex >= 0)
			{
				query = path.Substring(questionMarkIndex);
				absolutePathWithoutLanguageCode = path.Substring(0, questionMarkIndex);
			}
			else
			{
				query = string.Empty;
				absolutePathWithoutLanguageCode = path;
			}

			pathHasLanguageCode = false;
			language = null;
			string[] segments = absolutePathWithoutLanguageCode.Split('/');

			// If we did have a language code in the URL, it would be in the second segment.
			// Ex: "/en-us/abc/def" -> { "", "en-us", "abc", "def" }
			if (segments.Length > 1 && activeLanguages != null)
			{
				var codeSegment = segments[1];
				language = FindWebsiteLanguage(activeLanguages, codeSegment, false);

				if (language == null && TryGetLanguageFromMapping(activeLanguages, codeSegment, out language))
				{
					language.UsedAsFallback = true;
				}

				if (language != null)
				{
					// Language code found
					pathHasLanguageCode = true;
					List<string> segmentsWithoutLanguageCode = segments.ToList();
					segmentsWithoutLanguageCode.RemoveAt(1);
					absolutePathWithoutLanguageCode = string.Join("/", segmentsWithoutLanguageCode);
				}
			}

			// Make sure the path we return isn't empty (this will happen if the path coming in is "/en-us")
			if (absolutePathWithoutLanguageCode.Length == 0)
			{
				absolutePathWithoutLanguageCode = "/";
			}

			// Add the query back on
			absolutePathWithoutLanguageCode += query;
		}

		/// <summary>
		/// Gets the context language code from session cookie and returns if operation was successful or not.
		/// If fail then empty string language code will be returned.
		/// </summary>
		/// <param name="httpContext">The HTTP context.</param>
		/// <param name="languageCode">Language code read from session cookie.</param>
		/// <returns>Whether the get operation suceeded or not.</returns>
		private static bool TryReadLanguageCodeFromCookie(HttpContextBase httpContext, out string languageCode)
		{
			languageCode = string.Empty;
			if (httpContext.Request.Cookies != null)
			{
				var portalLanguageCookie = httpContext.Request.Cookies[ContextLanguageCookieName];
				if (portalLanguageCookie != null)
				{
					languageCode = portalLanguageCookie.Value;
					if (!string.IsNullOrEmpty(languageCode))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// From a list of WebsiteLanguage objects, finds the one that matches the given language code.
		/// If no match, then null will be returned.
		/// </summary>
		/// <param name="websiteLanguages">List of WebsiteLanguage objects to search.</param>
		/// <param name="code">Language code to search on, case-insensitive. ex: "en-US"</param>
		/// <param name="publishedOnly">>Whether to only match on published WebsiteLanguages.</param>
		/// <returns>Matching WebsiteLanguage object. If no match, then null will be returned.</returns>
		private static IWebsiteLanguage FindWebsiteLanguage(IEnumerable<IWebsiteLanguage> websiteLanguages, string code, bool publishedOnly)
		{
			if (websiteLanguages == null)
			{
				return null;
			}

			var results = websiteLanguages.Where(l => string.Equals(l.Code, code, StringComparison.InvariantCultureIgnoreCase));
			if (publishedOnly)
			{
				results = results.Where(l => l.IsPublished);
			}
			return results.FirstOrDefault();
		}
		
		/// <summary>
		/// From a list of WebsiteLanguage objects, finds the one whose WebsiteLanguage entity ID guid matches the given ID..
		/// If no match, then null will be returned.
		/// </summary>
		/// <param name="websiteLanguages">List of WebsiteLanguage objects to search.</param>
		/// <param name="id">WebsiteLanguage entity ID guid to match on.</param>
		/// <param name="publishedOnly">Whether to only match on published WebsiteLanguages.</param>
		/// <returns>Matching WebsiteLanguage object. If no match, then null will be returned.</returns>
		private static IWebsiteLanguage FindWebsiteLanguage(IEnumerable<IWebsiteLanguage> websiteLanguages, Guid id, bool publishedOnly)
		{
			if (websiteLanguages == null)
			{
				return null;
			}

			var results = websiteLanguages.Where(l => l.EntityReference.Id == id);
			if (publishedOnly)
			{
				results = results.Where(l => l.IsPublished);
			}
			return results.FirstOrDefault();
		}

		/// <summary>
		/// Gets all the active website languages (published and draft).
		/// Since the underlying activeWebsiteLanguages singleton is read-write lock protected, a clone (instead of original IEnumerable) is returned.
		/// If method is called before ContextLanguageInfo has been constructed, then HttpContextBase should be passed in since PortalContext won't be available yet.
		/// </summary>
		/// <param name="context">Optional HttpContextBase to use if method is called before ContextLanguageInfo has been constructed (and thus PortalContext isn't available yet).
		/// Will only be necessary to build active website languages in case the read lock on activeWebsiteLanguages fails.</param>
		/// <returns>All the active website languages (published and draft).</returns>
		private static IEnumerable<IWebsiteLanguage> GetActiveWebsiteLanguages(HttpContextBase context = null)
		{
			if (!ActiveWebsiteLanguagesLock.TryEnterReadLock(ActiveWebsiteLanguagesLockReadTimeout))
			{
				ADXTrace.TraceInfo(TraceCategory.Application, "Failed to acquire READ lock on activeWebsiteLanguages. Returning newly built list of website languages.");
				return BuildActiveWebsiteLanguages(false, context);
			}
			try
			{
				return activeWebsiteLanguages == null ? Enumerable.Empty<IWebsiteLanguage>() : activeWebsiteLanguages.ToList(); // Return a clone.
			}
			finally
			{
				ActiveWebsiteLanguagesLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Only if necessary, builds, saves, and returns the active website languages using HttpContextBase as data source.
		/// This is for instances where PortalContext.Current is not available yet, ex: SiteMarkerRoute.GetRouteData(). AdxstudioCrmConfigurationManager will be
		/// used to get the content map. 
		/// If activeWebsiteLanguages already has been initialized, then no action will be done, it will be returned.
		/// </summary>
		/// <param name="context">Http context to use to (if necessary) initialize the list of active website languages.</param>
		/// <returns>IEnumerable of active website languages.</returns>
		private static IEnumerable<IWebsiteLanguage> BuildActiveWebsiteLanguages(HttpContextBase context)
		{
			if (!ActiveWebsiteLanguagesLock.TryEnterUpgradeableReadLock(ActiveWebsiteLanguagesLockWriteTimeout))
			{
				ADXTrace.TraceInfo(TraceCategory.Application, "Failed to acquire UPGRADEABLE-READ lock on activeWebsiteLanguages. Returning newly built list of website languages.");
				return BuildActiveWebsiteLanguages(false, context);
			}
			try
			{
				if (activeWebsiteLanguages == null)
				{
					return BuildActiveWebsiteLanguages(true, context);
				}
			}
			finally
			{
				ActiveWebsiteLanguagesLock.ExitUpgradeableReadLock();
			}

			// Else if activeWebsiteLanguages has already been initialized, just return it.
			return GetActiveWebsiteLanguages(context);
		}

		/// <summary>
		/// Builds the active website languages from context.
		/// </summary>
		/// <param name="writeResultsToStatic">Whether to write the active website languages retrieved to the activeWebsiteLanguages static value.</param>
		/// <param name="httpContext">Optional HttpContext object. This is only necessary for instances where PortalContext.Current is not available yet, ex: SiteMarkerRoute.GetRouteData().
		/// If set to something other than null, then method will try to get website from the HttpContext rather than PortalContext.Current.Website. </param>
		/// <returns>IEnumerable of active website languages.</returns>
		private static IEnumerable<IWebsiteLanguage> BuildActiveWebsiteLanguages(bool writeResultsToStatic, HttpContextBase httpContext)
		{
			return null;
		}

		/// <summary>
		/// Gets languages from a website node
		/// </summary>
		/// <param name="website">Website Node</param>
		/// <returns>List of active website languages</returns>
		private static List<IWebsiteLanguage> GetLanguagesFromWebsiteNode(WebsiteNode website)
        {
            return null;
		}

		/// <summary>
		/// Filters the active website languages based on the enabled languages of the organization.
		/// </summary>
		/// <param name="languages">List of languages to filter</param>
		/// <param name="service">service context</param>
		/// <returns>Filtered list of active languages enabled in the organization</returns>
		public static IEnumerable<IWebsiteLanguage> FilterActiveLanguages(IEnumerable<IWebsiteLanguage> languages, IOrganizationService service)
		{
			var provisionedLanguages = GetProvisionedLanugages(service);

			return languages.Where(l => provisionedLanguages.Contains(l.CrmLcid));
		}

		/// <summary>Get the organization provisioned languages.</summary>
		/// <param name="service">The service.</param>
		/// <returns>The list of languages provisioned in the organization.</returns>
		public static IEnumerable<int> GetProvisionedLanugages(IOrganizationService service)
		{
			var provisionedLanguages = new List<int>();
			var req = new RetrieveProvisionedLanguagesRequest();
			var resp = (RetrieveProvisionedLanguagesResponse)service.Execute(req);
			var orgLanguages = resp.RetrieveProvisionedLanguages;

			if (orgLanguages != null)
			{
				provisionedLanguages = orgLanguages.ToList();
				ADXTrace.TraceVerbose(TraceCategory.Application, string.Format("Retrieved {0} enabled languages for the organization", provisionedLanguages.Count));
			}

			return provisionedLanguages;
		}

		/// <summary>
		/// Strips out the language code prefix from a given absolute path and returns the result. If path doesn't have language code prefix, then nothing will be done.
		/// This version of the method is only intended to be used in cases where PortalContext isn't available yet (ex: SiteMarkerRoute.GetRouteData).
		/// </summary>
		/// <param name="overridePath">Raw url.</param>
		/// <returns>Absolute path with language code prefix stripped out.</returns>
		public string StripLanguageCodeFromAbsolutePath(string overridePath)
		{
			string absolutePathWithoutLanguageCode;
			bool pathHasLanguageCode;
			IWebsiteLanguage websiteLanguage;
			var activeLanguages = ActiveWebsiteLanguages;

			ExtractLanguageCode(overridePath, activeLanguages, out pathHasLanguageCode, out websiteLanguage, out absolutePathWithoutLanguageCode);

			return absolutePathWithoutLanguageCode;
		}

		/// <summary>
		/// From a given language-root WebPageNode, finds the translated content WebPageNode whose language matches the context language.
		/// If the given WebpageNode is not a language-root WebPage then method will search the root's content pages.
		/// If there no match is found, then null will be returned.
		/// If multi-language is not enabled, then the given WebPageNode will be returned.
		/// </summary>
		/// <param name="node">WebPageNode to search.</param>
		/// <param name="onlyFindPublished">Whether to only find published pages.</param>
		/// <returns>Translated context WebPageNode that matches the context language.</returns>
		public WebPageNode FindLanguageSpecificWebPageNode(WebPageNode node, bool onlyFindPublished)
		{
			if (IsCrmMultiLanguageEnabled)
			{
				IEnumerable<WebPageNode> results = new WebPageNode[0];
				if (node.IsRoot == true)
				{
					results = node.LanguageContentPages.Where(p => p.WebPageLanguage != null && p.WebPageLanguage.Id == ContextLanguage.EntityReference.Id);
				}
				else if (node.RootWebPage != null)
				{
					// Find this node's root webpage, and search its content pages
					results = node.RootWebPage.LanguageContentPages.Where(p => p.WebPageLanguage != null && p.WebPageLanguage.Id == ContextLanguage.EntityReference.Id);
				}
				else if (node.WebPageLanguage != null && node.WebPageLanguage.Id == ContextLanguage.EntityReference.Id)
				{
					// The given web page doesn't have a root, but the page's language matches the context language,
					// so just include the given node in results since it's a language match.
					results = new[] { node };
				}

				// Note: if we fell through without hitting any of the if's, then it means the given page is not a root page, doesn't have a root page, and is the wrong language.
				// In that case just keep the result set empty.

				// If necessary, filter on Published
				if (onlyFindPublished)
				{
					results = results.Where(p => p.PublishingState.IsVisible == true);
				}

				return results.FirstOrDefault();
			}

			// If multi-language is not active, just return the given node.
			return node;
		}

		/// <summary>
		/// Looks for specific web page from a root page for the current language context
		/// </summary>
		/// <param name="serviceContext">Organization service context</param>
		/// <param name="rootPageId">Guid of the root page</param>
		/// <returns>Returns a webpage entity</returns>
		public Entity FindLangaugeSpecificWebPage(OrganizationServiceContext serviceContext, Guid rootPageId)
		{
			// this is a root page. Find it's translated page in the language we want
			if (IsCrmMultiLanguageEnabled)
			{
				return serviceContext.CreateQuery("adx_webpage")
					.FirstOrDefault(e =>
						e.GetAttributeValue<bool>("adx_isroot") == false &&
						e.GetAttributeValue<EntityReference>("adx_rootwebpageid").Id == rootPageId &&
						e.GetAttributeValue<EntityReference>("adx_webpagelanguageid").Id == ContextLanguage.EntityReference.Id);
			}

			return null;
		}

		/// <summary>
		/// Gets a WebPage entity's language root WebPage. If a root is passed in or multi-language is not active, then the given WebPage will be returned.
		/// </summary>
		/// <param name="context">Organization service context.</param>
		/// <param name="webPage">WebPage entity to find the root. Entity's logical name MUST be "adx_webpage".</param>
		/// <returns>Root WebPage entity.</returns>
		public Entity GetRootWebPageEntity(OrganizationServiceContext context, Entity webPage)
		{
			webPage.AssertEntityName("adx_webpage");
			if (IsCrmMultiLanguageEnabled && !webPage.GetAttributeValue<bool>("adx_isroot"))
			{
				var rootPageReference = webPage.GetEntityReferenceValue<EntityReference>("adx_rootwebpageid");

				// Take root page instead of content page
				return context.CreateQuery("adx_webpage").SingleOrDefault(p => p.GetAttributeValue<Guid>("adx_webpageid") == rootPageReference.Id);
			}
			return webPage;
		}

		/// <summary>
		/// Get avaialable website langauges for a certain content page
		/// </summary>
		/// <param name="webPageReference">Entity reference for web page</param>
		/// <param name="context">Http Context</param>
		/// <returns>List of website languages</returns>
		public IEnumerable<IWebsiteLanguage> GetWebPageWebsiteLanguages(EntityReference webPageReference, HttpContext context)
		{

            return null;
		}

		/// <summary>
		/// Gets the WebsiteLanguage object matches the given languageCode. If no match is found, then null will be returned.
		/// </summary>
		/// <param name="languageCode">Language code to match on, case-insensitive. ex: "en-US"</param>
		/// <param name="languages">List of website languages.</param>
		/// <param name="publishedOnly">Whether to only match on published WebsiteLanguages.</param>
		/// <returns>Matching WebsiteLanguage object. If no match, then null will be returned.</returns>
		public IWebsiteLanguage GetWebsiteLanguage(string languageCode, IEnumerable<IWebsiteLanguage> languages, bool publishedOnly = false)
		{
			return FindWebsiteLanguage(languages, languageCode, publishedOnly);
		}

		/// <summary>
		/// Gets the WebsiteLanguage object that has the given entity ID. If no match is found, then null will be returned.
		/// </summary>
		/// <param name="id">WebsiteLanguage ID to match on.</param>
		/// <param name="languages">List of website languages.</param>
		/// <param name="publishedOnly">Whether to only match on published WebsiteLanguages.</param>
		/// <returns>Matching WebsiteLanguage.</returns>
		public IWebsiteLanguage GetWebsiteLanguage(Guid id, IEnumerable<IWebsiteLanguage> languages, bool publishedOnly = false)
		{
			return FindWebsiteLanguage(languages, id, publishedOnly);
		}

		/// <summary>
		/// Gets the WebsiteLanguage object that corresponds to the given Portal Language entity ID. If no match is found, then null will be returned.
		/// </summary>
		/// <param name="portalLanguageId">Portal Language ID to match on.</param>
		/// <param name="languages">List of website languages.</param>
		/// <param name="publishedOnly">Whether to only match on published WebsiteLanguages.</param>
		/// <returns>Matching WebsiteLanguage.</returns>
		public IWebsiteLanguage GetWebsiteLanguageByPortalLanguageId(Guid portalLanguageId, IEnumerable<IWebsiteLanguage> languages, bool publishedOnly = false)
		{
			var results = languages.Where(l => l.PortalLanguageId == portalLanguageId);
			if (publishedOnly)
			{
				results = results.Where(l => l.IsPublished);
			}
			return results.FirstOrDefault();
		}

		/// <summary>
		/// Formats the current request URL's path and query with language code prefix. If the URL already has a language code prefix, then it will be replaced.
		/// Respects the DisplayLanguageCodeInUrl site setting.
		/// If multi-language is not activated, then the current path and query will be returned as is.
		/// </summary>
		/// <param name="excludeLeadingSlash">Optional: Whether to exclude the leading slash in the returned value.</param>
		/// <param name="overrideLanguageCode">Optional: Language code to use. Otherwise pass in null or empty string and the context language code will be used.</param>
		/// <param name="overrideUri">Optional: Uri value to use. Otherwise pass in null and the context request Uri will be used.</param>
		/// <param name="overrideDisplayLanguageCodeInUrl">Optional: Whether to include language code prefix in returned path.
		/// Otherwise pass in null and the context DisplayLanguageCodeInUrl site setting value will be used.</param>
		/// <returns>Absolute path and query with language code prefix.</returns>
		public string FormatUrlWithLanguage(bool excludeLeadingSlash = false, string overrideLanguageCode = null, Uri overrideUri = null, bool? overrideDisplayLanguageCodeInUrl = null)
		{
			string pathAndQueryWithLanguageCode;
			if (IsCrmMultiLanguageEnabled)
			{
				// Resolve override values
				string languageCode = string.IsNullOrEmpty(overrideLanguageCode) ? ContextLanguage.Code : overrideLanguageCode;
				bool displayLanguageCodeInUrl = overrideDisplayLanguageCodeInUrl ?? DisplayLanguageCodeInUrl;

				string absolutePathWithoutLanguageCode;
				string currentPath = overrideUri == null ? ContextUrl.PathAndQuery : overrideUri.PathAndQuery;
				
				bool pathHasLanguageCode;
				IWebsiteLanguage language;
				ExtractLanguageCode(currentPath, ActiveWebsiteLanguages, out pathHasLanguageCode, out language, out absolutePathWithoutLanguageCode);

				// if a fallback language was returned and it matches the language family of the current language, use the current language
				if (language != null && language.UsedAsFallback && ContextLanguage.CrmLcid == language.CrmLcid)
				{
					languageCode = ContextLanguage.Code;
				}

				pathAndQueryWithLanguageCode = displayLanguageCodeInUrl
					? string.Format("{0}{1}{2}", "/", languageCode, absolutePathWithoutLanguageCode)
					: absolutePathWithoutLanguageCode;
			}
			else
			{
				// If multi-language is not active, just return the Uri's PathAndQuery as is.
				pathAndQueryWithLanguageCode = (overrideUri ?? ContextUrl).PathAndQuery;
			}

			// If requested, strip out leading forward-slash
			if (excludeLeadingSlash && pathAndQueryWithLanguageCode.StartsWith("/"))
			{
				pathAndQueryWithLanguageCode = pathAndQueryWithLanguageCode.TrimStart('/');
			}

			return pathAndQueryWithLanguageCode;
		}

		/// <summary>
		/// Formats the current request URL's path and query with language code prefix. If the URL already has a language code prefix, then it will be replaced.
		/// Respects the DisplayLanguageCodeInUrl site setting.
		/// If multi-language is not activated, then the current path and query will be returned as is.
		/// </summary>
		/// <param name="languageCode">Optional: Language code to use. Otherwise pass in null or empty string and the context language code will be used.</param>
		/// <param name="uri">Uri value to use. Otherwise pass in null and the context request Uri will be used.</param>
		/// <param name="displayCodeInUrl">Optional: Whether to include language code prefix in returned path.
		/// Otherwise pass in null and the context DisplayLanguageCodeInUrl site setting value will be used.</param>
		/// <param name="excludeLeadingSlash">Optional: Whether to exclude the leading slash in the returned value.</param>
		/// <returns>Absolute path and query with language code prefix.</returns>
		public static ApplicationPath FormatUrl(string languageCode, Uri uri, bool displayCodeInUrl, bool excludeLeadingSlash = false)
		{
			string absolutePathWithoutLanguageCode;
			IEnumerable<IWebsiteLanguage> languages = GetActiveWebsiteLanguages();

			// Extract the absolute path without language code from the given Uri.
			bool pathHasLanguageCode;
			IWebsiteLanguage language;
			ExtractLanguageCode(uri.PathAndQuery, languages, out pathHasLanguageCode, out language, out absolutePathWithoutLanguageCode);

			string absolutePathWithNewLanguageCode = displayCodeInUrl
				  ? string.Format("{0}{1}{2}", "/", languageCode, absolutePathWithoutLanguageCode)
				  : absolutePathWithoutLanguageCode;


			// If requested, strip out leading forward-slash
			if (excludeLeadingSlash && absolutePathWithNewLanguageCode.StartsWith("/"))
			{
				absolutePathWithNewLanguageCode = absolutePathWithNewLanguageCode.TrimStart('/');
			}

			return ApplicationPath.FromAbsolutePath(absolutePathWithNewLanguageCode);
		}


		/// <summary>
		/// Adds language code to URL
		/// </summary>
		/// <param name="path">application path</param>
		/// <returns>application path with language code prefix</returns>
		public static ApplicationPath PrependLanguageCode(ApplicationPath path)
		{
			var language = ResolveContextLanguage(new HttpContextWrapper(HttpContext.Current));

			if (language == null)
			{
				return path;

			}

			// first part will not be used. Just adding for MLP api consistency 
			var uri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + path.AbsolutePath);

			var newPath = FormatUrl(language.Code, uri, DisplayLanguageCodeInUrl);

			return newPath;
		}

		/// <summary>
		/// Determines if the site is a single language portal
		/// </summary>
		/// <param name="languages">List of website languages</param>
		/// <param name="defaultLanguage">default language of the site</param>
		/// <returns>True if site has one language and the default language matches</returns>
		private bool IsSingleLanguageWebsite(IEnumerable<IWebsiteLanguage> languages, IWebsiteLanguage defaultLanguage)
		{
			var websiteLanguages = languages as IWebsiteLanguage[] ?? languages.ToArray();
			var first = websiteLanguages.FirstOrDefault();
			if (first == null)
			{
				return false;
			}
			return websiteLanguages.Count(l => !l.WebsiteLanguageNode.IsReference) == 1 && Equals(first.EntityReference.Id, defaultLanguage.EntityReference.Id);
		}

		/// <summary>
		/// IDisposable function
		/// </summary>
		void IDisposable.Dispose()
		{
			
		}
	}
}
