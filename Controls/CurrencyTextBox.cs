using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Types;

namespace MemberSuite.SDK.Web.Controls
{
    
    //public class CurrencyTextBox : CompositeControl 
    //{
    //    public CurrencyTextBox()
    //    {
    //    }

    //    public TextBox CurrencyBox { get; set; }
    //    public DropDownList CurrencyDropDownList { get; set; }

    //    public bool SuppressCurrency
    //    {
    //        get
    //        {
    //            object val = ViewState["SuppressCurrency"];
    //            if (val != null)
    //                return (bool)val;

    //            return false;
    //        }
    //        set { ViewState["SuppressCurrency"] = value; }
    //    }


    //        public Money? Money
    //    {
    //        get
    //        {
    //            if (CurrencyBox == null)
    //                return null;

    //            decimal amt;

    //            if (decimal.TryParse(CurrencyBox.Text, out amt))
    //                return new Money(amt, CurrencyDropDownList.SelectedValue ); // for now, us dollars only

    //            return null;
    //        }
    //        set
    //        {
    //            EnsureChildControls();
    //            CurrencyBox.Text = value != null ? value.Value.Amount.ToString("F2") : null;
    //        }
    //    }

    //    public bool IsRequired { get; set; }
    //    public string RequiredFieldErrorMessage { get; set; }

    //    //public bool IsRequired
    //    //{
    //    //    get { object o = ViewState["IsRequired"]; return o != null ? (bool)o : false; }
    //    //    set { ViewState["IsRequired"] = value; }
    //    //}

    //    //public bool ValidationErrorMessage
    //    //{
    //    //    get { object o = ViewState["ValidationErrorMessage"]; return o != null ? (bool)o : false; }
    //    //    set { ViewState["ValidationErrorMessage"] = value; }
    //    //}

    //    protected override void CreateChildControls()
    //    {
    //        base.CreateChildControls();

    //        HtmlDividerControl div = new HtmlDividerControl(null,  "width: 150px; padding: 0px !important;" );


    //        CurrencyBox = new TextBox();
    //        CurrencyBox.ID = "CurrencyBox";
    //        CurrencyBox.CssClass = "inputText";
    //        CurrencyBox.Width = Unit.Pixel(50); // set the box

    //        CurrencyDropDownList = new DropDownList();
    //        CurrencyDropDownList.Items.Add(new ListItem("USD", "USD"));
            
    //        CurrencyDropDownList.Width = Unit.Pixel(50);
    //        CurrencyDropDownList.Attributes["style"] = "float: none";

    //        div.Controls.Add(CurrencyBox);

    //        if (!SuppressCurrency)
    //        {
    //            div.Controls.Add(CurrencyDropDownList);

               
    //        }
    //        Controls.Add(div);
            
    //    }

    //    protected override void OnInit(EventArgs e)
    //    {
    //        base.OnInit(e);

    //        EnsureChildControls();

    //        // add the dang validators
    //        // and finally, validators
    //        var rv = new CompareValidator();
    //        rv.ControlToValidate = CurrencyBox.ID;
    //        rv.Display = ValidatorDisplay.None;
    //        rv.Type = ValidationDataType.Double;
    //        rv.Operator = ValidationCompareOperator.DataTypeCheck;
    //        rv.ErrorMessage = "An invalid amount was specified.";

    //        Controls.Add(rv);

    //        if (IsRequired)
    //        {
    //            var rfv = new RequiredFieldValidator();
    //            rfv.ControlToValidate = CurrencyBox.ID;
    //            rfv.Display = ValidatorDisplay.None;
    //            rfv.ErrorMessage = RequiredFieldErrorMessage;
    //            Controls.Add(rfv);
    //        }
    //    }
    //}
}