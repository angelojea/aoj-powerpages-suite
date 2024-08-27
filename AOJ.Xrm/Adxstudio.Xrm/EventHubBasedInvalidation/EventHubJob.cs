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
		/// The body.
		/// </summary>
		/// <param name="id">The activity id.</param>
		protected override void ExecuteInternal(Guid id)
		{
		}
	}
}
