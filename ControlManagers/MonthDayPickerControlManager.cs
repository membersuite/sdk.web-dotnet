using System;
using MemberSuite.SDK.Web.Controls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class MonthDayPickerControlManager : SingleControlManager<MonthDayPicker>
    {
        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            try
            {
                PrimaryControl.Date = Convert.ToDateTime(obj);
            }
            catch
            {
            }
        }

        public override bool CanBeValidated { get { return false; } }

        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.Date);
        }
    }
}