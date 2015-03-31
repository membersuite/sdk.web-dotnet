using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Manifests.Searching;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Searching.Operations;
using MemberSuite.SDK.Types;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class AjaxComboBoxControlManager : ComboBoxControlManager
    {
        public const string CONST_COMBOBOX_SEARCHCONTEXT_ATTR = "SearchContext";
        public const string CONST_COMBOBOX_SEARCHTYPE_ATTR = "SearchType";
        private const int CONST_QUICKSEARCH_DROPDOWN_WIDTH = 815;
        public const string CONST_QUICKSEARCH_NAMECOLUMN = "NameColumn";
        public const string CONST_QUICKSEARCH_SHOWLOCALID = "ShowLocalID";


        protected override void addDefaultValue()
        {
            // no-op
        }

        protected override RadComboBox instantiatePrimaryControl()
        {
            RadComboBox cb = base.instantiatePrimaryControl();

            cb.EnableLoadOnDemand = true;

            cb.ItemRequestTimeout = 1000;
            cb.CausesValidation = false;
            cb.DropDownWidth = Unit.Pixel(CONST_QUICKSEARCH_DROPDOWN_WIDTH);
            cb.EnableVirtualScrolling = true;
            cb.EmptyMessage = "Enter search term";
            cb.ShowMoreResultsBox = true;
            cb.HighlightTemplatedItems = true;
            cb.ShowWhileLoading = true;
            cb.EnableScreenBoundaryDetection = true;

            // javascript - should always be on console.js
            cb.OnClientItemsRequesting = "onItemsRequesting";
            cb.OnClientDropDownOpening = "onDropDownOpening";
            cb.Height = Unit.Pixel(200);
            cb.ShowDropDownOnTextboxClick = false;

            cb.ItemsRequested += OnComboBoxItemsRequested;

            cb.EnableLoadOnDemand = true;



            // set up the search
            NameValueStringPair referenceType = determineReferenceType();
            if (!string.IsNullOrWhiteSpace(referenceType.Name))
            {


                PrimaryControl.Attributes[CONST_COMBOBOX_SEARCHTYPE_ATTR] = referenceType.Name;

                PrimaryControl.Attributes[CONST_COMBOBOX_SEARCHCONTEXT_ATTR] = referenceType.Value;
                //if (ControlMetadata != null && ControlMetadata.ReferenceContext != null)
                //     =
                //        View.ResolveComplexExpression(ControlMetadata.ReferenceContext);

                SearchManifest sm = Host.DescribeSearch(referenceType.Name, referenceType.Value);


                // hack - only show one column when we're in a popup mode
                if (HttpContext.Current.Handler.GetType().Name.Contains("dialogbox")
                    &&
                    // we can override this behavior with the ForceColumnDisplay variable
                    (ControlMetadata == null ||
                    ControlMetadata.Properties == null ||
                    !ControlMetadata.Properties.Exists(x => x.Name == "ForceColumnDisplay" && string.Equals(x.Expression, "true", StringComparison.CurrentCultureIgnoreCase)))
                    )
                {
                    string nameColumn = PrimaryControl.Attributes[CONST_QUICKSEARCH_NAMECOLUMN] ?? "Name";
                    PrimaryControl.Attributes[CONST_QUICKSEARCH_NAMECOLUMN] = nameColumn;

                    sm.DefaultQuickSearchColumns = new List<SearchOutputColumn>() { new SearchOutputColumn { Name = nameColumn, ColumnWidth = 300 } };

                    bool showLocalId;

                    if (ControlMetadata.Properties != null &&
                        ControlMetadata.Properties.Exists(x => x.Name == CONST_QUICKSEARCH_SHOWLOCALID) && bool.TryParse(ControlMetadata.Properties.Find(x => x.Name == CONST_QUICKSEARCH_SHOWLOCALID).Expression, out showLocalId) && showLocalId)
                        sm.DefaultQuickSearchColumns.Add(new SearchOutputColumn { Name = "LocalID", ColumnWidth = 75 });
                }

                cb.ItemTemplate = new QuickSearchTemplate(sm);
                cb.HeaderTemplate = new QuickSearchHeaderTemplate(sm, QuickSearchJustification.Center);
            }

            // hack


            return cb;
        }

        public override void DataBind()
        {
            base.DataBind();
            // set the search type
        }

        protected override void setValue(string valueToSet)
        {
            string text = valueToSet;
            NameValueStringPair pair = determineReferenceType();

            //if (pair.Name == "LookupTableRow")
            //{
            //    try
            //    {
            //        Search s = new Search("LookupTableRow") {Context = pair.Value};
            //        s.AddOutputColumn("Name");
            //        s.AddCriteria(Expr.Equals("Value", valueToSet));

            //        using(IConciergeAPIService api = GetServiceAPIProxy())
            //        {
            //            ConciergeResult<SearchResult> sr = api.ExecuteSearch(s, 0, 1);
            //            if (sr.Success)
            //                text = (string)sr.ResultValue.Table.Rows[0]["Name"];
            //        }
            //    }
            //    catch {}
            //}else
            //{
            // we have to set the value and get the name
            if (!string.IsNullOrWhiteSpace(valueToSet))
                try
                {
                    using (IConciergeAPIService api = GetServiceAPIProxy())
                    {
                        text = api.GetName(valueToSet).ResultValue;
                    }
                }
                catch
                {
                }
            //}

            // MS-5185 - we'll add an item for this
            PrimaryControl.Items.Clear();
            PrimaryControl.Items.Add(new RadComboBoxItem(text, valueToSet));
            PrimaryControl.SelectedIndex = 0;
            //PrimaryControl.Text = text;
            //PrimaryControl.SelectedValue = valueToSet;
        }

        public static string CleanSearchCriteria(string str)
        {
            var modifiedString = str.TrimStart().TrimEnd();

            return modifiedString;
        }

        public static void OnComboBoxItemsRequested(object o, RadComboBoxItemsRequestedEventArgs e)
        {
            var cb = o as RadComboBox;

            if (cb == null)
                return;

            string searchName = cb.Attributes[CONST_COMBOBOX_SEARCHTYPE_ATTR];
            string searchContext = cb.Attributes[CONST_COMBOBOX_SEARCHCONTEXT_ATTR];

            if (String.IsNullOrEmpty(searchName))
                return;

            const int NUMBER_OF_ITEMS = 20;
            SearchResult result;

            // MS-5411 (Modified 12/9/2014) Users should not be able to switch to associations to which they do not have access.
            List<string> accessibleAssociations = null;
            if (searchName == msAssociation.CLASS_NAME)
                accessibleAssociations = GetAccessibleAssociationForCustomerUser();

            var host = (IControlHost)HttpContext.Current.Handler;

            using (var api = host.GetServiceAPIProxy())
            {
                var requestText = string.IsNullOrEmpty(e.Text) ? "" : CleanSearchCriteria(e.Text);

                var quickSearch = api.GenerateQuickSearch(searchName, searchContext, requestText).ResultValue;

                if (accessibleAssociations != null && accessibleAssociations.Count > 0)
                {
                    var sogQueryText = new SearchOperationGroup();
                    sogQueryText.GroupType = SearchOperationGroupType.Or;
                    quickSearch.AddCriteria(sogQueryText);
                    foreach (var accessibleAssociation in accessibleAssociations)
                    {
                        if (!string.IsNullOrWhiteSpace(requestText) && !accessibleAssociation.Contains(requestText))
                            continue;

                        sogQueryText.Criteria.Add(new Contains
                        {
                            FieldName = "Name",
                            ValuesToOperateOn = new List<object> {accessibleAssociation}
                        });
                    }
                }                    

                result =  api.ExecuteSearch(quickSearch, e.NumberOfItems, NUMBER_OF_ITEMS).ResultValue;                
            }

            int endOfRow = e.NumberOfItems + NUMBER_OF_ITEMS;
            e.EndOfItems = result.TotalRowCount <= endOfRow;

            var nameColumn = cb.Attributes[CONST_QUICKSEARCH_NAMECOLUMN] ?? "Name";

            // MS-4429 - changing column names results in callback error
            if (!result.Table.Columns.Contains(nameColumn))
            {
                // ok, let's try to find the first column with "Name" in the name
                foreach (DataColumn c in result.Table.Columns)
                    if (c.ColumnName.Contains("Name"))
                    {
                        nameColumn = c.ColumnName;
                        break;
                    }

                if (nameColumn == null) // otherwise, just use the first column
                    nameColumn = result.Table.Columns[0].ColumnName;
            }
            foreach (DataRow row in result.Table.Rows)
            {
               

                var item = new RadComboBoxItem(row[nameColumn] == DBNull.Value ? "(empty)" : Convert.ToString(row[nameColumn]),
                                               row["ID"].ToString());


                foreach (DataColumn column in result.Table.Columns)
                    if (column.ColumnName != "ID" &&
                        column.ColumnName != nameColumn)
                    {
                        object dataRow = row[column];
                        //if (dataRow == DBNull.Value || dataRow == null || dataRow.ToString().Trim() == "")
                        //  dataRow = null ;

                        string valueAsString;

                        if (dataRow is decimal)
                        {

                            FieldMetadata fm = column.ExtendedProperties["FieldMetadata"] as FieldMetadata;
                            if (fm != null && fm.DataType == FieldDataType.Money)
                                valueAsString = ((decimal)dataRow).ToString("C");
                            else
                                valueAsString = ((decimal)dataRow).ToString("F2");
                        }

                        else
                            valueAsString = Convert.ToString(dataRow);

                        item.Attributes[column.ColumnName] = valueAsString;
                    }

                cb.Items.Add(item);
                cb.Attributes[CONST_QUICKSEARCH_NAMECOLUMN] = nameColumn;

                item.DataBind();
            }

            if (endOfRow > result.TotalRowCount)
                endOfRow = result.TotalRowCount;

            if (result.TotalRowCount == 0)
                e.Message = string.Format("No results found matching \"{0}\"", e.Text);
            else
                e.Message = string.Format("Showing 1-{0:N0} out of {1}",
                                          endOfRow, result.TotalRowCount);
        }

        private static List<string> GetAccessibleAssociationForCustomerUser()
        {
            if (HttpContext.Current == null
                || HttpContext.Current.Items.Count == 0
                ||!HttpContext.Current.Items.Contains("UIContext"))
            return null;       // Do not apply the filter

            dynamic context = HttpContext.Current.Items["UIContext"];

            // Ensure that the UIContext field exists
            if (context == null
                || context.CurrentUser == null)
                return null;   // Do not apply the filter

            var currentUser = context.CurrentUser as msUser;

            if (currentUser == null
                || !currentUser.IsActive)
                return null;   // Do not apply the filter

            // Find the CustomerUser that is currently using the console.
            // If the User that is currently using the console is not a CustomerUser,
            // e.g. the User is a SystemWideUser, then he/she should still be allowed to see
            // all associations.
            var search = new Search(msCustomerUser.CLASS_NAME);
            search.AddCriteria(Expr.Equals("ID", currentUser.ID));
            search.AddCriteria(Expr.Equals(msUser.FIELDS.IsActive, true));            
            search.AddOutputColumn(msCustomerUser.FIELDS.AccessibleAssociations);

            var host = (IControlHost)HttpContext.Current.Handler;

            msCustomerUser customerUser = null;

            var result = new List<string>();

            using (var api = host.GetServiceAPIProxy())
            {
                ConciergeResult<SearchResult> searchResult = api.ExecuteSearch(search, 0, 1);

                if (searchResult == null
                    || searchResult.ResultValue == null
                    || searchResult.ResultValue.Table.Rows.Count == 0)
                    return null;   // Do not apply the filter

                var conciergeResult = api.Get(Convert.ToString(searchResult.ResultValue.Table.Rows[0]["ID"]));
                if (conciergeResult != null
                    && conciergeResult.ResultValue != null)
                    customerUser = conciergeResult.ResultValue.ConvertTo<msCustomerUser>();

                if (customerUser == null)
                    return null;   // Do not apply the filter

                var accessibleAssociations = customerUser.AccessibleAssociations.ConvertAll(s => s.ToLower());                

                foreach (var accessibleAssociation in accessibleAssociations)
                {
                    var mso = api.Get(accessibleAssociation).ResultValue;
                    if (mso == null)
                        continue;
                    var name = mso.SafeGetValue<string>("Name");
                    if (string.IsNullOrEmpty(name))
                        continue;
                    result.Add(name);
                }
            }

            return result;
        }

        #region Quick Search Template

        public class QuickSearchTemplate : ITemplate
        {
            protected SearchManifest _manifest;

            public QuickSearchTemplate(SearchManifest manifest)
            {
                _manifest = manifest;
            }

            #region ITemplate Members

            public void InstantiateIn(Control container)
            {
                if (_manifest == null)
                    return;
                // we need to build the header row

                var ht = new HtmlTable();
                var r = new HtmlTableRow();
                ht.Rows.Add(r);

                ht.CellPadding = 0;
                ht.CellSpacing = 0;


                container.Controls.Add(ht); // add the table


                int width = 0;
                foreach (SearchOutputColumn column in _manifest.DefaultQuickSearchColumns)
                {
                    int columnWidth = column.ColumnWidth;
                    if (columnWidth <= 0)
                        columnWidth = 150; // default


                    width += columnWidth;
                    var tc = new HtmlTableCell();
                    tc.Attributes["style"] = string.Format("width: {0}px; padding: 0px !important;", columnWidth);
                    r.Cells.Add(tc);
                    var l = new Label();
                    tc.Controls.Add(l);
                    l.DataBinding += l_DataBinding;
                    l.Text = column.Name; // the text field is used by l_DataBinding to figure out the column name!

                    var spacer = new HtmlTableCell();
                    spacer.Attributes["style"] = "width: 3px; padding: 0px !important";
                    r.Cells.Add(spacer);
                }

                ht.Attributes["class"] = "quickSearchTable";
                ht.Attributes["style"] = string.Format("width: {0} px", width);
            }

            #endregion

            private void l_DataBinding(object sender, EventArgs e)
            {
                var target = (Label)sender;
                var item = (RadComboBoxItem)target.BindingContainer;

                var nameColumn = item.ComboBoxParent.Attributes[CONST_QUICKSEARCH_NAMECOLUMN] ?? "Name";

                string column = target.Text;
                switch (column)
                {


                    case "ID":
                        target.Text = item.Value;
                        break;

                    default:
                        if (column == nameColumn)
                        {
                            target.Text = item.Text;
                            break;
                        }
                        // we have to use GetIndexedPropertyValue vs. DataBinder.Eval b/c some of our
                        // columns may contain periods
                        // http://stackoverflow.com/questions/459168/use-databinder-eval-with-an-indexer-containing-a-period
                        target.Text =
                            (string)
                            DataBinder.GetIndexedPropertyValue(item, string.Format("Attributes[\"{0}\"]", column));
                        break;
                }
            }
        }

        #endregion

        #region Quick Header Search Template

        public enum QuickSearchJustification
        {
            Left,
            Right,
            Center
        }

        public class QuickSearchHeaderTemplate : ITemplate
        {
            protected SearchManifest _manifest;
            protected QuickSearchJustification _justification;

            public QuickSearchHeaderTemplate(SearchManifest manifest, QuickSearchJustification justification)
            {
                _manifest = manifest;
                _justification = justification;
            }

            #region ITemplate Members

            public void InstantiateIn(Control container)
            {
                if (_manifest == null)
                    return;

                // we need to build the header row

                string searchType = _manifest.SearchType;

                var rc = (RadComboBox)container.Parent;


                var ht = new HtmlTable();
                var r = new HtmlTableRow();
                ht.Rows.Add(r);

                ht.CellPadding = 0;
                ht.CellSpacing = 0;


                container.Controls.Add(ht); // add the table


                int width = 0;
                if (_manifest.DefaultQuickSearchColumns == null)
                {
                    _manifest.DefaultQuickSearchColumns = new List<SearchOutputColumn>();
                    _manifest.DefaultQuickSearchColumns.Add(new SearchOutputColumn { Name = "Name" });
                }

                foreach (SearchOutputColumn column in _manifest.DefaultQuickSearchColumns)
                {
                    int columnWidth = column.ColumnWidth;
                    if (columnWidth <= 0)
                        columnWidth = 150; // default

                    FieldMetadata f = null;

                    if (_manifest.Fields != null)
                        f = _manifest.Fields.Find(f1 => f1.Name == column.Name);


                    width += columnWidth;
                    var tc = new HtmlTableCell();
                    tc.Attributes["style"] = string.Format("width: {0}px; padding: 0px !important;", columnWidth);
                    r.Cells.Add(tc);
                    var l = new Label();
                    tc.Controls.Add(l);

                    string columnName = column.DisplayName;

                    if (columnName == null && f != null)
                        columnName = f.Label;

                    if (columnName == null)
                        columnName = column.Name;

                    l.Text = columnName;

                    var spacer = new HtmlTableCell();
                    spacer.Attributes["style"] = "width: 3px; padding: 0px !important";
                    r.Cells.Add(spacer);
                }

                ht.Attributes["style"] =
                    string.Format("text-align: left;  table-layout: fixed; padding: 0px !important; width: {0} px",
                                  width);
                rc.DropDownWidth = Unit.Pixel(width + 25);

                var cbWidth = (int)rc.Width.Value;
                var cbDropDownWidth = (int)rc.DropDownWidth.Value;

                switch (_justification)
                {
                    case QuickSearchJustification.Right:

                        rc.OffsetX = cbWidth - cbDropDownWidth;
                        break;

                    case QuickSearchJustification.Center:

                        // what's the width
                        if (cbDropDownWidth > cbWidth) // this is our base 
                            // then, let's center if
                            rc.OffsetX = (cbWidth - cbDropDownWidth) / 2;
                        break;
                }
            }

            #endregion
        }

        #endregion

        
    }
}