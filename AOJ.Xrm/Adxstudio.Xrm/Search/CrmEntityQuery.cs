/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Search.Facets;

namespace Adxstudio.Xrm.Search
{
	public class CrmEntityQuery : ICrmEntityQuery
	{
		public CrmEntityQuery(string queryText, int pageNumber, int pageSize, IEnumerable<string> logicalNames, IWebsiteLanguage language, bool multiLanguageEnabled, string filter = null, IEnumerable<FacetConstraints> facetConstraints = null, string sortingOptions = null, string queryTerm = null)
        {
			if (queryText == null)
			{
				throw new ArgumentNullException("queryText");
			}

			QueryText = queryText;
			PageNumber = pageNumber;
			PageSize = pageSize;
			Filter = filter;
			ContextLanguage = language;
			FacetConstraints = facetConstraints == null ? Enumerable.Empty<FacetConstraints>() : facetConstraints;
			SortingOption = sortingOptions == null ? string.Empty : sortingOptions;
			MultiLanguageEnabled = multiLanguageEnabled;
			QueryTerm = queryTerm ?? string.Empty;

			LogicalNames = logicalNames == null ? Enumerable.Empty<string>() : logicalNames.ToArray();
        }

		public CrmEntityQuery(string queryText, int pageNumber, int pageSize, IWebsiteLanguage language, bool multiLanguageEnabled, string filter = null) : this(queryText, pageNumber, pageSize, Enumerable.Empty<string>(), language, multiLanguageEnabled, filter) { }

		public string Filter { get; }

		public IEnumerable<string> LogicalNames { get; }

		public int PageNumber { get; }

		public int PageSize { get; }

		public string QueryText { get; }

		public string LanguageCode { get; }

		public IEnumerable<FacetConstraints> FacetConstraints { get; }

		public string SortingOption { get; set; }

		public IWebsiteLanguage ContextLanguage { get; }

		public bool MultiLanguageEnabled { get; }

		public string QueryTerm { get; }

		public override string ToString()
		{
			return QueryText;
		}
	}
}
