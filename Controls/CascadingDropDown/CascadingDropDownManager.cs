using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.Controls.CascadingDropDown
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:CascadingDropDownManager runat=server></{0}:CascadingDropDownManager>")]
    public class CascadingDropDownManager : WebControl
    {
        private string _cachedJavascripTemplate;
        private List<CascadingPair> _cascadingPairs;

        /// <summary>
        /// Gets or sets the cascading pairs.
        /// </summary>
        /// <value>The cascading pairs.</value>
        [Bindable(true), Category("Behavior"), DefaultValue(""), Description("One or more cascading drop down pairs."),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         PersistenceMode(PersistenceMode.InnerProperty)]
        public List<CascadingPair> CascadingPairs
        {
            get
            {
                if (_cascadingPairs == null)
                    _cascadingPairs = new List<CascadingPair>();
                return _cascadingPairs;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
                decorateDropDownLists();
        }

        private void decorateDropDownLists()
        {
            foreach (CascadingPair pair in CascadingPairs)
            {
                // can we locate the control?
                DropDownList ddlParent = _locateDropDownOnPage(pair.ParentDropDownID);
                DropDownList ddlChild = _locateDropDownOnPage(pair.ChildDropDownID);

                ddlParent.Attributes["onchange"] +=
                    "if ( ! isInitializing ) updateCascadingDropDown( this ); return false;";
                ddlChild.Items.Insert(0, ""); // add a blank one

                // are there any validators that validate the child?
                foreach (BaseValidator rfv in Page.Validators)
                    if (rfv.ControlToValidate == ddlChild.ID)
                        rfv.EnableClientScript = false; // it doesn't play well with the cascading drop down
            }
        }


        protected override void RenderContents(HtmlTextWriter output)
        {
            //List<DropDownList> shadowLists  = new List<DropDownList>();
            string javascriptTemplate = _getJavascriptTempate();

            var cascadingPairs = new StringBuilder();
            var switchStatements = new StringBuilder();

            Page.ClientScript.RegisterStartupScript(typeof (string), "CascadingManager",
                                                    "<script>if ( typeof(prepareChildDropDowns) == 'undefined' ) alert('Cascading DropDown Manager Script cannot be located. Please make sure that you have put the script in the proper directory.'); else prepareChildDropDowns();</script>");

            foreach (CascadingPair pair in CascadingPairs)
            {
                // add the shadow list
                var ddlShadow = new DropDownList();
                ddlShadow.ID = pair.ChildDropDownID + "_Shadow";
                ddlShadow.Attributes["style"] = "display: none;";
                ddlShadow.RenderControl(output); // render it


                // text to display when the drop down is disabled
                string childDisabledText = pair.ChildDisabledText;
                if (String.IsNullOrEmpty(childDisabledText))
                    childDisabledText = "No items available for selection.";

                // can we locate the control?
                DropDownList ddlParent = _locateDropDownOnPage(pair.ParentDropDownID);
                DropDownList ddlChild = _locateDropDownOnPage(pair.ChildDropDownID);

                ddlParent.Attributes["onchange"] += "updateCascadingDropDown( this )";
                // now, add preparation logic
                cascadingPairs.AppendLine(string.Format("prepareChildDropDown( '{0}','{1}','{2}' );",
                                                        ddlParent.ClientID, ddlChild.ClientID, ddlShadow.ClientID));

                // now, add the swtich logic
                switchStatements.AppendLine();
                switchStatements.AppendLine(string.Format("case '{0}':", ddlParent.ClientID));

                switchStatements.AppendLine("var selectedValue = null;");
                switchStatements.AppendLine(
                    "if ( parent.selectedIndex >= 0 ) selectedValue = parent.options[parent.selectedIndex].value;");
                switchStatements.AppendLine("  switch (selectedValue) {");

                // let's go through each parent value
                foreach (ParentDropDownValue parentVal in pair.ParentDropDownValues)
                {
                    switchStatements.AppendLine(string.Format("     case '{0}':", parentVal.Value));


                    // now, let's go through all of the child values
                    var sbChildValues = new StringBuilder();
                    foreach (ChildDropDownValue childValue in parentVal.ChildDropDownValues)
                    {
                        ListItem item = ddlChild.Items.FindByValue(childValue.Value);
                        if (item == null) // can't find it
                            continue;
                        sbChildValues.AppendFormat("{0},", ddlChild.Items.IndexOf(item)); // b/c of the blank
                    }

                    if (parentVal.ChildDropDownValues.Count == 1) // gotta add a -1 so the Array consturctor works
                        sbChildValues.AppendLine("-1");

                    string itemArray = sbChildValues.ToString().Trim().TrimEnd(',');
                    string byDefaultShowAll = parentVal.ByDefault == ParentValueBehavior.ShowOnlyListedValues
                                                  ? "true"
                                                  : "false";
                    switchStatements.AppendLine(
                        string.Format(
                            "updateCascadingDropDownWithValues( '{0}','{1}','{2}', new Array({3}),{4},'{5}','{6}','{7}'); ",
                            ddlParent.ClientID, ddlChild.ClientID, ddlShadow.ClientID, itemArray,
                            byDefaultShowAll, pair.WhenNoChildValues, childDisabledText, pair.ElementToHide));

                    // run the onselect statement
                    if (!String.IsNullOrEmpty(parentVal.ClientOnSelect))
                        switchStatements.AppendLine(string.Format("{0};", parentVal.ClientOnSelect));
                    switchStatements.AppendLine("break;");
                }

                // then the default statement
                switchStatements.AppendLine(
                    string.Format(
                        "default: updateCascadingDropDownWithValues( '{0}','{1}','{2}', new Array(),{3},'{4}','{5}','{6}'); break;",
                        ddlParent.ClientID, ddlChild.ClientID, ddlShadow.ClientID,
                        pair.ByDefault == CascadingPairBehavior.HideSpecifiedChildValues ? "false" : "true",
                        // this seems incorrect but its right
                        pair.WhenNoChildValues, childDisabledText, pair.ElementToHide
                        ));

                switchStatements.AppendLine("}");
                switchStatements.AppendLine("break;");
                switchStatements.AppendLine();
            }

            javascriptTemplate = javascriptTemplate.Replace("%dropDownsToPrepare%", cascadingPairs.ToString())
                .Replace("%parentDropDownSwitchConditions%", switchStatements.ToString());

            output.WriteFullBeginTag("script");

            output.Write(javascriptTemplate);
            output.WriteEndTag("script");
        }

        /// <summary>
        /// Locates the drop down on page.
        /// </summary>
        /// <param name="dropDownID">The drop down ID.</param>
        /// <returns></returns>
        private DropDownList _locateDropDownOnPage(string dropDownID)
        {
            var ddl = _findControl(dropDownID) as DropDownList;

            if (ddl == null)
                throw new ApplicationException(string.Format("Unable to locate a DropDownList on the page named '{0}'",
                                                             dropDownID));

            return ddl;
        }

        private Control _findControl(string controlName)
        {
            if ( Page.Master != null)
            {
                Control bodyTag = Page.Master.FindControl("body");
                if ( bodyTag != null )
                    return bodyTag.FindControl(controlName);
            }
            
            return FindControl(controlName);
        }

        private string _getJavascriptTempate()
        {
            if (_cachedJavascripTemplate != null)
                return _cachedJavascripTemplate;

            // get the stream first
            Stream stream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "MemberSuite.SDK.Web.Controls.CascadingDropDown.CascadingDropDown.js");

            // is there a stream?
            if (stream == null)
                throw new ApplicationException("Fatal Error: Unable to load cascading dropdown javascript template");

            // get the string
            var sr = new StreamReader(stream);
            _cachedJavascripTemplate = sr.ReadToEnd();

            return _cachedJavascripTemplate;
        }
    }
}