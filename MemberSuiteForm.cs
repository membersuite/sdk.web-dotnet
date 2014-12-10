using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Manifests.Command.Views;
using MemberSuite.SDK.Web.Controls;

namespace MemberSuite.SDK.Web
{
     
    [ToolboxData("<{0}:MemberSuiteForm runat=server></{0}:MemberSuiteForm>")]
    public class MemberSuiteForm : CompositeControl, IHostedControl
    {

        #region IHostedControl Members

        public IControlHost Host { get; set; }

        #endregion

        public DataEntryViewMetadata Form { get; set; }

        //protected void renderControls( WebControl container)
        //{
        //    if (Form == null)
        //    {
        //        container.Controls.Add(new LiteralControl("No form provided."));
        //        return;
        //    }


        //    if (Form.Sections != null)
        //        foreach (var sec in Form.Sections)
        //            renderSection(phMainSection, sec);

          
        //}
        //protected void renderSection(PlaceHolder ph, ViewMetadata.ControlSection section)
        //{
        //    renderSection(ph, section, false);
        //}

        //protected void renderSection(PlaceHolder ph, ViewMetadata.ControlSection section, bool isSubSection)
        //{

        //    HtmlGenericControl span = new HtmlGenericControl("span");

        //    if (section.Name != null)
        //        span.Attributes["ID"] = section.Name;
        //    ph.Controls.Add(span);
            

        //    // create the header
        //    var divSectionHeader = new HtmlGenericControl("div");
        //    var divSectionContent = new HtmlGenericControl("div");

        //    divSectionHeader.Attributes["class"] = "stepSectHeader";

        //    if (section.Name != null)
        //    {
        //        PlaceHolder phSection = new PlaceHolder();
        //        phSection.ID = section.Name;
        //        span.Controls.Add(phSection);
        //        //  add it to the placeholder
        //        phSection.Controls.Add(divSectionHeader);
        //        phSection.Controls.Add(divSectionContent);
        //    }
        //    else
        //    {
        //        //  add it to the placeholder
        //        span.Controls.Add(divSectionHeader);
        //        span.Controls.Add(divSectionContent);
        //    }


        //    if (section.Name != null)
        //    {
        //        HtmlGenericControl a = new HtmlGenericControl("A");
        //        a.Attributes["Name"] = section.Name;
        //        divSectionHeader.Controls.Add(a);
        //    }

        //    string label = section.Label;
            
        //    var header = new HtmlGenericControl(isSubSection ? "h4" : "h1") { InnerHtml = label };
        //    if (UseLargeHeader && !isSubSection)
        //        header.Attributes["class"] = "sectionHeader";

        //    divSectionHeader.Controls.Add(header);


        //    if (section.IsEmpty())
        //        return; // nothing to do

        //    // now, create the content

        //    divSectionContent.Attributes["class"] = "stepSectContent";
        //    divSectionContent.Attributes["style"] = string.Format("width: {0}px !important;", Width * .8);
        //    if (section.Name != null)
        //        divSectionContent.ID = section.Name + "_Content";

        //    // first the text
        //    if (section.Text != null)
        //        divSectionContent.Controls.Add(new LiteralControl(section.Text + "<p/>"));

        //    // let's preprocess controls, making sure we can display all of them, removing the ones we can
        //    _preprocessControls(section.LeftControls);
        //    _preprocessControls(section.RightControls);

        //    // now, render controls
        //    _renderControls(section, divSectionContent);

       
        //}

        ///// <summary>
        ///// _preprocesses the controls.
        ///// </summary>
        ///// <param name="controls">The controls.</param>
        //private void _preprocessControls(IList<ControlMetadata> controls)
        //{
        //    for (int i = controls.Count - 1; i >= 0; i--)
        //    {
        //        var control = controls[i];
        //        if (getControlManagerFor(control) == null)
        //            controls.RemoveAt(i);   // remove it
        //    }
        //}

        //private void _renderSubSections(ViewMetadata.ControlSection section, HtmlGenericControl divSectionContent)
        //{
        //    if (section == null || section.SubSections == null || section.SubSections.Count == 0)
        //        return; // nothing to do

        //    PlaceHolder ph = new PlaceHolder();
        //    divSectionContent.Controls.Add(ph);

        //    foreach (var subsection in section.SubSections)
        //        renderSection(ph, subsection, true);

        //}

        //protected abstract bool UseLargeHeader { get; }

        //private void _renderCommands(ViewMetadata.ControlSection section, HtmlGenericControl divSectionContent)
        //{
        //    if (section.Commands == null || section.Commands.Count == 0)
        //        return; // nothing to do

