/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressCompositeControlTemplate.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;

	using Cms;
	using Globalization;
	using Resources;
	using WebControls;

	using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Metadata;

	/// <summary>
	/// Fullname control
	/// </summary>
	/// <seealso cref="Adxstudio.Xrm.Web.UI.CrmEntityFormView.CellTemplate" />
	/// <seealso cref="Adxstudio.Xrm.Web.UI.CrmEntityFormView.ICustomFieldControlTemplate" />
	public class AddressCompositeControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		/// <summary>
		/// Address Field alias
		/// </summary>
		private readonly string alias;

		/// <summary>
		/// The entity metadata
		/// </summary>
		private readonly EntityMetadata entityMetadata;

		/// <summary>
		/// The is editable
		/// </summary>
		private bool isEditable;

		/// <summary>
		/// Address Control Value
		/// </summary>
		private StringBuilder addressControlValue;

		/// <summary>
		/// Bind Control Value
		/// </summary>
		private readonly ICollection<Action<Entity>> bindControlValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="AddressCompositeControlTemplate"/> class.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="metadata">The metadata.</param>
		/// <param name="validationGroup">The validation group.</param>
		/// <param name="bindings">The bindings.</param>
		/// <param name="entityMetadata">The entity metadata.</param>
		/// <param name="mode">The mode.</param>
		public AddressCompositeControlTemplate(
			CrmEntityFormViewField field,
			FormXmlCellMetadata metadata,
			string validationGroup,
			IDictionary<string, CellBinding> bindings,
			EntityMetadata entityMetadata,
			FormViewMode? mode)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
			isEditable = mode != FormViewMode.ReadOnly;
			this.entityMetadata = entityMetadata;
			alias = ControlID.Split('_').First();
			bindControlValue = new List<Action<Entity>>();
		}

		/// <summary>
		/// CSS Class name assigned.
		/// </summary>
		public override string CssClass
		{
			get
			{
				return "text form-control";
			}
		}

		/// <summary>
		/// Gets the field.
		/// </summary>
		/// <value>
		/// The field.
		/// </value>
		public CrmEntityFormViewField Field { get; }

		/// <summary>
		/// Gets the validator display.
		/// </summary>
		/// <value>
		/// The validator display.
		/// </value>
		private ValidatorDisplay ValidatorDisplay
		{
			get
			{
				return string.IsNullOrWhiteSpace(Metadata.ValidationText) ? ValidatorDisplay.None : ValidatorDisplay.Dynamic;
			}
		}

		/// <summary>
		/// Instantiates the control in.
		/// </summary>
		/// <param name="container">The container.</param>
		protected override void InstantiateControlIn(Control container)
		{
			var contentId = Guid.NewGuid();
			if (Metadata.ReadOnly)
			{
				isEditable = false;
			}

			var addressTextBox = new TextBox
			{
				ID = ControlID,
				CssClass = string.Join(" ", "trigger", CssClass, Metadata.CssClass, "addressCompositeControl"),
				ToolTip = Metadata.ToolTip,
				TextMode = TextBoxMode.MultiLine
			};
			addressTextBox.Attributes.Add("data-composite-control", string.Empty);
			addressTextBox.Attributes.Add("data-content-id", contentId.ToString());
			addressTextBox.Attributes.Add("data-editable", isEditable.ToString());
			try
			{
				addressTextBox.Rows = checked((Metadata.RowSpan.GetValueOrDefault(2) * 3) - 2);
			}
			catch (OverflowException)
			{
				addressTextBox.Rows = 3;
			}
			

			container.Controls.Add(addressTextBox);

			// Creating container for all popover elements 
			var divContainer = new HtmlGenericControl("div");
			divContainer.Attributes["class"] = "content hide addressCompositeControlContainer";
			divContainer.ID = contentId.ToString();

			var addRangeControls =
				new Action<IEnumerable<Control>>(controls => { controls.ForEach(x => divContainer.Controls.Add(x)); });

			container.Controls.Add(divContainer);

			switch (CultureInfo.CurrentUICulture.LCID)
			{
				case LocaleIds.Japanese:
					addressTextBox.Attributes.Add(
						"data-content-template",
						string.Format("{{{0}_postalcode}}{{BREAK}}{{{0}_country}} {{{0}_stateorprovince}} {{{0}_city}}{{BREAK}}{{{0}_line1}} {{{0}_line2}}", ControlID));
					addressControlValue = new StringBuilder(addressTextBox.Attributes["data-content-template"]);
					MakePostalCode(addRangeControls);
					MakeCountry(addRangeControls);
					MakeState(addRangeControls);
					MakeCity(addRangeControls);
					MakeAddressLine1(addRangeControls);
					MakeAddressLine2(addRangeControls);
					break;
				case LocaleIds.ChineseSimplified:
				case LocaleIds.ChineseHongKong:
				case LocaleIds.ChineseTraditional:
				case LocaleIds.Korean:
					addressTextBox.Attributes.Add(
						"data-content-template",
						string.Format("{{{0}_postalcode}} {{{0}_country}}{{BREAK}}{{{0}_stateorprovince}} {{{0}_city}}{{BREAK}}{{{0}_line1}}", ControlID));
					addressControlValue = new StringBuilder(addressTextBox.Attributes["data-content-template"]);
					MakePostalCode(addRangeControls);
					MakeCountry(addRangeControls);
					MakeState(addRangeControls);
					MakeCity(addRangeControls);
					MakeAddressLine1(addRangeControls);
					break;
				default:
					addressTextBox.Attributes.Add(
						"data-content-template",
						string.Format("{{{0}_line1}} {{{0}_line2}} {{{0}_line3}}{{BREAK}}{{{0}_city}} {{{0}_stateorprovince}} {{{0}_postalcode}}{{BREAK}}{{{0}_country}}", ControlID));
					addressControlValue = new StringBuilder(addressTextBox.Attributes["data-content-template"]);
					MakeAddressLine1(addRangeControls);
					MakeAddressLine2(addRangeControls);
					MakeAddressLine3(addRangeControls);

					MakeCity(addRangeControls);

					MakeState(addRangeControls);
					MakePostalCode(addRangeControls);
					MakeCountry(addRangeControls);
					break;
			}
			
			var doneButton = new HtmlGenericControl("input");
			doneButton.Attributes["class"] = "btn btn-primary btn-block";
			doneButton.Attributes["role"] = "button";
			doneButton.ID = "popoverUpdateButton_" + Guid.NewGuid().ToString().Trim();
			doneButton.Attributes["readonly"] = "true";
			doneButton.Attributes.Add("value", ResourceManager.GetString("Composite_Control_Done"));

			addRangeControls(new[] { doneButton });
			var ariaLabelPattern = "{0}. {1}";
			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired)
			{
				addressTextBox.Attributes.Add("required", string.Empty);
				ariaLabelPattern = "{0}*. {1}";
			}

			if (isEditable)
			{
				addressTextBox.Attributes.Add("aria-label", string.Format(ariaLabelPattern, Metadata.Label, ResourceManager.GetString("Narrator_Label_For_Composite_Controls")));
			}
			else
			{
				addressTextBox.CssClass += " readonly ";
			}

			Bindings[ControlID] = new CellBinding
			{
				Get = () =>
				{
					var str = addressTextBox.Text;
					return str ?? string.Empty;
				},
				Set = obj =>
				{
					Entity entity = obj as Entity;

					foreach (var bindAction in bindControlValue)
					{
						bindAction(entity);
					}

					var textBoxValue = string.Join("\r\n",
						addressControlValue.ToString()
							.Split(new[] { "{BREAK}" }, StringSplitOptions.RemoveEmptyEntries)
							.Select(x => x.Trim()));


					addressTextBox.Text = textBoxValue;
				}
			};
		}

		/// <summary>
		/// Instantiates the validators in.
		/// </summary>
		/// <param name="container">The container.</param>
		protected override void InstantiateValidatorsIn(Control container)
		{
			if (Metadata.IsRequired || Metadata.WebFormForceFieldIsRequired || Metadata.IsFullNameControl)
			{
				container.Controls.Add(
					new RequiredFieldValidator
					{
						ID = string.Format("RequiredFieldValidator{0}", ControlID),
						ControlToValidate = ControlID,
						ValidationGroup = ValidationGroup,
						Display = ValidatorDisplay,
						ErrorMessage = ValidationSummaryMarkup(ValidationMessage()),
						Text = Metadata.ValidationText,
					});
			}

			this.InstantiateCustomValidatorsIn(container);
		}
		
		/// <summary>
		/// Make Field Address Line1
		/// </summary>
		/// <param name="addRangeControls">Add Corols Delegate</param>
		private void MakeAddressLine1(Action<IEnumerable<Control>> addRangeControls)
		{
			var line1 = MakeEditControls("_line1", "Address_Line_1_DefaultText");
			addRangeControls(line1);
		}

		/// <summary>
		/// Make Field Address Line2
		/// </summary>
		/// <param name="addRangeControls">Add Corols Delegate</param>
		private void MakeAddressLine2(Action<IEnumerable<Control>> addRangeControls)
		{
			var line2 = MakeEditControls("_line2", "Address_Line_2_DefaultText");
			addRangeControls(line2);
		}

		/// <summary>
		/// Make Field Address Line3
		/// </summary>
		/// <param name="addRangeControls">Add Corols Delegate</param>
		private void MakeAddressLine3(Action<IEnumerable<Control>> addRangeControls)
		{
			var line3 = MakeEditControls("_line3", "Address_Line_3_DefaultText");
			addRangeControls(line3);
		}

		/// <summary>
		/// Make Field City
		/// </summary>
		/// <param name="addRangeControls">Add Corols Delegate</param>
		private void MakeCity(Action<IEnumerable<Control>> addRangeControls)
		{
			var city = MakeEditControls("_city", "City_DefaultText");

			addRangeControls(city);
		}

		/// <summary>
		/// Make Field Country/Region
		/// </summary>
		/// <param name="addRangeControls">Add Corols Delegate</param>
		private void MakeCountry(Action<IEnumerable<Control>> addRangeControls)
		{
			var country = MakeEditControls("_country", "Country_DefaultText");
			addRangeControls(country);
		}

		/// <summary>
		/// Make Field Postal Code
		/// </summary>
		/// <param name="addRangeControls">Add Corols Delegate</param>
		private void MakePostalCode(Action<IEnumerable<Control>> addRangeControls)
		{
			var postalcode = MakeEditControls("_postalcode", "Zip_Postal_Code");
			addRangeControls(postalcode);
		}

		/// <summary>
		/// Make Field State
		/// </summary>
		/// <param name="addRangeControls">Add Corols Delegate</param>
		private void MakeState(Action<IEnumerable<Control>> addRangeControls)
		{
			var stateorprovince = MakeEditControls("_stateorprovince", "State_Province_DefaultText");
			addRangeControls(stateorprovince);
		}

		/// <summary>
		/// Generatefields the specified field name.
		/// </summary>
		/// <param name="fieldName">Name of Field.</param>
		/// <param name="labelName">Name of Label</param>
		/// <returns>Collection with Field and Label controls</returns>
		private IEnumerable<Control> MakeEditControls(string fieldName, string labelName)
		{
			var fieldMetaData = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == alias + fieldName);

			var controls = new List<Control>();
			var textBox = new TextBox
			{
				ID = string.Concat(ControlID, fieldName),
				CssClass = string.Concat(" content ", " ", CssClass, Metadata.CssClass),
				ToolTip = Metadata.ToolTip
			};

			var snippetName = "AddressCompositeControlTemplate/FieldLabel/" + labelName;

			var fieldLabelSnippet = new WebControls.Snippet
			{
				SnippetName = snippetName,
				DisplayName = snippetName,
				EditType = "text",
				Editable = true,
				DefaultText = ResourceManager.GetString(labelName),
				HtmlTag = HtmlTextWriterTag.Label
			};
			
			if (isEditable)
			{
				// Applying required parameters to first name if it's application required
				if (fieldMetaData != null
					&& (fieldMetaData.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired
						|| fieldMetaData.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired))
				{
					textBox.Attributes.Add("required", string.Empty);
					var requierdContainer = new HtmlGenericControl("div");
					requierdContainer.Attributes["class"] = "info required";
					requierdContainer.Controls.Add(fieldLabelSnippet);
					controls.Add(requierdContainer);
				}
				else
				{
					controls.Add(fieldLabelSnippet);
				}
			}
			controls.Add(textBox);
			textBox.Attributes.Add("onchange", "setIsDirty(this.id);");
			Bindings[ControlID + alias + fieldName] = new CellBinding
			{
				Get = () =>
				{
					var str = textBox.Text;
					return str != null ? str.Replace("\r\n", "\n") : string.Empty;
				},
				Set = obj =>
				{
					var entity = obj as Entity;

					if (entity != null)
					{
						textBox.Text =
							entity.GetAttributeValue<string>(
								alias + fieldName);
					}
				}
			};
			bindControlValue.Add(entity =>
			{
				addressControlValue.Replace("{" + textBox.ID + "}", entity.GetAttributeValue<string>(alias + fieldName));
			});
			return controls;
		}

		/// <summary>
		/// Validation Message
		/// </summary>
		/// <returns>string Validation Message</returns>
		private string ValidationMessage()
		{
			return string.IsNullOrWhiteSpace(Metadata.RequiredFieldValidationErrorMessage)
					   ? (Metadata.Messages == null || !Metadata.Messages.ContainsKey("Required"))
							 ? ResourceManager.GetString("Required_Field_Error").FormatWith(Metadata.Label)
							 : Metadata.Messages["Required"].FormatWith(Metadata.Label)
					   : Metadata.RequiredFieldValidationErrorMessage;
		}
	}
}
