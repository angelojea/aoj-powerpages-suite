/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Cases
{
	internal class CaseResolution : ICaseResolution
	{
		public CaseResolution(string description, DateTime createdOn, string subject = null)
		{
			CreatedOn = createdOn;
			Description = description;
			Subject = subject;
		}

		public DateTime CreatedOn { get; }

		public string Description { get; }

		public string Subject { get; }
	}
}
