/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search
{
	using Lucene.Net.Store;
	using Lucene.Net.Search;
	using Lucene.Net.Analysis;
	using Version = Lucene.Net.Util.Version;

	public class PortalIndex : WebCrmEntityIndex
	{
		private readonly string _articlesLanguageCode;

		public PortalIndex(string portalName, string articlesLanguageCode, string notesFilter, bool displayNotes, Directory directory, Analyzer analyzer, Version version, string indexQueryName) : base(directory, analyzer, version, indexQueryName)
		{
			PortalName = portalName;
			_articlesLanguageCode = articlesLanguageCode;
			NotesFilter = notesFilter;
			DisplayNotes = displayNotes;
		}

		public PortalIndex(string portalName, string articlesLanguageCode, string notesFilter, bool displayNotes, Directory directory, Analyzer analyzer, Version version, string indexQueryName, string dataContextName) : base(directory, analyzer, version, indexQueryName, dataContextName)
		{
			PortalName = portalName;
			_articlesLanguageCode = articlesLanguageCode;
			NotesFilter = notesFilter;
			DisplayNotes = displayNotes;
		}

		public override string LanguageLocaleCode { get { return _articlesLanguageCode; } }

		public override bool DisplayNotes { get; }

		public override string NotesFilter { get; }

		protected string PortalName { get; }

		public override ICrmEntitySearchResultFactory GetSearchResultFactory(Query query)
		{
			return new PortalSearchResultFactory(PortalName, this, new SimpleHtmlHighlightedFragmentProvider(this, query));
		}
	}
}
