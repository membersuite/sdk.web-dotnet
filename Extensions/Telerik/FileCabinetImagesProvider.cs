using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Results;
using MemberSuite.SDK.Searching;
using MemberSuite.SDK.Searching.Operations;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Utilities;
using Telerik.Web.UI;
using Telerik.Web.UI.Widgets;
using FileInfo = MemberSuite.SDK.Types.FileInfo;

namespace MemberSuite.SDK.Web.Extensions.Telerik
{
    public class FileCabinetImagesProvider : FileBrowserContentProvider
    {
        public override char PathSeparator
        {
            get
            {
                return '\\';
            }
        }

        public override bool CanCreateDirectory
        {
            get
            {
                return true;
            }
        }
        public const string IMAGES_FOLDER_NAME = "Uploaded Images";

        #region Constructors

        public FileCabinetImagesProvider(HttpContext context, string[] searchPatterns, string[] viewPaths, string[] uploadPaths, string[] deletePaths, string selectedUrl, string selectedItemTag)
            :
            base(context, searchPatterns, viewPaths, uploadPaths, deletePaths, selectedUrl, selectedItemTag)
        {
            using( var api = ConciergeAPIProxyGenerator.GenerateProxy() )
            {
                var msoAssoc = api.WhoAmI().ResultValue.Association;
                _partitionKey = msoAssoc.SafeGetValue<long>(msAssociation.FIELDS.PartitionKey);
                _associationID = msoAssoc.SafeGetValue<string>("ID");
            }
        }

        #endregion


        /// <summary>
        /// Loads a root directory with given path, where all subdirectories 
        /// contained in the SelectedUrl property are loaded
        /// </summary>
        /// <remarks>
        /// The ImagesPaths, DocumentsPaths, etc properties of RadEditor
        /// allow multiple root items to be specified, separated by comma, e.g.
        /// Photos,Paintings,Diagrams. The FileBrowser class calls the 
        /// ResolveRootDirectoryAsTree method for each of them.
        /// </remarks>
        /// <param name="path">the root directory path, passed by the FileBrowser</param>
        /// <returns>The root DirectoryItem or null if such does not exist</returns>
        public override DirectoryItem ResolveRootDirectoryAsTree(string path)
        {
            msFileFolder rootFolder = null;
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                var root =
                    api.GetFileCabinetRootFolder(null ).ResultValue.ConvertTo
                        <msFileFolder>();

                // find a folder called "Uploaded Images"
                Search s = new Search(msFileFolder.CLASS_NAME);
                s.AddCriteria(Expr.Equals(msFileFolder.FIELDS.ParentFolder, root.ID));
                s.AddCriteria(Expr.Equals("Name", IMAGES_FOLDER_NAME));


                var searchResult = api.ExecuteSearch(s, 0, 1).ResultValue;
                if (searchResult.TotalRowCount == 0) // we need to create it
                {

                    rootFolder = new msFileFolder();
                    rootFolder.Name = IMAGES_FOLDER_NAME;
                    rootFolder.FileCabinet = root.FileCabinet;
                    rootFolder.ParentFolder = root.ID;
                    rootFolder.Type = FileFolderType.Public;

                    api.Save(rootFolder);
                }
                //else
                //    // root folder is set
                //    rootFolder =
                //        api.Get(Convert.ToString(searchResult.Table.Rows[0]["ID"])).ResultValue.ConvertTo
                //            <msFileFolder>();



                // get the child folders
                var subFolders = GetChildDirectories(path);

                var parts = path.Split('\\');
                string name = parts[parts.Length - 1];
                DirectoryItem di = new DirectoryItem(name, path.Substring( 0, path.Length - name.Length + 1)    // remove the name and trailing edge
                    , path, string.Empty,
                                                      PathPermissions.Upload | PathPermissions.Read | PathPermissions.Delete,
                                                     GetChildFiles(path), 
                                                     subFolders);


                return di;
            }
        }

