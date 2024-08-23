/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Runtime;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Xrm.Portal.Web.Providers
{
	public class EntityWebsiteProvider : IEntityWebsiteProvider // MSBug #120034: Won't seal, inheritance is used extension point.
	{
		public virtual Entity GetWebsite(OrganizationServiceContext context, Entity entity)
		{
			context.ThrowOnNull("context");

			if (entity == null)
			{
				return null;
			}

			var entitiesWithoutWebsites = new Dictionary<string, Relationship>
			{
				{ "mspp_weblink", "mspp_weblinkset_weblink".ToRelationship() },
			};

			Relationship hasWebsiteRelationship;

			if (entitiesWithoutWebsites.TryGetValue(entity.LogicalName, out hasWebsiteRelationship))
			{
				return GetWebsite(context, entity.GetRelatedEntity(context, hasWebsiteRelationship));
			}

			var lookup = new Dictionary<string, Relationship>
			{
				{ "mspp_weblinkset", "mspp_website_weblinkset".ToRelationship() },
				{ "mspp_webpage", "mspp_website_webpage".ToRelationship() },
				{ "mspp_webfile", "mspp_website_webfile".ToRelationship() },
				{ "mspp_sitemarker", "mspp_website_sitemarker".ToRelationship() },
				{ "mspp_pagetemplate", "mspp_website_pagetemplate".ToRelationship() },
				{ "mspp_contentsnippet", "mspp_website_contentsnippet".ToRelationship() },
			};

			Relationship websiteRelationship;

			if (lookup.TryGetValue(entity.LogicalName, out websiteRelationship))
			{
				return entity.GetRelatedEntity(context, websiteRelationship);
			}

			return null;
		}
	}
}
