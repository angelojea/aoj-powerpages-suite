/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Providers
{
	public class EntityUrlProvider : IEntityUrlProvider // MSBug #120032: Won't seal, inheritance is used extension point.
	{
		public EntityUrlProvider(IEntityWebsiteProvider websiteProvider)
		{
			websiteProvider.ThrowOnNull("websiteProvider");

			WebsiteProvider = websiteProvider;
		}

		protected IEntityWebsiteProvider WebsiteProvider { get; private set; }

		public virtual string GetUrl(OrganizationServiceContext context, Entity entity)
		{
			var applicationPath = GetApplicationPath(context, entity);

			if (applicationPath != null)
			{
				return applicationPath.ExternalUrl ?? applicationPath.AbsolutePath;
			}

			return null;
		}

		public virtual ApplicationPath GetApplicationPath(OrganizationServiceContext context, Entity entity)
		{
			context.ThrowOnNull("context");

			if (entity == null)
			{
				return null;
			}

			if (entity.LogicalName == "mspp_weblink")
			{
				return GetWebLinkUrl(context, entity);
			}

			var lookup = new Dictionary<string, Tuple<string, Relationship, string>>
			{
				{
					"mspp_webpage",
					new Tuple<string, Relationship, string>(
						"mspp_partialurl",
						"mspp_webpage_webpage".ToRelationship(EntityRole.Referencing),
						"mspp_webpage")
				},
				{
					"mspp_webfile",
					new Tuple<string, Relationship, string>(
						"mspp_partialurl",
						"mspp_webpage_webfile".ToRelationship(),
						"mspp_webpage")
				},
			};

			Tuple<string, Relationship, string> urlData;

			if (lookup.TryGetValue(entity.LogicalName, out urlData))
			{
				var partialUrlLogicalName = urlData.Item1;
				var relationship = urlData.Item2;
				var otherEntityName = urlData.Item3;

				var websiteRelativeUrl = this.GetApplicationPath(context, entity, partialUrlLogicalName, relationship, otherEntityName, GetApplicationPath);

				var website = WebsiteProvider.GetWebsite(context, entity);

				var path = WebsitePathUtility.ToAbsolute(website, websiteRelativeUrl.PartialPath);

				return ApplicationPath.FromPartialPath(path);
			}

			return null;
		}

		private ApplicationPath GetWebLinkUrl(OrganizationServiceContext context, Entity webLink)
		{
			webLink.AssertEntityName("mspp_weblink");

			var externalUrl = webLink.GetAttributeValue<string>("mspp_externalurl");

			if (!string.IsNullOrWhiteSpace(externalUrl))
			{
				return ApplicationPath.FromExternalUrl(externalUrl);
			}

			var page = webLink.GetRelatedEntity(context, "mspp_webpage_weblink");

			return page == null ? null : GetApplicationPath(context, page);
		}
	}
}
