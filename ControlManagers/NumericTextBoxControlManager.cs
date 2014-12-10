using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class NumericTextBoxControlManager : TextBoxControlManager
    {
        private const int NUMERIC_TEXTBOX_WIDTH = 60;

        protected override TextBox instantiatePrimaryControl()
        {
            TextBox c = base.instantiatePrimaryControl();

            c.Width = Unit.Pixel(NUMERIC_TEXTBOX_WIDTH);

            return c;
        }

        public override void DataBind()
        {
            object obj = Host.Resolve(ControlMetadata);

            if (obj is decimal)
                PrimaryControl.Text = ((decimal)obj).ToString("F2");
            else
            
            try
            {
                PrimaryControl.Text = Convert.ToString(obj);
            }
            catch
            {
            }
        }

        public override List<Control> InstantiateValidationControls()
        {
            List<Control> c = base.InstantiateValidationControls();

            if (!_isBeingUsed)
            {
                var cv = new CompareValidator();
                cv.ControlToValidate = PrimaryControl.ID;
                cv.Type = ValidationDataType.Double;
                cv.Operator = ValidationCompareOperator.DataTypeCheck;
                cv.Display = ValidatorDisplay.None;

                cv.ErrorMessage = "Please enter a valid numeric value for " + GetLabel().Trim().TrimEnd(':'); // english

                c.Add(cv);
            }
            return c;
        }


        public override void DataUnbind()
        {
            base.DataUnbind();
            decimal value = 0;
            if (decimal.TryParse(PrimaryControl.Text.Replace("$",""), out value))
                Host.SetModelValue(ControlMetadata, value);
            else
                Host.SetModelValue(ControlMetadata, null);
        }
    }
}