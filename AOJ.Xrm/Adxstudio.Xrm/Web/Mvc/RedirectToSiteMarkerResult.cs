/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using Adxstudio.Xrm.Collections.Generic;
using Adxstudio.Xrm.Web.Mvc.Html;

namespace Adxstudio.Xrm.Web.Mvc
{
	public class RedirectToSiteMarkerResult : ActionResult
	{
		public string SiteMarker { get; }
		public NameValueCollection Query { get; }
		public bool RestrictRead { get; }

		public RedirectToSiteMarkerResult(string siteMarker)
			: this(siteMarker, null)
		{
		}

		public RedirectToSiteMarkerResult(string siteMarker, NameValueCollection query)
			: this(siteMarker, query, false)
		{
		}

		public RedirectToSiteMarkerResult(string siteMarker, NameValueCollection query, bool restrictRead)
		{
			if (string.IsNullOrWhiteSpace(siteMarker)) throw new ArgumentNullException("siteMarker");

			SiteMarker = siteMarker;
			Query = query;
			RestrictRead = restrictRead;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			object viewData;

			if (context.Controller.ViewData != null &&
			    context.Controller.ViewData.TryGetValue(PortalExtensions.PortalViewContextKey, out viewData))

			{
				var portalViewContext = viewData as IPortalViewContext;

				ExecuteResult(portalViewContext, context.HttpContext);
			}
		}

		public void ExecutePageResult(ViewDataDictionary contextViewData, HttpContextBase context)
		{
			object viewData;

			if (contextViewData != null && contextViewData.TryGetValue(PortalExtensions.PortalViewContextKey, out viewData))
			{
				var portalViewContext = viewData as IPortalViewContext;
				ExecuteResult(portalViewContext, context);
			}
		}

		private void ExecuteResult(IPortalViewContext portalViewContext, HttpContextBase context)
		{
			if (portalViewContext != null)
			{
				var siteMarker = RestrictRead
					? portalViewContext.SiteMarkers.SelectWithReadAccess(SiteMarker)
					: portalViewContext.SiteMarkers.Select(SiteMarker);

				if (siteMarker != null)
				{
					var url = Query != null
						? siteMarker.Url.AppendQueryString(Query)
						: siteMarker.Url;

					context.RedirectAndEndResponse(url);
				}
			}
		}
	}
}