        //    divSectionContent.Attributes["class"] += " killTablePadding";

        //    HtmlTable ht = new HtmlTable();
        //    ht.Attributes["style"] = "width: 540px";

        //    HtmlTableRow tr = new HtmlTableRow();
        //    ht.Rows.Add(tr);

        //    foreach (var cmd in section.Commands)
        //    {
        //        if (!cmd.ShouldShow())
        //            continue;

        //        HtmlTableCell cell = new HtmlTableCell();

        //        if (tr.Cells.Count >= 2)  // time for a new cell
        //        {
        //            tr = new HtmlTableRow();
        //            ht.Rows.Add(tr);
        //        }

        //        //cell.Attributes["style"] = "width: 520px";
        //        tr.Cells.Add(cell);

        //        HyperLink hlCommand = new HyperLink();
        //        hlCommand.NavigateUrl = cmd.ToUrl();
        //        hlCommand.Target = cmd.GetTarget();

        //        string label = cmd.Label ?? ResolveResource(cmd.ID);
        //        if (cmd.Icon == null && cmd.HelpText != null) // set a defaul;t
        //            cmd.Icon = "ico_membersuite.gif";

        //        if (cmd.Icon == null)
        //        {
        //            hlCommand.Text = string.Format("<h2>{0}</h2>", label);
        //        }
        //        else
        //        {
        //            // we got a lot to do
        //            hlCommand.CssClass = "hCommandIcon";
        //            HtmlGenericControl spanImg = new HtmlGenericControl("span");
        //            hlCommand.Controls.Add(spanImg);
        //            spanImg.Attributes["class"] = "hCommandIconImg";
        //            spanImg.Attributes["style"] = string.Format("background: url(/console/images/commandicons/{0}) no-repeat top left; ",
        //                cmd.Icon);

        //            HtmlGenericControl spanIconText = new HtmlGenericControl("span");
        //            hlCommand.Controls.Add(spanIconText);
        //            spanIconText.Attributes["class"] = "hCommandIconTxt";


        //            HtmlGenericControl spanPad = new HtmlGenericControl("span");
        //            spanIconText.Controls.Add(spanPad);
        //            spanPad.Attributes["class"] = "pad";

        //            HtmlGenericControl spanIconHeader = new HtmlGenericControl("span");
        //            spanPad.Controls.Add(spanIconHeader);
        //            spanIconHeader.Attributes["class"] = "hCommandIconHeader";
        //            spanIconHeader.InnerText = label;

        //            HtmlGenericControl spanIconHelpText = new HtmlGenericControl("span");
        //            spanPad.Controls.Add(spanIconHelpText);
        //            spanIconHelpText.Attributes["class"] = "hCommandIconHelpText";
        //            spanIconHelpText.InnerText = cmd.HelpText;

        //            HtmlGenericControl br = new HtmlGenericControl("br");
        //            br.Attributes["class"] = "clearBoth";
        //            hlCommand.Controls.Add(br);


        //        }
        //        cell.Controls.Add(hlCommand);
        //    }

        //    divSectionContent.Controls.Add(ht);
        //}

        //private void _renderReports(ViewMetadata.ControlSection section, HtmlGenericControl divSectionContent)
        //{
        //    if (section.Reports == null || section.Reports.Count == 0) return;

        //    // HtmlGenericControl divReports = new HtmlGenericControl("DIV");
        //    //divReports.Attributes["style"] = "width: 600px;";

        //    HtmlTable tReports = new HtmlTable();
        //    tReports.CellPadding = 0;
        //    tReports.CellSpacing = 0;
        //    tReports.Attributes["style"] = string.Format("width: {0}px;", Width * .8);

        //    bool isOdd = false;
        //    foreach (var report in section.Reports)
        //    {
        //        HtmlTableRow row = new HtmlTableRow();
        //        row.Attributes["style"] = "height: 5px";
        //        row.Attributes["class"] = isOdd ? "odd" : "even";
        //        isOdd = !isOdd;

        //        var leftCell = new HtmlTableCell();
        //        leftCell.Attributes["style"] = string.Format("width: {0}px;", Width * .4);
        //        var rightCell = new HtmlTableCell();
        //        row.Cells.Add(leftCell);
        //        row.Cells.Add(rightCell);

        //        HyperLink hl = new HyperLink();
        //        hl.Text = report.Label + "<BR/>";
        //        hl.NavigateUrl = "#";
        //        hl.ToolTip = report.Description;
        //        leftCell.Controls.Add(hl);


