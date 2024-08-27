/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;
	using System.Linq;
	using Threading;

	/// <summary>
	/// A continuous job for consuming the Service Bus topic.
	/// </summary>
	public class EventHubJob : FluentSchedulerJob
	{
		/// <summary>
		/// The mutex lock.
		/// </summary>
		private static readonly object JobLock = new object();

		/// <summary>
		/// The Event Hub manager.
		/// </summary>
		public EventHubJobManager Manager { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EventHubJob" /> class.
		/// </summary>
		/// <param name="manager">The Event Hub manager.</param>
		public EventHubJob(EventHubJobManager manager)
		{
			Manager = manager;
		}

		/// <summary>
		/// The body.
		/// </summary>
		/// <param name="id">The activity id.</param>
		protected override void ExecuteInternal(Guid id)
		{
			bool isSearchSubscription = Manager.Settings.SubscriptionType == EventHubSubscriptionType.SearchSubscription;
			if (Manager.SubscriptionClient != null)
			{
				lock (JobLock)
				{
					try
					{
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Subscription = '{0}' Topic = '{1}'", Manager.Subscription.Name, Manager.Subscription.TopicPath));

						// take N at a time
						var messages = Manager.SubscriptionClient
							.ReceiveBatch(Manager.Settings.ReceiveBatchMessageCount, Manager.Settings.ReceiveBatchServerWaitTime)
							.Where(message => message != null);

						foreach (var message in messages)
						{
							message.Complete();

							var crmSubscriptionMessage = CrmSubscriptionMessageFactory.Create(message);

							if (crmSubscriptionMessage != null)
							{
								NotificationUpdateManager.Instance.UpdateNotificationMessageTable(crmSubscriptionMessage, isSearchSubscription);
							}
						}
					}
					catch (Exception e)
					{
						Manager.Reset();
						ADXTrace.Instance.TraceInfo(TraceCategory.Application, e.ToString());
					}
				}
			}
		}
	}
}
