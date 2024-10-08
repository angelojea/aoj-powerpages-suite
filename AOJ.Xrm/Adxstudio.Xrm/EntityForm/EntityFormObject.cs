/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Client; //despite what resharper says this using is required.

//
namespace Adxstudio.Xrm.EntityForm
{
	public class EntityFormObject : IEntityForm
	{
		public string EntityName { get; set; }

		public string FormName { get; set; }

		public string TabName { get; set; }
		public bool RecommendedFieldsRequired { get; set; }
		public bool RenderWebResourcesInline { get; set; }
		public bool ShowOwnerFields { get; set; }
		public bool ShowUnsupportedFields { get; set; }
		public bool ToolTipEnabled { get; set; }
		public bool AutoGenerateStepsFromTabs { get; set; }
		public bool ForceAllFieldsRequired { get; set; }
		public bool HideFormOnSuccess { get; set; }
		public string ValidationGroup { get; set; }
		public bool ValidationSummaryLinksEnabled { get; set; }
		public string ValidationSummaryCssClass { get; set; }
		public string LocalizedValidationSummaryHeaderText { get; set; }

		public bool CaptchaRequired { get; set; }
		public bool ShowCaptchaForAuthenticatedUsers { get; set; }

		public bool AttachFile { get; set; }
		public bool AttachFileAllowMultiple { get; set; }
		public string AttachFileAccept { get; set; }
        public string AttachFileAcceptExtensions { get; set; }
		public bool AttachFileRestrictAccept { get; set; }
		public string AttachFileTypeErrorMessage { get; set; }
		public int? AttachFileMaxSize { get; set; }
		public bool AttachFileRestrictSize { get; set; }
		public string AttachFileSizeErrorMessage { get; set; }
		public bool AttachFileRequired { get; set; }
		public string LocalizedAttachFileLabel { get; set; }
		public string AttachFileLabel { get; set; }
		public string AttachFileRequiredErrorMessage { get; set; }
		public OptionSetValue AttachFileStorageLocation { get; set; }
		public OptionSetValue AttachFileSaveOption { get; set; }
        public string EntityformPreviousButtonCssClass { get; set; }
		public string EntityformNextButtonCssClass { get; set; }
		public string EntityformSubmitButtonCssClass { get; set; }
		public string LocalizedPreviousButtonText { get; set; }
		public string LocalizedNextButtonText { get; set; }
		public string LocalizedSubmitButtonText { get; set; }
		public string LocalizedSubmitButtonBusyText { get; set; }
		public string SuccessMessage { get; set; }
		public string Instructions { get; set; }

		public string PreviousButtonCssClass { get; set; }
		public string NextButtonCssClass { get; set; }
		public string SubmitButtonCssClass { get; set; }
		public string PreviousButtonText { get; set; }
		public string NextButtonText { get; set; }
		public string SubmitButtonText { get; set; }
		public string SubmitButtonBusyText { get; set; }

		public bool PopulateReferenceEntityLookupField { get; set; }
		public string TargetAttributeName { get; set; }
		public string ReferenceEntityLogicalName { get; set; }
		public string ReferenceQueryStringName { get; set; }
		public bool QuerystringIsPrimaryKey { get; set; }
		public string ReferenceEntityPrimaryKeyLogicalName { get; set; }
		public string ReferenceQueryAttributeName { get; set; }

		public Entity entity { get; set; }

		public IEnumerable<Entity> EntityFormMetadata { get; set; }

