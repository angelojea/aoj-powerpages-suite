/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Forums
{
	/// <summary>
	/// Provides query access to all Forum Posts (adx_communityforum) in a given Website (adx_website). Also provides
	/// query access to latest Forum Threads (adx_communityforumthread) across all Forums in that Website.
	/// </summary>
	public class WebsiteForumPostUserAggregationDataAdapter : IForumPostAggregationDataAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="userId">The unique identifier of the portal user to aggregate data for.</param>
		/// <param name="dependencies">The dependencies to use for getting data.</param>
		public WebsiteForumPostUserAggregationDataAdapter(Guid userId, IDataAdapterDependencies dependencies)
		{
			dependencies.ThrowOnNull("dependencies");

			var website = dependencies.GetWebsite();
			website.ThrowOnNull("dependencies", ResourceManager.GetString("Website_Reference_Retrieval_Exception"));
			website.AssertLogicalName("adx_website");

			Website = website;
			Dependencies = dependencies;
			UserId = userId;

		}

		protected Guid UserId { get; }

		protected IDataAdapterDependencies Dependencies { get; }

		protected EntityReference Website { get; private set; }

		protected Func<OrganizationServiceContext, IQueryable<Entity>> SelectThreadEntities { get; }

		public int SelectPostCount()
		{
			var serviceContext = Dependencies.GetServiceContext();
			var website = Dependencies.GetWebsite();

			return serviceContext.FetchAuthorForumPostCount(UserId, website.Id);
		}

		public IEnumerable<IForumPost> SelectPosts()
		{
			return SelectPosts(0);
		}

		public IEnumerable<IForumPost> SelectPosts(bool descending)
		{
			return SelectPosts(descending, 0);
		}

		public IEnumerable<IForumPost> SelectPosts(int startRowIndex, int maximumRows = -1)
		{
			return SelectPosts(false, startRowIndex, maximumRows);
		}

		public IEnumerable<IForumPost> SelectPosts(bool descending, int startRowIndex, int maximumRows = -1)
        {
            return new IForumPost[] { };
		}

		public IEnumerable<IForumPost> SelectPostsDescending()
		{
			return SelectPosts(true, 0);
		}

		public IEnumerable<IForumPost> SelectPostsDescending(int startRowIndex, int maximumRows = -1)
		{
			return SelectPosts(true, startRowIndex, maximumRows);
		}

		public IForumThread GetThread(Guid threadId)
		{
			var serviceContext = Dependencies.GetServiceContext();

			var entity = serviceContext.CreateQuery("adx_communityforumthread")
				.FirstOrDefault(ft => ft.GetAttributeValue<Guid>("adx_communityforumthreadid") == threadId);

			if (entity == null) return null;

			var securityProvider = Dependencies.GetSecurityProvider();

			if (!securityProvider.TryAssert(serviceContext, entity, CrmEntityRight.Read))
			{
                ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Thread={0}: Not Found", threadId));

				return null;
			}

			var viewEntity = new PortalViewEntity(serviceContext, entity, securityProvider, Dependencies.GetUrlProvider());
			var website = Dependencies.GetWebsite();
			var threadInfo = serviceContext.FetchForumThreadInfo(entity.Id, website.Id);
			var counterStrategy = Dependencies.GetCounterStrategy();

			var thread = new ForumThread(
				entity,
				viewEntity,
				threadInfo,
				() => counterStrategy.GetForumThreadPostCount(serviceContext, entity),
				Dependencies.GetUrlProvider().GetUrl(serviceContext, entity));

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Thread={0}: End", threadId));

			return thread;
		}

	}
}
