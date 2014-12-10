using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

namespace MemberSuite.SDK.Web.Extensions.Telerik
{
    public class ConciergeImagesProvider : FileBrowserContentProvider
    {
        #region Fields

        private static readonly List<string> mimeTypes = new List<string> { "image/bmp", "image/gif", "image/jpeg", "image/pjpeg" };

        
        #endregion

        #region Properties

        public override bool CanCreateDirectory
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Constructors

        public ConciergeImagesProvider(HttpContext context, string[] searchPatterns, string[] viewPaths, string[] uploadPaths, string[] deletePaths, string selectedUrl, string selectedItemTag)
            :
            base(context, searchPatterns, viewPaths, uploadPaths, deletePaths, selectedUrl, selectedItemTag)
        {
        }

        #endregion

        private DataRow GetAssociationRow()
        {
            

            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                MemberSuiteObject config = proxy.GetAssociationConfiguration().ResultValue;
                long partitionKey = (long) config["PartitionKey"];


                Search sAssociation = new Search(msAssociation.CLASS_NAME);
                sAssociation.AddOutputColumn("ID");
                sAssociation.AddOutputColumn("PartitionKey");
                sAssociation.AddCriteria(Expr.Equals("PartitionKey", partitionKey));

                ConciergeResult<SearchResult> srAssociation = proxy.ExecuteSearch(sAssociation, 0, null);
                DataRow dr = srAssociation.ResultValue.Table.Rows[0];
                return dr;
            }

        }

        private string GetName(string path)
        {
            if (String.IsNullOrEmpty(path) || path == "/")
            {
                return string.Empty;
            }
            path = VirtualPathUtility.RemoveTrailingSlash(path);
            return path.Substring(path.LastIndexOf('/') + 1);
        }


        private string GetDirectoryPath(string path)
        {
            //return path.Substring(0, path.LastIndexOf('/') + 1);
            return "ROOT/Images";
        }

        private FileItem[] GetChildFiles(string _path)
        {
            try
            {
                DataRow associationRow = GetAssociationRow();
                string partitionKey = associationRow["PartitionKey"].ToString();
                string associationId = associationRow["ID"].ToString();

                //No need to mess with the path right now - currently each association only has one root "directory"
                Search sImages = new Search(msFile.CLASS_NAME);
                sImages.AddOutputColumn("ID");
                sImages.AddOutputColumn("Name");
                sImages.AddOutputColumn("ContentLength");
                sImages.AddOutputColumn("Extension");

                SearchOperationGroup contentTypeGroup = new SearchOperationGroup
                                                            {
                                                                FieldName = msFile.FIELDS.ContentType,
                                                                GroupType = SearchOperationGroupType.Or
                
                                                            };

                foreach (string mimeType in mimeTypes)
                    contentTypeGroup.Criteria.Add(Expr.Equals(msFile.FIELDS.ContentType, mimeType));    

                sImages.AddCriteria(contentTypeGroup);

                using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
                {
                    ConciergeResult<SearchResult> srImages = proxy.ExecuteSearch(sImages, 0, null);
                    if (!srImages.Success || srImages.ResultValue.Table == null ||
                        srImages.ResultValue.Table.Rows.Count == 0)
                        return new FileItem[] {};



                    FileItem[] result = (from DataRow row in srImages.ResultValue.Table.Rows
                            let id = row["ID"].ToString()
                            let url = string.Format("{0}/{1}/{2}/{3}", ConfigurationManager.AppSettings["ImageServerUri"], associationId,
                                     partitionKey, id)
                            let permissions = GetPermissions(id)
                            let extension = row["Extension"] == DBNull.Value ? "" : (string) row["Extension"]
                            let contentLength = row["ContentLength"] == DBNull.Value ? 0 : (long) row["ContentLength"]
                            let name = row["Name"].ToString()
                            let filename = GetFileName(name, id, extension)
                            select
                                new FileItem(filename,
                                             extension,
                                             contentLength,
                                             filename,
                                             url, 
                                             string.Empty,
                                             permissions)
                           ).ToArray();
                    return result;
                }

            }
            catch (Exception )
            {
                return new FileItem[] { };
            }
        }

