/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// Class to handle the the storage of messages and timestamps
	/// </summary>
	public class NotificationUpdateManager
	{
		private static readonly string EntityKey = @"{0}:{1}";
		private static readonly string FromSearchSubscription = "FromSearchSubscription";
		private static readonly string FromCacheSubscription = "FromCacheSubscription";

		private static volatile NotificationUpdateManager instance;
		private static readonly object syncRoot = new object();
		private static readonly object mutexLock = new object();
		private static readonly object metadataFlagLock = new object();

		// Construct the dictionary with the desired concurrencyLevel and initialCapacity(Recommended Prime Number)
		private ConcurrentDictionary<string, bool> portalUsedEntities = new ConcurrentDictionary<string, bool>(Environment.ProcessorCount * 2, 10009);

		#region unsafe thread accessors

		/// <summary>
		/// Retrieves the version timestamp from the Timestamp table in the cache for the given entity name
		/// If you don't know last updated timestamp, use empty string
		/// </summary>
		/// <param name="entityName">Entity for which to check the timestamp cache table</param>
		/// <param name="isSearchIndexInvalidation">Whether to retrieve timestamps for search index invlaidation or cache invalidation</param>
		/// <returns>String of the timestamp representation</returns>
		private string GetEntityVersionNumber(string entityName, bool isSearchIndexInvalidation = false)
		{
			var timeStampTable = isSearchIndexInvalidation ? TimeStampTableForSearchIndex : TimeStampTableForCache;
			string timestamp;
			if (!timeStampTable.TryGetValue(entityName, out timestamp))
			{
				timestamp = string.Empty;
			}

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Timestamp for entity {0} {1}", entityName, timestamp));

			return timestamp;
		}

		/// <summary>
		/// Gets the Dirty table from the Cache
		/// Should be called within a thread safe location
		/// </summary>
		private ConcurrentDictionary<string, EntityInvalidationMessageAndType> DirtyTable { get; } = new ConcurrentDictionary<string, EntityInvalidationMessageAndType>(Environment.ProcessorCount * 2, 10009);

		/// <summary>
		/// Gets the Processing table from the Cache
		/// Should be called within a thread safe location
		/// </summary>
		private ConcurrentDictionary<string, EntityRecordMessage> ProcessingTable { get; } = new ConcurrentDictionary<string, EntityRecordMessage>(Environment.ProcessorCount * 2, 10009);

		/// <summary>
		/// Gets the TimeStamp table from the Cache
		/// Should be called within a thread safe location
		/// </summary>
		private ConcurrentDictionary<string, string> TimeStampTableForCache { get; } = new ConcurrentDictionary<string, string>(Environment.ProcessorCount * 2, 10009);

		/// <summary>
		/// Gets the TimeStamp table for SearchIndex from the Cache
		/// Should be called within a thread safe location
		/// </summary>
		private ConcurrentDictionary<string, string> TimeStampTableForSearchIndex { get; } = new ConcurrentDictionary<string, string>(Environment.ProcessorCount * 2, 10009);

		/// <summary>
		/// Gets the Metadata Dirty entry from the Cache
		/// Should be called from a thread safe location
		/// </summary>
		private bool MetadataDirtyEntry { get; set; }

		#endregion unsafe thread accessors

		#region thread safe accessors

		/// <summary>
		/// Gets the copy of the TimeStamp table from the Cache
		/// </summary>
		public Dictionary<string, string> TimeStampsForSearchIndex
		{
			get
			{
				return new Dictionary<string, string>(TimeStampTableForSearchIndex.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
			}
		}

		/// <summary>
		/// Gets the copy of the TimeStamp table from the Cache
		/// </summary>
		public Dictionary<string, string> TimeStampsForCache
		{
			get
			{
				return new Dictionary<string, string>(TimeStampTableForCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
			}
		}

		/// <summary>
		/// Thread safe accessor to the Metadata Dirty Cache Entry
		/// </summary>
		internal bool MetadataDirty
		{
			get
			{
				bool _isMetadataDirty;
				//reading value under read lock
				lock (metadataFlagLock)
				{
					_isMetadataDirty = MetadataDirtyEntry;
				}
				return _isMetadataDirty;
			}
			set
			{
				lock (metadataFlagLock)
				{
					MetadataDirtyEntry = value;
				}
			}
		}
		#endregion thread safe accessors

		/// <summary>
		/// private constructor
		/// </summary>
		private NotificationUpdateManager()
		{
		}

		/// <summary>
		/// Gets the instance of this class
		/// </summary>
		public static NotificationUpdateManager Instance
		{
			get
			{
				if (instance == null)
				{
					lock (syncRoot)
					{
						if (instance == null)
							instance = new NotificationUpdateManager();
					}
				}

				return instance;
			}
		}

		/// <summary>
		/// Pushes CrmSubscriptionMessages into the cache
		/// </summary>
		/// <param name="crmSubscriptionMessage">Message to push into the cache</param>
		/// <param name="isSearchIndexInvalidationMessage">Whther to update message from search subscription</param>
		// TODO perf might require us to keep a table in memory for lookup and access cache only to insert
		internal void UpdateNotificationMessageTable(ICrmSubscriptionMessage crmSubscriptionMessage, bool isSearchIndexInvalidationMessage = false)
		{
			EntityRecordMessage message = crmSubscriptionMessage as EntityRecordMessage;
			portalUsedEntities = WebAppConfigurationProvider.PortalUsedEntities;

			//Filter's out Entities Not Used in Portal
			if (message != null && portalUsedEntities.ContainsKey(message.EntityName))
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Pushing MessageId: {0} CRMSubscriptionMessage with entity: {1} and message name: {2} into the Cache.", message.MessageId, message.EntityName, message.MessageName));
				var entityKey = string.Format(EntityKey, message.EntityName,
						isSearchIndexInvalidationMessage ? FromSearchSubscription : FromCacheSubscription);
				if (!DirtyTable.TryAdd(entityKey, new EntityInvalidationMessageAndType(message, isSearchIndexInvalidationMessage)))
				{
					EntityInvalidationMessageAndType val = null;
					var success = DirtyTable.TryGetValue(message.EntityName, out val);
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Dirty Table already contains entity: {0} and message name: {1} in the Cache with MessageId: {2}. Dropping MessageId: {3} ", message.EntityName, message.MessageName, success ? val.Message.MessageId : Guid.Empty, message.MessageId));
				}
			}

		}

		/// <summary>
		/// Prepares the cache for a query.
		/// Copies the values from the Dirty table to the Processing table.
		/// </summary>
		/// <param name="isSearchIndexInvalidation">Is search index invalidation</param>
		/// <returns> True if DirtyTable has messages. </returns>
		internal bool PrepDirtyEntitiesForProcessing(bool isSearchIndexInvalidation)
		{
			//This Lock is For Sequential Execution
			lock (mutexLock)
			{
				// Copy Dirty table into Processing table
				var dirtyTable = DirtyTable.Where(message => message.Value.IsSearchIndexInvalidationMessage == isSearchIndexInvalidation).ToList();
				if (!dirtyTable.Any())
				{
					return false;
				}
				foreach (var entity in dirtyTable)
				{
					EntityInvalidationMessageAndType record;
					ProcessingTable.TryAdd(GetEntityNameFromKey(entity.Key), entity.Value.Message);
					DirtyTable.TryRemove(entity.Key, out record);
				}
				return true;
			}
		}

		/// <summary>
		/// Creates dictionary with entity name and corresponding timestamp.
		/// </summary>
		/// <param name="isSearchIndexInvalidation">Is search index invalidation</param>
		/// <returns>Dictionary of entities in the following format:
		/// 	[account, timestamp]
		/// 	[contact, timestamp]
		/// </returns>
		internal Dictionary<string, string> GetEntitiesWithTimeStamps(bool isSearchIndexInvalidation)
		{
			// Return a dictionary of type [entity - timestamp]
			Dictionary<string, string> preppedEntityList = ProcessingTable.Keys
				.Select(key => new KeyValuePair<string, string>(key, GetEntityVersionNumber(key, isSearchIndexInvalidation)))
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			return preppedEntityList;
		}

		/// <summary>
		/// Prepares a copy of the processing table
		/// </summary>
		/// <returns>Dictionary of entities in the following format:
		/// 	[account, entityRecordMessage]
		/// 	[contact, entityRecordMessage]
		/// </returns>
		internal Dictionary<string, EntityRecordMessage> ProcessingEntitiesTable()
		{
			return new Dictionary<string, EntityRecordMessage>(ProcessingTable.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
		}

		/// <summary>
		/// Clears the processing table and updates the timestamps for the updated entities
		/// Replaces the non-updated entities back into the Dirty table
		/// </summary>
		/// <param name="updatedEntities">Dictionary of entities and their updated timestamps</param>
		/// <param name="entitiesWithSuccessfulInvalidation">List of entities for which invlaidation was successful.</param>
		/// <param name="isSearchIndexInvalidation">Is search index invalidation</param>
		internal void CompleteEntityProcessing(Dictionary<string, string> updatedEntities, List<string> entitiesWithSuccessfulInvalidation, bool isSearchIndexInvalidation = false)
		{
			var timeStampTable = isSearchIndexInvalidation ? TimeStampTableForSearchIndex : TimeStampTableForCache;
			//This Lock is For Sequential Execution
			lock (mutexLock)
			{
				foreach (KeyValuePair<string, string> updatedEntity in updatedEntities)
				{
					// update the timestamp
					if (entitiesWithSuccessfulInvalidation.Contains(updatedEntity.Key))
					{
						timeStampTable.AddOrUpdate(updatedEntity.Key, updatedEntity.Value, (entityname, timestamp) => updatedEntity.Value);
					}
					// remove from the processing table
					EntityRecordMessage record;
					ProcessingTable.TryRemove(updatedEntity.Key, out record);
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Updated Entity: {0} at Timestamp: {1} and cleared the MessageId:{2} from the Processing table.", updatedEntity.Key, updatedEntity.Value, record.MessageId));
				}

				// If an entity fails to get updated we should add it back to the dirty table
				foreach (var keyValuePair in ProcessingTable)
				{
					ADXTrace.Instance.TraceWarning(TraceCategory.Application, string.Format("Failed to update the Entity: {0}. So adding MessageId: {1} back to the Dirty table.", keyValuePair.Key, keyValuePair.Value.MessageId));
					// Any items left in processing table add back into Dirty table
					var entityKey = string.Format(EntityKey, keyValuePair.Key,
						isSearchIndexInvalidation ? FromSearchSubscription : FromCacheSubscription);
					DirtyTable.TryAdd(entityKey, new EntityInvalidationMessageAndType(keyValuePair.Value, isSearchIndexInvalidation));
				}

				// Clear the Processing table
				ProcessingTable.Clear();
			}
		}

		/// <summary>
		/// Returns entity name from the given key in DirtyTable.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private static string GetEntityNameFromKey(string key)
		{
			return key.Split(':').FirstOrDefault();
		}
	}
}