		//
		public EntityFormObject(Entity entityform, int languageCode, OrganizationServiceContext context, string defaultPreviousButtonCssClass = null,
			string defaultNextButtonCssClass = null, string defaultSubmitButtonCssClass = null, string defaultPreviousButtonText = null,
			string defaultNextButtonText = null, string defaultSubmitButtonText = null, string defaultSubmitButtonBusyText = null)
		{
			EntityName = entityform.GetAttributeValue<string>("mspp_entityname");
			FormName = entityform.GetAttributeValue<string>("mspp_formname");

			if (string.IsNullOrWhiteSpace(EntityName)) throw new ApplicationException("mspp_entityform.mspp_entityname must not be null.");
			if (string.IsNullOrWhiteSpace(FormName)) throw new ApplicationException("mspp_entityform.mspp_formname must not be null.");

			PreviousButtonCssClass = defaultPreviousButtonCssClass;
			NextButtonCssClass = defaultNextButtonCssClass;
			SubmitButtonCssClass = defaultSubmitButtonCssClass;
			PreviousButtonText = defaultPreviousButtonText;
			NextButtonText = defaultNextButtonText;
			SubmitButtonText = defaultSubmitButtonText;
			SubmitButtonBusyText = defaultSubmitButtonBusyText;
			TabName = entityform.GetAttributeValue<string>("mspp_tabname");
			RecommendedFieldsRequired = entityform.GetAttributeValue<bool?>("mspp_recommendedfieldsrequired") ?? false;
			RenderWebResourcesInline = entityform.GetAttributeValue<bool?>("mspp_renderwebresourcesinline") ?? false;
			ShowOwnerFields = entityform.GetAttributeValue<bool?>("mspp_showownerfields") ?? false;
			ShowUnsupportedFields = entityform.GetAttributeValue<bool?>("mspp_showunsupportedfields") ?? false;
			ToolTipEnabled = entityform.GetAttributeValue<bool?>("mspp_tooltipenabled") ?? false;
            AutoGenerateStepsFromTabs = entityform.GetAttributeValue<bool?>("mspp_autogeneratesteps") ?? false;
            ForceAllFieldsRequired = entityform.GetAttributeValue<bool?>("mspp_forceallfieldsrequired") ?? false;
			HideFormOnSuccess = entityform.GetAttributeValue<bool?>("mspp_hideformonsuccess") ?? true;
			ValidationGroup = entityform.GetAttributeValue<string>("mspp_validationgroup") ?? string.Empty;
			ValidationSummaryLinksEnabled = entityform.GetAttributeValue<bool?>("mspp_validationsummarylinksenabled") ?? true;
			
			ValidationSummaryCssClass = entityform.GetAttributeValue<string>("mspp_validationsummarycssclass") ?? string.Empty;
			LocalizedValidationSummaryHeaderText = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_validationsummaryheadertext"), languageCode) ?? string.Empty;
			CaptchaRequired = entityform.GetAttributeValue<bool?>("mspp_captcharequired") ?? false;
			ShowCaptchaForAuthenticatedUsers = entityform.GetAttributeValue<bool?>("mspp_showcaptchaforauthenticatedusers") ?? false;

			AttachFile = entityform.GetAttributeValue<bool?>("mspp_attachfile") ?? false;
			AttachFileAllowMultiple = entityform.GetAttributeValue<bool?>("mspp_attachfileallowmultiple") ?? false;
			AttachFileAccept = entityform.GetAttributeValue<string>("mspp_attachfileaccept");
            AttachFileAcceptExtensions = entityform.GetAttributeValue<string>("mspp_attachfileacceptextensions");
            AttachFileRestrictAccept = entityform.GetAttributeValue<bool?>("mspp_attachfilerestrictaccept").GetValueOrDefault(false);
			AttachFileTypeErrorMessage = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_attachfiletypeerrormessage"), languageCode);
			AttachFileMaxSize = entityform.GetAttributeValue<int?>("mspp_attachfilemaxsize");
			AttachFileRestrictSize = entityform.GetAttributeValue<int?>("mspp_attachfilemaxsize").HasValue;
			AttachFileSizeErrorMessage = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_attachfilesizeerrormessage"), languageCode);
			AttachFileRequired = entityform.GetAttributeValue<bool?>("mspp_attachfilerequired") ?? false;
			LocalizedAttachFileLabel = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_attachfilelabel"), languageCode);
			AttachFileLabel = string.IsNullOrWhiteSpace(LocalizedAttachFileLabel) ? EntityFormFunctions.DefaultAttachFileLabel : LocalizedAttachFileLabel;
			AttachFileRequiredErrorMessage = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_attachfilerequirederrormessage"), languageCode);
			AttachFileStorageLocation = entityform.GetAttributeValue<OptionSetValue>("mspp_attachfilestoragelocation");
            AttachFileSaveOption = entityform.GetAttributeValue<OptionSetValue>("mspp_attachfilesaveoption");
            EntityformPreviousButtonCssClass = entityform.GetAttributeValue<string>("mspp_previousbuttoncssclass");
			EntityformNextButtonCssClass = entityform.GetAttributeValue<string>("mspp_nextbuttoncssclass");
			EntityformSubmitButtonCssClass = entityform.GetAttributeValue<string>("mspp_submitbuttoncssclass");
			LocalizedPreviousButtonText = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_previousbuttontext"), languageCode);
			LocalizedNextButtonText = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_nextbuttontext"), languageCode);
			LocalizedSubmitButtonText = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_submitbuttontext"), languageCode);
			LocalizedSubmitButtonBusyText = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_submitbuttonbusytext"), languageCode);
			SuccessMessage = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_successmessage"), languageCode);
			Instructions = Localization.GetLocalizedString(entityform.GetAttributeValue<string>("mspp_instructions"), languageCode);

			PreviousButtonCssClass = string.IsNullOrWhiteSpace(EntityformPreviousButtonCssClass) ? (string.IsNullOrWhiteSpace(PreviousButtonCssClass) ? EntityFormFunctions.DefaultPreviousButtonCssClass : PreviousButtonCssClass) : EntityformPreviousButtonCssClass;
			NextButtonCssClass = string.IsNullOrWhiteSpace(EntityformNextButtonCssClass) ? (string.IsNullOrWhiteSpace(NextButtonCssClass) ? EntityFormFunctions.DefaultNextButtonCssClass : NextButtonCssClass) : EntityformNextButtonCssClass;
			SubmitButtonCssClass = string.IsNullOrWhiteSpace(EntityformSubmitButtonCssClass) ? (string.IsNullOrWhiteSpace(SubmitButtonCssClass) ? EntityFormFunctions.DefaultSubmitButtonCssClass : SubmitButtonCssClass) : EntityformSubmitButtonCssClass;
			PreviousButtonText = string.IsNullOrWhiteSpace(LocalizedPreviousButtonText) ? (string.IsNullOrWhiteSpace(PreviousButtonText) ? EntityFormFunctions.DefaultPreviousButtonText : PreviousButtonText) : LocalizedPreviousButtonText;
			NextButtonText = string.IsNullOrWhiteSpace(LocalizedNextButtonText) ? (string.IsNullOrWhiteSpace(NextButtonText) ? EntityFormFunctions.DefaultNextButtonText : NextButtonText) : LocalizedNextButtonText;
			SubmitButtonText = string.IsNullOrWhiteSpace(LocalizedSubmitButtonText) ? (string.IsNullOrWhiteSpace(SubmitButtonText) ? EntityFormFunctions.DefaultSubmitButtonText : SubmitButtonText) : LocalizedSubmitButtonText;
			SubmitButtonBusyText = string.IsNullOrWhiteSpace(LocalizedSubmitButtonBusyText) ? (string.IsNullOrWhiteSpace(SubmitButtonBusyText) ? EntityFormFunctions.DefaultSubmitButtonBusyText : SubmitButtonBusyText) : LocalizedSubmitButtonBusyText;

			EntityFormMetadata = entityform.GetRelatedEntities(context, new Relationship("mspp_entityformmetadata_entityform"));

			PopulateReferenceEntityLookupField = entityform.GetAttributeValue<bool?>("mspp_populatereferenceentitylookupfield") ?? false;
			TargetAttributeName = entityform.GetAttributeValue<string>("mspp_referencetargetlookupattributelogicalname");
			ReferenceEntityLogicalName = entityform.GetAttributeValue<string>("mspp_referenceentitylogicalname");
			ReferenceQueryStringName = entityform.GetAttributeValue<string>("mspp_referencequerystringname") ?? string.Empty;
			QuerystringIsPrimaryKey = entityform.GetAttributeValue<bool?>("mspp_referencequerystringisprimarykey") ?? false;
			ReferenceEntityPrimaryKeyLogicalName = entityform.GetAttributeValue<string>("mspp_referenceentityprimarykeylogicalname");
			ReferenceQueryAttributeName = entityform.GetAttributeValue<string>("mspp_referencequeryattributelogicalname");
		}
	}

	public enum AttachFileSaveOption
	{
		Notes = 756150000,
		PortalComment = 756150001
	}
}