        /// <summary>
        /// Returns the permissions for the provided path
        /// </summary>
        /// <param name="pathToItem">Path to an item</param>
        /// <returns></returns>
        private PathPermissions GetPermissions(string pathToItem)
        {
            return PathPermissions.Read | PathPermissions.Upload | PathPermissions.Delete;
        }


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
            DirectoryItem returnValue = new DirectoryItem(GetName(path),
                                                            GetDirectoryPath(path),
                                                            path,
                                                            string.Empty,
                                                            GetPermissions(path),
                                                            null, // The files  are added in ResolveDirectory()
                                                            new DirectoryItem[]{}); // currently each association only has one root "directory"
            return returnValue;
        }

        public override DirectoryItem ResolveDirectory(string path)
        {
            //DirectoryItem[] directories = GetChildDirectories(path);

            DirectoryItem returnValue = new DirectoryItem(GetName(path),
                                                              VirtualPathUtility.AppendTrailingSlash(GetDirectoryPath(path)),
                                                              path,
                                                              string.Empty,
                                                              GetPermissions(path),
                                                              GetChildFiles(path),
                                                              null // Directories are added in ResolveRootDirectoryAsTree()
                                                              );

            return returnValue;
        }

        public string GetFileName(string name, string id, string extension)
        {
            string firstPart = name.Contains(".") ? name.Substring(0, name.LastIndexOf(".")) : name;
            return string.Format("{0}_{1}.{2}", firstPart, id, extension);
        }

        public override string GetFileName(string url)
        {
            string path = RemoveProtocolNameAndServerName(url);
            Match m = Regex.Match(path, RegularExpressions.FileNameIdRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (!m.Success)
                return null;

            string result = m.Groups["FileId"].Value;
            return result;
        }

        public override string GetPath(string url)
        {
            string result = GetDirectoryPath(RemoveProtocolNameAndServerName(url));
            return result;
        }

        public override Stream GetFile(string url)
        {
            string fileName = GetFileName(url);
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            Search sImage = new Search(msFile.CLASS_NAME);
            sImage.AddOutputColumn("FileContents");
            sImage.AddCriteria(Expr.Equals("ID", fileName));

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

        public override string StoreFile(UploadedFile file, string path, string name, params string[] arguments)
        {
            int fileLength = Convert.ToInt32(file.InputStream.Length);
            byte[] content = new byte[fileLength];
            file.InputStream.Read(content, 0, fileLength);

            MemberSuiteObject uploadFile = new MemberSuiteObject();
            uploadFile.ClassType = msFile.CLASS_NAME;
            uploadFile["ContentType"] = file.ContentType;
            uploadFile["ContentLength"] = file.ContentLength;
            uploadFile["FileContents"] = content;
            uploadFile["Name"] = name;

            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                proxy.Save(uploadFile);
            }

            return string.Empty;
        }

        public override string DeleteFile(string path)
        {
            string fileName = GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            using (IConciergeAPIService proxy = ConciergeAPIProxyGenerator.GenerateProxy())
            {
                proxy.Delete(fileName);
            }
            return string.Empty;
        }

        public override string StoreBitmap(Bitmap bitmap, string url, ImageFormat format)
        {
            throw new NotImplementedException();
        }

        public override string MoveFile(string path, string newPath)
        {
            throw new NotImplementedException();
        }

        public override string MoveDirectory(string path, string newPath)
        {
            throw new NotImplementedException();
        }

        public override string CopyFile(string path, string newPath)
        {
            throw new NotImplementedException();
        }

        public override string CopyDirectory(string path, string newPath)
        {
            throw new NotImplementedException();
        }

        public override string DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public override string CreateDirectory(string path, string name)
        {
            throw new NotImplementedException();
        }
    }
}