/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
	using System;
	using System.Threading;
	using System.Web;
	using Services;
	using Web;
	using Feedback;
	using Microsoft.Xrm.Sdk.Query;
	using Microsoft.Xrm.Portal.Configuration;
	using Microsoft.Xrm.Portal.Web.Providers;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Represents an Knowledge Article in an Adxstudio.
	/// </summary>
	public class KnowledgeArticle : IKnowledgeArticle
	{
		/// <summary>The timespan to keep in cache.</summary>
		private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(1);

		/// <summary>The url.</summary>
		private readonly Lazy<string> url;

		/// <summary>The article views.</summary>
		private readonly Lazy<int> articleViews;

		/// <summary>The article rating.</summary>
		private readonly Lazy<decimal> articleRating;

		/// <summary>The http context.</summary>
		private readonly HttpContextBase httpContext;

		/// <summary>Gets the id.</summary>
		public Guid Id { get; }

		/// <summary>Gets the title.</summary>
		public string Title { get; }

		/// <summary>Gets the article public number.</summary>
		public string ArticlePublicNumber { get; }

		/// <summary>Gets the content.</summary>
		public string Content { get; }

		/// <summary>Gets the keywords.</summary>
		public string Keywords { get; }

		/// <summary>Gets the knowledge article views.</summary>
		public int KnowledgeArticleViews
		{
			get { return articleViews.Value; }
		}

		/// <summary>Gets the root article.</summary>
		public EntityReference RootArticle { get; }

		/// <summary>Gets the entity.</summary>
		public Entity Entity { get; }

		/// <summary>Gets the entity reference.</summary>
		public EntityReference EntityReference { get; }

		/// <summary>Gets the comment count.</summary>
		public int CommentCount { get; }

		/// <summary>Gets the rating.</summary>
		public decimal Rating
		{
			get { return articleRating.Value; }
		}

		/// <summary>Gets a value indicating whether is rating enabled.</summary>
		public bool IsRatingEnabled { get; }

		/// <summary>Gets a value indicating whether current user can comment.</summary>
		public bool CurrentUserCanComment { get; }

		/// <summary>Gets the comment policy.</summary>
		public CommentPolicy CommentPolicy { get; }

		/// <summary>Gets the url.</summary>
		public string Url
		{
			get { return url.Value; }
		}

		/// <summary>Initializes a new instance of the <see cref="KnowledgeArticle"/> class. Knowledge Article initialization</summary>
		/// <param name="article">Knowledge Article record entity</param>
		/// <param name="commentCount">The comment Count.</param>
		/// <param name="commentPolicy">The comment Policy.</param>
		/// <param name="isRatingEnabled">The is Rating Enabled.</param>
		/// <param name="httpContext">The http Context.</param>
		public KnowledgeArticle(
			Entity article,
			int commentCount,
			CommentPolicy commentPolicy,
			bool isRatingEnabled,
			HttpContextBase httpContext)
		{
			article.ThrowOnNull("article");
			article.AssertEntityName("knowledgearticle");

			this.httpContext = httpContext;
			Entity = article;
			EntityReference = article.ToEntityReference();
			Id = article.Id;
			Title = article.GetAttributeValue<string>("title");
			ArticlePublicNumber = article.GetAttributeValue<string>("articlepublicnumber");
			RootArticle = article.GetAttributeValue<EntityReference>("rootarticleid");
			Content = article.GetAttributeValue<string>("content");
			Keywords = article.GetAttributeValue<string>("keywords");
			CommentCount = commentCount;
			IsRatingEnabled = isRatingEnabled;
			CommentPolicy = commentPolicy;
			CurrentUserCanComment =
				commentPolicy == CommentPolicy.Open ||
				commentPolicy == CommentPolicy.Moderated ||
				(commentPolicy == CommentPolicy.OpenToAuthenticatedUsers && httpContext.Request.IsAuthenticated);
			url = new Lazy<string>(GetUrl, LazyThreadSafetyMode.None);
			articleViews = new Lazy<int>(GetArticleViews);
			articleRating = new Lazy<decimal>(GetArticleRating);
		}

		/// <summary>Get the article rul.</summary>
		/// <returns>The article url.</returns>
		private string GetUrl()
		{
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext();
			var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider().GetDependency<IEntityUrlProvider>();

			return urlProvider.GetUrl(serviceContext, Entity);
		}

		/// <summary>Get the number of article views.</summary>
		/// <returns>The numbr of views.</returns>
		private int GetArticleViews()
		{
			var service = httpContext.GetOrganizationService();
			var attributes = service.RetrieveSingle(
				EntityReference,
				new ColumnSet("knowledgearticleviews"),
				RequestFlag.AllowStaleData | RequestFlag.SkipDependencyCalculation,
				DefaultDuration);

			return attributes.Contains("knowledgearticleviews") ? attributes["knowledgearticleviews"] as int? ?? 0 : 0;
		}

		/// <summary> Get the rating of the article </summary>
		/// <returns> The article rating. </returns>
		private decimal GetArticleRating()
		{
			var service = httpContext.GetOrganizationService();
			var entity = service.RetrieveSingle(
				EntityReference,
				new ColumnSet("rating"),
				RequestFlag.AllowStaleData | RequestFlag.SkipDependencyCalculation,
				DefaultDuration);

			return entity.Attributes.Contains("rating") ? entity.Attributes["rating"] as decimal? ?? 0 : 0;
		}
	}
}
