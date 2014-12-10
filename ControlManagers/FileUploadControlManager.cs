using System;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Web.Controls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class FileUploadControlManager : SingleControlManager<FileUploadCoordinator>
    {
        public override void DataBind()
        {
            base.DataBind();
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            try
            {
                PrimaryControl.FileID = Convert.ToString(obj);
                if (PrimaryControl.FileID == "")
                    PrimaryControl.FileID = null;
            }
            catch
            {
            }
        }


        public override void DataUnbind()
        {
            base.DataUnbind();

            // ATTENTION: If you're for some reason finding that files are inexplicable NOT being uploaded,
            // you should know that FileUpload controls don't work when the AjaxTransitionMessage is showing; so if you
            // have that enabled, files aren't working

            switch (PrimaryControl.State)
            {
                case FileUploadCoordinator.FileUploadState.NoFileSpecified:
                     string oldExpression = ControlMetadata.DataSourceExpression;
                    ControlMetadata.DataSourceExpression += "_Contents";
                    Host.SetModelValue(ControlMetadata, null );
                    ControlMetadata.DataSourceExpression = oldExpression;
                    return;

                case FileUploadCoordinator.FileUploadState.NewFileSpecified:
                    //MS-1289 - Unable to delete an image in the portal/console
                    //If the control is in the state for uploading a file then the user has clicked on upload a different file or delete file
                    //In this case we should set null for the _contents field if no file is specified so the API knows to delete the file
                    //Only if a file is specified should we construct the MemberSuiteFile
                    MemberSuiteFile f = null;
                    if (PrimaryControl.FileUpload.HasFile && PrimaryControl.FileUpload.PostedFile != null)
                    {
                        f = new MemberSuiteFile();
                        f.FileContents = PrimaryControl.FileUpload.FileBytes;
                        f.FileName = PrimaryControl.FileUpload.FileName;
                        f.FileType = PrimaryControl.FileUpload.PostedFile.ContentType;
                    }

                    string oldExpressin = ControlMetadata.DataSourceExpression;
                    ControlMetadata.DataSourceExpression += "_Contents";
                    Host.SetModelValue(ControlMetadata, f);
                    ControlMetadata.DataSourceExpression = oldExpressin;
                    break;

                // MS-4973. If state has not been changed then do nothing. 
                // This part works just as placeholder here...
                case FileUploadCoordinator.FileUploadState.OldFileSpecified:
                    break;
            }
        }
    }
}