using System.Web.UI.HtmlControls;

namespace MemberSuite.SDK.Web.Controls
{
    public class HtmlDividerControl : HtmlGenericControl
    {
        public HtmlDividerControl(string cssClass)
            : this(cssClass, null)
        {
        }

        public HtmlDividerControl(string cssClass, string style)
            : base("div")
        {
            Attributes["Class"] = cssClass;
            Attributes["style"] = style;
        }
    }
}