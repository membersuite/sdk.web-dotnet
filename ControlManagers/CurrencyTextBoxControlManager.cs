using System.Web.UI;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Web.Controls;
using MemberSuite.SDK.Web.Globalization;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class CurrencyTextBoxControlManager : NumericTextBoxControlManager
    {
        public override System.Collections.Generic.List<System.Web.UI.Control> Instantiate()
        {
            var controls = base.Instantiate();

            string currency = Currency.Current ;
            
            controls.Add(new LiteralControl(string.Format("<B>&nbsp;{0}</B>", currency)));

            HtmlDividerControl div = new HtmlDividerControl(null, "width: 150px; padding: 0px !important;");
            foreach (var c in controls)
                div.Controls.Add(c);
            controls.Clear();
            controls.Add(div);

            return controls;
        }
    }
}