/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;

	using Query;

	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;

	/// <summary>
	/// Telemetry properties describing a cache item.
	/// </summary>
	internal class CacheItemTelemetry
	{
		/// <summary>
		/// Startup flag.
		/// </summary>
		public static bool StartupFlag = true;

		/// <summary>
		/// The request.
		/// </summary>
		public OrganizationRequest Request { get; }

		/// <summary>
		/// The caller.
		/// </summary>
		public Caller Caller { get; private set; }

		/// <summary>
		/// The request duration.
		/// </summary>
		public TimeSpan Duration { get; set; }

		/// <summary>
		/// The count of cache item hits.
		/// </summary>
		public ulong AccessCount { get; private set; }

		/// <summary>
		/// The count of stale cache item hits.
		/// </summary>
		public ulong StaleAccessCount { get; private set; }

		/// <summary>
		/// The time of the last access.
		/// </summary>
		public DateTimeOffset LastAccessedOn { get; private set; }

		/// <summary>
		/// Gets or sets the pre loaded.
		/// </summary>
		public bool IsStartup { get; set; }

		/// <summary>
		/// The attributes being requested
		/// </summary>
		public List<string> Attributes { get; set; }

		/// <summary>
		/// True if all columns are requested
		/// </summary>
		public bool IsAllColumns { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheItemTelemetry" /> class.
		/// </summary>
		public CacheItemTelemetry()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheItemTelemetry" /> class.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="caller">The caller.</param>
		public CacheItemTelemetry(OrganizationRequest request, Caller caller)
		{
			Request = request;
			Caller = caller;
			IsStartup = StartupFlag;
			LastAccessedOn = DateTimeOffset.UtcNow;
			SetColumnsTelemetry();
		}

		/// <summary>
		/// Increment the access counter.
		/// </summary>
		public void IncrementAccessCount()
		{
			++AccessCount;
			LastAccessedOn = DateTimeOffset.UtcNow;
		}

		/// <summary>
		/// Increment the access counter.
		/// </summary>
		public void IncrementStaleAccessCount()
		{
			++StaleAccessCount;
			LastAccessedOn = DateTimeOffset.UtcNow;
		}

		/// <summary>
		/// Get the attributes for the columns of the request
		/// </summary>
		private void SetColumnsTelemetry()
		{
			Attributes = new List<string>();

			if (Request == null)
			{
				return;
			}

			var fmr = Request as FetchMultipleRequest;
			if (fmr != null)
			{
				if (fmr.Fetch.Entity.Attributes != null && fmr.Fetch.Entity.Attributes.Any())
				{
					Attributes.AddRange(fmr.Fetch.Entity.Attributes.Select(attr => attr.Name));
					IsAllColumns = Equals(fmr.Fetch.Entity.Attributes, FetchAttribute.All);
				}
			}

			var rr = Request as RetrieveRequest;
			if (rr != null)
			{
				if (rr.ColumnSet.Columns != null && rr.ColumnSet.Columns.Any())
				{
					Attributes.AddRange(rr.ColumnSet.Columns);
				}
				IsAllColumns = rr.ColumnSet.AllColumns;
			}

			var rmr = Request as RetrieveMultipleRequest;
			if (rmr == null)
			{
				var rsr = Request as RetrieveSingleRequest;
				if (rsr != null)
				{
					rmr = rsr.Request;
				}
			}

			if (rmr != null)
			{
				var query = rmr.Query as QueryExpression;
				if (query != null)
				{
					if (query.ColumnSet.Columns != null && query.ColumnSet.Columns.Any())
					{
						Attributes.AddRange(query.ColumnSet.Columns);
					}
					IsAllColumns = query.ColumnSet.AllColumns;
				}

				var fe = rmr.Query as FetchExpression;
				if (fe != null)
				{
					var fetch = Fetch.Parse(fe.Query);
					if (fetch.Entity.Attributes != null && fetch.Entity.Attributes.Any())
					{
						Attributes.AddRange(fetch.Entity.Attributes.Select(attr => attr.Name));
						IsAllColumns = Equals(fetch.Entity.Attributes, FetchAttribute.All);
					}
				}
			}
		}
	}
}
