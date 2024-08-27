/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Category
{
    using System;
    using System.Threading;
    using System.Web;
    using Microsoft.Xrm.Portal.Configuration;
    using Microsoft.Xrm.Portal.Web.Providers;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Category Class
    /// </summary>
    public class Category : ICategory
    {
        /// <summary>
        /// Category URL
        /// </summary>
        private readonly Lazy<string> url;

        /// <summary>
        /// Category Id
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Category Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Category Number
        /// </summary>
        public string CategoryNumber { get; }

        /// <summary>
        /// Entity to hold the Category entity
        /// </summary>
        public Entity Entity { get; }

        /// <summary>
        /// Entity Reference to hold the Category reference
        /// </summary>
        public EntityReference EntityReference { get; }

        /// <summary>
        /// Entity Reference to hold the Parent category reference
        /// </summary>
        public EntityReference ParentCategory { get; }

        /// <summary>
        /// Category URL
        /// </summary>
        public string Url
        {
            get { return url.Value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        /// </summary>
        /// <param name="category">Category Entity</param>
        public Category(Entity category)
        {
            category.ThrowOnNull("category");
            category.AssertEntityName("category");

            Entity = category;
            EntityReference = category.ToEntityReference();
            Id = category.Id;
            Title = category.GetAttributeValue<string>("title");
            CategoryNumber = category.GetAttributeValue<string>("categorynumber");
            url = new Lazy<string>(GetUrl, LazyThreadSafetyMode.None);
            ParentCategory = category.GetAttributeValue<EntityReference>("parentcategoryid");
        }

        /// <summary>
        /// Gets the Category URL
        /// </summary>
        /// <returns>URL as a string</returns>
        private string GetUrl()
        {
            var serviceContext = PortalCrmConfigurationManager.CreateServiceContext();
            var urlProvider = PortalCrmConfigurationManager.CreateDependencyProvider().GetDependency<IEntityUrlProvider>();

            return urlProvider.GetUrl(serviceContext, Entity);
        }
    }
}
