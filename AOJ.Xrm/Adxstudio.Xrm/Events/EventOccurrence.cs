/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	/// <summary>
	/// Represents a single occurrence of an event, as defined by both an adx_event and adx_eventschedule, and
	/// a specific start time.
	/// </summary>
	public class EventOccurrence : IEventOccurrence
	{
		public EventOccurrence(Entity @event, Entity eventSchedule, DateTime start, string url, string location)
		{
			if (@event == null) throw new ArgumentNullException("event");
			if (eventSchedule == null) throw new ArgumentNullException("eventSchedule");

			Microsoft.Xrm.Client.EntityExtensions.AssertEntityName(@event, "adx_event");
			Microsoft.Xrm.Client.EntityExtensions.AssertEntityName(eventSchedule, "adx_eventschedule");

			Event = @event;
			EventSchedule = eventSchedule;
			Start = start;
			Url = url;
			Location = location;

			End = GetEndTime(eventSchedule, start) ?? start;
			IsAllDayEvent = eventSchedule.GetAttributeValue<bool?>("adx_alldayevent").GetValueOrDefault(false);
		}

		public DateTime End { get; }

		public Entity Event { get; }

		public Entity EventSchedule { get; }

		public bool IsAllDayEvent { get; }

		public string Location { get; }

		public DateTime Start { get; }

		public string Url { get; }

		private static DateTime? GetEndTime(Entity eventSchedule, DateTime date)
		{
			var startTime = eventSchedule.GetAttributeValue<DateTime?>("adx_starttime");
			var endTime = eventSchedule.GetAttributeValue<DateTime?>("adx_endtime");

			return startTime.HasValue && endTime.HasValue && (endTime > startTime)
				? new DateTime?(date.Add(endTime.Value - startTime.Value))
				: null;
		}
	}
}
