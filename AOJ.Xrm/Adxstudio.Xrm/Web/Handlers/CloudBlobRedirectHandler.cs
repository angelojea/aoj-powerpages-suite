/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Handlers
{
	internal class CloudBlobRedirectHandler
	{
		private readonly string _blobAddress;
		private readonly bool _enableTracking;
		private readonly EntityReference _entity;

		public CloudBlobRedirectHandler(Entity entity, string portalName = null)
		{
			if (entity == null) throw new ArgumentNullException("entity");

			PortalName = portalName;

			_blobAddress = entity.GetAttributeValue<string>("adx_cloudblobaddress");
			_entity = entity.ToEntityReference();
			_enableTracking = entity.GetAttributeValue<bool?>("adx_enabletracking").GetValueOrDefault();
		}

		protected string PortalName { get; }

		public static bool IsCloudBlob(Entity entity)
		{
			return entity != null && !string.IsNullOrEmpty(entity.GetAttributeValue<string>("adx_cloudblobaddress"));
		}

		public static bool TryGetCloudBlobHandler(Entity entity, out IHttpHandler handler, string portalName = null)
		{
			handler = null;

			return false;
		}
	}
}
