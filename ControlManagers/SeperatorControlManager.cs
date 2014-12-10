using System;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class SeperatorControlManager : SingleControlManager<Label>
    {
        protected override Label instantiatePrimaryControl()
        {
            Label l = base.instantiatePrimaryControl();
            l.Text = IsInPortal ? "&nbsp;" : "";
            return l;
        }

        protected override void setupTabIndex()
        {
            // no-op - this isn't a tab
        }

        protected override string processLabel(string label)
        {
            if (String.IsNullOrEmpty(label))
                return null;

            return string.Format("<span class='separatorLabel'>{0}</span>", label);
        }
    }
}