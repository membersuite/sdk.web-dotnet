using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MemberSuite.SDK.Web.Globalization
{
    public static class Currency
    {
        /// <summary>
        /// Tracks the current currency
        /// </summary>
        [ThreadStatic]
        private static string _current;

        public static string Current
        {
            get
            {
                string currentCurrency = null;
                if (HttpContext.Current != null)
                    currentCurrency =
                        HttpContext.Current.Items["MemberSuite.SDK.Web.Globalization.CurrentCurrency"] as string;

                if (currentCurrency == null)
                    currentCurrency = _current;

                if (currentCurrency == null)
                    currentCurrency = "USD";

                return currentCurrency;

            }
            set
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Items["MemberSuite.SDK.Web.Globalization.CurrentCurrency"] = value;
                else
                    _current = value;
            }
        }
    }
}
