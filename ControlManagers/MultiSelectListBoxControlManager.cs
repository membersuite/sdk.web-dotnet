using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class MultiSelectListBoxControlManager : MultiItemListControlManager<ListBox>
    {
        protected override ListBox instantiatePrimaryControl()
        {
            ListBox lb = base.instantiatePrimaryControl();

            lb.SelectionMode = ListSelectionMode.Multiple;
            ;
            return lb;
        }
    }
}