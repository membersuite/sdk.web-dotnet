using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MemberSuite.SDK.Manifests.Command;
using MemberSuite.SDK.Utilities;
using MemberSuite.SDK.Web.Controls;
using MemberSuite.SDK.Web.ControlManagers;
 

namespace MemberSuite.SDK.Web.ControlManagers
{
    /// <summary>
    /// The purpose of this class is to encapsulate the 80% case of a control manager
    /// that manages a single control
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingleControlManager<T> : ControlManager where T : WebControl, new()
    {
        /// <summary>
        /// Gets or sets the primary control.
        /// </summary>
        /// <value>The primary control.</value>
        public T PrimaryControl { get; protected set; }
        public RequiredFieldValidator RequiredFieldValidator { get; protected set; }
        public virtual bool CanBeValidated { get { return true; } }

        /// <summary>
        /// Instantiates this instance.
        /// </summary>
        /// <returns></returns>
        public override List<Control> Instantiate()
        {
            var controls = InstantiateWithPostprocessing(renderCustomLabel);

            return controls;
        }

        /// <summary>
        /// Instantiates this instance. Uses <param name="controlAction"></param> for postprocessing.
        /// </summary>
        /// <returns></returns>
        public override List<Control> InstantiateWithPostprocessing(Action<List<Control>> controlAction)
        {
            T primaryControl = instantiatePrimaryControl();
            var controls = new List<Control> {primaryControl};

            if (ControlMetadata != null && ControlMetadata.Properties != null)
            {
                // apply property expressions
                foreach (ControlMetadata.ControlProperty property in ControlMetadata.Properties)
                {
                    // let's try to apply it
                    try
                    {
                        SpringExpression.SetValue(PrimaryControl, property.Name,
                                                     property.Expression);
                    }
                    catch (Exception  )
                    {
                    }
                }
            }

            if (controlAction != null)
            {
                controlAction(controls);
            }

            return controls;
        }

        protected virtual void renderCustomLabel(List<Control> controls)
        {
            if (CustomLabel) // we have to render a label
                controls.Insert(0, new LiteralControl(GetLabel() + "<p/>"));
        }

        protected virtual T instantiatePrimaryControl()
        {
            PrimaryControl = new T();

            IHostedControl hc = PrimaryControl as IHostedControl;
            if (hc != null)
                hc.Host = Host; // set the host

            if (ControlMetadata != null)
            {
                PrimaryControl.ID = ControlMetadata.ID;
                PrimaryControl.Enabled = ControlMetadata.Enabled;
            }

            if (PrimaryControl.ID == null)    // we need an ID
                PrimaryControl.ID = generateIdentifierFor(PrimaryControl);

            setupTabIndex();

            return PrimaryControl;
        }

        protected virtual void setupTabIndex()
        {
            if (TabIndex != null)
                PrimaryControl.TabIndex = TabIndex.Value;
        }


        private static int _idSeed
        {
            get
            {
                object o = HttpContext.Current.Items["ControlIDSeed"];
                if (o == null) return 1;
                return (int)o;
            }
            set { HttpContext.Current.Items["ControlIDSeed"] = value; }
        }

            private string generateIdentifierFor(WebControl wc )
        {
            return wc.GetType().Name + _idSeed++;
        }

        /// <summary>
        /// Instantiates the validation controls.
        /// </summary>
        /// <returns></returns>
        public override List<Control> InstantiateValidationControls()
        {
            List<Control> controls = base.InstantiateValidationControls();

            if (!_isBeingUsed && CanBeValidated )
            {
                RequiredFieldValidator = instantiateRequiredFieldValiator(controls, PrimaryControl.ID);

                if (RequiredFieldValidator != null)
                    controls.Add(RequiredFieldValidator);
            }

            return controls;
        }

        protected bool _isBeingUsed;
        public override bool Use(Control c)
        {
            PrimaryControl = c as T;
            _isBeingUsed = true;
            return PrimaryControl != null;
        }
    }
}