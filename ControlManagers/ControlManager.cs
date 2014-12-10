using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Concierge;

using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Types;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.ControlManagers
{
    /// <summary>
    /// Responsible for instantiating a control, and performing one-way or two-way data binding
    /// </summary>
    public abstract class ControlManager
    {
        /// <summary>
        /// Gets the row span.
        /// </summary>
        /// <value>The row span.</value>
        /// <remarks>This is used by controls that take up multiple rows - 
        /// like the AddressControl.</remarks>
        public virtual int RowSpan
        {
            get { return 1; }
        }

        public ControlMetadata ControlMetadata { get; protected set; }
        protected IControlHost Host { get; private set; }


        /// <summary>
        /// Gets a value indicating whether supports two way databinding.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [supports two way databinding]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Most controls do - some (like labels), do not.</remarks>
        public virtual bool SupportsTwoWayDatabinding
        {
            get { return true; }
        }

        public virtual bool CustomLabel
        {
            get { return false; }
        }

        public bool IsInPortal { get; set; }

        public short? TabIndex { get; set; }

        public void Initialize(IControlHost controlHost, ControlMetadata controlSpec)
        {
            if (controlHost == null) throw new ArgumentNullException("controlHost");

            ControlMetadata = controlSpec;
            Host = controlHost;
        }

        /// <summary>
        /// Responsible for instantitating the controls necessary for the provided
        /// control specification. Uses <param name="controlAction"></param> for postprocessing.
        /// </summary>
        /// <returns></returns>
        public abstract List<Control> InstantiateWithPostprocessing(Action<List<Control>> controlAction);

        /// <summary>
        /// Responsible for instantitating the controls necessary for the provided
        /// control specification..
        /// </summary>
        /// <returns></returns>
        public abstract List<Control> Instantiate();

        public virtual List<Control> InstantiateValidationControls()
        {
            return new List<Control>();
        }

        public virtual void DataBind()
        {
        }

        public virtual void DataUnbind()
        {
        }

        /// <summary>
        /// Gets the label for this control
        /// </summary>
        /// <returns></returns>
        public string GetLabel()
        {
            string label = null;
            // ok - is there an explicit label specified?
            if (ControlMetadata.Label != null)
                return processLabel(ControlMetadata.Label);


            // is one specified by ID?
            if (ControlMetadata.ID != null)
            {
                string idLabel = Host.ResolveResource(ControlMetadata.ID, true);
                if (idLabel != null)
                    return idLabel;
            }


            // lastly, try the actual metadata
            FieldMetadata fMeta = Host.GetBoundFieldFor(ControlMetadata);

            if (fMeta != null)
                // we found it!
                label = processLabel(String.IsNullOrWhiteSpace(fMeta.PortalPrompt) ? fMeta.Label : fMeta.PortalPrompt);


            return label ?? "";
        }

        protected virtual string processLabel(string label)
        {
            if (!String.IsNullOrEmpty(label) && !label.EndsWith("?") && !label.EndsWith(":") && !label.EndsWith("!"))
                label += ":";

            return label;
        }


        /// <summary>
        /// Incorporates the specified control
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>False if this isn't possible</returns>
        public abstract bool Use(Control c);

        protected virtual RequiredFieldValidator instantiateRequiredFieldValiator(List<Control> controls, string controlID)
        {
            if (IsRequired() && ControlMetadata.DisplayType != FieldDisplayType.Label)
            {
                var rfv = new RequiredFieldValidator();
                
                rfv.ControlToValidate = controlID;
                
                // IMPORTANT - many UI components depend on this convention - that the required field validator
                // is rfv_ + the control ID. If you change it, you will break good amount of UI code.
                rfv.ID = "rfv_" + controlID.Replace(".","_");

                if (ControlMetadata.ErrorMessage_RequiredField != null )
                    rfv.ErrorMessage = ControlMetadata.ErrorMessage_RequiredField;
                else
                {
                    string label = GetLabel();
                    if (label != null)
                    {
                        string fieldName = label.Trim().TrimEnd(':');
                        if (String.IsNullOrWhiteSpace(fieldName))
                            fieldName = "one or more required fields.";
                        rfv.ErrorMessage = String.Format("You have not entered a value for {0}.", fieldName);
                    }
                    else
                        rfv.ErrorMessage = "One or more values have not been specified.";
                }
                rfv.Display = ValidatorDisplay.None;
                
                return rfv;
            }

            return null;

             
        }

        public virtual bool IsRequired()
        {
            // control meta comes first
            if (ControlMetadata != null && ControlMetadata.IsRequired != null)
                return ControlMetadata.IsRequired.Value;

            FieldMetadata fMeta = Host.GetBoundFieldFor(ControlMetadata);

            bool isRequired = false;
            if (fMeta != null)
            {
                isRequired = fMeta.IsRequired;

                // now, are we in the portal?
                if (IsInPortal && !isRequired)
                    isRequired = fMeta.IsRequiredInPortal;
            }


            return isRequired;
        }

        protected virtual List<PickListEntry> getPickListEntries()
        {
            // what about any associated with metadata?
            FieldMetadata meta = Host.GetBoundFieldFor(ControlMetadata);

            var entries = new List<PickListEntry>();

            if (meta != null && meta.PickListEntries != null)
                entries.AddRange(meta.PickListEntries);

            // now, lets add any explicit values - but NOT dupes
            if (ControlMetadata.PickListEntries != null)
                foreach (var pe in ControlMetadata.PickListEntries)
                {
                    entries.RemoveAll(x => x.Value == pe.Value); // de-dupe
                    entries.Add(pe);
                }


            if (ShouldExpandLookupTable)
            {
                // now - are we dealing with a lookup table? If so, we have to get the values directly from the api
                 
                if (meta == null || meta.LookupTableID == null)
                    return entries;

                // let's go get them
                using (var api = GetServiceAPIProxy())
                {
                    Search s = new Search("LookupTableRow");
                    s.Context = meta.LookupTableID;
                    s.AddOutputColumn("Name");
                    s.AddOutputColumn("Value");
                    SearchResult result;

                    // now, we can only get 500 records at a time from search, so we have to keep going until we get it
                    do
                    {
                        result = api.ExecuteSearch(s, entries.Count, null).ResultValue;
                        entries.AddRange(from DataRow dr in result.Table.Rows select new PickListEntry(Convert.ToString(dr["Name"]), Convert.ToString(dr["Value"])));
                    } while (result.TotalRowCount > entries.Count);
                }

            }


            return entries;
        }

        protected virtual bool ShouldExpandLookupTable
        {
            get { return false; }
            
        }

        protected IConciergeAPIService GetServiceAPIProxy()
        {
            return Host.GetServiceAPIProxy();
        }

        protected ListItem _findItemsByValueCaseInsensitive(ListItemCollection items, string valueToFind)
        {
            if (items == null) return null;

            foreach (ListItem item in items)
                if (String.Equals(item.Value, valueToFind, StringComparison.CurrentCultureIgnoreCase))
                    return item;

            return null;
        }

        protected RadComboBoxItem _findItemsByValueCaseInsensitive(RadComboBoxItemCollection items, string valueToFind)
        {
            if (items == null) return null;

            foreach (RadComboBoxItem item in items)
                if (String.Equals(item.Value, valueToFind, StringComparison.CurrentCultureIgnoreCase))
                    return item;

            return null;
        }
    }
}