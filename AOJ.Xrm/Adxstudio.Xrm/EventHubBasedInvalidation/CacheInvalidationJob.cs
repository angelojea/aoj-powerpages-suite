/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;
	using AspNet;
	using Threading;

	/// <summary>
	/// A continuous job for invalidating cache based on Event Hub messages.
	/// </summary>
	public class CacheInvalidationJob : FluentSchedulerJob
	{
		/// <summary>
		/// The settings.
		/// </summary>
		public CacheInvalidationJobSettings Settings { get; private set; }

		/// <summary>
		/// The context.
		/// </summary>
		public CrmDbContext Context { get; }

		/// <summary>
		/// Current website id.
		/// </summary>
		public Guid WebsiteId { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheInvalidationJob" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="context">The context.</param>
		/// <param name="websiteId">Current website id.</param>
		public CacheInvalidationJob(CacheInvalidationJobSettings settings, CrmDbContext context, Guid websiteId)
		{
			Settings = settings;
			Context = context;
			WebsiteId = websiteId;
		}

		/// <summary>
		/// The body.
		/// </summary>
		/// <param name="id">The activity id.</param>
		protected override void ExecuteInternal(Guid id)
		{
			PortalCacheInvalidatorThread.Instance.Run(Context, WebsiteId);
		}
	}
}
