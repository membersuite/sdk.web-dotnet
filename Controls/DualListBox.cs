using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:DualListBox runat=server></{0}:DualListBox>")]
    public class DualListBox : CompositeControl, INamingContainer
    {
        public DualListBox()
        {
            Source = new RadListBox();
            Destination = new RadListBox();
            Height = Unit.Pixel(200);
            Width = Unit.Pixel(400);
        }


        public RadListBox Source { get; set; }

        public RadListBox Destination { get; set; }

        #region Properties

        [Bindable(true), Category("Behavior"), DefaultValue(true)]
        public bool EnableDragAndDrop
        {
            get { return Source.EnableDragAndDrop; }
            set { Source.EnableDragAndDrop = value; }
        }

        [Bindable(true), Category("Appearance"), DefaultValue(true)]
        public string EmptyMessage
        {
            get { return Source.EmptyMessage; }
            set { Source.EmptyMessage = Destination.EmptyMessage = value; }
        }

        [Bindable(true), Category("Behavior"), DefaultValue(true)]
        public bool AllowReorder
        {
            get { return Destination.AllowReorder; }
            set { Destination.AllowReorder = value; }
        }


        public override Unit Height
        {
            get { return Source.Height; }
            set { Source.Height = Destination.Height = value; }
        }

        public override Unit Width
        {
            get { return Source.Width; }
            set { Source.Width = Destination.Width = value; }
        }


        public string LeftLabel
        {
            get { return (string)ViewState["LeftLabel"]; }
            set { ViewState["LeftLabel"] = value; }
        }

        public string RightLabel
        {
            get { return (string)ViewState["RightLabel"]; }
            set { ViewState["RightLabel"] = value; }
        }

            #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            
            Controls.Add(Source);
            Controls.Add(Destination);
            
            if ( ! string.IsNullOrWhiteSpace( LeftLabel ) ) Source.Header.Controls.Add( new LiteralControl( string.Format( "<div align=center>{0}</div>", LeftLabel )) );
            if (!string.IsNullOrWhiteSpace(RightLabel)) Destination.Header.Controls.Add(new LiteralControl(string.Format("<div align=center>{0}</div>", RightLabel )));
            
            Source.ID = "lbSrc";
            Source.PersistClientChanges = true;
            Source.EnableViewState = true;
            Source.TransferMode = ListBoxTransferMode.Move;
            Source.AllowTransferOnDoubleClick = true;
            Source.SelectionMode = ListBoxSelectionMode.Multiple;
            Source.CssClass = "killTablePadding";
            Source.AllowTransfer = true;
            Source.Sort = RadListBoxSort.Ascending;

            Destination.ID = "lbDest";
            Destination.PersistClientChanges = true;
            Destination.EnableViewState = true;
            Destination.AllowTransferOnDoubleClick = true;
            Destination.SelectionMode = ListBoxSelectionMode.Multiple;
            Destination.CssClass = "killTablePadding";
            Destination.AllowReorder = true;


            Source.TransferToID = Destination.ID;
            Source.EnableDragAndDrop = true;
        }


        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);

        //    if (!Page.IsPostBack)
        //    {
        //        foreach (ListItem item in Items)
        //        {
        //            var rli = new RadListBoxItem(item.Text, item.Value);
        //            if (!item.Selected)
        //                Source.Items.Add(rli);
        //            else
        //                Destination.Items.Add(rli);
        //        }
        //    }
        //}

        protected override void Render(HtmlTextWriter writer)
        {
            if (Controls.Count == 0) // design mode
                CreateChildControls();

            Source.RenderControl(writer);
            Destination.RenderControl(writer);
        }
    }
}