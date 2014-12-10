using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class CheckBoxListControlManager : MultiItemListControlManager<CheckBoxList>
    {
        protected override CheckBoxList instantiatePrimaryControl()
        {
            CheckBoxList c = base.instantiatePrimaryControl();
            c.RepeatLayout = RepeatLayout.Table;
            if (ControlMetadata.Columns > 1)
                c.RepeatColumns = ControlMetadata.Columns;
            c.CssClass += " killTablePadding";
            return c;
        }

        public override bool CanBeValidated { get { return false; } }
    }
}