        //        CommandShortcut csReport = null;
        //        switch (report.Type)
        //        {
        //            case ViewMetadata.ReportLinkType.BuiltInSearch:
        //                csReport = new CommandShortcut { Name = "Console.Search", Arg1 = "Individual", Arg2 = report.Report, Arg4 = "run" };

        //                break;

        //            case ViewMetadata.ReportLinkType.Report:
        //                csReport = new CommandShortcut { Name = "Console.Report.Frame", Arg1 = report.Report };
        //                break;
        //        }
        //        rightCell.Controls.Add(new HyperLink() { Text = "(run report)", NavigateUrl = csReport.ToUrl() });


        //        tReports.Rows.Add(row);
        //        //  divSectionContent.Controls.Add(new LiteralControl(report.Description));
        //        // divSectionContent.Controls.Add(new HtmlGenericControl("P"));
        //    }
        //    divSectionContent.Controls.Add(tReports);
        //}

        //private void _renderControls(ViewMetadata.ControlSection section, HtmlGenericControl divSectionContent)
        //{
        //    if (section.LeftControls == null)
        //        return;

        //    if (section.RightControls == null)
        //        section.RightControls = new List<ControlMetadata>();

        //    int maxLength = section.LeftControls.Count;

        //    if (section.RightControls != null && section.RightControls.Count > maxLength)
        //        maxLength = section.RightControls.Count;

        //    if (maxLength == 0)
        //        return;


        //    var table = new HtmlGenericControl("table");
        //    table.Attributes["style"] = string.Format("width: {0}px !important;", Width * .8);
        //    divSectionContent.Controls.Add(table);

        //    bool isLeftColumnOnly = section.RightControls == null || section.RightControls.Count == 0;
        //    Queue<ControlMetadata> leftControls = new Queue<ControlMetadata>(section.LeftControls);
        //    Queue<ControlMetadata> rightControls = new Queue<ControlMetadata>(section.RightControls);

        //    for (int i = 0; i < maxLength; i++)
        //    {
        //        var tr = new HtmlGenericControl("tr");

        //        table.Controls.Add(tr);




        //        if (_leftRowSpan == 0 && leftControls.Count > 0)
        //        {

        //            var control = leftControls.Dequeue();
        //            var leftInstructions = processControlGroup(tr, control);
        //            var length = leftInstructions.Length;
        //            _leftRowSpan += length;
        //            maxLength += length;

        //            if (leftInstructions.CreateNewRow)
        //            {
        //                tr = new HtmlGenericControl("tr");
        //                table.Controls.Add(tr);
        //            }
        //        }
        //        else
        //            if (_leftRowSpan == 0)
        //                addSpacer(tr);
        //            else
        //                _leftRowSpan--;




        //        if (section.RightControls != null && _rightRowSpan == 0 && rightControls.Count > 0)
        //        {
        //            var control = rightControls.Dequeue();
        //            var rightInstructions = processControlGroup(tr, control);
        //            var length = rightInstructions.Length;
        //            _rightRowSpan += length;
        //            maxLength += length;
        //        }
        //        else
        //            if (_rightRowSpan == 0)
        //                addSpacer(tr);
        //            else
        //                _rightRowSpan--;


        //        // add a spacer for the one column-ers
        //        //if (isLeftColumnOnly)
        //        //{
        //        //    var tdSpacer1 = new HtmlGenericControl("td");
        //        //    var tdSpacer2 = new HtmlGenericControl("td");

        //        //    //tdSpacer1.Attributes["style"] = "width: 15%";
        //        //    //tdSpacer1.Attributes["style"] = "width: 15%";

        //        //    tdSpacer1.Attributes["class"] = "labelCell";
        //        //    tdSpacer2.Attributes["class"] = "controlCell";

        //        //    tr.Controls.Add(tdSpacer1);
        //        //    tr.Controls.Add(tdSpacer2);

        //        //}



        //    }


        //}

        //private int _leftRowSpan = 0;
        //private int _rightRowSpan = 0;

        //private void addSpacer(HtmlGenericControl tr)
        //{
        //    var tdSpacer1 = new HtmlGenericControl("td");
        //    var tdSpacer2 = new HtmlGenericControl("td");


        //    tdSpacer1.Attributes["class"] = "labelCell";
        //    tdSpacer2.Attributes["class"] = "controlCell";
        //    tdSpacer1.Attributes["style"] = "width: 0px !important";
        //    tdSpacer2.Attributes["style"] = "width: 0px !important";

        //    tr.Controls.Add(tdSpacer1);
        //    tr.Controls.Add(tdSpacer2);
        //}

