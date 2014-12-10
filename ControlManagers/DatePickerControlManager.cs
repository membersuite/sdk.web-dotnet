using System;
using MemberSuite.SDK.Web.ControlManagers;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class DatePickerControlManager : DatePickerControlManager<RadDatePicker>
    {
    }

    /// <summary>
    /// We derive both date picker AND datetime picker from this class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DatePickerControlManager<T> : SingleControlManager<T> where T : RadDatePicker, new()
    {
        protected override T instantiatePrimaryControl()
        {
            T dp = base.instantiatePrimaryControl();
            dp.MinDate = DateTime.MinValue;
            dp.MaxDate = DateTime.MaxValue;

            return dp;
        }

        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            try
            {
                DateTime date = Convert.ToDateTime(obj);
                date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);    // important to avoid serialization errors

                if (date != default(DateTime))
                    PrimaryControl.SelectedDate = date;
                else
                    PrimaryControl.SelectedDate = null;
            }
            catch
            {
            }
        }

        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.SelectedDate);
        }
    }
}