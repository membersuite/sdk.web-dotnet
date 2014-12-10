using System;
using System.Web.UI.WebControls;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class TextBoxControlManager : SingleControlManager<TextBox>
    {
        protected TextBoxMode _textBoxMode;

        public TextBoxControlManager()
        {
        }

        public TextBoxControlManager(TextBoxMode mode)
        {
            _textBoxMode = mode;
        }


        public override bool CustomLabel
        {
            get { return _textBoxMode == TextBoxMode.MultiLine; }
        }


        /// <summary>
        /// Instantiates the primary control.
        /// </summary>
        /// <returns></returns>
        protected override TextBox instantiatePrimaryControl()
        {
            TextBox tb = base.instantiatePrimaryControl();
            tb.TextMode = _textBoxMode;
            if (_textBoxMode == TextBoxMode.MultiLine)
            {
                if (ControlMetadata.UseEntireRow)
                {
                    tb.Rows = 10;
                    tb.Width = Unit.Pixel(650);
                }
                else
                {
                    tb.Rows = 5;
                    tb.Width = Unit.Pixel(350);
                }
            }

            tb.CssClass = "inputText";
            return tb;
        }


        public override void DataBind()
        {
            base.DataBind();
            object obj = Host.Resolve(ControlMetadata);

            try
            {
                PrimaryControl.Text = Convert.ToString(obj);
            }
            catch
            {
            }
        }

        public override void DataUnbind()
        {
            base.DataUnbind();
            Host.SetModelValue(ControlMetadata, PrimaryControl.Text);
        }
    }
}