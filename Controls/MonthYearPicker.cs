using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.Controls
{
    [ValidationProperty("Date")]
    public class MonthYearPicker : CompositeControl
    {
        #region Composite Controls

        public DropDownList MonthDropDownList { get; set; }
        public DropDownList YearDropDownList { get; set; }

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

                string month = MonthDropDownList.SelectedValue;
                string year = YearDropDownList.SelectedValue;
                DateTime dt;

                if (DateTime.TryParse(string.Format("{0}/1/{1}", month, year), out dt))
                    return dt;
                return null;
            }
            set
            {
                EnsureChildControls();

                DateTime dt = DateTime.Now;

                if (value.HasValue)
                    dt = value.Value;


                MonthDropDownList.SelectedValue = dt.Month.ToString();
                YearDropDownList.SelectedValue = dt.Year.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the start year.
        /// </summary>
        /// <value>The start year.</value>
        public int StartYear
        {
            get
            {
                object startYear = ViewState["StartYear"];
                if (startYear == null)
                    return DateTime.Now.Year ;
                return (int) startYear;
            }
            set { ViewState["StartYear"] = value; }
        }

        /// <summary>
        /// Gets or sets the start year.
        /// </summary>
        /// <value>The End year.</value>
        public int EndYear
        {
            get
            {
                object endYear = ViewState["EndYear"];
                if (endYear == null)
                    return DateTime.Now.Year + 50;
                return (int) endYear;
            }
            set { ViewState["EndYear"] = value; }
        }


        protected override void CreateChildControls()
        {
            base.CreateChildControls();

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
            Controls.Add(MonthDropDownList);

            // now, the years

            YearDropDownList = new DropDownList {Width = Unit.Pixel(70)};

            for (int i = StartYear; i <= EndYear; i++)
                YearDropDownList.Items.Add(new ListItem(i.ToString()));

            string currentYear = DateTime.Now.Year.ToString();

            ListItem li = YearDropDownList.Items.FindByValue(currentYear);
            if (li != null)
            {
                YearDropDownList.ClearSelection();
                li.Selected = true;
            }

            Controls.Add(YearDropDownList);
        }
    }
}