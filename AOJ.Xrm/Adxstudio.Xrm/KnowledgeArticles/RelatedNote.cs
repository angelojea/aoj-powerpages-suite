/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.KnowledgeArticles
{
	/// <summary>
	/// Notes related to Entity
	/// </summary>
	public class RelatedNote : IRelatedNote
	{
		/// <summary>
		/// Description of note
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// File Name
		/// </summary>
		public string FileName { get; }

		/// <summary>
		/// File Url
		/// </summary>
		public string FileUrl { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RelatedNote" /> class.
		/// </summary>
		/// <param name="description">Description of Note</param>
		/// <param name="fileName">File Name</param>
		/// <param name="fileUrl">File Url</param>
		public RelatedNote(string description, string fileName, string fileUrl)
		{
			Description = description;
			FileName = fileName;
			FileUrl = fileUrl;
		}
	}
}
