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

        public static string CurrentCustomerID
        {
            get { return  HttpContext.Current.Items["CurrentCustomerID"] as string ; }
            set { HttpContext.Current.Items["CurrentCustomerID"] = value; }
        }

        public static string CurrentResellerID
        {
            get { return HttpContext.Current.Items["CurrentResellerID"] as string; }
            set { HttpContext.Current.Items["CurrentResellerID"] = value; }
        }

        public static string CurrentUserID
        {
            get { return HttpContext.Current.Items["CurrentUserID"] as string ; }
            set { HttpContext.Current.Items["CurrentUserID"] = value; }
        }

        public static string CurrentUserType
        {
            get { return HttpContext.Current.Items["CurrentUserType"] as string; }
            set { HttpContext.Current.Items["CurrentUserType"] = value; }
        }
    }
}
