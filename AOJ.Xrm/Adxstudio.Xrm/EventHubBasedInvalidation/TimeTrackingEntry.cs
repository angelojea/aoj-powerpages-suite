/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// Container class for the Time Tracking Telemetry entry
	/// </summary>
	internal sealed class TimeTrackingEntry
	{
		public TimeTrackingEntry(string entity, DateTime pushed, DateTime modified, DateTime received)
		{
			entity.ThrowOnNullOrWhitespace("entity");
			pushed.ThrowOnNull("pushed");
			modified.ThrowOnNull("modified");
			received.ThrowOnNull("received");

			EntityLogicalName = entity;
			PushedToCache = pushed;
			ModifiedInCrm = modified;
			ReceivedInPortal = received;
		}

		public string EntityLogicalName { get; private set; }

		public DateTime PushedToCache { get; }

		public DateTime ModifiedInCrm { get; }

		public DateTime ReceivedInPortal { get; }

		/// <summary>
		/// Overall Delta from the change in CRM until the cache is updated in the portal
		/// </summary>
		public TimeSpan OverallDelta
		{
			get { return PushedToCache - ModifiedInCrm; }
		}

		/// <summary>
		/// Delta from the change in CRM until the portal was notified
		/// </summary>
		public TimeSpan AzureProcessingDelta
		{
			get { return ReceivedInPortal - ModifiedInCrm; }
		}

		/// <summary>
		/// Delta from the change in CRM until the portal was notified
		/// </summary>
		public TimeSpan InvalidationDelta
		{
			get { return PushedToCache - ReceivedInPortal; }
		}
	}
}
