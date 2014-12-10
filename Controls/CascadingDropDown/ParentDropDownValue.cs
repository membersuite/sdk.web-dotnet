using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;

namespace MemberSuite.SDK.Web.Controls.CascadingDropDown
{
    /// <summary>
    /// 
    /// </summary>
    public class ParentDropDownValue
    {
        private List<ChildDropDownValue> _childDropDownValues;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [
            Bindable(true),
            Category("Behavior"),
        ]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the by default behavior.
        /// </summary>
        /// <value>The by default.</value>
        [
            Bindable(true),
            Category("Behavior"),
        ]
        public ParentValueBehavior ByDefault { get; set; }

        /// <summary>
        /// Gets or sets the child drop down values.
        /// </summary>
        /// <value>The child drop down values.</value>
        [
            Bindable(true),
            Category("Behavior"),
            DefaultValue(""),
            DesignerSerializationVisibility(
                DesignerSerializationVisibility.Content),
            PersistenceMode(PersistenceMode.InnerProperty),
        ]
        public List<ChildDropDownValue> ChildDropDownValues
        {
            get
            {
                if (_childDropDownValues == null)
                    _childDropDownValues = new List<ChildDropDownValue>();

                return _childDropDownValues;
            }
        }

        [Bindable(true), Category("Behavior"),]
        public string ClientOnSelect { get; set; }
    }
}