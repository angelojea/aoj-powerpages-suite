/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Forums
{
	internal class ForumPostInfo : IForumPostInfo
	{
		public ForumPostInfo(EntityReference entityReference, IForumAuthor author, DateTime postedOn, 
			IEnumerable<IForumPostAttachmentInfo> attachmentInfo = null, EntityReference forumThread = null)
		{
			if (entityReference == null) throw new ArgumentNullException("entityReference");
			//if (author == null) throw new ArgumentNullException("author");

			EntityReference = entityReference;
			Author = author;
			PostedOn = postedOn;
			AttachmentInfo = attachmentInfo == null ? new IForumPostAttachmentInfo[] { } : attachmentInfo.ToArray();
			ThreadEntity = forumThread;
		}

		public IEnumerable<IForumPostAttachmentInfo> AttachmentInfo { get; }

		public IForumAuthor Author { get; }

		public EntityReference EntityReference { get; }

		public DateTime PostedOn { get; }

		public EntityReference ThreadEntity { get; }
	}
}
