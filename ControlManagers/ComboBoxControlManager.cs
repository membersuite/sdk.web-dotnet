using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Types;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class ComboBoxControlManager : SingleControlManager<RadComboBox>
    {
        protected virtual bool SuppressPickListEntries
        {
            get { return false; }
        }

        protected override RadComboBox instantiatePrimaryControl()
        {
            RadComboBox cb = base.instantiatePrimaryControl();
            cb.AllowCustomText = true;
            cb.MarkFirstMatch = true;
            cb.ShowDropDownOnTextboxClick = false;
            cb.DropDownWidth = Unit.Pixel(300);
            cb.Width = Unit.Pixel(176);
        
            return cb;
        }

        //protected override void instantiateRequiredFieldValiator(List<Control> controls, string controlID)
        //{
        //    // we have to use a custom validator b/c of a known issue with the control
        //    if (IsRequired() && ControlMetadata.DisplayType != FieldDisplayType.Label)
        //    {
        //        var cv = new CustomValidator();
        //        cv.ServerValidate += cv_ServerValidate;
        //        //  cv.ClientValidationFunction = "validateRadComboBox"; // doesn't work, can't determine source

        //        cv.ControlToValidate = controlID;
        //        cv.ErrorMessage = string.Format("You have not entered a value for <B>{0}</B>.", GetLabel());
        //        cv.Display = ValidatorDisplay.None;
        //        // rfv.EnableClientScript = false;
        //        controls.Add(cv);
        //    }

        //    return;
        //}

        protected override bool ShouldExpandLookupTable
        {
            get { return true; }
        }
       
        private void cv_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !String.IsNullOrEmpty(PrimaryControl.SelectedValue);
        }

        public override void DataBind()
        {
            base.DataBind();

            if (ControlMetadata.AcceptableValuesDataSource != null)
            {
                PrimaryControl.DataSource = Host.ResolveAcceptableValues(ControlMetadata);
                PrimaryControl.DataTextField = ControlMetadata.AcceptableValuesDataTextField ?? "Name";
                PrimaryControl.DataValueField = ControlMetadata.AcceptableValuesDataValueField ?? "Value";
                PrimaryControl.DataBind();
            }
            else
            {

                if (!SuppressPickListEntries)
                {
                    List<PickListEntry> entries = getPickListEntries();

                    //Hack - dev data has massive lookup tables which is causing extreemly long load times and a flood of API calls
                    //Needs to be refactored to account for large numbers of picklistentries
                    if(entries.Count < 500)
                    foreach (PickListEntry ple in entries)
                    {
                        if (!ple.IsActive ||  _findItemsByValueCaseInsensitive( PrimaryControl.Items, ple.Value) != null)
                            continue;

                        string text = ple.Text;
                        string value = ple.Value;

                        if (String.IsNullOrEmpty(text)) // try a resource
                            text = Host.ResolveResource(value, false);

                        var li = new RadComboBoxItem(text, value);
                        if (ple.IsDefault)
                        {
                            PrimaryControl.ClearSelection();
                            li.Selected = true;
                        }

                        PrimaryControl.Items.Add(li);
                    }
                }
            }

            if (PrimaryControl == null)
                return;

            if (PrimaryControl.FindItemByValue("") == null)
                addDefaultValue();

            // combobox validates on the text in the box, NOT on the value
            // so we need to set the initial value to whatever text is shown
            // for a empty value
            if (RequiredFieldValidator != null )
            {
                var nullItem = PrimaryControl.FindItemByValue("");
                if (nullItem != null)
                    RequiredFieldValidator.InitialValue = nullItem.Text;
            }
         

            // now bind
            object obj = Host.Resolve(ControlMetadata);
            if (obj == null) obj = ControlMetadata.DefaultValue;
            if (obj != null)
            {
                PrimaryControl.ClearSelection();
                string val = Convert.ToString(obj);
                setValue(val);
            }
        }

        protected virtual void addDefaultValue()
        {
            
            string label = "---- Select ----";
            string nullValueLabel = ControlMetadata.NullValueLabel;

            // check the null value label
            if (nullValueLabel == null)
            {
                FieldMetadata obj = Host.GetBoundFieldFor(ControlMetadata);
                if (obj != null)
                    nullValueLabel = obj.NullValueLabel;
            }

            if (nullValueLabel != null)
            {
                if (nullValueLabel == "<showfirstvalue>") // show the first value
                    return;

                label = nullValueLabel;
            }

            PrimaryControl.Items.Insert(0, new RadComboBoxItem(label, ""));
        }


        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="valueToSet">The value to set.</param>
        protected virtual void setValue(string valueToSet)
        {
            if ( ! string.IsNullOrWhiteSpace( valueToSet ) ) // try to find
            {
                RadComboBoxItem li = PrimaryControl.FindItemByValue(valueToSet);

                if (li == null)
                {
                    li = new RadComboBoxItem(valueToSet, valueToSet);
                    PrimaryControl.Items.Add(li);
                }

                li.Selected = true;
            }
            else
                PrimaryControl.SelectedValue = valueToSet; // set to null
        }


        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.SelectedValue);

            //and, let's set the text, just in case
            var mso = Host.Resolve(ControlMetadata);

            string text = PrimaryControl.SelectedItem != null ? PrimaryControl.SelectedItem.Text : PrimaryControl.Text;
            ControlMetadata textMeta = ControlMetadata.Clone();
            textMeta.DataSourceExpression += "_Name__transient";
            Host.SetModelValue(textMeta, text, true);
        }
    }
}