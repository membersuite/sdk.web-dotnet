using System;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class CheckBoxControlManager : SingleControlManager<CheckBox>
    {
        public override void DataBind()
        {
            base.DataBind();
            try
            {
                PrimaryControl.Checked = Convert.ToBoolean(Host.Resolve(ControlMetadata));
            }
            catch
            {
            }
        }



        protected override CheckBox instantiatePrimaryControl()
        {
            var cb = base.instantiatePrimaryControl();

            if (ControlMetadata != null && ControlMetadata.UseEntireRow) // we need to supply the text
                cb.Text = GetLabel();

            return cb;
        }

        protected override string processLabel(string label)
        {
            return label;   // don't add a colon
        }

        protected override RequiredFieldValidator instantiateRequiredFieldValiator(System.Collections.Generic.List<System.Web.UI.Control> controls, string controlID)
        {
            // no-op, since checkboxes don't have required field validators
            return null;
        }

        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.Checked);
        }
    }
}