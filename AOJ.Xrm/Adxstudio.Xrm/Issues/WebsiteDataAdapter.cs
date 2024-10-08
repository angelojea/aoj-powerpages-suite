/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Collections.Generic;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Issues
{
	/// <summary>
	/// Provides methods to get data for an Adxstudio Portals Website for the Adxstudio.Xrm.Issues namespace.
	/// </summary>
	/// <remarks>Issue Forums are returned alphabetically by their title.</remarks>
	public class WebsiteDataAdapter : IWebsiteDataAdapter
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="dependencies">The dependencies to use for getting data.</param>
		public WebsiteDataAdapter(IDataAdapterDependencies dependencies)
		{
			dependencies.ThrowOnNull("dependencies");

			var website = dependencies.GetWebsite();
			website.ThrowOnNull("dependencies", ResourceManager.GetString("Website_Reference_Retrieval_Exception"));
			website.AssertLogicalName("adx_website");

			Website = website;
			Dependencies = dependencies;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="portalName">The configured name of the portal to get and set data for.</param>
		public WebsiteDataAdapter(string portalName = null) : this(new PortalConfigurationDataAdapterDependencies(portalName)) { }

		protected IDataAdapterDependencies Dependencies { get; }

		protected EntityReference Website { get; }

		/// <summary>
		/// Returns issue forums that have been created in the website this adapter applies to.
		/// </summary>
		/// <param name="startRowIndex">The row index of the first issue forum to be returned.</param>
		/// <param name="maximumRows">The maximum number of issue forums to return.</param>
		public virtual IEnumerable<IIssueForum> SelectIssueForums(int startRowIndex = 0, int maximumRows = -1)
		{
			if (startRowIndex < 0)
			{
                throw new ArgumentException("Value must be a positive integer.", "startRowIndex");
			}

			if (maximumRows == 0)
			{
				return new IIssueForum[] { };
			}

			var serviceContext = Dependencies.GetServiceContext();
			var security = Dependencies.GetSecurityProvider();

			var query = serviceContext.CreateQuery("adx_issueforum")
				.Where(issueForum => issueForum.GetAttributeValue<EntityReference>("adx_websiteid") == Website
					&& issueForum.GetAttributeValue<OptionSetValue>("statecode") != null && issueForum.GetAttributeValue<OptionSetValue>("statecode").Value == 0);

			query = query.OrderBy(issueForum => issueForum.GetAttributeValue<string>("adx_name"));

			if (maximumRows < 0)
			{
				var entities = query.ToArray()
					.Where(e => security.TryAssert(serviceContext, e, CrmEntityRight.Read))
					.Skip(startRowIndex);

				return new IssueForumFactory(Dependencies.GetHttpContext()).Create(entities);
			}

			var pagedQuery = query;

			var paginator = new PostFilterPaginator<Entity>(
				(offset, limit) => pagedQuery.Skip(offset).Take(limit).ToArray(),
				e => security.TryAssert(serviceContext, e, CrmEntityRight.Read),
				2);

			return new IssueForumFactory(Dependencies.GetHttpContext()).Create(paginator.Select(startRowIndex, maximumRows));
		}

		/// <summary>
		/// Returns the number of issue forums that have been created in the website this adapter applies to.
		/// </summary>
		public virtual int SelectIssueForumCount()
		{
			var serviceContext = Dependencies.GetServiceContext();

			return serviceContext.FetchCount("adx_issueforum", "adx_issueforumid", addCondition =>
			{
				addCondition("adx_websiteid", "eq", Website.Id.ToString());
				addCondition("statecode", "eq", "0");
			});
		}
	}
}
