using System;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class HyperLinkControlManager : SingleControlManager<HyperLink>
    {
        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            PrimaryControl.NavigateUrl = Convert.ToString(obj);
            PrimaryControl.Text = GetLabel();
        }
    }
}