        //private ControlGroupInstructions processControlGroup(HtmlGenericControl tableRow, ControlMetadata control)
        //{
        //    tableRow.Attributes["style"] = "padding-right: 0px !important";
        //    var manager = getControlManagerFor(control);
        //    if (manager == null)
        //        return new ControlGroupInstructions(-1, false);

        //    bool createNewRow = false;
        //    tableRow.Attributes["style"] = "vertical-align: top";
        //    // add the form group
        //    var tdLabel = new HtmlGenericControl("td");
        //    var tdControl = new HtmlGenericControl("td");
        //    tdLabel.Attributes["class"] = "labelCell";

        //    if (manager.RowSpan > 1)
        //        tdLabel.Attributes["RowSpan"] = tdControl.Attributes["RowSpan"] = manager.RowSpan.ToString();

        //    // add the label
        //    if (!manager.CustomLabel && !control.UseEntireRow)
        //    {
        //        var label = new HtmlGenericControl("label");
        //        var controlLabel = manager.GetLabel();

        //        if (manager.IsRequired())
        //            controlLabel += "<font color=red>*</font>";

        //        label.Controls.Add(new LiteralControl(controlLabel));
        //        tdLabel.Controls.Add(label);

        //        // only  add the label cell if we need to
        //        tableRow.Controls.Add(tdLabel);
        //        tdControl.Attributes["class"] = "controlCell";

        //    }
        //    else
        //    {
        //        if (control.UseEntireRow)
        //        {
        //            tdControl.Attributes["colspan"] = "4"; // of course, the control spans two controls
        //            tdControl.Attributes["Class"] = "singleControlCell";
        //            createNewRow = true;
        //        }
        //        else
        //        {
        //            tdControl.Attributes["colspan"] = "2"; // of course, the control spans two controls
        //            tdControl.Attributes["Class"] = "singleControlCell";
        //        }
        //    }

        //    // now, add the field


        //    tableRow.Controls.Add(tdControl);

        //    var controls = manager.Instantiate();
        //    if (controls != null)
        //        foreach (var c in controls)
        //            tdControl.Controls.Add(c);

        //    var validationContorls = manager.InstantiateValidationControls();
        //    if (validationContorls != null)
        //        foreach (var c in validationContorls)
        //            tdControl.Controls.Add(c);

        //    return new ControlGroupInstructions(manager.RowSpan - 1, createNewRow); ;

        //}

        //#region Utils/Helpers
        //protected DataEntryViewMetadata _DataEntryViewMetadata;
        //public DataEntryViewMetadata DataEntryViewMetadata
        //{
        //    get
        //    {
        //        if (_DataEntryViewMetadata == null)
        //        {
        //            var specific = Manifest.ViewMetadata.SpecificViewMetadata;
        //            if (specific == null || specific.DataEntryViewMetadata == null)
        //                throw new ViewPaintingException("No view specific metadata found for DataEntryView");

        //            _DataEntryViewMetadata = specific.DataEntryViewMetadata;
        //        }
        //        return _DataEntryViewMetadata;
        //    }
        //}
        //#endregion

        //#region Common Controls
        ///// <summary>
        ///// phMainSection control.
        ///// </summary>
        ///// <remarks>
        ///// Auto-generated field.
        ///// To modify move field declaration from designer file to code-behind file.
        ///// </remarks>
        //protected global::System.Web.UI.WebControls.PlaceHolder phMainSection;

        ///// <summary>
        ///// phTransitions control.
        ///// </summary>
        ///// <remarks>
        ///// Auto-generated field.
        ///// To modify move field declaration from designer file to code-behind file.
        ///// </remarks>
        //protected global::System.Web.UI.WebControls.PlaceHolder phTransitions;

        ///// <summary>
        ///// phOtherControls control.
        ///// </summary>
        ///// <remarks>
        ///// Auto-generated field.
        ///// To modify move field declaration from designer file to code-behind file.
        ///// </remarks>
        //protected global::System.Web.UI.WebControls.PlaceHolder phOtherControls;
        //#endregion

        //#region Abstract Properties

        //public abstract int Width { get; }
        //#endregion

        //#region Nested Types
        //public struct ControlGroupInstructions
        //{
        //    private int _length;
        //    private bool _createNewRow;

        //    public ControlGroupInstructions(int length, bool createNewRow)
        //    {
        //        _length = length;
        //        _createNewRow = createNewRow;
        //    }

        //    public int Length
        //    {
        //        get { return _length; }
        //        set { _length = value; }
        //    }

        //    public bool CreateNewRow
        //    {
        //        get { return _createNewRow; }
        //        set { _createNewRow = value; }
        //    }
        //}
        //#endregion

    }
}
