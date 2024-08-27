/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Linq;
	using System.Runtime.Serialization;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Newtonsoft.Json;

	/// <summary>
	/// A customized <see cref="RetrieveSingleResponse"/> wrapper indicating a request for a single entity.
	/// </summary>
	[Serializable]
	[DataContract(Name = "RetrieveMultipleResponse", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
	[KnownType(typeof(RetrieveSingleResponse))]
	internal sealed class RetrieveSingleResponse : OrganizationResponse
	{
		/// <summary>
		/// The original request.
		/// </summary>
		[DataMember]
		public RetrieveSingleRequest Request { get; private set; }

		/// <summary>
		/// The wrapped response.
		/// </summary>
		[DataMember]
		public RetrieveMultipleResponse Response { get; private set; }

		/// <summary>
		/// The retrieved entity.
		/// </summary>
		public Entity Entity
		{
			get
			{
				if (Request.EnforceSingle)
				{
					return Response.EntityCollection.Entities.Count > 1
						? null
						: Response.EntityCollection.Entities.SingleOrDefault();
				}

				return Request.EnforceFirst 
					? Response.EntityCollection.Entities.First() 
					: Response.EntityCollection.Entities.FirstOrDefault();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RetrieveSingleResponse" /> class.
		/// </summary>
		/// <param name="request">The original request.</param>
		/// <param name="response">The wrapped response.</param>
		public RetrieveSingleResponse(RetrieveSingleRequest request, RetrieveMultipleResponse response)
		{
			Request = request;
			Response = response;
			ExtensionData = response.ExtensionData;
			Results = response.Results;
			ResponseName = response.ResponseName;
		}

		/// <summary>
		/// Prevents a default instance of the <see cref="RetrieveSingleResponse" /> class from being created.
		/// </summary>
		/// <remarks>
		/// Required for json deserialization.
		/// </remarks>
		[JsonConstructor]
		private RetrieveSingleResponse()
		{
		}
	}
}
