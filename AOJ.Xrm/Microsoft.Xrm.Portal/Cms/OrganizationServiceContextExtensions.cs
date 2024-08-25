/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Cms
{
	public static class OrganizationServiceContextExtensions
	{
		#region generic

		public static string GetUrl(this OrganizationServiceContext context, Entity entity, string portalName = null)
		{
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>();

			return urlProvider.GetUrl(context, entity);
		}

		public static ApplicationPath GetApplicationPath(this OrganizationServiceContext context, Entity entity, string portalName = null)
		{
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityUrlProvider>();

			return urlProvider.GetApplicationPath(context, entity);
		}

		public static Entity GetWebsite(this OrganizationServiceContext context, Entity entity, string portalName = null)
		{
			var websiteProvider = PortalCrmConfigurationManager.CreateDependencyProvider(portalName).GetDependency<IEntityWebsiteProvider>();

			return websiteProvider.GetWebsite(context, entity);
		}

		#endregion

		#region mspp_website

		public static IEnumerable<Entity> GetLinkSets(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("mspp_website");

			var webLinkSets = website.GetRelatedEntities(context, "mspp_website_weblinkset");
			return webLinkSets;
		}

		public static Entity GetLinkSetByName(this OrganizationServiceContext context, Entity website, string webLinkSetName)
		{
			var webLinkSets = context.GetLinkSets(website);
			return webLinkSets.Where(wls => wls.GetAttributeValue<string>("mspp_name") == webLinkSetName).FirstOrDefault();
		}

		public static IEnumerable<Entity> GetMembershipTypes(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("mspp_website");

			var membershipTypes = website.GetRelatedEntities(context, "mspp_website_membershiptype");
			return membershipTypes;
		}

		public static Entity GetMembershipTypeByName(this OrganizationServiceContext context, Entity website, string membershipTypeName)
		{
			var membershipTypes = context.GetMembershipTypes(website);
			return membershipTypes.Where(mt => mt.GetAttributeValue<string>("mspp_name") == membershipTypeName).FirstOrDefault();
		}

		public static Entity GetPageBySiteMarkerName(this OrganizationServiceContext context, Entity website, string siteMarkerName)
		{
			website.AssertEntityName("mspp_website");

			var siteMarkers = website.GetRelatedEntities(context, "mspp_website_sitemarker");
			var siteMarker = siteMarkers.Where(sm => sm.GetAttributeValue<string>("mspp_name") == siteMarkerName).FirstOrDefault();
			var webPage = siteMarker == null ? null : siteMarker.GetRelatedEntity(context, "mspp_webpage_sitemarker");
			return webPage;
		}

		public static IEnumerable<Entity> GetSiteSettings(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("mspp_website");

			var siteSettings = website.GetRelatedEntities(context, "mspp_website_sitesetting");
			return siteSettings;
		}

		public static Entity GetSiteSettingByName(this OrganizationServiceContext context, Entity website, string siteSettingName)
		{
			var siteSettings = context.GetSiteSettings(website);
			return (from s in siteSettings where s.GetAttributeValue<string>("mspp_name") == siteSettingName select s).FirstOrDefault();
		}

		public static string GetSiteSettingValueByName(this OrganizationServiceContext context, Entity website, string siteSettingName)
		{
			var siteSetting = context.GetSiteSettingByName(website, siteSettingName);
			return (siteSetting == null ? null : siteSetting.GetAttributeValue<string>("mspp_value"));
		}

		public static IEnumerable<Entity> GetSnippets(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("mspp_website");

			var snippets = website.GetRelatedEntities(context, "mspp_website_contentsnippet");
			return snippets;
		}

		public static Entity GetSnippetByName(this OrganizationServiceContext context, Entity website, string snippetName)
		{
			var snippets = context.GetSnippets(website);
			return snippets.Where(cs => cs.GetAttributeValue<string>("mspp_name") == snippetName).FirstOrDefault();
		}

		public static string GetSnippetValueByName(this OrganizationServiceContext context, Entity website, string snippetName)
		{
			var snippet = context.GetSnippetByName(website, snippetName);
			return snippet != null ? snippet.GetAttributeValue<string>("mspp_value") : null;
		}

		public static IEnumerable<Entity> GetSiteMarkers(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("mspp_website");

			var siteMarkers = website.GetRelatedEntities(context, "mspp_website_sitemarker");
			return siteMarkers;
		}

		public static Entity GetSiteMarkerByName(this OrganizationServiceContext context, Entity website, string siteMarkerName)
		{
			var siteMarkers = context.GetSiteMarkers(website);
			return siteMarkers.Where(sm => sm.GetAttributeValue<string>("mspp_name") == siteMarkerName).FirstOrDefault();
		}

		/// <summary>
		/// Retrieves visible child Web Page entities for a given site-marker.
		/// </summary>
		public static IEnumerable<Entity> GetVisibleChildPagesBySiteMarker(this OrganizationServiceContext context, Entity website, string siteMarker)
		{
			website.AssertEntityName("mspp_website");

			var websiteID = website.GetAttributeValue<Guid>("mspp_websiteid");

			var findPages =
				from cwp in context.CreateQuery("mspp_webpage")
				join wp in context.CreateQuery("mspp_webpage")
					on cwp.GetAttributeValue<Guid>("mspp_parentpageid") equals wp.GetAttributeValue<Guid>("mspp_webpageid")
				join sm in context.CreateQuery("mspp_sitemarker")
					on wp.GetAttributeValue<Guid>("mspp_webpageid") equals sm.GetAttributeValue<Guid>("mspp_pageid")
				// filter to current site
				where sm.GetAttributeValue<Guid?>("mspp_websiteid") == websiteID && sm.GetAttributeValue<string>("mspp_name") == siteMarker
				select cwp;

			return findPages.Cast<Entity>().ToList();
		}

		public static TimeZoneInfo GetTimeZone(this OrganizationServiceContext context, Entity website)
		{
			website.AssertEntityName("mspp_website");

			var timezoneid = context.GetSiteSettingValueByName(website, "timezone/id");

			return !string.IsNullOrEmpty(timezoneid)
				? TimeZoneInfo.FindSystemTimeZoneById(timezoneid)
				: TimeZoneInfo.Local;
		}

		#endregion

		#region mspp_webpage

		/// <summary>
		/// Retrieves the child pages of this page.
		/// </summary>
		public static IEnumerable<Entity> GetChildPages(this OrganizationServiceContext context, Entity webPage)
		{
			webPage.AssertEntityName("mspp_webpage");

			var childPages = webPage.GetRelatedEntities(context, "mspp_webpage_webpage", EntityRole.Referenced);
			return childPages;
		}

		/// <summary>
		/// Retrieves the child files of this page.
		/// </summary>
		public static IEnumerable<Entity> GetChildFiles(this OrganizationServiceContext context, Entity webPage)
		{
			webPage.AssertEntityName("mspp_webpage");

			var childFiles = webPage.GetRelatedEntities(context, "mspp_webpage_webfile");
			return childFiles;
		}

		/// <summary>
		/// Retrieves the visible child pages of this page.
		/// </summary>
		public static IEnumerable<Entity> GetVisibleChildPages(this OrganizationServiceContext context, Entity webPage)
		{
			var childPages = context.GetChildPages(webPage);
			var visibleChildPages = childPages.Where(cp => !cp.GetAttributeValue<bool?>("mspp_hiddenfromsitemap").GetValueOrDefault(false));
			return visibleChildPages;
		}

		#endregion

		#region mspp_weblinkset

		public static IOrderedEnumerable<Entity> GetOrderedWebLinks(this OrganizationServiceContext context, Entity webLinkSet)
		{
			webLinkSet.AssertEntityName("mspp_weblinkset");

			var webLinks = webLinkSet.GetRelatedEntities(context, "mspp_weblinkset_weblink");
			return webLinks.OrderBy(wl => wl.GetAttributeValue<int?>("mspp_displayorder"));
		}

		#endregion

		#region mspp_weblink

		/// <summary>
		/// Retrieves a list of visible child pages for this web link.
		/// </summary>
		public static IEnumerable<Entity> GetVisibleChildPagesForWebLink(this OrganizationServiceContext context, Entity webLink)
		{
			webLink.AssertEntityName("mspp_weblink");

			var page = webLink.GetRelatedEntity(context, "mspp_webpage_weblink");
			return page == null ? null : context.GetVisibleChildPages(page);
		}

		#endregion

		#region mspp_webfile

		/// <summary>
		/// Retrieves the CRM notes that the file is uploaded to.
		/// </summary>
		public static IEnumerable<Entity> GetNotes(this OrganizationServiceContext context, Entity file)
		{
			file.AssertEntityName("mspp_webfile");

			var notes = file.GetRelatedEntities(context, "mspp_webfile_Annotations");
			return notes;
		}

		/// <summary>
		/// Retrieves the CRM note that the file is uploaded to.
		/// </summary>
		public static Entity GetNote(this OrganizationServiceContext context, Entity file)
		{
			var notes = context.GetNotes(file);
			return notes.FirstOrDefault();
		}

		#endregion

		#region mspp_sitemarker

		public static Entity GetWebPage(this OrganizationServiceContext context, Entity siteMarker)
		{
			siteMarker.AssertEntityName("mspp_sitemarker");

			var webPage = siteMarker.GetRelatedEntity(context, "mspp_webpage_sitemarker");
			return webPage;
		}

		#endregion


	}
}
