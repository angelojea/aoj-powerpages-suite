/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	using Resources;
	using WebForms;
	using Microsoft.Xrm.Portal;
	using Microsoft.Xrm.Portal.Web;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// Class for Create related record action
	/// </summary>
	public class CreateRelatedRecordActionLink : FormViewActionLink
	{

		/// <summary>
		/// Gets or sets the name of the entity.
		/// </summary>
		/// <value>
		/// The name of the entity.
		/// </value>
		public string EntityName { get; set; }

		/// <summary>
		/// Gets or sets the relationship.
		/// </summary>
		/// <value>
		/// The relationship.
		/// </value>
		public string Relationship { get; set; }

		/// <summary>
		/// Gets or sets the parent record.
		/// </summary>
		/// <value>
		/// The parent record.
		/// </value>
		public string ParentRecord { get; set; }

		/// <summary>
		/// Modal property
		/// </summary>
		public ViewCreateRelatedRecordModal Modal { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CreateRelatedRecordActionLink" /> class.
		/// </summary>
		public CreateRelatedRecordActionLink()
		{
			Modal = new ViewCreateRelatedRecordModal();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CreateRelatedRecordActionLink"/> class.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="enabled">if set to <c>true</c> [enabled].</param>
		/// <param name="label">The label.</param>
		/// <param name="tooltip">The tooltip.</param>
		/// <param name="queryStringIdParameterName">Name of the query string identifier parameter.</param>
		public CreateRelatedRecordActionLink(UrlBuilder url = null, bool enabled = false,
			string label = null, string tooltip = null,
			string queryStringIdParameterName = "id")
			: base(LinkActionType.CreateRelatedRecord, enabled, url, label, tooltip, queryStringIdParameterName)
		{
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CreateRelatedRecordActionLink"/> class.
		/// </summary>
		/// <param name="entityForm">The entity form.</param>
		/// <param name="enabled">if set to <c>true</c> [enabled].</param>
		/// <param name="label">The label.</param>
		/// <param name="tooltip">The tooltip.</param>
		/// <param name="queryStringIdParameterName">Name of the query string identifier parameter.</param>
		public CreateRelatedRecordActionLink(EntityReference entityForm, bool enabled = false, string label = null,
			string tooltip = null, string queryStringIdParameterName = "id")
			: base(entityForm, LinkActionType.CreateRelatedRecord, enabled, label, tooltip, queryStringIdParameterName)
		{
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CreateRelatedRecordActionLink" /> class.
		/// </summary>
		/// <param name="portalContext">The portal context.</param>
		/// <param name="gridMetadata">The grid metadata.</param>
		/// <param name="languageCode">The language code.</param>
		/// <param name="action">The action.</param>
		/// <param name="enabled">if set to <c>true</c> [enabled].</param>
		/// <param name="portalName">Name of the portal.</param>
		/// <param name="label">The label.</param>
		/// <param name="tooltip">The tooltip.</param>
		/// <param name="queryStringIdParameterName">Name of the query string identifier parameter.</param>
		public CreateRelatedRecordActionLink(IPortalContext portalContext, GridMetadata gridMetadata, int languageCode, CreateRelatedRecordAction action,
					bool enabled = false, string portalName = null, string label = null, string tooltip = null, string queryStringIdParameterName = "id")
					: base(portalContext, languageCode, action, LinkActionType.CreateRelatedRecord, enabled, portalName, label, tooltip)
		{
			Initialize();

			QueryStringIdParameterName = !string.IsNullOrWhiteSpace(action.RecordIdQueryStringParameterName)
				? action.RecordIdQueryStringParameterName
				: queryStringIdParameterName;

			if (gridMetadata.CreateFormDialog == null)
			{
				return;
			}

			Modal.CloseButtonCssClass = gridMetadata.CreateRelatedRecordDialog.CloseButtonCssClass;
			Modal.CloseButtonText = gridMetadata.CreateRelatedRecordDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = gridMetadata.CreateRelatedRecordDialog.CssClass;
			Modal.DismissButtonSrText = gridMetadata.CreateRelatedRecordDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.LoadingMessage = gridMetadata.CreateRelatedRecordDialog.LoadingMessage.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = gridMetadata.CreateRelatedRecordDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = Tooltip;
			Modal.Size = gridMetadata.CreateRelatedRecordDialog.Size;
			Modal.Title = Label;
			Modal.TitleCssClass = gridMetadata.CreateRelatedRecordDialog.TitleCssClass;

			FilterCriteria = action.FilterCriteria;
			EntityName = action.EntityName;
			ParentRecord = action.ParentRecord;
			Relationship = action.Relationship;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CreateRelatedRecordActionLink"/> class.
		/// </summary>
		/// <param name="portalContext">The portal context.</param>
		/// <param name="formMetadata">The form metadata.</param>
		/// <param name="languageCode">The language code.</param>
		/// <param name="action">The action.</param>
		/// <param name="enabled">if set to <c>true</c> [enabled].</param>
		/// <param name="portalName">Name of the portal.</param>
		public CreateRelatedRecordActionLink(IPortalContext portalContext, FormActionMetadata formMetadata, int languageCode, CreateRelatedRecordAction action, bool enabled = true, string portalName = null)
			: base(portalContext, languageCode, action, LinkActionType.CreateRelatedRecord, enabled, portalName)
		{
			Initialize();

			if (formMetadata.CreateRelatedRecordDialog == null)
			{
				return;
			}
			Modal.CloseButtonCssClass = formMetadata.CreateRelatedRecordDialog.CloseButtonCssClass;
			Modal.CloseButtonText = formMetadata.CreateRelatedRecordDialog.CloseButtonText.GetLocalizedString(languageCode);
			Modal.CssClass = formMetadata.CreateRelatedRecordDialog.CssClass;
			Modal.DismissButtonSrText = formMetadata.CreateRelatedRecordDialog.DismissButtonSrText.GetLocalizedString(languageCode);
			Modal.LoadingMessage = formMetadata.CreateRelatedRecordDialog.LoadingMessage.GetLocalizedString(languageCode);
			Modal.PrimaryButtonCssClass = formMetadata.CreateRelatedRecordDialog.PrimaryButtonCssClass;
			Modal.PrimaryButtonText = Tooltip;
			Modal.Size = formMetadata.CreateRelatedRecordDialog.Size;
			Modal.Title = Label;
			Modal.TitleCssClass = formMetadata.CreateRelatedRecordDialog.TitleCssClass;

			if (action.ShowModal != null)
			{
				ShowModal = action.ShowModal.Value;
			}
			EntityName = action.EntityName;
			ParentRecord = action.ParentRecord;
			Relationship = action.Relationship;
			FilterCriteria = action.FilterCriteria;
		}

		/// <summary>
		/// Class initializer
		/// </summary>
		private void Initialize()
		{
			var createText = ResourceManager.GetString("CreateRelatedRecord_Text");

			if (string.IsNullOrEmpty(Label))
			{
				Label = string.Format("<span class='fa fa-plus-circle' aria-hidden='true'></span> {0}", createText);
			}

			if (Tooltip == null)
			{
				Tooltip = createText;
			}

			Modal = new ViewCreateRelatedRecordModal();
		}

		
	}
}
