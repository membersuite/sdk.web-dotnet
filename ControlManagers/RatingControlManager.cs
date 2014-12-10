using System;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Types;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class RatingControlManager : SingleControlManager<RadRating>
    {
        protected override RadRating instantiatePrimaryControl()
        {
            RadRating pc = base.instantiatePrimaryControl();

            pc.ItemCount = 5;

            if (ControlMetadata != null)
            {
                var meta = Host.GetBoundFieldFor(ControlMetadata);
                if (meta != null)
                    pc.Precision = meta.DataType == FieldDataType.Integer ? RatingPrecision.Item : RatingPrecision.Half;
            }

            pc.Orientation = Orientation.Horizontal;
            pc.SelectionMode = RatingSelectionMode.Continuous;

            return pc;
        }

        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            try
            {
                PrimaryControl.Value = Convert.ToDecimal(obj);
            }
            catch
            {
            }
        }

        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.Value);
        }
    }
}