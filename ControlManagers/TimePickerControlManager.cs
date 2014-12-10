using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class TimePickerControlManager : SingleControlManager<RadTimePicker> 
    {
        protected override RadTimePicker instantiatePrimaryControl()
        {
            var tp = base.instantiatePrimaryControl();
            tp.MinDate = DateTime.MinValue;
            tp.MaxDate = DateTime.MaxValue;

            return tp;
        }
        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            if (obj == null) return;
          try
            {
                var time = Convert.ToDateTime(obj);

                PrimaryControl.SelectedDate = time;
                
            }
            catch
            {
            }
        }

        public override void DataUnbind()
        {
            base.DataUnbind();

            if (PrimaryControl.SelectedDate == null)
                Host.SetModelValue(ControlMetadata, null);
            else
            {
                DateTime val = PrimaryControl.SelectedDate.Value;
                Host.SetModelValue(ControlMetadata, val  );
            }
        }
    }
}
