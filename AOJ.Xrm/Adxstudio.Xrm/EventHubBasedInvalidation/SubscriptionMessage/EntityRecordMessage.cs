/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	using System;

	/// <summary>
	/// CrmSubscriptionMessage wrapping a BrokeredMessage indicating a CRM Entity Record change
	/// </summary>
	public class EntityRecordMessage : CrmSubscriptionMessage
	{
		/// <summary>
		/// ObjectId
		/// </summary>
		public Guid ObjectId { get; set; }

		/// <summary>
		/// EntityName
		/// 	Entity logical name for the entity
		/// </summary>
		public string EntityName { get; set; }

		/// <summary>
		/// ObjectType
		/// 	ObjectTypeCode of the entity
		/// </summary>
		public int ObjectType { get; set; }

		/// <summary>
		/// Returns true if the properties are all present, otherwise false
		/// </summary>
		protected override bool ValidMessage
		{
			get
			{
				return ObjectId != default(Guid)
						&& ObjectType != default(int)
						&& !string.IsNullOrEmpty(EntityName)
						&& base.ValidMessage;
			}
		}
	}
}
