using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class ListBoxControlManager : SingleItemListControlManager<ListBox>
    {
        protected override ListBox instantiatePrimaryControl()
        {
            ListBox lb = base.instantiatePrimaryControl();

            lb.SelectionMode = ListSelectionMode.Single;
            ;
            return lb;
        }
    }
}