using System.Web;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Types;
using MemberSuite.SDK.Web.ControlManagers;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public static class ControlManagerResolver
    {
        public static ControlManager Resolve(FieldDisplayType displayType)
        {
            switch (displayType)
            {
                case FieldDisplayType.TextBox:
                case FieldDisplayType.EmailTextBox:
                case FieldDisplayType.UrlTextBox:
                    return new TextBoxControlManager();

                case FieldDisplayType.CurrencyTextBox:
                    return new CurrencyTextBoxControlManager();

                case FieldDisplayType.Rating:
                    return new RatingControlManager();

                case FieldDisplayType.Password:
                    return new TextBoxControlManager(TextBoxMode.Password);

                case FieldDisplayType.CheckBox:
                    return new CheckBoxControlManager();

                case FieldDisplayType.DatePicker:
                    return new DatePickerControlManager();

                case FieldDisplayType.LegacyDropDownList:
                    return new LegacyDropDownListManager();

                case FieldDisplayType.DropDownList:
                    return new DropDownListManager();

                case FieldDisplayType.LargeTextBox:
                    return new TextBoxControlManager(TextBoxMode.MultiLine);

                case FieldDisplayType.Label:
                    return new LabelControlManager();

                case FieldDisplayType.NumericTextBox:
                    return new NumericTextBoxControlManager();

                case FieldDisplayType.HtmlTextBox:
                    return new HtmlTextBoxControlManager();

                case FieldDisplayType.DateTimePicker:
                    return new DateTimePickerControlManager();

                case FieldDisplayType.MonthDayPicker:
                    return new MonthDayPickerControlManager();

                case FieldDisplayType.MonthYearPicker:
                    return new MonthYearPickerControlManager();

                case FieldDisplayType.TimePicker:
                    return new TimePickerControlManager();

                case FieldDisplayType.CheckBoxes:
                    return new CheckBoxListControlManager();

                case FieldDisplayType.RadioButtons:
                    return new RadioButtonListControlManager();

                case FieldDisplayType.ComboBox:
                    return new ComboBoxControlManager();

                 

                case FieldDisplayType.ListBox:
                    return new ListBoxControlManager();

                case FieldDisplayType.MultiSelectListBox:
                    return new MultiSelectListBoxControlManager();

                case FieldDisplayType.DualListBox:
                    return new DualListBoxControlManager();

                case FieldDisplayType.Hyperlink:
                    return new HyperLinkControlManager();

                case FieldDisplayType.Separator:
                    return new SeperatorControlManager();

                case FieldDisplayType.FileUpload:
                case FieldDisplayType.Image:
                    return new FileUploadControlManager();

                case FieldDisplayType.Address:
                    return new AddressControlManager();

                case FieldDisplayType.AjaxComboBox:
                    //object objTypeAhead = HttpContext.Current.Items[TypeaheadControlManager.AJAX_USETYPEAHEAD];
                    //if (objTypeAhead != null)
                    //{
                    //    var usage = (IndexedQuickSearchUsage)objTypeAhead;
                    //    if (usage > IndexedQuickSearchUsage.QuickSearchesOnly)
                    //        return new TypeaheadControlManager();
                    //}
                    return new AjaxComboBoxControlManager();
            }

            return null;
        }
    }
}