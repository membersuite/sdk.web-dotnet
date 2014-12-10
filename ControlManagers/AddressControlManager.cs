using System.Collections.Generic;
using System.Web.UI;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Web.Controls;
using MemberSuite.SDK.Web.Globalization;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class AddressControlManager : SingleControlManager<AddressControl>
    {
        public override bool CustomLabel
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the row span.
        /// </summary>
        /// <value>The row span.</value>
        /// <remarks>This is used by controls that take up multiple rows -
        /// like the AddressControl.</remarks>
        public override int RowSpan
        {
            get { return 7; }
        }

        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            try
            {
                PrimaryControl.Address = obj as Address;
            }
            catch
            {
            }
        }

        protected override void renderCustomLabel(List<Control> controls)
        {
            // no-op
        }

        protected override AddressControl instantiatePrimaryControl()
        {
            AddressControl ac = base.instantiatePrimaryControl();
            ac.AddressName = GetLabel().TrimEnd(':');
            if (Currency.Current != "USD")    // then don't enable address validation
                ac.EnableValidation = false;

            return ac;
        }

        protected override System.Web.UI.WebControls.RequiredFieldValidator instantiateRequiredFieldValiator(System.Collections.Generic.List<System.Web.UI.Control> controls, string controlID)
        {
            PrimaryControl.IsRequired = IsRequired();
            return null;
        }

        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.Address);
        }
    }
}