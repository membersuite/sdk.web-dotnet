using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Utilities;
using MemberSuite.SDK.Web.ControlManagers;

namespace MemberSuite.SDK.Web.Controls
{
    [ValidationProperty("IsValid")]
    public class FileUploadCoordinator : CompositeControl
    {
        #region Control Fields

        public HyperLink FileDownloadLink { get; set; }
        public HyperLink ChooseADifferentFileLink { get; set; }
        public HyperLink DeleteFileLink { get; set; }
        public LiteralControl UploadedFileText;
        public FileUpload FileUpload { get; set; }
        public HyperLink CancelUploadLink { get; set; }
        public HtmlGenericControl ViewPanel { get; set; }
        public HtmlGenericControl UploadPanel { get; set; }
        public HiddenField HiddenState { get; set; }

        #endregion

        #region Control Properties

        /// <summary>
        /// Used by the required field validator to ensure that an
        /// actual file is selected.
        /// </summary>
        /// <value>The is valid.</value>
        public string IsValid
        {
            get
            {
                // this should definitely be a string (not a bool like you might think) because
                // of how validation works. Check out:
                // http://forums.asp.net/t/1325623.aspx OR
                // http://msdn.microsoft.com/en-us/library/aa479045.aspx to see why. The validators use
                // this string for validation ;so the requiredfieldvalue will look at this and determine
                // if a value has been specified.
                if (! String.IsNullOrEmpty(FileID) || FileUpload.HasFile) return "Valid";
                return null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Web server control is enabled.
        /// </summary>
        /// <value></value>
        /// <returns>true if control is enabled; otherwise, false. The default is true.
        /// </returns>
        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;

                EnsureChildControls();

                FileUpload.Visible = value;
                ChooseADifferentFileLink.Visible = value;
            }
        }

        #endregion

        #region Regular Properties

        public string FileID
        {
            get { return (string) ViewState["FileID"]; }
            set { ViewState["FileID"] = value; }
        }


        public string FileName
        {
            get { return (string) ViewState["FileName"]; }
            set { ViewState["FileName"] = value; }
        }


        public FileUploadState State
        {
            get
            {
                if (HiddenState == null)
                    return FileUploadState.NoFileSpecified;

                switch (HiddenState.Value)
                {
                    case "NONE":
                        return FileUploadState.NoFileSpecified;

                    // MS-4467
                    // MS-4899
                    case "NEW":
                    case "":
                        return FileUploadState.NewFileSpecified;

                    // MS-4973.
                    case "OLD":
                        return FileUploadState.OldFileSpecified;
                }

                return FileUploadState.NoFileSpecified;
            }
            set
            {
                EnsureChildControls();

                switch (value)
                {
                    case FileUploadState.NoFileSpecified:
                        HiddenState.Value = "NONE";
                        break;

                    case FileUploadState.NewFileSpecified:
                        // MS-4467
                        // HiddenState.Value = "NEW";
                        HiddenState.Value = "";
                        break;

                    // MS-4973.
                    case FileUploadState.OldFileSpecified:
                        HiddenState.Value = "OLD";
                        break;

                    default:
                        throw new NotSupportedException("Cannot deal with file uploader state " + value);
                }
            }
        }

        #endregion

        #region FileUploadState enum

        public enum FileUploadState
        {
            NoFileSpecified,

            NewFileSpecified,

            OldFileSpecified
        }

        #endregion

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            UploadPanel = new HtmlGenericControl("span");
            ViewPanel = new HtmlGenericControl("span");

            UploadPanel.ID = "spanUpload";
            ViewPanel.ID = "spanView";

            Controls.Add(ViewPanel);
            Controls.Add(UploadPanel);

            FileDownloadLink = new HyperLink();
            FileDownloadLink.ID = "hlFileDownload";
            FileDownloadLink.Text = "View";
            ViewPanel.Controls.Add(FileDownloadLink);

            ChooseADifferentFileLink = new HyperLink();
            ChooseADifferentFileLink.ID = "hlChooseADifferentFile";

            ViewPanel.Controls.Add(new LiteralControl(", ")); // english
            ChooseADifferentFileLink.Text = "change";
            ViewPanel.Controls.Add(ChooseADifferentFileLink);

            DeleteFileLink = new HyperLink();
            DeleteFileLink.ID = "hlDeleteFile";

            ViewPanel.Controls.Add(new LiteralControl(", or ")); // english
            DeleteFileLink.Text = "delete";
            ViewPanel.Controls.Add(DeleteFileLink);

            UploadedFileText = new LiteralControl(" uploaded file");
            ViewPanel.Controls.Add(UploadedFileText); // english

            FileUpload = new FileUpload();
            FileUpload.TabIndex = TabIndex;
            FileUpload.ID = "fuUpload";
            UploadPanel.Controls.Add(FileUpload);

            CancelUploadLink = new HyperLink();
            CancelUploadLink.ID = "hlCancelUpload";
            CancelUploadLink.Text = "(cancel)";
            UploadPanel.Controls.Add(CancelUploadLink);

            HiddenState = new HiddenField();
            HiddenState.ID = "state";
            Controls.Add(HiddenState);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                ChooseADifferentFileLink.NavigateUrl = DeleteFileLink.NavigateUrl =
                    string.Format("javascript: FileUploadCoordinator_ChooseADifferentFile( '{0}' );", ClientID);
                CancelUploadLink.NavigateUrl = string.Format("javascript: FileUploadCoordinator_CancelUpload( '{0}' );",
                                                             ClientID);

                FileDownloadLink.NavigateUrl = string.Format("{0}", LabelControlManager.getImageServerUri(FileID) );
                if (!string.IsNullOrWhiteSpace(FileName))
                    UploadedFileText.Text = string.Format(" {0}", FileName);

                if (! String.IsNullOrEmpty(FileID)) // we have a file
                {
                    UploadPanel.Attributes["style"] = "display: none;";
                    // MS-4973
                    HiddenState.Value = "OLD";

                    FileDownloadLink.CssClass = "standardLink";
                    ChooseADifferentFileLink.CssClass = DeleteFileLink.CssClass = "standardLink";

                    if (!FileID.IsGuid())    // it's not downloadable
                    {
                        FileDownloadLink.NavigateUrl = null;
                        FileDownloadLink.Enabled = false;
                        FileDownloadLink.Text = "Use existing file";
                    }
                }
                else
                {
                    ViewPanel.Attributes["style"] = "display: none;";
                    // MS-4467. "NEW" makes browser to think that value has been specified and
                    // client-side validation message does not include file upload field even if it is marked as required.
                    // HiddenState.Value = "NEW";
                    HiddenState.Value = "";
                    CancelUploadLink.Visible = false; // no need to show the link
                }
            }
        }
    }
}