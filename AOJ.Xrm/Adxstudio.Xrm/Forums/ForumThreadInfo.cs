/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Adxstudio.Xrm.Forums
{
	internal class ForumThreadInfo : IForumThreadInfo
	{
		public ForumThreadInfo(IForumAuthor author, DateTime postedOn, IForumThreadType threadType, IEnumerable<IForumThreadTag> tags, IForumPostInfo latestPost)
		{
			if (author == null) throw new ArgumentNullException("author");
			if (threadType == null) throw new ArgumentNullException("threadType");
			if (tags == null) throw new ArgumentNullException("tags");
			if (latestPost == null) throw new ArgumentNullException("latestPost");

			Author = author;
			PostedOn = postedOn;
			ThreadType = threadType;
			Tags = tags.ToArray();
			LatestPost = latestPost;
		}

		public IForumAuthor Author { get; }

		public IForumPostInfo LatestPost { get; }

		public string LatestPostUrl
		{
			get { return null; }
		}

		public DateTime PostedOn { get; }

		public IEnumerable<IForumThreadTag> Tags { get; }

		public IForumThreadType ThreadType { get; }
	}
}
