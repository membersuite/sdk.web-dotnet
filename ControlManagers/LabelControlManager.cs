using System;
using System.Collections;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Concierge;
using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Utilities;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class LabelControlManager : SingleControlManager<Label>
    {
        private readonly bool _suppressNullLabelReplacement;
       
        public LabelControlManager(){}

        public LabelControlManager(bool suppressNullLabelReplacement)
        {
            _suppressNullLabelReplacement = suppressNullLabelReplacement;
        }

        public override bool CanBeValidated
        {
            get
            {
                return false;
            }
        }
        public const string FILE_GUID_HINT = "-001C-";
        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            string label = null;
            if (obj == null) // try to lookup the id
            {
                if (_suppressNullLabelReplacement)
                    return;

                if (ControlMetadata.ID != null)
                    label = Host.ResolveResource(ControlMetadata.ID, true );

                if (label == null) // that didn't work
                    label = ControlMetadata.Label;

                if (PrimaryControl != null)
                    PrimaryControl.Text = label;
                return;
            }

            var strObj = obj.ToString();
            if (Regex.IsMatch(strObj, RegularExpressions.GuidRegex, RegexOptions.Compiled) &&
                // but we don't want to do this for files
                ! strObj.ToUpper().Contains( FILE_GUID_HINT) &&
                ( ControlMetadata.Properties == null || !ControlMetadata.Properties.Exists( x=>x.Name == "DisplayGuid" && x.Expression == "true"))
                ) // its a guid - by chance is there a transiet name?
            {

                ControlMetadata cm2 = ControlMetadata.Clone();
                cm2.DataSourceExpression += "_Name__transient";
                object obj2 = Host.Resolve(cm2);
                if (obj2 != null)
                    obj = obj2;

                // let's try to resolve it
                using (var api = Host.GetServiceAPIProxy())
                {
                    try
                    {
                        obj = api.GetName(strObj).ResultValue;
                    }
                    catch
                    {
                    }
                }
            }


            try
            {
                FieldMetadata meta = Host.GetBoundFieldFor(ControlMetadata);
                FieldDataType dt = FieldDataType.Text;
                if (ControlMetadata.DataType != null)
                    dt = ControlMetadata.DataType.Value;
                else
                {
                    if (meta != null)
                        dt = meta.DataType;
                }

                // check for references - we wanna light them up with links

                if (ControlMetadata != null && ControlMetadata.DataSourceExpression != null &&
                    ControlMetadata.DataSourceExpression.EndsWith(".Name"))
                {
                    string referenceField =
                        ControlMetadata.DataSourceExpression.Substring(0,
                                                                       ControlMetadata.DataSourceExpression.Length -
                                                                       ".Name".Length) + ".ID";
                    object refField =
                        Host.Resolve(new ControlMetadata
                                         {
                                             DataSource = ControlMetadata.DataSource,
                                             DataSourceExpression = referenceField
                                         });

                    if (refField != null) // wow! This is actually a reference!
                    {
                        PrimaryControl.Text = string.Format("<a href='/app/console/record/view?c={1}'>{0}</a>", obj,
                                                            /*new CommandShortcut
                                                                {
                                                                    Name = "Console.Record.View",
                                                                    Context = refField.ToString()
                                                                }.
                                                                ToUrl()*/ refField.ToString() );
                        return;
                    }
                }

                if (obj as string != "Loading...")
                    PrimaryControl.Text = DisplayValueAsFormattedString( Host.GetCurrentTimeZone(), dt, obj, true);
            }
            catch
            {
            }
        }

        

        protected override void setupTabIndex()
        {
            // no-op, no tab index here
        }
        public static string DisplayValueAsFormattedString( TimeZoneInfo currentTimeZone, FieldDataType dataType, object obj, bool expandLinks)
        {
            if (obj == null || obj == DBNull.Value )
                return null;

            var list = obj as IList;
            if (list != null)
                return _convertToString(list);

            var a = obj as Address;

            if (a != null)
                return a.ToHtmlString();


            // deal with the date time
            if (obj is DateTime)
            {
                var dt = (DateTime) obj;


                if (dataType == FieldDataType.Date)
                    return dt.ToShortDateString(); // just return the date


                string dateTime;
                if (dataType == FieldDataType.Time)
                    dateTime =  dt.ToShortTimeString();
                else 
                dateTime = string.Format("{0} {1}", dt.ToShortDateString(), dt.ToShortTimeString());


                // MS-2905 - Can't do this anymore, b/c we don't know what time zone this represents
                //try
                //{
                    
                //    dateTime += " " + TimeZoneUtils.GetTimeZoneAbbreviatedTime(dt, currentTimeZone);
                //}
                //catch
                //{
                //    dateTime += " EST";
                //}


                return dateTime;
            }

            if (dataType == FieldDataType.Money)
            {
                // show as Money
                return Convert.ToDecimal(obj).ToString("C");
            }

            if (dataType == FieldDataType.Percentage)
            {
                // show as Money
                return ( Convert.ToDecimal(obj) / 100).ToString("P");
            }

            string objAsString = Convert.ToString(obj);
            if (String.IsNullOrEmpty(objAsString))
                return null;


            if (!expandLinks)
                return objAsString;

            if (dataType == FieldDataType.Email) // display an email
                return string.Format("<a href='mailto:{0}'>{0}</a>", objAsString);

            if (dataType == FieldDataType.Url)
                return string.Format("<a href='{0}' target='_blank'>{0}</a>", objAsString);

            if (dataType == FieldDataType.Document)
                return string.Format("<a href='{0}'>Click here to download file.</a>", getImageServerUri( objAsString ));

            if (dataType == FieldDataType.Image)

                return string.Format("<img src='{0}' width=315 border=0/>", getImageServerUri(objAsString) );
     
             

            return objAsString;
        }
        
        public static string getImageServerUriForPublicImage(string fileID)
        {
            


            return string.Format("{0}/{1}/{2}/{3}", ConfigurationManager.AppSettings["ImageServerUri"],
                                              ControlContext.CurrentAssociationID, ControlContext.CurrentAssociationKey , fileID);


        }
        public static string getImageServerUri( string fileID )
        {
            if (string.IsNullOrWhiteSpace(fileID)) return null;

            string baseUri = getImageServerUriForPublicImage(fileID);

            string imagesSecret = ConfigurationManager.AppSettings["ImagesSecret"];

            if (string.IsNullOrWhiteSpace(imagesSecret))
                throw new ApplicationException(
                    "No images secret has been configured. Please set an images secret in the app.config.");

            DateTime expiration = DateTime.Now.AddMinutes(5);
            // let's contruct the url, with the secret
            string requestExpiration = expiration.ToOADate().ToString();
            string hash = CryptoManager.GetMd5Hash(fileID + requestExpiration + imagesSecret);

            // figure out the parameter character
            string paramChar = baseUri.Contains("?") ? "&" : "?";

            return string.Format("{0}{1}request={2}&signature={3}", baseUri , paramChar, requestExpiration, hash  );
                                             
     
        }

        //public static string getImageServerUri()
        //{
        //    // Fix for  MS-872
        //    var context = HttpContext.Current;
        //    if (context == null) return "/file";  // just use the default
        //    var uri = context.Request.Url.ToString();

        //     Match m = _uriRegex.Match(uri);

        //    if (!m.Success || uri.Contains( "console.")) return "/file";

        //    string environment = m.Groups[2].Value;     // so, if it's portal.trial.membersuite.com, we get trial

        //    string console = "console";
        //    if (environment == "ps") environment = "production";    // HACK!
        //    if ( environment == "qa" ) console = "web";

        //    return string.Format("http{0}://{1}.{2}.membersuite.com/file", context.Request.IsSecureConnection ? "s" : "", console, environment);
           


        //}

        private static string _convertToString(IList list)
        {
            var sb = new StringBuilder();
            foreach (object item in list)
                sb.AppendFormat("{0}, ", Convert.ToString(item));

            return sb.ToString().Trim().TrimEnd(',');
        }
    }
}
