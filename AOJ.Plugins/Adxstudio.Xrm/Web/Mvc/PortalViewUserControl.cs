/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.Mvc;
using System.Web.UI;

namespace Adxstudio.Xrm.Web.Mvc
{
	public abstract class PortalViewUserControl : UserControl
	{
		private Lazy<HtmlHelper> _html;
		private Lazy<UrlHelper> _url;

		protected HtmlHelper Html
		{
			get { return _html.Value; }
		}

		protected virtual string PortalName { get; set; }

		protected UrlHelper Url
		{
			get { return _url.Value; }
		}

		protected override void OnInit(EventArgs args)
		{
			_html = PortalViewPage.GetLazyHtmlHelper(PortalName, Request.RequestContext, Response);
			_url = PortalViewPage.GetLazyUrlHelper(Request.RequestContext);

			base.OnInit(args);
		}

		private void RedirectToHttpsIfNecessary()
		{
			if (Request.IsSecureConnection)
			{
				return;
			}

			var redirectUrl = Uri.UriSchemeHttps + Uri.SchemeDelimiter + Request.Url.Authority + Request.Url.PathAndQuery;

			Response.Redirect(redirectUrl);
		}
	}
}
