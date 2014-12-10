using System;
using System.Web;

namespace MemberSuite.SDK.Web
{
    public static class ControlContext
    {
        public static string CurrentAssociationID
        {
            get { return HttpContext.Current.Items["CurrentAssociationID"] as string; }
            set { HttpContext.Current.Items["CurrentAssociationID"] = value; }
        }

        
        public static long CurrentAssociationKey
        {
            get { return Convert.ToInt64(HttpContext.Current.Items["CurrentAssociationKey"]); }
            set { HttpContext.Current.Items["CurrentAssociationKey"] = value; }
        }
    }
}
