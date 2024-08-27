/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;
	using Newtonsoft.Json;


	public abstract class CrmSubscriptionMessage : ICrmSubscriptionMessage
	{
		private MessageType messageType;

		public DateTime Received { get; }


		public DateTime EnqueuedTopicTimeUtc { get; private set; }


		public DateTime DequeuedTopicTimeUtc { get; private set; }


		public DateTime EnqueuedEventhubTimeUtc { get; private set; }


		public DateTime DequeuedEventhubTimeUtc { get; private set; }


		public Guid OrganizationId { get; set; }


		public Guid MessageId { get; set; }


		public string MessageName { get; set; }


		protected virtual bool ValidMessage
		{
			get
			{
				return OrganizationId != default(Guid)
						&& !string.IsNullOrEmpty(MessageName);
			}
		}


		public MessageType MessageType
		{
			get
			{
				if (messageType == MessageType.Unknown)
					if (!Enum.TryParse(MessageName, true, out messageType))
						messageType = MessageType.Other;

				return messageType;
			}
		}
	}
}
