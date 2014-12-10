using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;

namespace MemberSuite.SDK.Web.Controls.CascadingDropDown
{
    /// <summary>
    /// Represents a pair of drop down lists that should be connected
    /// </summary>
    public class CascadingPair
    {
        private List<ParentDropDownValue> _parentDropDownValues;

        /// <summary>
        /// Gets or sets the ID of the parent control.
        /// </summary>
        /// <value>The parent.</value>
        [
            Bindable(true),
            Category("Behavior"),
        ]
        public string ParentDropDownID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent control.
        /// </summary>
        /// <value>The parent.</value>
        [
            Bindable(true),
            Category("Behavior"),
        ]
        public string ChildDropDownID { get; set; }


        /// <summary>
        /// Gets or sets the by default.
        /// </summary>
        /// <value>The by default.</value>
        [
            Bindable(true),
            Category("Behavior"),
        ]
        public CascadingPairBehavior ByDefault { get; set; }

        /// <summary>
        /// Gets or sets the parent drop down values.
        /// </summary>
        /// <value>The parent drop down values.</value>
        [
            Bindable(true),
            Category("Behavior"),
            DefaultValue(""),
            DesignerSerializationVisibility(
                DesignerSerializationVisibility.Content),
            PersistenceMode(PersistenceMode.InnerProperty),
        ]
        public List<ParentDropDownValue> ParentDropDownValues
        {
            get
            {
                if (_parentDropDownValues == null)
                    _parentDropDownValues = new List<ParentDropDownValue>();
                return _parentDropDownValues;
            }
        }

        [Bindable(true), Category("Behavior"), DefaultValue(NoValueInChildBehavior.DisableChildDropDown)]
        public NoValueInChildBehavior WhenNoChildValues { get; set; }

        [Bindable(true), Category("Behavior"),]
        public string ElementToHide { get; set; }

        [Bindable(true), Category("Behavior"), DefaultValue("No selection is available.")]
        public string ChildDisabledText { get; set; }
    }
}