        protected DirectoryItem[] GetChildDirectories(string path )
        {
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                Search sSubFolders = new Search(msFileFolder.CLASS_NAME);
                sSubFolders.AddCriteria(Expr.DoesNotEqual("FolderPath", path)); // don't include the current folder!
                sSubFolders.AddCriteria(Expr.Equals("FileCabinet.FileCabinetType", "Association"));
                sSubFolders.AddCriteria(Expr.Equals("ParentFolder.FolderPath", path));
                sSubFolders.AddOutputColumn("Name");
                sSubFolders.AddOutputColumn("FolderPath");
                sSubFolders.AddOutputColumn("ParentFolder.FolderPath");

                var dtSubFolders = api.ExecuteSearch(sSubFolders, 0, null).ResultValue;

                DirectoryItem[] subFolders = new DirectoryItem[dtSubFolders.Table.Rows.Count];
                for (int i = 0; i < dtSubFolders.Table.Rows.Count; i++)
                {
                    DataRow dr = dtSubFolders.Table.Rows[i];
                    subFolders[i] = new DirectoryItem(Convert.ToString(dr["Name"]), // name
                                                      Convert.ToString(dr["ParentFolder.FolderPath"]), // location
                                                      Convert.ToString(dr["FolderPath"]), "",
                                                      PathPermissions.Upload | PathPermissions.Read |
                                                      PathPermissions.Delete,
                                                      null, null);
                }
                return subFolders;
            }
        }

        public override DirectoryItem ResolveDirectory(string path)
        {

            var files = GetChildFiles(path);
            var parts = path.Split('\\');
            string name = parts[parts.Length - 1];
            DirectoryItem di = new DirectoryItem(name, path, path, string.Empty,
                                                     PathPermissions.Upload | PathPermissions.Read | PathPermissions.Delete,
                                                     files,
                                                     GetChildDirectories(path ) // The directory are added in ResolveRootDirectory()
                                                     );
                return di;
           
        }

        protected FileItem[] GetChildFiles(string path)
        {
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                // get the child files
                Search sFilesInFolder = new Search(msFile.CLASS_NAME);

                sFilesInFolder.AddCriteria(Expr.Equals("FileCabinet.FileCabinetType", "Association"));
                sFilesInFolder.AddCriteria(Expr.Equals("FileFolder.FolderPath", path));
                sFilesInFolder.AddOutputColumn("Name");
                sFilesInFolder.AddOutputColumn("Extension");
                sFilesInFolder.AddOutputColumn("ContentLength");
                sFilesInFolder.AddOutputColumn("FolderPath");

                var dtFiles = api.ExecuteSearch(sFilesInFolder, 0, null).ResultValue;

                FileItem[] files = new FileItem[dtFiles.Table.Rows.Count];
                for (int i = 0; i < dtFiles.Table.Rows.Count; i++)
                {
                    DataRow dr = dtFiles.Table.Rows[i];
                    files[i] = new FileItem(Convert.ToString(dr["Name"]), // name
                                            Convert.ToString(dr["Extension"]), // extension
                                            Convert.ToInt32(dr["ContentLength"]),
                                            Convert.ToString(dr["FolderPath"]), // location
                                            GetImageUrl( Convert.ToString( dr["ID"] ) ),
                                             "", 
                                             PathPermissions.Upload | PathPermissions.Read | PathPermissions.Delete );
                    
                }

                return files;
            }
        }

        protected long? _partitionKey;
        protected string _associationID;

        public string GetImageUrl(string imageID)
        {
            return String.Format("{0}/{1}/{2}/{3}", ConfigurationManager.AppSettings["ImageServerUri"],
                                 _associationID  ,
                                 _partitionKey ,
                                 imageID);
        }

        public override string GetFileName(string url)
        {
             // get that last file ID
            string[] parts = url.Split('\\');
            return parts[parts.Length - 1];

            //Search sImage = new Search(msFile.CLASS_NAME);
            //sImage.AddOutputColumn("FolderPath");
            //sImage.AddCriteria(Expr.Equals("ID", fileID ));

            //using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            //{
            //    ConciergeResult<SearchResult> srImage = proxy.ExecuteSearch(sImage, 0, null);
            //    if (!srImage.Success || srImage.ResultValue.Table == null || srImage.ResultValue.Table.Rows.Count == 0)
            //        return null;

            //    return Convert.ToString(srImage.ResultValue.Table.Rows[0]["Name"]); // return the path
            //}
        }

        public override string GetPath(string url)
        {
            return url;
            //if (string.IsNullOrWhiteSpace(url))
            //    return null;

            //// get that last file ID
            //string[] parts = url.Split('\\');
            //string fileID = parts[parts.Length - 1];

            //Search sImage = new Search(msFile.CLASS_NAME);
            //sImage.AddOutputColumn("FolderPath");
            //sImage.AddCriteria(Expr.Equals("ID", fileID ));

            //using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            //{
            //    ConciergeResult<SearchResult> srImage = proxy.ExecuteSearch(sImage, 0, null);
            //    if (!srImage.Success || srImage.ResultValue.Table == null || srImage.ResultValue.Table.Rows.Count == 0)
            //        return null;

            //    return Convert.ToString(srImage.ResultValue.Table.Rows[0]["FolderPath"]);// return the path

            //}
        }

        public override Stream GetFile(string url)
        {

            if (string.IsNullOrWhiteSpace(url))
                return null;

            Search sImage = new Search(msFile.CLASS_NAME);
            sImage.AddOutputColumn("FileContents");
            sImage.AddCriteria(Expr.Equals("FolderPath", url));
            sImage.AddCriteria(Expr.Equals("FileCabinet.FileCabinetType", "Association"));  // make sure we're in the main cabinet

            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                ConciergeResult<SearchResult> srImage = proxy.ExecuteSearch(sImage, 0, null);
                if (!srImage.Success || srImage.ResultValue.Table == null || srImage.ResultValue.Table.Rows.Count == 0)
                    return null;

                byte[] content = (byte[])srImage.ResultValue.Table.Rows[0]["FileContents"];
                if (!Object.Equals(content, null))
                {
                    return new MemoryStream(content);
                }
                return null;
            }
        }

        public override string StoreBitmap(Bitmap bitmap, string url, ImageFormat format)
        {
            throw new NotImplementedException();
        }

        public override string StoreFile(UploadedFile file, string path, string name, params string[] arguments)
        {

            path = path.Trim('\\'); // important

            int fileLength = Convert.ToInt32(file.InputStream.Length);
            byte[] content = new byte[fileLength];
            file.InputStream.Read(content, 0, fileLength);

            msFile uploadFile = new msFile();
            
            uploadFile.ContentType = file.ContentType;
            uploadFile.ContentLength  = file.ContentLength;
            uploadFile["FileContents"]  = content;
            uploadFile.Name = name;


            saveFileToPath(path, uploadFile);

            return string.Empty;
        }

        protected FolderInfo GetFolderByPath(string path)
        {
            path = path.Trim('\\'); // important
            Search s = new Search(msFileFolder.CLASS_NAME);
            s.AddCriteria(Expr.Equals("FileCabinet.FileCabinetType", "Association")); // make sure we're in the main cabinet
            s.AddCriteria(Expr.Equals("FolderPath", path));
            s.AddOutputColumn("FileCabinet");
            s.AddOutputColumn("Name");


            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                var dtFolderInfo = proxy.ExecuteSearch(s, 0, 1).ResultValue;
                if (dtFolderInfo.TotalRowCount == 0)
                    return null ;

                FolderInfo pi = new FolderInfo();
                pi.FolderName = Convert.ToString(dtFolderInfo.Table.Rows[0]["Name"]);
                pi.FileCabinetID = Convert.ToString(dtFolderInfo.Table.Rows[0]["FileCabinet"]);
                pi.FolderID = Convert.ToString(dtFolderInfo.Table.Rows[0]["ID"]);

                return pi;
            }
        }

        protected FileInfo GetFileByPath(string path)
        {
            path = path.Trim('\\'); // important
            Search s = new Search(msFile.CLASS_NAME);
            s.AddCriteria(Expr.Equals("FileCabinet.FileCabinetType", "Association")); // make sure we're in the main cabinet
            s.AddCriteria(Expr.Equals("FolderPath", path));
            s.AddOutputColumn("FileCabinet");
            s.AddOutputColumn("Name");


            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                var dtFolderInfo = proxy.ExecuteSearch(s, 0, 1).ResultValue;
                if (dtFolderInfo.TotalRowCount == 0)
                    return null;

                FileInfo pi = new FileInfo();
                pi.FileName = Convert.ToString(dtFolderInfo.Table.Rows[0]["Name"]);
                pi.FileID = Convert.ToString(dtFolderInfo.Table.Rows[0]["ID"]);

                return pi;
            }
        }

        protected void saveFileToPath(string path, msFile uploadFile )
        {
            var fi = GetFolderByPath(path);
            if (fi == null)
                return;

            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
            

                // set the folder
                uploadFile.FileCabinet = fi.FileCabinetID;
                uploadFile.FileFolder = fi.FolderID;
                proxy.Save(uploadFile);
            }
         
        }

        public override string DeleteFile(string url)
        {
            url = url.Trim('\\'); // important
            Search sImage = new Search(msFile.CLASS_NAME);
            sImage.AddOutputColumn("FileContents");
            sImage.AddCriteria(Expr.Equals("FolderPath", url));
            sImage.AddCriteria(Expr.Equals("FileCabinet.FileCabinetType", "Association"));  // make sure we're in the main cabinet

            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                ConciergeResult<SearchResult> srImage = proxy.ExecuteSearch(sImage, 0, null);
                if (!srImage.Success || srImage.ResultValue.Table == null || srImage.ResultValue.Table.Rows.Count == 0)
                    return "File not found";

                proxy.Delete(Convert.ToString(srImage.ResultValue.Table.Rows[0]["ID"]));    // delete the file
 
                return "";
            }
        }

        public override string MoveDirectory(string path, string newPath)
        {
            var fi = GetFolderByPath(path);

            // so the new path has the folder name on it, so
            var m = Regex.Match(newPath, RegularExpressions.PathSplitterRegex, RegexOptions.Compiled);

            string newFolder = m.Groups[1].Value;
            string newName = m.Groups[2].Value; 

            var fiNew = GetFolderByPath(newFolder);

            if (fi == null || fiNew == null) return "Unable to resolve path.";

            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                msFileFolder ff = api.Get(fi.FolderID).ResultValue.ConvertTo<msFileFolder>();
                ff.Name = newName;
                ff.ParentFolder = fiNew.FolderID ;

                api.Save(ff); // update
            }
            return "";
        }

        public override string MoveFile(string path, string newPath)
        {
            var fi = GetFileByPath(path);

            // so the new path has the folder name on it, so
            var m = Regex.Match(newPath, RegularExpressions.PathSplitterRegex, RegexOptions.Compiled);

            string newFolder = m.Groups[1].Value;
            string newName = m.Groups[2].Value;

            var fiNew = GetFolderByPath(newFolder);

            if (fi == null || fiNew == null) return "Unable to resolve path.";

            

            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                msFile ff = api.Get(fi.FileID ).ResultValue.ConvertTo<msFile >();
                ff.Name = newName;

                // do not allow renames - there's a bug in the telerik control
                if (ff.FileFolder == fiNew.FolderID)
                    return "Renames are not allowed. Please perform your rename in the Documents module.";
                ff.FileFolder  = fiNew.FolderID;

                api.Save(ff); // update
            }
            return "";
        }

        public override string DeleteDirectory(string path)
        {
            var fi = GetFolderByPath(path);
            if (fi == null) return "Folder not found";
            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
                api.DeleteFolderTree(fi.FolderID);

            return "";
        }

        public override string CreateDirectory(string path, string name)
        {
            path = path.TrimEnd('\\');
            var fi = GetFolderByPath(path);
            if (fi == null) return "Path not found.";

            msFileFolder ff = new msFileFolder();
            ff.Name = name;
            ff.FileCabinet = fi.FileCabinetID;
            ff.ParentFolder = fi.FolderID;
            ff.Type = FileFolderType.Public;

            using (var api = ConciergeAPIProxyGenerator.GenerateProxy())
                api.Save(ff);

            return "";
        }
    }
}
