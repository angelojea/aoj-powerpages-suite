/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Visualizations
{
	using System;
	using Services.Query;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Definition of a CRM view (savedquery).
	/// </summary>
	public class CrmView
	{
		/// <summary>
		/// The query resulting from parsing the <see cref="FetchXml"/>.
		/// </summary>
		public Fetch Fetch { get; set; }

		/// <summary>
		/// The FetchXml of the view that defines the query.
		/// </summary>
		public string FetchXml { get; set; }

		/// <summary>
		/// Unique identifier of the view.
		/// </summary>
		public Guid Id { get; set; }
		
		/// <summary>
		/// The XML that defines the presentation of the view.
		/// </summary>
		public string LayoutXml { get; set; }

		/// <summary>
		/// The name of the view.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The logical name of the entity associated with the view.
		/// </summary>
		public string ReturnedTypeCode { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CrmView" /> class.
		/// </summary>
		/// <param name="view">An <see cref="Entity"/> record that must be of type savedquery.</param>
		public CrmView(Entity view)
		{
			if (view == null || view.LogicalName != "savedquery")
			{
				return;
			}

			FetchXml = view.GetAttributeValue<string>("fetchxml");

			Fetch = Fetch.Parse(FetchXml);

			Id = view.Id;

			LayoutXml = view.GetAttributeValue<string>("layoutxml");

			Name = view.GetAttributeValue<string>("name");

			ReturnedTypeCode = view.GetAttributeValue<string>("returnedtypecode");
		}
	}
}
