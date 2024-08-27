/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	/// <summary>
	/// CrmSubscriptionMessage wrapping a BrokeredMessage indicating a CRM Associate Disassociate record change
	/// </summary>
	public sealed class AssociateDisassociateMessage : EntityRecordMessage
	{
		/// <summary>
		/// Relationship Name for the relationship
		/// </summary>
		public string RelationshipName { get; set; }

		/// <summary>
		/// Related Entity Name
		/// 	Entity logical name for the associated entity
		/// </summary>
		public string RelatedEntity1Name { get; set; }

		/// <summary>
		/// Related Entity Name
		/// 	Entity logical name for the associated entity
		/// </summary>
		public string RelatedEntity2Name { get; set; }


		/// <summary>
		/// Returns true if the properties are all present, otherwise false
		/// </summary>
		protected override bool ValidMessage
		{
			get
			{
				return !string.IsNullOrEmpty(RelationshipName)
					&& !string.IsNullOrEmpty(RelatedEntity1Name)
					&& !string.IsNullOrEmpty(RelatedEntity2Name)
					&& base.ValidMessage;
			}
		}
	}
}
