/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Category;
	using IDataAdapterDependencies = KnowledgeArticles.IDataAdapterDependencies;

	/// <summary>
	/// Categories Drop
	/// </summary>
	public class CategoriesDrop : PortalDrop
	{
		/// <summary>
		/// Root Categories
		/// </summary>
		private readonly Lazy<CategoryDrop[]> topLevelCategories;

		/// <summary>
		/// Recent Categories
		/// </summary>
		private readonly Lazy<CategoryDrop[]> recentCategories;

		/// <summary>
		/// Popular Categories
		/// </summary>
		private readonly Lazy<CategoryDrop[]> popularCategories;

		/// <summary>
		/// ICategoryAggregationDataAdapter Instance
		/// </summary>
		private readonly ICategoryAggregationDataAdapter dataAdapter;

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoriesDrop"/> class
		/// </summary>
		/// <param name="portalLiquidContext">Portal Liquid Context</param>
		/// <param name="dependencies">Data Adapter Dependencies</param>
		/// <param name="pageSize">Results Page Size</param>
		public CategoriesDrop(IPortalLiquidContext portalLiquidContext, IDataAdapterDependencies dependencies, int pageSize = 5) : base(portalLiquidContext)
		{
			if (dependencies == null) { throw new ArgumentException("dependencies"); }

			PortalLiquidContext = portalLiquidContext;

			Dependencies = dependencies;

			dataAdapter = new CategoryAggregationDataAdapter(dependencies);

			topLevelCategories = new Lazy<CategoryDrop[]>(() => dataAdapter.SelectTopLevelCategories(pageSize).Select(e => new CategoryDrop(this, dependencies, e)).ToArray(), LazyThreadSafetyMode.None);

			recentCategories = new Lazy<CategoryDrop[]>(() => dataAdapter.SelectRecentCategories(pageSize).Select(e => new CategoryDrop(this, dependencies, e)).ToArray(), LazyThreadSafetyMode.None);

			popularCategories = new Lazy<CategoryDrop[]>(() => dataAdapter.SelectPopularCategories(pageSize).Select(e => new CategoryDrop(this, dependencies, e)).ToArray(), LazyThreadSafetyMode.None);

        }

        /// <summary>
        /// Data Adpater Dependencies
        /// </summary>
        internal IDataAdapterDependencies Dependencies { get; }

		/// <summary>
		/// Portal Liquid Context
		/// </summary>
		internal IPortalLiquidContext PortalLiquidContext { get; }

		/// <summary>
		/// Category Drop to get Top Level (ParentCategoryId = null) categories
		/// </summary>
		public IEnumerable<CategoryDrop> TopLevel
		{
			get { return topLevelCategories.Value.AsEnumerable(); }
		}

		/// <summary>
		///  Category Drop to get Recent Categories
		/// </summary>
		public IEnumerable<CategoryDrop> Recent
		{
			get { return recentCategories.Value.AsEnumerable(); }
		}

		/// <summary>
		/// Category Drop to get Popular Categories
		/// </summary>
		public IEnumerable<CategoryDrop> Popular
		{
			get { return popularCategories.Value.AsEnumerable(); }
		}

		/// <summary>
		/// Category Drop to get a Category by Category Number
		/// </summary>
		/// <param name="categoryNumber">Category Number</param>
		/// <returns>Category Drop with corresponding <paramref name="categoryNumber"/></returns>
		public CategoryDrop SelectByCategoryNumber(string categoryNumber)
		{
			if (string.IsNullOrEmpty(categoryNumber))
			{
				return null;
			}

			var category = dataAdapter.SelectByCategoryNumber(categoryNumber);
			if (category == null)
			{
				return null;
			}

			return new CategoryDrop(PortalLiquidContext, Dependencies, category);
		}

        /// <summary>
        /// Category Drop to get related Categories by parentcategoryid
        /// </summary>
        /// <param name="parentCategoryId">Parent Category Id</param>
        /// <param name="pageSize">Results Page Size</param>
        /// <returns>Category Drop Collection under <paramref name="parentCategoryId"/></returns>
        public IEnumerable<CategoryDrop> GetRelatedCategories(Guid parentCategoryId, int pageSize)
		{
			return dataAdapter.SelectRelatedCategories(parentCategoryId, pageSize).Select(category => new CategoryDrop(PortalLiquidContext, Dependencies, category));
		}
	}
}
