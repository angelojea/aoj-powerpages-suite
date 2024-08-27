/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Web.Routing;
using Adxstudio.Xrm.Web.Routing;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SharePointDocumentDrop : PortalDrop
	{
		public SharePointDocumentDrop(IPortalLiquidContext portalLiquidContext, File file, Entity documentLocation) : base(portalLiquidContext)
		{
			if (file == null) throw new ArgumentNullException("file");

			Name = file.Name;
			Length = file.Length;
			TimeCreated = file.TimeCreated;
			TimeLastModified = file.TimeLastModified;
			Title = file.Title;


			var portalContext = PortalCrmConfigurationManager.CreatePortalContext();
			var website = portalContext.Website;

			var virtualPath = website == null
				? RouteTable.Routes.GetVirtualPath(null, typeof(EntityRouteHandler).FullName,
					new RouteValueDictionary
					{
						{ "prefix", "_entity" },
						{ "logicalName", documentLocation.LogicalName },
						{ "id", documentLocation.Id },
						{ "file", file.Name }
					})
				: RouteTable.Routes.GetVirtualPath(null, typeof(EntityRouteHandler).FullName + "PortalScoped",
					new RouteValueDictionary
					{
						{ "prefix", "_entity" },
						{ "logicalName", documentLocation.LogicalName },
						{ "id", documentLocation.Id },
						{ "__portalScopeId__", website.Id },
						{ "file", file.Name }
					});

			Url = virtualPath == null
				? null
				: VirtualPathUtility.ToAbsolute(virtualPath.VirtualPath);
		}

		public virtual long Length { get; }

		public virtual string Name { get; }

		public virtual DateTime TimeCreated { get; }

		public virtual DateTime TimeLastModified { get; }

		public virtual string Title { get; }

		public virtual string Url { get; }
	}
}
