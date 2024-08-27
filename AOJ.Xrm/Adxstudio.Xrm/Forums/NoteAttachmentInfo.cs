/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Notes;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	public class NoteAttachmentInfo : IForumPostAttachmentInfo
	{
		public NoteAttachmentInfo(EntityReference annotation, string name, string contentType, int size, Guid? websiteId = null)
		{
			if (annotation == null) throw new ArgumentNullException("annotation");

			Name = name;
			ContentType = contentType;
			Path = (new Entity(annotation.LogicalName) { Id = annotation.Id }).GetFileAttachmentPath(websiteId);
			Size = new FileSize(Convert.ToUInt64(size < 0 ? 0 : size));
		}

		public string Name { get; }

		public string ContentType { get; }

		public ApplicationPath Path { get; }

		public FileSize Size { get; }
	}
}
