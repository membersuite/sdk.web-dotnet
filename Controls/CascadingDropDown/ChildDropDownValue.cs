using System.ComponentModel;

namespace MemberSuite.SDK.Web.Controls.CascadingDropDown
{
    /// <summary>
    /// 
    /// </summary>
    public class ChildDropDownValue
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [
            Bindable(true),
            Category("Behavior"),
        ]
        public string Value { get; set; }
    }
}