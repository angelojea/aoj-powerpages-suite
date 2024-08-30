/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Ideas
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Client.Security;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Client;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Cms.Security;
	using Security;
	using Services;
	using Services.Query;
	using Cms;

	internal class IdeaSecurityProvider : ContentMapAccessProvider
	{
		protected string PortalName { get; private set; }
		
		public IdeaSecurityProvider(HttpContext context, string portalName = null) 
			: base(context)
		{
			PortalName = portalName;
		}

		protected override bool TryAssert(OrganizationServiceContext serviceContext, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			entity.ThrowOnNull("entity");
			dependencies.AddEntityDependency(entity);
			ADXTrace.TraceInfo(TraceCategory.Application, string.Format(@"Testing right {0} on {1} ""{2}"" ({3}).", right, entity.LogicalName, entity.GetAttributeValue<string>("adx_name"), entity.Id));

			switch (entity.LogicalName)
			{
				case "adx_ideaforum":
					dependencies.AddEntitySetDependency("adx_webrole");
					return TryAssertIdeaForum(entity, right, dependencies, map);
				case "adx_idea":
					return TryAssertIdea(serviceContext, entity, right, dependencies, map);
				case "adx_ideacomment":
					return TryAssertIdeaComment(serviceContext, entity, right, dependencies, map);
				case "adx_ideavote":
					return TryAssertIdeaVote(serviceContext, entity, right, dependencies, map);
				default:
					throw new NotSupportedException("Entities of type {0} are not supported by this provider.".FormatWith(entity.LogicalName));
			}
		}

		private bool UserInRole(CrmEntityRight right, bool defaultIfNoRoles, Entity ideaForumEntity, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			if (!Roles.Enabled)
			{
				ADXTrace.TraceInfo(TraceCategory.Application, string.Format("Roles are not enabled for this application. Returning {0}.", defaultIfNoRoles));
				return defaultIfNoRoles;
			}
			
			IEnumerable<Entity> roles = new List<Entity>();
			IdeaForumNode ideaForumNode;
			if (!map.TryGetValue(ideaForumEntity, out ideaForumNode))
			{
				return false;
			}

			if (right == CrmEntityRight.Read)
			{
				roles = ideaForumNode.WebRolesRead.Select(wr => wr.ToEntity());
			}
			else if (right == CrmEntityRight.Change)
			{
				roles = ideaForumNode.WebRolesWrite.Select(wr => wr.ToEntity());
			}

			if (!roles.Any())
			{
				ADXTrace.TraceInfo(TraceCategory.Application, string.Format("Read is not restricted to any particular roles. Returning {0}.", defaultIfNoRoles));
				return defaultIfNoRoles;
			}

			dependencies.AddEntityDependencies(roles);

			var userRoles = GetUserRoles();

			return roles.Select(e => e.GetAttributeValue<string>("adx_name")).Intersect(userRoles, StringComparer.InvariantCulture).Any();
		}

		private bool TryAssertIdeaForum(Entity ideaForumEntity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			return right == CrmEntityRight.Change
				? UserInRole(CrmEntityRight.Change, false, ideaForumEntity, dependencies, map)
				: UserInRole(CrmEntityRight.Read, true, ideaForumEntity, dependencies, map)
					|| UserInRole(CrmEntityRight.Change, false, ideaForumEntity, dependencies, map);
		}

		private bool TryAssertIdea(OrganizationServiceContext serviceContext, Entity ideaEntity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			var ideaForumReference = ideaEntity.GetAttributeValue<EntityReference>("adx_ideaforumid");

			IdeaForumNode ideaForumNode;
			if (!map.TryGetValue(ideaForumReference, out ideaForumNode))
			{
				return false;
			}
			var ideaForum = ideaForumNode.AttachTo(serviceContext);
			if (ideaForum == null)
			{
				return false;
			}
			
			var approved = ideaEntity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);

			// If the right being asserted is Read, and the idea is approved, assert whether the idea forum is readable.
			if (right == CrmEntityRight.Read && approved)
			{
				return TryAssert(serviceContext, ideaForum, right, dependencies, map);
			}

			return TryAssert(serviceContext, ideaForum, CrmEntityRight.Change, dependencies, map);
		}

		private bool TryAssertIdeaComment(OrganizationServiceContext serviceContext, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			var idea = entity.GetRelatedEntity(serviceContext, new Relationship("adx_idea_ideacomment"));

			if (idea == null)
			{
				return false;
			}

			var approved = entity.GetAttributeValue<bool?>("adx_approved").GetValueOrDefault(false);

			// If the right being asserted is Read, and the comment is approved, assert whether the idea is readable.
			if (right == CrmEntityRight.Read && approved)
			{
				return TryAssert(serviceContext, idea, right, dependencies);
			}

			return TryAssert(serviceContext, idea, CrmEntityRight.Change, dependencies, map);
		}

		private bool TryAssertIdeaVote(OrganizationServiceContext serviceContext, Entity entity, CrmEntityRight right, CrmEntityCacheDependencyTrace dependencies, ContentMap map)
		{
			var idea = entity.GetRelatedEntity(serviceContext, new Relationship("adx_idea_ideavote"));
			return idea != null && TryAssert(serviceContext, idea, right, dependencies, map);
		}

	}
}
