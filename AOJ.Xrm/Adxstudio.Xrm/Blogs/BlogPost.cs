/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogPost : IBlogPost
	{
		public BlogPost(Entity entity, ApplicationPath applicationPath, IBlogAuthor author, BlogCommentPolicy commentPolicy,
			int commentCount, IEnumerable<IBlogPostTag> tags, IRatingInfo ratingInfo = null)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (entity.LogicalName != "adx_blogpost")
			{
				throw new ArgumentException(string.Format("Value must have logical name {0}.", entity.LogicalName), "entity");
			}

			if (applicationPath == null)
			{
				throw new ArgumentNullException("applicationPath");
			}

			if (commentCount < 0)
			{
				throw new ArgumentException("Value can't be negative.", "commentCount");
			}

			if (tags == null)
			{
				throw new ArgumentNullException("tags");
			}

			Entity = entity;
			ApplicationPath = applicationPath;
			Author = author ?? new NullBlogAuthor();
			CommentPolicy = commentPolicy;
			CommentCount = commentCount;
			HasExcerpt = !string.IsNullOrWhiteSpace(entity.GetAttributeValue<string>("adx_summary"));
			Id = entity.Id;
			IsPublished = entity.GetAttributeValue<bool?>("adx_published").GetValueOrDefault(false);
			LastUpdatedTime = entity.GetAttributeValue<DateTime?>("modifiedon").GetValueOrDefault(DateTime.UtcNow);
			PublishDate = entity.GetAttributeValue<DateTime?>("adx_date").GetValueOrDefault(DateTime.UtcNow);
			Summary = new HtmlString(entity.GetAttributeValue<string>("adx_summary"));
			Tags = tags.ToArray();
			Title = entity.GetAttributeValue<string>("adx_name");

			RatingInfo = ratingInfo;

			var copy = entity.GetAttributeValue<string>("adx_copy");
			Content = new HtmlString(HasExcerpt ? "{0}\n{1}".FormatWith(Summary, copy) : copy);
		}

		public ApplicationPath ApplicationPath { get; }

		public IBlogAuthor Author { get; }

		public int CommentCount { get; }

		public BlogCommentPolicy CommentPolicy { get; }

		public IHtmlString Content { get; }

		public DateTime LastUpdatedTime { get; }

		public Entity Entity { get; }

		public bool HasExcerpt { get; }

		public Guid Id { get; }

		public bool IsPublished { get; }

		public DateTime PublishDate { get; }

		public IHtmlString Summary { get; }

		public IEnumerable<IBlogPostTag> Tags { get; }

		public string Title { get; }

		public IRatingInfo RatingInfo { get; }

		public bool RatingEnabled { get; }
	}
}
