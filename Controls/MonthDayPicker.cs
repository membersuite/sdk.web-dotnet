using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ValidationProperty("Date")]
    public class MonthDayPicker : CompositeControl
    {
        #region Composite Controls

        public DropDownList MonthDropDownList { get; set; }
        public TextBox DayTextBox { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime? Date
        {
            get
            {
                if (!ChildControlsCreated)
                    return null;
                string day = DayTextBox.Text;
                string month = MonthDropDownList.SelectedValue;

                DateTime dt;
                if (DateTime.TryParse(string.Format("{0}/{1}/1900", month, day), out dt))
                    return dt;
                return null;
            }
            set
            {
                EnsureChildControls(); // make sure controls are created

                DateTime dt = DateTime.Now;
                if (value.HasValue)
                    dt = value.Value;

                MonthDropDownList.SelectedValue = dt.Month.ToString();
                DayTextBox.Text = dt.Day.ToString();
            }
        }


        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // let's make sure there are no child controls
            Controls.Clear();
            MonthDropDownList = new DropDownList {ID = "MonthDropDownList"};

            // ok - we're going to add months
            // english
            MonthDropDownList.Width = Unit.Pixel(95);
            MonthDropDownList.Items.Add(new ListItem("January", "1"));
            MonthDropDownList.Items.Add(new ListItem("February", "2"));
            MonthDropDownList.Items.Add(new ListItem("March", "3"));
            MonthDropDownList.Items.Add(new ListItem("April", "4"));
            MonthDropDownList.Items.Add(new ListItem("May", "5"));
            MonthDropDownList.Items.Add(new ListItem("June", "6"));
            MonthDropDownList.Items.Add(new ListItem("July", "7"));
            MonthDropDownList.Items.Add(new ListItem("August", "8"));
            MonthDropDownList.Items.Add(new ListItem("September", "9"));
            MonthDropDownList.Items.Add(new ListItem("October ", "10"));
            MonthDropDownList.Items.Add(new ListItem("November", "11"));
            MonthDropDownList.Items.Add(new ListItem("December", "12"));
            MonthDropDownList.SelectedValue = DateTime.Now.Month.ToString();

            // now, let's instantiate the texbox
            DayTextBox = new TextBox {MaxLength = 2, ID = "DayTextBox", Width = Unit.Pixel(25), CssClass = "inputText"};
            // set the width

            // and add the controls
            Controls.Add(MonthDropDownList);
            Controls.Add(DayTextBox);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureChildControls();

            // add the dang validators
            // and finally, validators
            var rv = new RangeValidator();
            rv.ControlToValidate = DayTextBox.ID;
            rv.Display = ValidatorDisplay.Dynamic;
            rv.Text = "Invalid date detected."; // English
            rv.Type = ValidationDataType.Integer;
            rv.MinimumValue = "0";
            rv.MaximumValue = "31";

            Controls.Add(rv);
        }
    }
}