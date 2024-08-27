/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Adxstudio.Xrm.Partner;
using Adxstudio.Xrm.Web.UI.JsonConfiguration;
using Adxstudio.Xrm.Web.UI.WebForms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using GridMetadata = Adxstudio.Xrm.Web.UI.JsonConfiguration.GridMetadata;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	/// <summary>
	/// Metadata derived from the form XML pertaining to the specific cell.
	/// </summary>
	public class FormXmlCellMetadata : Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView.FormXmlCellMetadata
	{
		private static readonly Guid QuickformClassId = new Guid("5C5600E0-1D6E-4205-A272-BE80DA87FD42");

		private readonly AttributeMetadata _attributeMetadata;
		private readonly string _controlID;
		private readonly string _dataFieldName;
		private readonly string _webResourceAltText;
		private readonly bool _webResourceBorder;
		private readonly string _webResourceData;
		private readonly int? _webResourceHeight;
		private readonly string _webResourceHorizontalAlignment;
		private readonly bool _webResourceIsHtml;
		private readonly bool _webResourcePassParameters;
		private readonly string _webResourceScrolling;
		private readonly bool _webResourceSecurity;
		private readonly string _webResourceSizeType;
		private readonly string _webResourceUrl;
		private readonly string _webResourceVerticalAlignment;
		private readonly int? _webResourceWidth;
		private readonly Guid _classID;
		private readonly string _validationText;
		private readonly Guid _lookupViewID;
		private readonly bool _forceAllFieldsRequired;
		private readonly string _viewID;
		private readonly string _viewRelationshipName;
		private readonly string _viewTargetEntityType;
		private readonly bool _viewEnableQuickFind;
		private readonly bool _viewEnableViewPicker;
		private readonly string _viewIds;
		private readonly int? _viewRecordsPerPage;
		private readonly bool _isSubgrid;
		private readonly int? _notesPageSize;
		private readonly int? _sharePointGridPageSize;
		private readonly bool _lookupDisableQuickFind;
		private readonly bool _lookupDisableViewPicker;
		private readonly bool _lookupAllowFilterOff;
		private readonly string _lookupAvailableViewIds;
		private readonly string _lookupFilterRelationshipName;
		private readonly string _lookupDependentAttributeName;
		private readonly string _lookupDependentAttributeType;

		/// <summary>
		/// Default text displayed in the field's validator control.
		/// </summary>
		public readonly string DefaultValidationText = "*";

		/// <summary>
		/// Indicates if the control is read-only.
		/// </summary>
		public bool ReadOnly { get; set; }
		/// <summary>
		/// The control's label text.
		/// </summary>
		public new string Label { get; set; }

		/// <summary>
		/// The parent control so cell templates can access properties on the control.
		/// </summary>
		public WebControls.CrmEntityFormView FormView { get; set; }

		/// <summary>
		/// Custom Metadata derived from the entity form metadata for the specified cell.
		/// </summary>
		/// <param name="cellNode"></param>
		/// <param name="entityMetadata"></param>
		/// <param name="languageCode"></param>
		/// <param name="toolTipEnabled"></param>
		/// <param name="recommendedFieldsRequired"></param>
		/// <param name="validationText"></param>
		/// <param name="webFormMetadata"></param>
		/// <param name="forceAllFieldsRequired"></param>
		/// <param name="enableValidationSummaryLinks"></param>
		/// <param name="validationSummaryLinkText"></param>
		/// <param name="messages"> </param>
		public FormXmlCellMetadata(XNode cellNode, EntityMetadata entityMetadata, int languageCode, bool? toolTipEnabled, bool? recommendedFieldsRequired, string validationText, IEnumerable<Entity> webFormMetadata, bool? forceAllFieldsRequired, bool? enableValidationSummaryLinks, string validationSummaryLinkText, Dictionary<string, string> messages, int baseOrganizationLanguageCode = 0)
			: base(cellNode, entityMetadata, languageCode)
		{
			TargetEntityName = entityMetadata.LogicalName;

			TargetEntityPrimaryKeyName = entityMetadata.PrimaryIdAttribute;
			
			TargetEntityPrimaryAttributeName = entityMetadata.PrimaryNameAttribute;

			Messages = messages ?? new Dictionary<string, string>();

			EnableValidationSummaryLinks = enableValidationSummaryLinks ?? true;

			ValidationSummaryLinkText = string.IsNullOrWhiteSpace(validationSummaryLinkText) ? string.Empty : validationSummaryLinkText;

			_forceAllFieldsRequired = forceAllFieldsRequired ?? false;

			StateOptionSetOptions = new OptionMetadataCollection();

			StatusOptionSetOptions = new OptionMetadataCollection();

			Label = base.Label;

			if (string.IsNullOrWhiteSpace(Label))
			{
				string label;
				cellNode.TryGetLanguageSpecificLabelValue(LanguageCode, out label, baseOrganizationLanguageCode);
				Label = label;
			}

			if (!cellNode.TryGetAttributeValue("control", "id", out _controlID))
			{
				return;
			}

			string classIdString;

			if (cellNode.TryGetAttributeValue("control", "classid", out classIdString))
			{
				_classID = new Guid(classIdString);
			}
			
			bool readOnly;

			if (cellNode.TryGetBooleanAttributeValue("control", "disabled", out readOnly))
			{
				// Preserve any existing true value of Disabled.
				ReadOnly = readOnly;
			}
			
			bool visible;

			if (!cellNode.TryGetBooleanAttributeValue(".", "visible", out visible))
			{
				visible = true; // The control is visible by default.
			}

			Disabled = !visible; // if not visible then disabled

			if (validationText != null) _validationText = validationText;

			string defaultTabId;
			cellNode.TryGetElementValue("control/parameters/DefaultTabId", out defaultTabId);

			if (_controlID == "notescontrol" && (!string.IsNullOrEmpty(defaultTabId) && defaultTabId == "ActivitiesTab"))
			{
				IsActivityTimelineControl = true;

				cellNode.TryGetIntegerAttributeValue(".", "rowspan", out _notesPageSize);

				// The base class will disable this cell because there's no datafieldname.
				// The notes control, however, cannot be disabled so we can just set disabled to false. 
				Disabled = false;

				if (webFormMetadata != null)
				{
					var timelineWebFormMetadata = webFormMetadata.FirstOrDefault(wfm => wfm.GetAttributeValue<OptionSetValue>("mspp_type") != null && wfm.GetAttributeValue<OptionSetValue>("mspp_type").Value == 756150000);

					if (timelineWebFormMetadata != null)
					{
						var timelineSettingsJson = timelineWebFormMetadata.GetAttributeValue<string>("mspp_timeline_settings");

						if (!string.IsNullOrWhiteSpace(timelineSettingsJson))
						{
							try
							{
								TimelineSettings = JsonConvert.DeserializeObject<TimelineMetadata>(timelineSettingsJson,
									new JsonSerializerSettings { ContractResolver = JsonConfigurationContractResolver.Instance, TypeNameHandling = TypeNameHandling.Objects, Binder = new ActionSerializationBinder() });
							}
							catch (Exception e)
							{
								ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("FormXmlCellMetadata Constructor {0}", e));
                            }
						}
					}
				}
			}
			else if (_controlID == "notescontrol")
			{
				IsNotesControl = true;

				cellNode.TryGetIntegerAttributeValue(".", "rowspan", out _notesPageSize);

				// The base class will disable this cell because there's no datafieldname.
				// The notes control, however, cannot be disabled so we can just set disabled to false. 
				Disabled = false;

				if (webFormMetadata != null)
				{
					var notesWebFormMetadata = webFormMetadata.FirstOrDefault(wfm => wfm.GetAttributeValue<OptionSetValue>("mspp_type") != null && wfm.GetAttributeValue<OptionSetValue>("mspp_type").Value == 100000005);

					if (notesWebFormMetadata != null)
					{
						var notesSettingsJson = notesWebFormMetadata.GetAttributeValue<string>("mspp_notes_settings");

						if (!string.IsNullOrWhiteSpace(notesSettingsJson))
						{
							try
							{
								NotesSettings = JsonConvert.DeserializeObject<JsonConfiguration.NotesMetadata>(notesSettingsJson,
									new JsonSerializerSettings { ContractResolver = JsonConfigurationContractResolver.Instance, TypeNameHandling = TypeNameHandling.Objects, Binder = new ActionSerializationBinder() });
							}
							catch (Exception e)
							{
								ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("FormXmlCellMetadata Constructor {0}", e));
                            }
						}
					}
				}
			}
			else if (_controlID.StartsWith("WebResource_"))
			{
				IsWebResource = true;

				// HACK: There is no data in the formxml that tells us what the web resource type is,
				// so we're using a very fragile logical deduction based on the parameters passed.
				_webResourceIsHtml = cellNode.TryGetBooleanElementValue("control/parameters/Border", out _webResourceBorder);
				WebResourceIsImage = cellNode.TryGetElementValue("control/parameters/HorizontalAlignment", out _webResourceHorizontalAlignment);
				WebResourceIsSilverlight = cellNode.TryGetBooleanElementValue("control/parameters/PassParameters", out _webResourcePassParameters) && !_webResourceIsHtml;
				
				cellNode.TryGetElementValue("control/parameters/AltText", out _webResourceAltText);
				cellNode.TryGetElementValue("control/parameters/Data", out _webResourceData);
				cellNode.TryGetIntegerElementValue("control/parameters/Height", out _webResourceHeight);
				cellNode.TryGetElementValue("control/parameters/Scrolling", out _webResourceScrolling);
				cellNode.TryGetBooleanElementValue("control/parameters/Security", out _webResourceSecurity);
				cellNode.TryGetElementValue("control/parameters/SizeType", out _webResourceSizeType);
				cellNode.TryGetElementValue("control/parameters/Url", out _webResourceUrl);
				cellNode.TryGetElementValue("control/parameters/VerticalAlignment", out _webResourceVerticalAlignment);
				cellNode.TryGetIntegerElementValue("control/parameters/Width", out _webResourceWidth);
			}
			else if (cellNode.TryGetElementValue("control/parameters/ViewId", out _viewID))
			{
				// The base class will disable this cell because there's no datafieldname.
				// The subgrid control, however, cannot be disabled so we can just set disabled to false.
				Disabled = false;

				cellNode.TryGetElementValue("control/parameters/RelationshipName", out _viewRelationshipName);

				cellNode.TryGetElementValue("control/parameters/TargetEntityType", out _viewTargetEntityType);

				cellNode.TryGetBooleanElementValue("control/parameters/EnableQuickFind", out _viewEnableQuickFind);

				cellNode.TryGetBooleanElementValue("control/parameters/EnableViewPicker", out _viewEnableViewPicker);

				cellNode.TryGetIntegerElementValue("control/parameters/RecordsPerPage", out _viewRecordsPerPage);

				cellNode.TryGetElementValue("control/parameters/ViewIds", out _viewIds);

				if (_viewTargetEntityType == "sharepointdocumentlocation")
				{
					IsSharePointDocuments = true;

					cellNode.TryGetIntegerAttributeValue(".", "rowspan", out _sharePointGridPageSize);
				}
				else
				{
					_isSubgrid = true;
				}

				if (_isSubgrid && webFormMetadata != null)
				{
					string subgridName;
					if (cellNode.TryGetAttributeValue("control", "id", out subgridName))
					{
						var subgridWebFormMetadata = webFormMetadata.FirstOrDefault(wfm => wfm.GetAttributeValue<string>("mspp_subgrid_name") == subgridName);

						if (subgridWebFormMetadata != null)
						{
							var subgridSettingsJson = subgridWebFormMetadata.GetAttributeValue<string>("mspp_subgrid_settings");

							if (!string.IsNullOrWhiteSpace(subgridSettingsJson))
							{
								try
								{
									SubgridSettings = JsonConvert.DeserializeObject<GridMetadata>(subgridSettingsJson,
										new JsonSerializerSettings
										{
											ContractResolver = JsonConfigurationContractResolver.Instance,
											TypeNameHandling = TypeNameHandling.Objects,
											Converters = new List<JsonConverter> { new GuidConverter() },
											Binder = new ActionSerializationBinder(),
											NullValueHandling = NullValueHandling.Ignore
										});
								}
								catch (Exception e)
								{
									ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("FormXmlCellMetadata Constructor {0}", e));
                                }
							}
						}
					}
				}
			}
			else if (_classID == QuickformClassId) // QuickForm
			{
				IsQuickForm = true;
				cellNode.TryGetAttributeValue("control", "datafieldname", out _dataFieldName);
				if (string.IsNullOrWhiteSpace(_dataFieldName))
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, "QuickForm XML is invalid. The attribute datafieldname value is missing or is null.");
                    return;
				}

				string quickFormsString;
				cellNode.TryGetElementValue("control/parameters/QuickForms", out quickFormsString);
				if (string.IsNullOrWhiteSpace(quickFormsString))
				{
                    ADXTrace.Instance.TraceError(TraceCategory.Application, "QuickForm XML is invalid. The parameter QuickForms is missing or is null.");
                    return;
				}
				var quickFormsIdsXml = XDocument.Parse(quickFormsString);
				var quickFormIdElements = quickFormsIdsXml.Descendants("QuickFormId");
				var quickFormIds = new List<CrmQuickForm.QuickFormId>();
				
				foreach (var quickFormIdElement in quickFormIdElements)
				{
					Guid quickFormId;
					string quickFormIdString;
					string entityName;
					quickFormIdElement.TryGetAttributeValue(".", "entityname", out entityName);
					quickFormIdElement.TryGetElementValue(".", out quickFormIdString);
					if (string.IsNullOrWhiteSpace(entityName))
					{
                        ADXTrace.Instance.TraceError(TraceCategory.Application, "QuickForm XML is invalid. The element QuickFormId is missing or is null.");
                        continue;
					}
					if (string.IsNullOrWhiteSpace(quickFormIdString))
					{
                        ADXTrace.Instance.TraceError(TraceCategory.Application, "QuickForm XML is invalid. The element QuickFormId is missing or is null.");
                        continue;
					}
					if (!Guid.TryParse(quickFormIdString, out quickFormId))
					{
                        ADXTrace.Instance.TraceError(TraceCategory.Application, "QuickForm XML is invalid. The element QuickFormId is not a valid Guid.");
                        continue;
					}
					quickFormIds.Add(new CrmQuickForm.QuickFormId(entityName, quickFormId));
				}
				if (!quickFormIds.Any()) return;
				QuickForm = new CrmQuickForm(_dataFieldName, quickFormIds.ToArray());
			}
			else if (_controlID == "fullname")
			{
				IsFullNameControl = true;
			}
			else if (_controlID.StartsWith("address") && _controlID.EndsWith("composite"))
			{
				IsAddressCompositeControl = true;
			}
			else
			{
				if (!cellNode.TryGetAttributeValue("control", "datafieldname", out _dataFieldName))
				{
					return;
				}

				if (webFormMetadata != null)
				{
					webFormMetadata = webFormMetadata.ToList();

					var attributeWebFormMetadata =
						webFormMetadata.FirstOrDefault(wfm => wfm.GetAttributeValue<string>("mspp_attributelogicalname") == _dataFieldName);

					if (attributeWebFormMetadata != null)
					{
						if (attributeWebFormMetadata.GetAttributeValue<OptionSetValue>("mspp_type") != null
							&& attributeWebFormMetadata.GetAttributeValue<OptionSetValue>("mspp_type").Value == 100000000
							&& attributeWebFormMetadata.GetAttributeValue<EntityReference>("mspp_entityformforcreate") != null
							&& attributeWebFormMetadata.GetAttributeValue<OptionSetValue>("statuscode") != null
							&& attributeWebFormMetadata.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)Enums.EntityFormStatusCode.Active)
							LookupReferenceEntityFormId = attributeWebFormMetadata.GetAttributeValue<EntityReference>("mspp_entityformforcreate").Id;

						GroupName = attributeWebFormMetadata.GetAttributeValue<string>("mspp_groupname") ?? string.Empty;

						CssClass = attributeWebFormMetadata.GetAttributeValue<string>("mspp_cssclass") ?? string.Empty;

						var overrideLabel = attributeWebFormMetadata.GetAttributeValue<string>("mspp_label");

						if (!string.IsNullOrWhiteSpace(overrideLabel))
						{
							var label = Localization.GetLocalizedString(overrideLabel, LanguageCode);
							if (!string.IsNullOrEmpty(label))
							{
								Label = label;
							}
						}

						var controlStyle = attributeWebFormMetadata.GetAttributeValue<OptionSetValue>("mspp_controlstyle");

						if (controlStyle != null)
						{
							switch (controlStyle.Value)
							{
								case (int)WebFormMetadata.ControlStyle.VerticalRadioButtonList:
									ControlStyle = WebFormMetadata.ControlStyle.VerticalRadioButtonList;
									break;
								case (int)WebFormMetadata.ControlStyle.HorizontalRadioButtonList:
									ControlStyle = WebFormMetadata.ControlStyle.HorizontalRadioButtonList;
									break;
								case (int)WebFormMetadata.ControlStyle.GeolocationLookupValidator:
									ControlStyle = WebFormMetadata.ControlStyle.GeolocationLookupValidator;
									GeolocationValidatorErrorMessage =
										Localization.GetLocalizedString(
											attributeWebFormMetadata.GetAttributeValue<string>("mspp_geolocationvalidatorerrormessage"), LanguageCode);
									break;
								case (int)WebFormMetadata.ControlStyle.ConstantSum:
									ControlStyle = WebFormMetadata.ControlStyle.ConstantSum;
									ConstantSumAttributeNames =
										webFormMetadata.Where(
											w =>
												w.GetAttributeValue<OptionSetValue>("mspp_controlstyle") != null &&
												w.GetAttributeValue<OptionSetValue>("mspp_controlstyle").Value ==
												(int)WebFormMetadata.ControlStyle.ConstantSum && w.GetAttributeValue<string>("mspp_groupname") == GroupName)
											.Select(w => w.GetAttributeValue<string>("mspp_attributelogicalname"))
											.ToArray();
									break;
								case (int)WebFormMetadata.ControlStyle.RankOrderNoTies:
									ControlStyle = WebFormMetadata.ControlStyle.RankOrderNoTies;
									break;
								case (int)WebFormMetadata.ControlStyle.RankOrderAllowTies:
									ControlStyle = WebFormMetadata.ControlStyle.RankOrderAllowTies;
									break;
								case (int)WebFormMetadata.ControlStyle.MultipleChoiceMatrix:
									ControlStyle = WebFormMetadata.ControlStyle.MultipleChoiceMatrix;
									break;
								case (int)WebFormMetadata.ControlStyle.MultipleChoice:
									ControlStyle = WebFormMetadata.ControlStyle.MultipleChoice;
									break;
								case (int)WebFormMetadata.ControlStyle.StackRank:
									ControlStyle = WebFormMetadata.ControlStyle.StackRank;
									break;
								case (int)WebFormMetadata.ControlStyle.LookupDropdown:
									ControlStyle = WebFormMetadata.ControlStyle.LookupDropdown;
									break;
							}
						}

						IgnoreDefaultValue = attributeWebFormMetadata.GetAttributeValue<bool?>("mspp_ignoredefaultvalue") ?? false;

						AddDescription = attributeWebFormMetadata.GetAttributeValue<bool?>("mspp_adddescription") ?? false;

						var useAttributeDescription =
							attributeWebFormMetadata.GetAttributeValue<bool?>("mspp_useattributedescriptionproperty") ?? false;

						if (useAttributeDescription)
						{
							var localizedDescription =
								AttributeMetadata.Description.LocalizedLabels.SingleOrDefault(label => label.LanguageCode == LanguageCode);

							if (localizedDescription != null)
							{
								Description = localizedDescription.Label;
							}
						}
						else
						{
							Description =
								Localization.GetLocalizedString(attributeWebFormMetadata.GetAttributeValue<string>("mspp_description"),
									LanguageCode);
						}

						var descriptionPosition = attributeWebFormMetadata.GetAttributeValue<OptionSetValue>("mspp_descriptionposition");

						if (descriptionPosition != null)
						{
							switch (descriptionPosition.Value)
							{
								case (int)WebFormMetadata.DescriptionPosition.AboveControl:
									DescriptionPosition = WebFormMetadata.DescriptionPosition.AboveControl;
									break;
								case (int)WebFormMetadata.DescriptionPosition.BelowControl:
									DescriptionPosition = WebFormMetadata.DescriptionPosition.BelowControl;
									break;
								case (int)WebFormMetadata.DescriptionPosition.AboveLabel:
									DescriptionPosition = WebFormMetadata.DescriptionPosition.AboveLabel;
									break;
							}
						}

						MinMultipleChoiceSelectedCount =
							attributeWebFormMetadata.GetAttributeValue<int?>("mspp_minmultiplechoiceselectedcount") ?? 0;

						MaxMultipleChoiceSelectedCount =
							attributeWebFormMetadata.GetAttributeValue<int?>("mspp_maxmultiplechoiceselectedcount") ?? 0;

						ConstantSumMinimumTotal = attributeWebFormMetadata.GetAttributeValue<int?>("mspp_constantsumminimumtotal") ?? 0;

						ConstantSumMaximumTotal = attributeWebFormMetadata.GetAttributeValue<int?>("mspp_constantsummaximumtotal") ?? 100;

						RandomizeOptionSetValues = attributeWebFormMetadata.GetAttributeValue<bool?>("mspp_randomizeoptionsetvalues") ??
						                            false;

						WebFormForceFieldIsRequired = attributeWebFormMetadata.GetAttributeValue<bool?>("mspp_fieldisrequired") ?? false;

						RequiredFieldValidationErrorMessage =
							Localization.GetLocalizedString(
								attributeWebFormMetadata.GetAttributeValue<string>("mspp_requiredfieldvalidationerrormessage"), LanguageCode);

						ValidationRegularExpression =
							attributeWebFormMetadata.GetAttributeValue<string>("mspp_validationregularexpression") ?? string.Empty;

						ValidationRegularExpressionErrorMessage =
							Localization.GetLocalizedString(
								attributeWebFormMetadata.GetAttributeValue<string>("mspp_validationregularexpressionerrormessage"), LanguageCode);

						ValidationErrorMessage =
							Localization.GetLocalizedString(
								attributeWebFormMetadata.GetAttributeValue<string>("mspp_validationerrormessage"), LanguageCode);

						RangeValidationErrorMessage =
							Localization.GetLocalizedString(
								attributeWebFormMetadata.GetAttributeValue<string>("mspp_rangevalidationerrormessage"), LanguageCode);

						ConstantSumValidationErrorMessage =
							Localization.GetLocalizedString(
								attributeWebFormMetadata.GetAttributeValue<string>("mspp_constantsumvalidationerrormessage"), LanguageCode);

						RankOrderNoTiesValidationErrorMessage =
							Localization.GetLocalizedString(
								attributeWebFormMetadata.GetAttributeValue<string>("mspp_rankordernotiesvalidationerrormessage"), LanguageCode);

						MultipleChoiceValidationErrorMessage =
							Localization.GetLocalizedString(
								attributeWebFormMetadata.GetAttributeValue<string>("mspp_multiplechoicevalidationerrormessage"), LanguageCode);
					}
				}

				// Not all the necessary attribute metadata properties were provided in the CellMetadata so
				// we need to do some additional retreival and processing here
				var attributeMetadata =
					_attributeMetadata =
					entityMetadata.Attributes.FirstOrDefault(attribute => attribute.LogicalName == _dataFieldName);

				if (attributeMetadata == null)
				{
					return;
				}

				switch (attributeMetadata.AttributeType)
				{
					case AttributeTypeCode.Lookup:
					{
						string lookupViewID;

						if (cellNode.TryGetElementValue("control/parameters/DefaultViewId", out lookupViewID))
						{
							Guid.TryParse(lookupViewID, out _lookupViewID);
						}

						cellNode.TryGetBooleanElementValue("control/parameters/DisableQuickFind", out _lookupDisableQuickFind);

						cellNode.TryGetBooleanElementValue("control/parameters/DisableViewPicker", out _lookupDisableViewPicker);

						cellNode.TryGetBooleanElementValue("control/parameters/AllowFilterOff", out _lookupAllowFilterOff);

						cellNode.TryGetElementValue("control/parameters/AvailableViewIds", out _lookupAvailableViewIds);

						cellNode.TryGetElementValue("control/parameters/FilterRelationshipName", out _lookupFilterRelationshipName);

						cellNode.TryGetElementValue("control/parameters/DependentAttributeName", out _lookupDependentAttributeName);

						cellNode.TryGetElementValue("control/parameters/DependentAttributeType", out _lookupDependentAttributeType);

						var lookupAttributeMetadata = attributeMetadata as LookupAttributeMetadata;

						if (lookupAttributeMetadata == null)
						{
							return;
						}

						LookupTargets = lookupAttributeMetadata.Targets;
					}
						break;
					case AttributeTypeCode.Customer:
					{
						var lookupAttributeMetadata = attributeMetadata as LookupAttributeMetadata;

						if (lookupAttributeMetadata == null)
						{
							return;
						}

						LookupTargets = lookupAttributeMetadata.Targets;
					}
						break;
					case AttributeTypeCode.Owner:
					{
						ReadOnly = true;

						var lookupAttributeMetadata = attributeMetadata as LookupAttributeMetadata;

						if (lookupAttributeMetadata == null)
						{
							return;
						}

						LookupTargets = lookupAttributeMetadata.Targets;
					}
						break;
					case AttributeTypeCode.Boolean:
					{
						var booleanAttributeMetadata = attributeMetadata as BooleanAttributeMetadata;

						cellNode.TryGetAttributeValue("control", "datafieldname", out _dataFieldName);

						if (booleanAttributeMetadata == null)
						{
							return;
						}

						string classIDstring;

						cellNode.TryGetAttributeValue("control", "classid", out classIDstring);

						_classID = new Guid(classIDstring);

						BooleanOptionSetMetadata = booleanAttributeMetadata.OptionSet;

						DefaultValue = booleanAttributeMetadata.DefaultValue;
					}
						break;
					case AttributeTypeCode.String:
					{
						var stringAttributeMetadata = attributeMetadata as StringAttributeMetadata;

						if (stringAttributeMetadata == null)
						{
							return;
						}

						MaxLength = stringAttributeMetadata.MaxLength ?? 0;
					}
						break;
					case AttributeTypeCode.Picklist:
					{
						var picklistAttributeMetadata = attributeMetadata as PicklistAttributeMetadata;

						if (picklistAttributeMetadata == null)
						{
							return;
						}

						DefaultValue = picklistAttributeMetadata.DefaultFormValue;
					}
						break;
					case AttributeTypeCode.DateTime:
					{
						var dateTimeAttributeMetadata = attributeMetadata as DateTimeAttributeMetadata;

						if (dateTimeAttributeMetadata == null)
						{
							return;
						}

						DateTimeBehavior = dateTimeAttributeMetadata.DateTimeBehavior;
					}
						break;
					case AttributeTypeCode.Decimal:
					{
						var decimalAttributeMetadata = attributeMetadata as DecimalAttributeMetadata;

						if (decimalAttributeMetadata == null)
						{
							return;
						}

						Precision = decimalAttributeMetadata.Precision;
					}
						break;
					case AttributeTypeCode.Double:
					{
						var doubleAttributeMetadata = attributeMetadata as DoubleAttributeMetadata;

						if (doubleAttributeMetadata == null)
						{
							return;
						}

						Precision = doubleAttributeMetadata.Precision;
					}
						break;
					case AttributeTypeCode.Money:
					{
						var moneyAttributeMetadata = attributeMetadata as MoneyAttributeMetadata;

						if (moneyAttributeMetadata == null)
						{
							return;
						}

						IsBaseCurrency = moneyAttributeMetadata.IsBaseCurrency.GetValueOrDefault();
						Precision = moneyAttributeMetadata.Precision;
						PrecisionSource = moneyAttributeMetadata.PrecisionSource;
					}
						break;
					case AttributeTypeCode.Memo:
					{
						var memoAttributeMetadata = attributeMetadata as MemoAttributeMetadata;
						if (memoAttributeMetadata == null)
						{
							return;
						}

						MaxLength = memoAttributeMetadata.MaxLength ?? 0;
					}
						break;
					case AttributeTypeCode.State:
					{
						LabelNotAssociated = true;
						ReadOnly = true;
						var stateAttributeMetadata = attributeMetadata as StateAttributeMetadata;
						if (stateAttributeMetadata == null)
						{
							return;
						}
						StateOptionSetOptions = stateAttributeMetadata.OptionSet.Options;
					}
						break;
					case AttributeTypeCode.Status:
					{
						LabelNotAssociated = true;
						ReadOnly = true;
						var statusAttributeMetadata = attributeMetadata as StatusAttributeMetadata;
						if (statusAttributeMetadata == null)
						{
							return;
						}
						StatusOptionSetOptions = statusAttributeMetadata.OptionSet.Options;
					}
						break;
				}

				ToolTip = toolTipEnabled != false ? base.ToolTip : null;

				if (recommendedFieldsRequired == true &&
				    (attributeMetadata.RequiredLevel.Value == AttributeRequiredLevel.Recommended))
				{
					IsRequired = true;
				}
				else if (_forceAllFieldsRequired)
				{
					IsRequired = true;
				}
				else
				{
					IsRequired = base.IsRequired;
				}
			}
		}

		///<summary>
		/// Stores the BooleanOptionSetMetadata for an attribute of type boolean.
		///</summary>
		public BooleanOptionSetMetadata BooleanOptionSetMetadata { get; private set; }

		/// <summary>
		/// Tooltip text of the control.
		/// </summary>
		public new string ToolTip
		{
			get; private set;
		}

		/// <summary>
		/// Date Time Behaviour of the control.
		/// </summary>
		public DateTimeBehavior DateTimeBehavior
		{
			get; private set;
		}

		/// <summary>
		/// Validation text displayed next to the control when validation fails. Default value is '*'.
		/// </summary>
		public string ValidationText
		{
			get { return _validationText ?? DefaultValidationText; }
		}

		/// <summary>
		/// Denotes if the field is required to contain a value.
		/// </summary>
		public new bool IsRequired
		{
			get; set;
		}

		/// <summary>
		/// The id of the control.
		/// </summary>
		public virtual string ControlID
		{
			get { return _controlID; }
		}

		/// <summary>
		/// Class id of the cell in the form XML.
		/// </summary>
		public virtual Guid ClassID
		{
			get { return _classID;  }
		}

		/// <summary>
		/// Gets whether the attribute represents the base currency or the transaction currency (if this is a currency attribute).
		/// </summary>
		public virtual bool IsBaseCurrency { get; private set; }

		/// <summary>
		/// Indicates if the cell contains a notes control.
		/// </summary>
		public virtual bool IsNotesControl { get; }

		/// <summary>
		/// Indicates if the cell contains a full name control.
		/// </summary>
		public virtual bool IsFullNameControl { get; }

		/// <summary>
		/// Indicates if the cell contains a Address Composite Control.
		/// </summary>
		public virtual bool IsAddressCompositeControl { get; }

		/// <summary>
		/// Indicates if the cell contains an activity timeline control.
		/// </summary>
		public virtual bool IsActivityTimelineControl { get; }

		/// <summary>
		/// Indicates if the cell contains a web resource.
		/// </summary>
		public virtual bool IsWebResource { get; }

		/// <summary>
		/// Array of targets of a given lookup control.
		/// </summary>
		public string[] LookupTargets { get; private set; }

		/// <summary>
		/// Maximum length of the control value.
		/// </summary>
		public int MaxLength { get; private set; }

		/// <summary>
		/// Precision of a Decimal Number, Floating Point Number or Currency field.
		/// </summary>
		public int? Precision { get; private set; }

		/// <summary>
		/// Gets the precision source for the attribute.
		/// </summary>
		public int? PrecisionSource { get; private set; }

		/// <summary>
		/// Alternate Text of the web resource.
		/// </summary>
		public virtual string WebResourceAltText
		{
			get { return _webResourceAltText; }
		}

		/// <summary>
		/// Indicates if a border should be rendered on the web resource container.
		/// </summary>
		public virtual bool WebResourceBorder
		{
			get { return _webResourceBorder; }
		}

		/// <summary>
		/// The data of the web resource.
		/// </summary>
		public virtual string WebResourceData
		{
			get { return _webResourceData; }
		}

		/// <summary>
		/// The height of the web resource container.
		/// </summary>
		public virtual int? WebResourceHeight
		{
			get { return _webResourceHeight; }
		}

		/// <summary>
		/// Horizontal alignment of the web resource.
		/// </summary>
		public virtual string WebResourceHorizontalAlignment
		{
			get { return _webResourceHorizontalAlignment; }
		}

		/// <summary>
		/// Indicates if the web resource type is HTML.
		/// </summary>
		public virtual bool WebResourceIsHtml
		{
			get { return _webResourceIsHtml; }
		}

		/// <summary>
		/// Indicates if the web resource type is Image.
		/// </summary>
		public virtual bool WebResourceIsImage { get; }

		/// <summary>
		/// Indicates if the web resource type is Silverlight.
		/// </summary>
		public virtual bool WebResourceIsSilverlight { get; }

		/// <summary>
		/// Indicates if the web resource passes parameters.
		/// </summary>
		public virtual bool WebResourcePassParameters
		{
			get { return _webResourcePassParameters; }
		}

		/// <summary>
		/// Scrolling of the web resource.
		/// </summary>
		public virtual string WebResourceScrolling
		{
			get { return _webResourceScrolling; }
		}

		/// <summary>
		/// Security of the web resource.
		/// </summary>
		public virtual bool WebResourceSecurity
		{
			get { return _webResourceSecurity; }
		}

		/// <summary>
		/// Size type of the web resource.
		/// </summary>
		public virtual string WebResourceSizeType
		{
			get { return _webResourceSizeType; }
		}

		/// <summary>
		/// URL of the web resource.
		/// </summary>
		public virtual string WebResourceUrl
		{
			get { return _webResourceUrl; }
		}

		/// <summary>
		/// Vertical alignment of the web resource.
		/// </summary>
		public virtual string WebResourceVerticalAlignment
		{
			get { return _webResourceVerticalAlignment; }
		}

		/// <summary>
		/// Width of the web resource.
		/// </summary>
		public virtual int? WebResourceWidth
		{
			get { return _webResourceWidth; }
		}

		/// <summary>
		/// Default value of the field.
		/// </summary>
		public object DefaultValue { get; private set; }

		/// <summary>
		/// Web Form Metadata property used to specify a control style for a field to provide advanced functionality.
		/// </summary>
		public virtual WebFormMetadata.ControlStyle ControlStyle { get; }

		/// <summary>
		/// Web Form Metadata property used to specify the error message to be displayed when the geolocation validator validation fails.
		/// </summary>
		public string GeolocationValidatorErrorMessage { get; }

		/// <summary>
		/// The id of the saved query view used to retrieve data to populate the lookup field. An Empty Guid indicates we need to find the default view.
		/// </summary>
		public Guid LookupViewID
		{
			get { return _lookupViewID; }
		}

		/// <summary>
		/// Web Form metadata provides the option to ignore a field's default value. Useful for Two Option control radio buttons.
		/// </summary>
		public bool IgnoreDefaultValue { get; }

		/// <summary>
		/// Web Form Metadata property used to indicate a description should be added.
		/// </summary>
		public bool AddDescription { get; }

		/// <summary>
		/// Web Form Metadata property used to add a description or special instructions for a field.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Web Form Metadata property used to specify the position of the description relative to the position of the field is is associated with.
		/// </summary>
		public WebFormMetadata.DescriptionPosition DescriptionPosition { get; }

		/// <summary>
		/// Web Form Metadata property used to make a field required.
		/// </summary>
		public bool WebFormForceFieldIsRequired { get; }

		/// <summary>
		/// Web Form Metadata property used to add a custom error message for a field's Required Field Validator.
		/// </summary>
		public string RequiredFieldValidationErrorMessage { get; }

		/// <summary>
		/// Web Form Metadata property used to add a Regular Expression Validator for a field.
		/// </summary>
		public string ValidationRegularExpression { get; }

		/// <summary>
		/// Web Form Metadata property used to add an error message for the Regular Expression Validator for a field.
		/// </summary>
		public string ValidationRegularExpressionErrorMessage { get; }

		/// <summary>
		/// Web Form Metadata property used to add an error message for the Range Validator for a field.
		/// </summary>
		public string RangeValidationErrorMessage { get; }

		/// <summary>
		/// Web Form Metadata property used to add an error message for the Custom Validator for a field.
		/// </summary>
		public string ValidationErrorMessage { get; }

		/// <summary>
		/// Web Form Metadata property used to add an error message for the Constant Sum Custom Validator.
		/// </summary>
		public string ConstantSumValidationErrorMessage { get; }

		/// <summary>
		/// Web Form Metadata property used to add an error message for the Rank Order No Ties Custom Validator.
		/// </summary>
		public string RankOrderNoTiesValidationErrorMessage { get; }

		/// <summary>
		/// Web Form Metadata property used to add an error message for the Multiple Choice Custom Validator.
		/// </summary>
		public string MultipleChoiceValidationErrorMessage { get; }

		/// <summary>
		/// Web Form Metadata property used to group fields by adding a CSS class name to like fields to allow custom processing.
		/// </summary>
		public string GroupName { get; }

		/// <summary>
		/// Array of attribute names of the constant sum group
		/// </summary>
		public string[] ConstantSumAttributeNames { get; }

		/// <summary>
		/// Randomizes the rendering order of the option set values.
		/// </summary>
		public bool RandomizeOptionSetValues { get; }

		/// <summary>
		/// Minimum required number of multiple choice check boxes in a group that must be selected.
		/// </summary>
		public int MinMultipleChoiceSelectedCount { get; }

		/// <summary>
		/// Maximum allowable number of multiple choice check boxes in a group that can be selected.
		/// </summary>
		public int MaxMultipleChoiceSelectedCount { get; }

		/// <summary>
		/// Enable or disable the rendering of anchor links in the validation summary.
		/// </summary>
		public bool EnableValidationSummaryLinks { get; }

		/// <summary>
		/// Validation summary render hyperlinks to control anchors using this text.
		/// </summary>
		public string ValidationSummaryLinkText { get; }

		/// <summary>
		/// Minimum allowable total of the constant sum.
		/// </summary>
		public int ConstantSumMinimumTotal { get; }

		/// <summary>
		/// Maximum allowable total of the constant sum.
		/// </summary>
		public int ConstantSumMaximumTotal { get; }

		/// <summary>
		/// Web Form Metadata property used to add a CSS class name(s) to a field.
		/// </summary>
		public string CssClass { get; }

		/// <summary>
		/// Collecton of Format Strings specified in the CrmEntityFormView templating used for validation messages.
		/// </summary>
		public Dictionary<string, string> Messages { get; }

		/// <summary>
		/// Indicates if the cell contains a SharePoint documents control.
		/// </summary>
		public virtual bool IsSharePointDocuments { get; }

		/// <summary>
		/// Indicates if the cell contains a subgrid control.
		/// </summary>
		public virtual bool IsSubgrid
		{
			get { return _isSubgrid; }
		}

		/// <summary>
		/// The ID of the savedquery view for the subgrid control.
		/// </summary>
		public virtual string ViewID
		{
			get { return _viewID; }
		}

		/// <summary>
		/// The relationship name of the savedquery view for the subgrid control.
		/// </summary>
		public virtual string ViewRelationshipName
		{
			get { return _viewRelationshipName; }
		}

		/// <summary>
		/// The entity type of the savedquery view for the subgrid control.
		/// </summary>
		public virtual string ViewTargetEntityType
		{
			get { return _viewTargetEntityType; }
		}

		/// <summary>
		/// Indicates if the quick find search is enabled or not on the savedquery view for the subgrid control.
		/// </summary>
		public virtual bool ViewEnableQuickFind
		{
			get { return _viewEnableQuickFind; }
		}

		/// <summary>
		/// Indicates if the view picker is enabled or not on the savedquery view for the subgrid control.
		/// </summary>
		public virtual bool ViewEnableViewPicker
		{
			get { return _viewEnableViewPicker; }
		}

		/// <summary>
		/// The IDs of the savedquery views available for the view picker for the subgrid control.
		/// </summary>
		public virtual string ViewIds
		{
			get { return _viewIds; }
		}

		/// <summary>
		/// The number of records per page of the savedquery view for the subgrid control.
		/// </summary>
		public virtual int? ViewRecordsPerPage
		{
			get { return _viewRecordsPerPage; }
		}

		/// <summary>
		/// The logical name of the form's target entity.
		/// </summary>
		public virtual string TargetEntityName { get; }

		/// <summary>
		/// The logical name of the form's target entity primary key.
		/// </summary>
		public virtual string TargetEntityPrimaryKeyName { get; }

		/// <summary>
		/// The logical name of the form's target entity primary attribute.
		/// </summary>
		public virtual string TargetEntityPrimaryAttributeName { get; }

		/// <summary>
		/// The settings of a subgrid.
		/// </summary>
		public virtual GridMetadata SubgridSettings { get; }

		/// <summary>
		/// The settings of a notes control.
		/// </summary>
		public virtual JsonConfiguration.NotesMetadata NotesSettings { get; }

		/// <summary>
		/// The settings of a timeline control.
		/// </summary>
		public virtual TimelineMetadata TimelineSettings { get; }

		/// <summary>
		/// The number of notes per page to display for a notes control.
		/// </summary>
		public virtual int? NotesPageSize
		{
			get { return _notesPageSize; }
		}

		/// <summary>
		/// The settings of a SharePoint grid.
		/// </summary>
		public virtual SharePointGridMetadata SharePointSettings { get; }

		/// <summary>
		/// The number of folders and files per page to display for a SharePoint grid.
		/// </summary>
		public virtual int? SharePointGridPageSize
		{
			get { return _sharePointGridPageSize; }
		}

		/// <summary>
		/// Indicates if the quick find search is enabled or not on the savedquery view for the lookup control.
		/// </summary>
		public virtual bool LookupDisableQuickFind
		{
			get { return _lookupDisableQuickFind; }
		}

		/// <summary>
		///  Indicates if the view picker is enabled or not on the savedquery view for the lookup control.
		/// </summary>
		public virtual bool LookupDisableViewPicker
		{
			get { return _lookupDisableViewPicker; }
		}

		/// <summary>
		/// Indicates if the users can toggle the filter on the savedquery view for the lookup control.
		/// </summary>
		public virtual bool LookupAllowFilterOff
		{
			get { return _lookupAllowFilterOff; }
		}

		/// <summary>
		/// A comma delimited list of ID's of savedquery views that to allow a user to select from views on the lookup control.
		/// </summary>
		public virtual string LookupAvailableViewIds
		{
			get { return _lookupAvailableViewIds; }
		}

		/// <summary>
		/// The relationship schema name used to build a filter on a savedquery view for the lookup control.
		/// </summary>
		public virtual string LookupFilterRelationshipName
		{
			get { return _lookupFilterRelationshipName; }
		}

		/// <summary>
		/// The attribute logical name of the property that contains the ID of the record to be applied to build the filter on a savedquery view for the lookup control.
		/// </summary>
		public virtual string LookupDependentAttributeName
		{
			get { return _lookupDependentAttributeName; }
		}

		/// <summary>
		/// The entity logical name of used to build the filter on a savedquery view for the lookup control.
		/// </summary>
		public virtual string LookupDependentAttributeType
		{
			get { return _lookupDependentAttributeType; }
		}

		/// <summary>
		/// The OptionSetValues of the state option set.
		/// </summary>
		public virtual OptionMetadataCollection StateOptionSetOptions { get; }

		/// <summary>
		/// The OptionSetValues of the status reason option set.
		/// </summary>
		public virtual OptionMetadataCollection StatusOptionSetOptions { get; }

		/// <summary>
		/// Indicates if the label should not be associated to a control
		/// </summary>
		public virtual bool LabelNotAssociated { get; }

		/// <summary>
		/// Indicates if the cell contains a quick form
		/// </summary>
		public virtual bool IsQuickForm { get; }

		/// <summary>
		/// The definition of the quick form
		/// </summary>
		public virtual CrmQuickForm QuickForm { get; }

		/// <summary>
		/// Gets whether the value can be set when a record is created.
		/// </summary>
		public virtual bool IsValidForCreate
		{
			get { return _attributeMetadata == null || _attributeMetadata.IsValidForCreate.GetValueOrDefault(); }
		}

		/// <summary>
		/// Gets whether the value can be updated.
		/// </summary>
		public virtual bool IsValidForUpdate
		{
			get { return _attributeMetadata == null || _attributeMetadata.IsValidForUpdate.GetValueOrDefault(); }
		}

		/// <summary>
		/// Gets the lookup reference entity form id
		/// </summary>
		public virtual Guid? LookupReferenceEntityFormId { get; }
	}
}
