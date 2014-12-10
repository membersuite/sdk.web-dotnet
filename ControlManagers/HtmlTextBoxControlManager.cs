using System;
using System.Collections.Generic;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Web.Extensions.Telerik;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class HtmlTextBoxControlManager : SingleControlManager<RadEditor>
    {
        public override bool CustomLabel
        {
            get { return true; }
        }


        public static string ToolsFile { get; set; }
        /// <summary>
        /// Instantiates the primary control.
        /// </summary>
        /// <returns></returns>
        protected override RadEditor instantiatePrimaryControl()
        {
            RadEditor tb = base.instantiatePrimaryControl();
            //tb.ToolsFile = "~/Console/controls/telerik/ToolsFileLimited.xml";

            if (ToolsFile != null)
                tb.ToolsFile = ToolsFile;   // use what's set
            else
                tb.ToolsFile = "~/Console/controls/telerik/ToolsFileDeluxe.xml";
            tb.NewLineMode = EditorNewLineModes.P;

            tb.ImageManager.ContentProviderTypeName = typeof (FileCabinetImagesProvider).AssemblyQualifiedName;

            string rootDir = "Root\\" + FileCabinetImagesProvider.IMAGES_FOLDER_NAME;
            tb.ImageManager.ViewPaths = new string[] { rootDir };
            tb.ImageManager.UploadPaths = new string[] { rootDir };
            tb.ImageManager.DeletePaths = new string[] { rootDir };
            tb.ImageManager.EnableImageEditor = false;
            tb.ImageManager.EnableThumbnailLinking = false;
            return tb;
        }


        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            try
            {
                PrimaryControl.Content = Convert.ToString(obj);
            }
            catch
            {
            }

            try
            {
                // let's try to bind code snippets
                if (ControlMetadata.AcceptableValuesDataSource != null)
                {
                    var rawValues = Host.ResolveAcceptableValues(ControlMetadata) ;

                    var values = rawValues as List<NameValueStringPair>;

                    if (rawValues != null && values == null)
                        throw new ApplicationException("Only name/value string pairs can be bound to a Html TextBox manager acceptable values.");

                    if (values != null)
                        foreach (NameValueStringPair value in values)
                            PrimaryControl.Snippets.Add(value.Name, value.Value.StartsWith( "##") ?  value.Value : string.Format( "##{0}##", value.Value ));
                }
            }
            catch
            {
            }
        }

        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.Content);
        }
    }
}