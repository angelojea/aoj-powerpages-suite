/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Events
{
	public class EventSchedule : IEventSchedule
	{
		public EventSchedule(Entity @event, Entity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			Entity = entity;

			Event = @event;

			IsAllDayEvent = entity.GetAttributeValue<bool?>("adx_alldayevent").GetValueOrDefault(false);

			Name = entity.GetAttributeValue<string>("adx_name");

			StartTime = entity.GetAttributeValue<DateTime>("adx_starttime");

			EndTime = entity.GetAttributeValue<DateTime>("adx_endtime");
		}

		public string Name { get; }

		public Entity Entity { get; }

		public DateTime StartTime { get; }

		public DateTime EndTime { get; }

		public Entity Event { get; }

		public bool IsAllDayEvent { get; }
	}
}
