/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Web.UI.WebForms;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Client; //despite what resharper says this using is required.

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	public class WebFormStepObject
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

		public string ValidationGroup { get; set; }
		public bool ValidationSummaryLinksEnabled { get; set; }
		public string ValidationSummaryCssClass { get; set; }
		public string LocalizedValidationSummaryHeaderText { get; set; }
		public bool EnableValidationSummaryLinks { get; set; }
		
		public bool CaptchaRequired { get; set; }
		public bool ShowCaptchaForAuthenticatedUsers { get; set; }

		public bool AttachFile { get; set; }
		public bool AttachFileAllowMultiple { get; set; }
		public string AttachFileAccept { get; set; }
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

		public bool MovePreviousPermitted { get; set; }

		public bool PopulateReferenceEntityLookupField { get; set; }
		public string TargetAttributeName { get; set; }
		public string ReferenceEntityLogicalName { get; set; }
		public string ReferenceQueryStringName { get; set; }
		public bool QuerystringIsPrimaryKey { get; set; }
		public string ReferenceEntityPrimaryKeyLogicalName { get; set; }
		public string ReferenceQueryAttributeName { get; set; }

		public bool ConfirmOnExit { get; set; }
		public string ConfirmOnExitMessage { get; set; }

		public Entity NextStep { get; set; }

		public IEnumerable<Entity> WebFormMetadata { get; set; }

		public string UserControlPath { get; set; }
		public string PostBackUrl { get; set; }
		public bool SetEntityReference { get; set; }
		public string RelationshipName { get; set; }

		public WebFormStepObject(Entity webform, Entity step, int languageCode, OrganizationServiceContext context, string defaultPreviousButtonCssClass = null,
			string defaultNextButtonCssClass = null, string defaultSubmitButtonCssClass = null, string defaultPreviousButtonText = null,
			string defaultNextButtonText = null, string defaultSubmitButtonText = null, string defaultSubmitButtonBusyText = null)
		{
			EntityName = step.GetAttributeValue<string>("mspp_targetentitylogicalname");
			FormName = step.GetAttributeValue<string>("mspp_formname");

			if (string.IsNullOrWhiteSpace(EntityName)) throw new ApplicationException(ResourceManager.GetString("TargetEntity_LogicalName_Null_Exception"));
			
			PreviousButtonCssClass = defaultPreviousButtonCssClass;
			NextButtonCssClass = defaultNextButtonCssClass;
			SubmitButtonCssClass = defaultSubmitButtonCssClass;

			PreviousButtonText = defaultPreviousButtonText;
			NextButtonText = defaultNextButtonText;
			SubmitButtonText = defaultSubmitButtonText;
			SubmitButtonBusyText = defaultSubmitButtonBusyText;

			TabName = step.GetAttributeValue<string>("mspp_tabname");

			RecommendedFieldsRequired = step.GetAttributeValue<bool?>("mspp_recommendedfieldsrequired") ?? false; 
			RenderWebResourcesInline = step.GetAttributeValue<bool?>("mspp_renderwebresourcesinline") ?? false;
			ShowOwnerFields = step.GetAttributeValue<bool?>("mspp_showownerfields") ?? false;
			ShowUnsupportedFields = step.GetAttributeValue<bool?>("mspp_showunsupportedfields") ?? false;
			ToolTipEnabled = step.GetAttributeValue<bool?>("mspp_tooltipenabled") ?? false;

			AutoGenerateStepsFromTabs = step.GetAttributeValue<bool?>("mspp_autogeneratesteps") ?? false;

			ForceAllFieldsRequired = step.GetAttributeValue<bool?>("mspp_forceallfieldsrequired") ?? false;

			ValidationGroup = step.GetAttributeValue<string>("mspp_validationgroup") ?? string.Empty;
			ValidationSummaryLinksEnabled = step.GetAttributeValue<bool?>("mspp_validationsummarylinksenabled") ?? true;
			ValidationSummaryCssClass = step.GetAttributeValue<string>("mspp_validationsummarycssclass") ?? string.Empty;

			EnableValidationSummaryLinks = step.GetAttributeValue<bool?>("mspp_validationsummarylinksenabled") ?? true;

			LocalizedValidationSummaryHeaderText = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_validationsummaryheadertext"), languageCode) ?? string.Empty;
			
			CaptchaRequired = step.GetAttributeValue<bool?>("mspp_captcharequired") ?? false;
			ShowCaptchaForAuthenticatedUsers = step.GetAttributeValue<bool?>("mspp_showcaptchaforauthenticatedusers") ?? false;


			AttachFile = step.GetAttributeValue<bool?>("mspp_attachfile") ?? false;
			AttachFileAllowMultiple = step.GetAttributeValue<bool?>("mspp_allowmultiplefiles") ?? false;
			AttachFileAccept = step.GetAttributeValue<string>("mspp_accept");
			AttachFileRestrictAccept = step.GetAttributeValue<bool?>("mspp_attachfilerestrictaccept").GetValueOrDefault(false);
			AttachFileTypeErrorMessage = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_attachfiletypeerrormessage"), languageCode);
			AttachFileMaxSize = step.GetAttributeValue<int?>("mspp_attachfilemaxsize");
			AttachFileRestrictSize = step.GetAttributeValue<int?>("mspp_attachfilemaxsize").HasValue;
			AttachFileSizeErrorMessage = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_attachfilesizeerrormessage"), languageCode);
			AttachFileRequired = step.GetAttributeValue<bool?>("mspp_attachfilerequired") ?? false;
			LocalizedAttachFileLabel = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_attachfilelabel"), languageCode);
			AttachFileLabel = string.IsNullOrWhiteSpace(LocalizedAttachFileLabel) ? WebFormFunctions.DefaultAttachFileLabel : LocalizedAttachFileLabel;
			AttachFileRequiredErrorMessage = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_attachfilerequirederrormessage"), languageCode);
			AttachFileStorageLocation = step.GetAttributeValue<OptionSetValue>("mspp_attachfilestoragelocation");

			var stepPreviousButtonCssClass = step.GetAttributeValue<string>("mspp_previousbuttoncssclass");
			var stepNextButtonCssClass = step.GetAttributeValue<string>("mspp_nextbuttoncssclass");
			var stepSubmitButtonCssClass = step.GetAttributeValue<string>("mspp_submitbuttoncssclass");

			LocalizedPreviousButtonText = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_previousbuttontext"), languageCode);
			LocalizedNextButtonText = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_nextbuttontext"), languageCode);
			LocalizedSubmitButtonText = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_submitbuttontext"), languageCode);
			LocalizedSubmitButtonBusyText = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_submitbuttonbusytext"), languageCode);

			SuccessMessage = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_successmessage"), languageCode);
			Instructions = Localization.GetLocalizedString(step.GetAttributeValue<string>("mspp_instructions"), languageCode);

			PreviousButtonCssClass = string.IsNullOrWhiteSpace(stepPreviousButtonCssClass) ? (string.IsNullOrWhiteSpace(PreviousButtonCssClass) ? WebFormFunctions.DefaultPreviousButtonCssClass : PreviousButtonCssClass) : stepPreviousButtonCssClass;
			NextButtonCssClass = string.IsNullOrWhiteSpace(stepNextButtonCssClass) ? (string.IsNullOrWhiteSpace(NextButtonCssClass) ? WebFormFunctions.DefaultNextButtonCssClass : NextButtonCssClass) : stepNextButtonCssClass;
			SubmitButtonCssClass = string.IsNullOrWhiteSpace(stepSubmitButtonCssClass) ? (string.IsNullOrWhiteSpace(SubmitButtonCssClass) ? WebFormFunctions.DefaultSubmitButtonCssClass : SubmitButtonCssClass) : stepSubmitButtonCssClass;

			ConfirmOnExit = webform.GetAttributeValue<bool?>("mspp_savechangeswarningonclose") ?? false;
			ConfirmOnExitMessage = Localization.GetLocalizedString(webform.GetAttributeValue<string>("mspp_savechangeswarningmessage"), languageCode);

			PreviousButtonText = string.IsNullOrWhiteSpace(LocalizedPreviousButtonText) ? (string.IsNullOrWhiteSpace(PreviousButtonText) ? WebFormFunctions.DefaultPreviousButtonText : PreviousButtonText) : LocalizedPreviousButtonText;
			NextButtonText = string.IsNullOrWhiteSpace(LocalizedNextButtonText) ? (string.IsNullOrWhiteSpace(NextButtonText) ? WebFormFunctions.DefaultNextButtonText : NextButtonText) : LocalizedNextButtonText;
			SubmitButtonText = string.IsNullOrWhiteSpace(LocalizedSubmitButtonText) ? (string.IsNullOrWhiteSpace(SubmitButtonText) ? WebFormFunctions.DefaultSubmitButtonText : SubmitButtonText) : LocalizedSubmitButtonText;
			SubmitButtonBusyText = string.IsNullOrWhiteSpace(LocalizedSubmitButtonBusyText) ? (string.IsNullOrWhiteSpace(SubmitButtonBusyText) ? WebFormFunctions.DefaultSubmitButtonBusyText : SubmitButtonBusyText) : LocalizedSubmitButtonBusyText;

			MovePreviousPermitted = step.GetAttributeValue<bool?>("mspp_movepreviouspermitted") ?? true;

			NextStep = step.GetRelatedEntity(context, "mspp_webformstep_nextstep", EntityRole.Referencing);
			WebFormMetadata = step.GetRelatedEntities(context, "mspp_webformmetadata_webformstep");

			UserControlPath = step.GetAttributeValue<string>("mspp_usercontrolpath");
			PostBackUrl = step.GetAttributeValue<string>("mspp_postbackurl") ?? string.Empty;

			SetEntityReference = step.GetAttributeValue<bool?>("mspp_setentityreference") ?? false;
			RelationshipName = step.GetAttributeValue<string>("mspp_referenceentityrelationshipname") ?? string.Empty;
		}
	}
}
