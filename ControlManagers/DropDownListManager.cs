using System;
using System.Data;
using System.ServiceModel;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Interfaces;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Searching.Operations;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.WCF;
using Telerik.Web.UI;


namespace MemberSuite.SDK.Web.ControlManagers
{
    public class DropDownListManager : ComboBoxControlManager
    {
        protected override RadComboBox instantiatePrimaryControl()
        {
            RadComboBox cb = base.instantiatePrimaryControl();

            cb.AllowCustomText = false;
            cb.MarkFirstMatch = true;
            cb.ShowDropDownOnTextboxClick = true;
            cb.EnableTextSelection = false;

            setupLoadOnDemandIfLookupTablePresent(cb);
            setupLoadOnDemandIfExtensionServiceIsPresent(cb);





            return cb;
        }



        public override void DataBind()
        {
            if (_isUsingOnDemandAndIsRequired)
                ControlMetadata.NullValueLabel = "<showfirstvalue>";
            base.DataBind();
        }

        private bool _isUsingOnDemandAndIsRequired = false;

        private void setupLoadOnDemandIfLookupTablePresent(RadComboBox cb)
        {
            if (ControlMetadata == null) return;

            FieldMetadata meta = Host.GetBoundFieldFor(ControlMetadata);

            if (meta == null)
                return;

            if (string.IsNullOrWhiteSpace(meta.LookupTableID)) return;  // no lookup table
            if (!string.IsNullOrWhiteSpace(meta.ExtensionServiceID))    // then this takes precedence
                return;

            cb.EnableLoadOnDemand = true;
            cb.CausesValidation = false;
            cb.EnableScreenBoundaryDetection = true;

            cb.Height = Unit.Pixel(200);
            cb.ShowDropDownOnTextboxClick = false;

            cb.ItemsRequested += OnComboBoxItemsRequestedLookupTable;
            cb.OnClientDropDownOpening = "GetItemsIfNeeded";

            cb.EnableLoadOnDemand = true;
            cb.AllowCustomText = false;


            cb.Attributes["LookupTableID"] = meta.LookupTableID;
            cb.EnableVirtualScrolling = true;
            // MS-6019 (Modified 1/9/2015) Modified to account for whether or not the ondemand field's value is required
            _isUsingOnDemandAndIsRequired = cb.EnableLoadOnDemand && (meta.IsRequired || meta.IsRequiredInPortal);

        }
        const int NUM_ITEMS = 100;
        private void OnComboBoxItemsRequestedLookupTable(object o, RadComboBoxItemsRequestedEventArgs e)
        {
            var cb = o as RadComboBox;


            if (cb == null)
                return;

            string lookuptableID = cb.Attributes["LookupTableID"];
            if (string.IsNullOrWhiteSpace(lookuptableID)) return;

            using (var api = GetServiceAPIProxy())
            {
                Search s = new Search("LookupTableRow");
                s.Context = lookuptableID;

                if (!string.IsNullOrWhiteSpace(e.Text))
                    s.AddCriteria(Expr.Contains("Name", e.Text));

                s.AddOutputColumn("Name");
                s.AddOutputColumn("Value");
                s.AddSortColumn("Name");

                var result = api.ExecuteSearch(s, e.NumberOfItems, NUM_ITEMS).ResultValue;
                int endOfRow = e.NumberOfItems + NUM_ITEMS;
                e.EndOfItems = result.TotalRowCount <= endOfRow;

                foreach (DataRow dr in result.Table.Rows)
                {
                    RadComboBoxItem item = new RadComboBoxItem(Convert.ToString(dr["Name"]),
                        Convert.ToString(dr["Value"]));
                    cb.Items.Add(item);
                }

            }
        }

        private void setupLoadOnDemandIfExtensionServiceIsPresent(RadComboBox cb)
        {
            if (ControlMetadata == null) return;

            FieldMetadata meta = Host.GetBoundFieldFor(ControlMetadata);

            if (meta == null || string.IsNullOrWhiteSpace(meta.ExtensionServiceID))
                return;



            // now, the extension service
            _isUsingOnDemandAndIsRequired = true;

            msExtensionService es;
            using (var api = GetServiceAPIProxy())
            {
                var mso = api.Get(meta.ExtensionServiceID).ResultValue;
                if (mso == null)    // the service has been deleted
                    return;

                es = mso.ConvertTo<msExtensionService>();

            }

            cb.EnableLoadOnDemand = true;


            cb.CausesValidation = false;




            cb.EnableScreenBoundaryDetection = true;

            cb.Height = Unit.Pixel(200);
            cb.ShowDropDownOnTextboxClick = false;

            cb.ItemsRequested += OnComboBoxItemsRequested;
            cb.OnClientDropDownOpening = "GetItemsIfNeeded";

            cb.EnableLoadOnDemand = true;
            cb.AllowCustomText = false;


            cb.Attributes["ServiceUri"] = es.Uri;
            cb.Attributes["FieldName"] = meta.Name;




        }

        private void OnComboBoxItemsRequested(object o, RadComboBoxItemsRequestedEventArgs e)
        {
            var cb = o as RadComboBox;

            if (cb == null)
                return;

            string serviceUri = cb.Attributes["ServiceUri"];

            if (String.IsNullOrWhiteSpace(serviceUri))
                return;


            // get the owner object
            MemberSuiteObject obj = Host.Resolve(new Manifests.Command.ControlMetadata { DataSource = ControlMetadata.DataSource }) as MemberSuiteObject;
            if (obj == null) return;


            try
            {
                var binding = new BasicHttpBinding();
                var endpoint = new EndpointAddress(serviceUri);
                binding.OpenTimeout = TimeSpan.FromSeconds(5);
                binding.CloseTimeout = TimeSpan.FromSeconds(5);
                binding.ReceiveTimeout = TimeSpan.FromSeconds(5);

                //todo - put channel factories in caches
                var factory = new ChannelFactory<IExtensionService>(binding, endpoint);

                var channel = (IExtensionService)factory.CreateChannel();

                var nameValues = channel.PopulateDropdownList(obj.ClassType, cb.Attributes["RecordType"], obj);


                cb.Items.Clear();

                if (nameValues == null) return;
                //cb.Items.Add(new RadComboBoxItem("--- Select ---", null));



                foreach (var nv in nameValues)
                    cb.Items.Add(new RadComboBoxItem { Text = nv.Name, Value = nv.Value });

            }
            catch (UriFormatException)
            {
                throw new ConciergeClientException(ConciergeErrorCode.IllegalParameter,
                                             "Uri '{0}' could not verified as valid.", serviceUri);
            }


        }
    }
}