/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;
	using System.Configuration;
	using Microsoft.ServiceBus.Messaging;
	using Core.Flighting;

	/// <summary>
	/// Settings for the <see cref="EventHubJob"/>.
	/// </summary>
	public class EventHubJobSettings
	{
		/// <summary>
		/// The Service Bus connection string.
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// The interval between running the job in seconds.
		/// </summary>
		public int JobInterval { get; set; }

		/// <summary>
		/// Allows overlapping jobs to run in parallel.
		/// </summary>
		public bool Reentrant { get; set; }

		/// <summary>
		/// The subscription settings. For search we create one instance per webapp and for cache we create one subscription per instance of the webapp.
		/// </summary>
		public SubscriptionDescription Subscription { get; set; }

		/// <summary>
		/// The flag indicating that the subscription should be re-created at application startup."
		/// </summary>
		public bool RecreateSubscription { get; set; }

		/// <summary>
		/// The message count of the Service Bus batch request.
		/// </summary>
		public int ReceiveBatchMessageCount { get; set; }

		/// <summary>
		/// The time span the server waits for processing messages.
		/// </summary>
		public TimeSpan ReceiveBatchServerWaitTime { get; set; }

		/// <summary>
		/// Indicates that the Service Bus settings are configured.
		/// </summary>
		public bool IsEnabled
		{
			get
			{
				return FeatureCheckHelper.IsFeatureEnabled(FeatureNames.EventHubCacheInvalidation)
					&& Subscription != null
					&& !string.IsNullOrWhiteSpace(ConnectionString)
					&& !string.IsNullOrWhiteSpace(Subscription.Name)
					&& !string.IsNullOrWhiteSpace(Subscription.TopicPath);
			}
		}

		/// <summary>
		/// Type of the subscription.
		/// </summary>
		public EventHubSubscriptionType SubscriptionType { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EventHubJobSettings" /> class.
		/// </summary>
		/// <param name="subscriptionName">Name of the subscription.</param>
		/// <param name="subscriptionType">Type of subscription.</param>
		public EventHubJobSettings(string subscriptionName, EventHubSubscriptionType subscriptionType)
		{
			SubscriptionType = subscriptionType;
			subscriptionName = subscriptionName == null ? null : subscriptionName.Substring(0, Math.Min(subscriptionName.Length, 50));
			var topicPath = "adxportaltopic";
			var connectionStringCollection = ConfigurationManager.ConnectionStrings["ADXPortalSBConnection"];
			var connectionString = connectionStringCollection != null ? connectionStringCollection.ConnectionString : null;

			RecreateSubscription = true;
			JobInterval = 2;
			ReceiveBatchMessageCount = 100;
			ReceiveBatchServerWaitTime = new TimeSpan(0, 0, 0);
			ConnectionString = connectionString;

			if (!string.IsNullOrWhiteSpace(topicPath) && !string.IsNullOrWhiteSpace(subscriptionName))
			{
				Subscription = new SubscriptionDescription(topicPath, subscriptionName)
				{
					EnableDeadLetteringOnFilterEvaluationExceptions = true,
					EnableDeadLetteringOnMessageExpiration = false,
					EnableBatchedOperations = true,
					MaxDeliveryCount = 4000, // for debugging

					// Check if we need this
					////LockDuration = TimeSpan.FromMilliseconds(500),
				};
			}
		}
	}

	/// <summary>
	/// All different types of subscriptions.
	/// </summary>
	public enum EventHubSubscriptionType
	{
		CacheSubscription,
		SearchSubscription
	}
}
