using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Searching.Operations;
using MemberSuite.SDK.Types;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public abstract class ListControlManager<T> : SingleControlManager<T> where T : ListControl, new()
    {
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


            List<PickListEntry> entries = getPickListEntries();


            foreach (PickListEntry ple in entries)
            {
                if (!ple.IsActive)
                    continue;

                string text = ple.Text;
                string value = ple.Value;

                if (String.IsNullOrEmpty(text)) // try a resource
                    text = Host.ResolveResource(value, false);

                var li = _findItemsByValueCaseInsensitive(PrimaryControl.Items, value);
                bool add = false;

                if(li == null)
                {
                    li = new ListItem(text, value);
                    add = true;
                }

                if (ple.IsDefault  )
                {
                    PrimaryControl.ClearSelection();
                    li.Selected = true;
                }

                if(add)
                    PrimaryControl.Items.Add(li);
            }


            // now bind
            object obj = Host.Resolve(ControlMetadata);
            if (obj == null) return;
            PrimaryControl.ClearSelection();

            var l = obj as IList;
            if (l != null) // it's a list
                foreach (object o in l)
                    select(o);
            else
                select(Convert.ToString(obj)); // just select the single
        }

      
        protected override bool ShouldExpandLookupTable { get { return true; } }

        private void select(object o)
        {
            string val = Convert.ToString(o);

            if (val != null) // try to find
            {
                ListItem li = PrimaryControl.Items.FindByValue(val);

                if (li == null)
                {
                    li = new ListItem(val, val);
                    PrimaryControl.Items.Add(li);
                }

                li.Selected = true;
            }
            else
                PrimaryControl.SelectedValue = val; // set to null
        }
    }
}