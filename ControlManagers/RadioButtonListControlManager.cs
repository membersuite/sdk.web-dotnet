using System.Web.UI.WebControls;
using MemberSuite.SDK.Types;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class RadioButtonListControlManager : SingleItemListControlManager<RadioButtonList>
    {
        public override void DataBind()
        {
           
            base.DataBind();

            // let's enforce the show first value
            if (PrimaryControl.SelectedIndex == -1)
            {
                FieldMetadata obj = Host.GetBoundFieldFor(ControlMetadata);
                if (obj != null && obj.NullValueLabel == "<showfirstvalue>") // show the first value
                    PrimaryControl.SelectedIndex = 0;
           
            }


        }
        public override bool CustomLabel
        {
            get
            {
                return ControlMetadata != null && ControlMetadata.UseEntireRow;
            }
        }
        protected override RadioButtonList instantiatePrimaryControl()
        {
            RadioButtonList c = base.instantiatePrimaryControl();
            c.RepeatLayout = RepeatLayout.Flow;
            return c;
        }
    }
}