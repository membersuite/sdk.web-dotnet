using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Manifests.Searching;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Utilities;
using Spring.Objects.Factory.Support;

namespace MemberSuite.SDK.Web.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:AlgoliaTypeaheadControl runat=\"server\">")]
    [ValidationProperty("SelectedValue")]
    public class AlgoliaTypeaheadControl : WebControl, IPostBackDataHandler
    {

        public const int DEFAULT_COLUMN_WIDTH = 150;
        [Bindable(true)]
        
        [DefaultValue("")]
         public string AlgoliaIndex
        {
            get
            {
                String s = (String)ViewState["AlgoliaIndex"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["AlgoliaIndex"] = value;
            }
        }


        [DefaultValue("")]
        public string AlgoliaApplicationID
        {
            get
            {
                String s = (String)ViewState["AlgoliaApplicationID"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["AlgoliaApplicationID"] = value;
            }
        }

        [DefaultValue("")]
        public string AlgoliaApiKey
        {
            get
            {
                String s = (String)ViewState["AlgoliaApiKey"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["AlgoliaApiKey"] = value;
            }
        }

        [DefaultValue("")]
        public string ClientOnSelectedIndexChanged
        {
            get
            {
                String s = (String)ViewState["ClientOnSelectedIndexChanged"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["ClientOnSelectedIndexChanged"] = value;
            }
        }

        [DefaultValue("")]
        public string ClientOnSearchError
        {
            get
            {
                String s = (String)ViewState["ClientOnSearchError"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["ClientOnSearchError"] = value;
            }
        }

        public string SelectedValue
        {
            get
            {
                String s = (String)ViewState["SelectedValue"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["SelectedValue"] = value;
            }
        }

        public string SelectedText
        {
            get
            {
                String s = (String)ViewState["SelectedText"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["SelectedText"] = value;
            }
        }


        public string TagFilters
        {
            get
            {
                return (String)ViewState["TagFilters"];
                
            }

            set
            {
                ViewState["TagFilters"] = value;
            }
        }


        public List<SearchOutputColumn> Columns
        {
            get
            {
                return (List<SearchOutputColumn>)ViewState["Columns"];
            }

            set
            {
                ViewState["Columns"] = value;
            }
        }


        [DefaultValue("")]
        public int? DropDownWidth
        {
            get
            {
                return (int?)ViewState["DropDownWidth"];
                
            }

            set
            {
                ViewState["DropDownWidth"] = value;
            }
        }

        [DefaultValue("100")]
        public int HitsPerPage
        {
            get
            {
                return (int?)ViewState["HitsPerPage"] ?? 100;

            }

            set
            {
                ViewState["HitsPerPage"] = value;
            }
        }

        //todo: examine if new keyword should be used or if overrride and incorporate base.Width funcionality
        [DefaultValue("150")]
        public new int Width
        {
            get
            {
                return (int?)ViewState["Width"] ?? 110;

            }

            set
            {
                ViewState["Width"] = value;
            }
        }

        
        public bool RightAlign
        {
            get { return Convert.ToBoolean(ViewState["RightAlign"]); }

            set
            {
                ViewState["RightAlign"] = value;
            }
        }

        string primaryInputClientID;
        string primaryInputUniqueID;
        
        string searchItemId;
        string selectedItemHiddenInputClientID;
        string selectedItemHiddenInputUniqueId;

        string searchPlaceholderID;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            primaryInputClientID = string.Format("{0}_primaryInput", ClientID);
            primaryInputUniqueID = string.Format("{0}_primaryInput", UniqueID);
            searchItemId = string.Format("{0}_searchItem", ClientID.Replace('.', '_'));
            selectedItemHiddenInputClientID = string.Format("{0}_selectedItem", ClientID);
            selectedItemHiddenInputUniqueId = string.Format("{0}_selectedItem", UniqueID);
            searchPlaceholderID = string.Format("{0}_searchresults", ClientID );
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Page != null)
                Page.RegisterRequiresPostBack(this);

            // include my JS files
            var indexingEnvironment = ConfigurationManager.AppSettings["indexing_environment"] ?? "prod";
            var cs = Page.ClientScript;

            cs.RegisterClientScriptInclude("algoliasearch.min.js", "/console/controls/typeahead/Scripts/algoliasearch.min.js");
            cs.RegisterClientScriptInclude("hogan.js", "/console/controls/typeahead/Scripts/hogan.js");
            cs.RegisterClientScriptInclude("typeahead.jquery.min.js",
                indexingEnvironment == "dev"
                    ? "/console/controls/typeahead/Scripts/membersuite-typeahead.js"
                    : "/console/controls/typeahead/Scripts/membersuite-typeahead.min.js");

            // render the script
          
            var script = EmbeddedResource.LoadAsString(this.GetType().Namespace + ".TypeaheadControl.js");

            if (script == null) throw new ApplicationException("Unable to locate embedded resource");

            // let's build the columns
            var sbColumns = new StringBuilder();
            

            if (Columns != null)
                foreach (var c in Columns)
                {
                    int columnWidth = c.ColumnWidth > 0 ? c.ColumnWidth : DEFAULT_COLUMN_WIDTH;

                    string label = c.DisplayName ?? c.Name;
                    sbColumns.AppendLine(
                        string.Format("{{ name: '{0}', value: '_highlightResult.{1}.value', width: {2} }},",
                            label.Replace('.', '_'), c.Name.Replace('.', '_'), columnWidth));
                }

            var dropDownMenuContent = sbColumns.ToString().Trim().TrimEnd(',');
            /*var devDropDownMenuContent = string.Format(
                    "{{ name: 'Name', value: '_highlightResult.Name.value', width: 100 }}," +
                    "{{ name: 'Membership_ExpirationDate', value: '_highlightResult.Membership.ExpirationDate.value', width: 170 }}," +
                    "{{ name: 'LocalID', value: '_highlightResult.LocalID.value', width: 70 }}," +
                    "{{ name: 'Membership_Type_Name', value: '_highlightResult.Membership.Type.Name.value', width: 100 }}");*/

            string itemChangedScript = null;
            string onErrorScript = null;

            if (!string.IsNullOrWhiteSpace(ClientOnSelectedIndexChanged))
                itemChangedScript = string.Format("OnClientSelectedIndexChanged(this, this.selectedObj);");

            if (!string.IsNullOrWhiteSpace(ClientOnSearchError))
                onErrorScript = string.Format("OnSearchError(this, {{ error: this.get_error() }})");

            // replace tokens
            string theIndexToUse = generateClientSideIndex();

            script = script
                   .Replace("{AlgoliaApplicationID}", AlgoliaApplicationID)
                   .Replace("{AlgoliaAPIKey}", AlgoliaApiKey)
                   .Replace("{AlgoliaIndex}", theIndexToUse)
                   .Replace("{PrimaryInputID}", primaryInputClientID)
                   .Replace("{SearchPlaceholderID}", searchPlaceholderID)
                   .Replace("{SearchItemControlID}", searchItemId)
                   .Replace("{PlaceholderText}", "Enter search phrase") //ToDo: needs configurable viewstate property
                   .Replace("{SelectedSearchItemControlID}", selectedItemHiddenInputClientID)
                   .Replace("%DropDownMenuContent%", dropDownMenuContent)
                   .Replace("%ClientOnSelectedItemChanged%", itemChangedScript)
                   .Replace("%ClientOnSearchError%", onErrorScript)
                   .Replace("{Width}", string.Format("{0}px", Width.ToString())) 
                   .Replace("{NoResultsText}", "No results returned...") //ToDo: needs configurable viewstate property 
                   .Replace("{Alignment}", RightAlign ? "right" : "left")
                   .Replace("%SelectedValue%",
                       !string.IsNullOrWhiteSpace(SelectedValue) ? string.Format("\"{0}\"", SelectedValue) : "null")
                   .Replace("%TagFilters%",
                       !string.IsNullOrWhiteSpace(TagFilters) ? string.Format("\"{0}\"", TagFilters) : "null")
                   .Replace("%HitsPerPage%", HitsPerPage.ToString());

            /*script = script
                .Replace("{AlgoliaApplicationID}", "DA0HGR28CC")
                .Replace("{AlgoliaAPIKey}", "58f000b818ec8121139cb343205f8b9d")
                .Replace("{AlgoliaIndex}", "dev_ANDREWSWORK-PC_qs_1_Individual")
                .Replace("{PrimaryInputID}", primaryInputClientID)
                .Replace("{SearchPlaceholderID}", searchPlaceholderID)
                .Replace("{SearchItemControlID}", searchItemId)
                .Replace("{PlaceholderText}", "Enter search")
                .Replace("{SelectedSearchItemControlID}", selectedItemHiddenInputClientID)
                .Replace("%DropDownMenuContent%", devDropDownMenuContent)
                .Replace("%ClientOnSelectedItemChanged%", itemChangedScript)
                .Replace("%ClientOnSearchError%", onErrorScript)
                .Replace("{Width}", "110px")
                .Replace("{NoResultsText}", "No results returned...")
                .Replace("{Alignment}", RightAlign ? "right" : "left")
                .Replace("%SelectedValue%",
                    !string.IsNullOrWhiteSpace(SelectedValue) ? string.Format("\"{0}\"", SelectedValue) : "null")
                .Replace("%TagFilters%",
                    !string.IsNullOrWhiteSpace(TagFilters) ? string.Format("\"{0}\"", TagFilters) : "null")
                .Replace("%HitsPerPage%", HitsPerPage.ToString());*/



            cs.RegisterClientScriptBlock(this.GetType(), "Algolia" + UniqueID  , script, true);


        }

       
        protected override void RenderContents(HtmlTextWriter writer)
        {

            // <ul style="list-style-type: none; margin: 0; padding: 0;">
            writer.AddAttribute("style", "list-style-type: none; margin: 0; padding: 0; position: absolute; display: inline-block; margin-left: 4px;");
            writer.AddAttribute("class", "search-container");
            writer.RenderBeginTag("ul");

            // <li>
            writer.RenderBeginTag("li");

            // <ul style="list-style-type: none; margin: 0; padding: 0;">
            writer.AddAttribute("style", "list-style-type: none; margin: 0; padding: 0;");
            writer.RenderBeginTag("ul");

            // li style="float: left;">
            writer.AddAttribute("style", "float: left; padding:0;");
            writer.RenderBeginTag("li");


            // the input
            writer.AddAttribute("class", "member-suite-typeahead");
            writer.AddAttribute("autocomplete", "off");
            writer.AddAttribute("type", "text");
            writer.AddAttribute("id", primaryInputClientID);
            writer.AddAttribute("name", primaryInputUniqueID);
            writer.AddAttribute("spellcheck", "false");
            writer.RenderBeginTag("input");
            writer.RenderEndTag();

            // <li>
            writer.RenderEndTag();

            // li style="float: left;">
            writer.AddAttribute("style", "float: left; padding:0;");
            writer.RenderBeginTag("li");

            // the span
            writer.AddAttribute("class", "search-icon");
            writer.RenderBeginTag("span");
            writer.RenderEndTag();

            // <li>
            writer.RenderEndTag();

            // </ul>
            writer.RenderEndTag();

            // </li>
            writer.RenderEndTag();

            // LI
            writer.RenderBeginTag("li");

            //   <div id="placeholder-search2"></div>
            writer.AddAttribute("id", searchPlaceholderID);
            writer.RenderBeginTag("div");
            writer.RenderEndTag();

            // </li>
            writer.RenderEndTag();

            // </ul>
            writer.RenderEndTag();

           
           
   
            // now, the two inputs
            writer.AddAttribute("id", searchItemId);
            writer.AddAttribute("value", "Name");
            writer.AddAttribute("type", "hidden");
            writer.RenderBeginTag("input");
            writer.RenderEndTag();

            // second one
            writer.AddAttribute("id", selectedItemHiddenInputClientID);
            writer.AddAttribute("name", selectedItemHiddenInputUniqueId);
            writer.AddAttribute("value", "");
            writer.AddAttribute("type", "hidden");
            writer.RenderBeginTag("input");
            writer.RenderEndTag();


           


        }

        private string generateClientSideIndex()
        {
            // we're duplicating code from AlgoliaDataIndexingProvider
           
                var _indexingEnvironment = ConfigurationManager.AppSettings["indexing_environment"] ?? "prod";

                if (_indexingEnvironment == "dev")
                    _indexingEnvironment += "_" + Environment.MachineName;

            return string.Format("{0}_{1}", _indexingEnvironment, AlgoliaIndex);
        }

        /// <summary>
        /// Sets the quick search columns.
        /// </summary>
        /// <param name="sm">The sm.</param>
        public void SetQuickSearchColumns(SearchManifest sm)
        {
            var columns = sm.DefaultQuickSearchColumns;

            // if the display name isn't set explicitly, let's get it from the label of the field
            foreach (var c in columns.FindAll(x => x.DisplayName == null))
            {
                var theField = sm.Fields.Find(x => x.Name == c.Name);
                if (theField != null)
                    c.DisplayName = theField.Label; // set the label
            }
            Columns = columns;
        }

      

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var oldSelectedValue = SelectedValue;
            var newSelectedValue =postCollection[  selectedItemHiddenInputUniqueId ];
            SelectedValue = newSelectedValue;
            SelectedText = postCollection[primaryInputUniqueID];
            // now, return a true if the value has changed
            return oldSelectedValue != newSelectedValue;


        }

        public void RaisePostDataChangedEvent()
        {
             
        }
    }
}
