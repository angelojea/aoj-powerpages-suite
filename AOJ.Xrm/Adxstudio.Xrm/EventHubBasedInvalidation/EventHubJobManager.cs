/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;
	using System.Threading;
	using Microsoft.Crm.Sdk.Messages;
	using Microsoft.ServiceBus;
	using Microsoft.ServiceBus.Messaging;
	using AspNet;
	using Web;

	/// <summary>
	/// The Event Hub context.
	/// </summary>
	public class EventHubJobManager : IDisposable
	{
		/// <summary>
		/// The settings.
		/// </summary>
		public EventHubJobSettings Settings { get; }

		/// <summary>
		/// The organization Id field.
		/// </summary>
		private readonly Lazy<Guid> organizationId;

		/// <summary>
		/// The organization Id.
		/// </summary>
		public Guid OrganizationId
		{
			get
			{
				try
				{
					return organizationId.Value;
				}
				catch (Exception e)
				{
					WebEventSource.Log.GenericErrorException(e);

					return Guid.Empty;
				}
			}
		}

		/// <summary>
		/// The namespace manager field.
		/// </summary>
		private Lazy<NamespaceManager> namespaceManager;

		/// <summary>
		/// The namespace manager.
		/// </summary>
		public NamespaceManager NamespaceManager
		{
			get
			{
				try
				{
					return namespaceManager.Value;
				}
				catch (Exception e)
				{
					WebEventSource.Log.GenericErrorException(e);

					return null;
				}
			}
		}

		/// <summary>
		/// The topic exists field.
		/// </summary>
		private Lazy<bool> topicExists;

		/// <summary>
		/// The topic exists flag.
		/// </summary>
		public bool TopicExists
		{
			get
			{
				try
				{
					return topicExists.Value;
				}
				catch (Exception e)
				{
					WebEventSource.Log.GenericErrorException(e);

					return false;
				}
			}
		}

		/// <summary>
		/// The subscription field.
		/// </summary>
		private Lazy<SubscriptionDescription> subscription;

		/// <summary>
		/// The subscription.
		/// </summary>
		public SubscriptionDescription Subscription
		{
			get
			{
				try
				{
					return subscription.Value;
				}
				catch (Exception e)
				{
					WebEventSource.Log.GenericErrorException(e);

					return null;
				}
			}
		}

		/// <summary>
		/// The subscription client field.
		/// </summary>
		private Lazy<SubscriptionClient> subscriptionClient;

		/// <summary>
		/// The subscription client.
		/// </summary>
		public SubscriptionClient SubscriptionClient
		{
			get
			{
				try
				{
					return subscriptionClient.Value;
				}
				catch (Exception e)
				{
					WebEventSource.Log.GenericErrorException(e);

					return null;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventHubJobManager" /> class.
		/// </summary>
		/// <param name="context">The organization service context.</param>
		/// <param name="settings">The settings.</param>
		public EventHubJobManager(CrmDbContext context, EventHubJobSettings settings)
		{
			Settings = settings;

			organizationId = new Lazy<Guid>(() => GetOrganizationId(context), LazyThreadSafetyMode.PublicationOnly);
			Reset();
		}

		/// <summary>
		/// Resets the properties.
		/// </summary>
		public void Reset()
		{
			namespaceManager = new Lazy<NamespaceManager>(CreateNamespaceManager(Settings), LazyThreadSafetyMode.PublicationOnly);
			topicExists = new Lazy<bool>(() => GetTopicExists(Settings), LazyThreadSafetyMode.PublicationOnly);
			subscription = new Lazy<SubscriptionDescription>(() => GetSubscription(Settings), LazyThreadSafetyMode.PublicationOnly);
			subscriptionClient = new Lazy<SubscriptionClient>(() => GetSubscriptionClient(Settings), LazyThreadSafetyMode.PublicationOnly);
		}

		/// <summary>
		/// Retrieves the organization Id.
		/// </summary>
		/// <param name="context">The organization service context.</param>
		/// <returns>The organization Id.</returns>
		private static Guid GetOrganizationId(CrmDbContext context)
		{
			var response = context.Service.Execute(new WhoAmIRequest()) as WhoAmIResponse;
			return response.OrganizationId;
		}

		/// <summary>
		/// Initializes the namespace manager field.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <returns>The namespace manager.</returns>
		private static Func<NamespaceManager> CreateNamespaceManager(EventHubJobSettings settings)
		{
			return () => NamespaceManager.CreateFromConnectionString(settings.ConnectionString);
		}

		/// <summary>
		/// Initializes the topic exists field.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <returns>The topic exists flag.</returns>
		private bool GetTopicExists(EventHubJobSettings settings)
		{
			var topicPath = settings.Subscription.TopicPath;
			var exists = NamespaceManager.TopicExists(topicPath);

			if (!exists)
			{
				throw new InvalidOperationException(string.Format("The topic '{0}' does not exist.", topicPath));
			}

			return true;
		}

		/// <summary>
		/// Initializes the subscription field.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <returns>The subscription.</returns>
		private SubscriptionDescription GetSubscription(EventHubJobSettings settings)
		{
			var topicPath = settings.Subscription.TopicPath;
			var subscriptionName = settings.Subscription.Name;

			if (!TopicExists)
			{
				throw new InvalidOperationException(string.Format("The topic '{0}' does not exist.", topicPath));
			}

			var subscriptionExists = NamespaceManager.SubscriptionExists(topicPath, subscriptionName);

			if (!subscriptionExists)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Creating Subscription '{0}' for topic '{1}'.", subscriptionName, topicPath));

				return CreateSubscription(settings.Subscription);
			}

			if (settings.RecreateSubscription)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Deleting Subscription '{0}' for topic '{1}'.", subscriptionName, topicPath));

				NamespaceManager.DeleteSubscription(topicPath, subscriptionName);

				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Creating Subscription '{0}' for topic '{1}'.", subscriptionName, topicPath));

				return CreateSubscription(settings.Subscription);
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Using Subscription '{0}' for topic '{1}'.", subscriptionName, topicPath));

			return NamespaceManager.GetSubscription(topicPath, subscriptionName);
		}

		/// <summary>
		/// Creates the subscription.
		/// </summary>
		/// <param name="description">The subscription description.</param>
		/// <returns>The subscription.</returns>
		private SubscriptionDescription CreateSubscription(SubscriptionDescription description)
		{
            try
            {
                return NamespaceManager.CreateSubscription(description, CreateFilter());
            }
            catch (MessagingEntityAlreadyExistsException e)
            {
                WebEventSource.Log.GenericWarningException(e, string.Format("MessagingEntityAlreadyExistsException: Using Subscription '{0}' for topic '{1}'.", description.Name, description.TopicPath));
                return NamespaceManager.GetSubscription(description.TopicPath, description.Name);
            }
        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <returns>The filter.</returns>
        private Filter CreateFilter()
		{
			return new SqlFilter(string.Format("OrganizationId = '{0}'", OrganizationId));
		}

		/// <summary>
		/// Creates the subscription client.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <returns>The subscription client.</returns>
		private SubscriptionClient GetSubscriptionClient(EventHubJobSettings settings)
		{
			if (Subscription != null)
			{
				var topicPath = Subscription.TopicPath;
				var subscriptionName = Subscription.Name;
				return SubscriptionClient.CreateFromConnectionString(settings.ConnectionString, topicPath, subscriptionName);
			}

			throw new InvalidOperationException(string.Format("The subscription '{0}' for topic '{1}' is not ready.", settings.Subscription.Name, settings.Subscription.TopicPath));
		}

		/// <summary>
		/// Internal use only.
		/// </summary>
		void IDisposable.Dispose()
		{
			// IDisposable is only required to satisfy the IdentityFactoryOptions<T> constraint
		}
	}
}
