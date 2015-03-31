using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Manifests.Searching;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Utilities;
using MemberSuite.SDK.Web.Controls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class TypeaheadControlManager : SingleControlManager<AlgoliaTypeaheadControl>
    {
        public const string AJAX_USETYPEAHEAD = "UseTypeaheadForAjaxComboBoxes";
        protected override MemberSuite.SDK.Web.Controls.AlgoliaTypeaheadControl instantiatePrimaryControl()
        {
            var c = base.instantiatePrimaryControl();

            c.EnableViewState = false;
            NameValueStringPair referenceType = determineReferenceType();


            if (string.IsNullOrWhiteSpace(referenceType.Name))
                return c;


            c.AlgoliaApplicationID = ConfigurationManager.AppSettings["algolia_application_id"];
            SearchManifest sm = Host.DescribeSearch(referenceType.Name, referenceType.Value);

            // set the index
            PrimaryControl.AlgoliaIndex = ClientSideSearch.GetNameOfQuickSearchClientSideIndex(ControlContext.CurrentAssociationKey, referenceType.Name );

            AssignAppropriateTagFilters(referenceType.Name);

            // hack - only show one column when we're in a popup mode
            if (HttpContext.Current.Handler.GetType().Name.Contains("dialogbox")
                &&
                // we can override this behavior with the ForceColumnDisplay variable
                (ControlMetadata == null ||
                 ControlMetadata.Properties == null ||
                 !ControlMetadata.Properties.Exists(
                     x =>
                         x.Name == "ForceColumnDisplay" &&
                         string.Equals(x.Expression, "true", StringComparison.CurrentCultureIgnoreCase)))
                )
            {
                string nameColumn =
                    PrimaryControl.Attributes[AjaxComboBoxControlManager.CONST_QUICKSEARCH_NAMECOLUMN] ?? "Name";
                PrimaryControl.Attributes[AjaxComboBoxControlManager.CONST_QUICKSEARCH_NAMECOLUMN] = nameColumn;

                sm.DefaultQuickSearchColumns = new List<SearchOutputColumn>()
                {
                    new SearchOutputColumn {Name = nameColumn, ColumnWidth = 300}
                };

                bool showLocalId;

                if (ControlMetadata.Properties != null &&
                    ControlMetadata.Properties.Exists(x => x.Name == AjaxComboBoxControlManager.CONST_QUICKSEARCH_SHOWLOCALID) &&
                    bool.TryParse(
                        ControlMetadata.Properties.Find(x => x.Name == AjaxComboBoxControlManager.CONST_QUICKSEARCH_SHOWLOCALID).Expression,
                        out showLocalId) && showLocalId)
                    sm.DefaultQuickSearchColumns.Add(new SearchOutputColumn {Name = "LocalID", ColumnWidth = 75});
            }

            c.SetQuickSearchColumns(sm);
            

            return c;
        }

        public void AssignAppropriateTagFilters(string nameOfTheSearch)
        {
            if ( ControlContext.CurrentUserType == msSystemWideUser.CLASS_NAME )
                return; // system wide users can always access anything


            switch (nameOfTheSearch)
            {
                case msAssociation.CLASS_NAME:
                case msUser.CLASS_NAME:
                    _assignAppropriateTagFiltersForAssociation();
                    break;
            }
        }

        private void _assignAppropriateTagFiltersForAssociation()
        {
            switch (ControlContext.CurrentUserType)
            {
                case msResellerUser.CLASS_NAME:
                    PrimaryControl.TagFilters += "reseller:" + ControlContext.CurrentResellerID;
                    break;

                case msCustomerUser.CLASS_NAME:
                    PrimaryControl.TagFilters += "customer:" + ControlContext.CurrentCustomerID;
                    break;
            }
        }

        public override void DataBind()
        {
            base.DataBind();

            object obj = Host.Resolve(ControlMetadata);

            try
            {
                PrimaryControl.SelectedValue = Convert.ToString(obj);
            }
            catch
            {
            }
        }

        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.SelectedValue);

            //and, let's set the text, just in case
            var mso = Host.Resolve(ControlMetadata);

            
            ControlMetadata textMeta = ControlMetadata.Clone();
            textMeta.DataSourceExpression += "_Name__transient";
            Host.SetModelValue(textMeta, PrimaryControl.SelectedText , true);
        }
 
    }
}
