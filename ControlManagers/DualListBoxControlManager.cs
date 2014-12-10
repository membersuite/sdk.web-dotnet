using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Utilities;
using MemberSuite.SDK.Web.Controls;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class DualListBoxControlManager : SingleControlManager<DualListBox>
    {
        public override bool CustomLabel
        {
            get { return true; }
        }

        protected override DualListBox instantiatePrimaryControl()
        {
            DualListBox dlb = base.instantiatePrimaryControl();
            dlb.Width = Unit.Pixel(240);
            //dlb.LeftLabel = "Available Items";
            //dlb.RightLabel = "Selected Items";

            return dlb;
        }

        public override void DataBind()
        {
            base.DataBind();

            if (ControlMetadata.AcceptableValuesDataSource != null)
            {
                PrimaryControl.Source.DataSource = Host.ResolveAcceptableValues(ControlMetadata);
                PrimaryControl.Source.DataTextField = ControlMetadata.AcceptableValuesDataTextField ?? "Name";
                PrimaryControl.Source.DataValueField = ControlMetadata.AcceptableValuesDataValueField ?? "Value";
                PrimaryControl.DataBind();
            }

            List<PickListEntry> entries = getPickListEntries();


            foreach (PickListEntry ple in entries)
            {
                if (!ple.IsActive)
                    continue;

                string text = ple.Text;
                string value = ple.Value;

                if (String.IsNullOrEmpty(text)) // try a resource
                    text = Host.ResolveResource(value, false );


                var li = new RadListBoxItem(text, value);
                PrimaryControl.Source.Items.Add(li);

                if (ple.IsDefault)
                    PrimaryControl.Source.Transfer(li, PrimaryControl.Source, PrimaryControl.Destination);
            }

            // now bind
            object obj = Host.Resolve(ControlMetadata);
            var l = obj as IList;
            if (l != null)
            {
                foreach (object o in l)
                {
                    string val = Convert.ToString(o);

                    RadListBoxItem li = PrimaryControl.Source.FindItemByValue(val);

                    // ms-3954
                    if (li == null && Regex.IsMatch(val, RegularExpressions.GuidRegex, RegexOptions.Compiled))
                        li = PrimaryControl.Source.FindItemByValue(val.ToLower() );

                    if (li == null)
                    {
                        li = new RadListBoxItem(val, val);
                        PrimaryControl.Source.Items.Add(li);
                    }

                    PrimaryControl.Source.Transfer(li, PrimaryControl.Source, PrimaryControl.Destination);
                }
            }
        }

        protected override bool ShouldExpandLookupTable { get { return true; } }
        public override bool CanBeValidated { get { return false; } }

        public override void DataUnbind()
        {
            base.DataUnbind();

            var selectedItems = new List<string>();
            foreach (RadListBoxItem item in PrimaryControl.Destination.Items)
                selectedItems.Add(item.Value);
            Host.SetModelValue(ControlMetadata, selectedItems);
        }
    }
}