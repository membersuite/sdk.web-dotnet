using System;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Web.Controls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class MonthYearPickerControlManager : SingleControlManager<MonthYearPicker>
    {
        protected override MonthYearPicker instantiatePrimaryControl()
        {
            var myp = base.instantiatePrimaryControl();

            FieldMetadata meta = Host.GetBoundFieldFor(ControlMetadata);

            if (meta != null)
            {
                int startYear;
                int endYear;

                if (int.TryParse(meta.StartingYear, out startYear))
                    myp.StartYear = startYear;

                if (int.TryParse(meta.EndingYear, out endYear))
                    myp.EndYear = endYear;
            }


            return myp;
        }
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