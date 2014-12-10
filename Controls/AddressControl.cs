using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using MemberSuite.SDK.Types;
using MemberSuite.SDK.Utilities;
using Telerik.Web.UI;

namespace MemberSuite.SDK.Web.Controls
{
    public class AddressControl : CompositeControl, IHostedControl
    {
        const int COMBOXBOX_DROPDOWN_HEIGHT = 200;
        public const string ADDRESSCONTROL_COMBOBOX = "ADDRESSCONTROL_COMBOBOX";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (HttpContext.Current == null)
                return;

            if (HttpContext.Current.Items[ADDRESSCONTROL_COMBOBOX] != null)
                RenderStateAsComboBox =
                    RenderCountryAsComboBox = (bool) HttpContext.Current.Items[ADDRESSCONTROL_COMBOBOX];
        }

        #region Static Methods

        public static List<NameValuePair> Countries;
        public static List<NameValuePair> States;

        static AddressControl()
        {
            Initialize();
        }

        private static void Initialize()
        {
            string resourcePath = typeof(AddressControl).Namespace + ".StatesAndCountries.xml";
            XDocument statesAndCountries = EmbeddedResource.LoadAsXmlLinq(resourcePath);

            States = new List<NameValuePair>();
            Countries = new List<NameValuePair>();

            if (statesAndCountries == null)
            {
                throw new ApplicationException("Unable to find states and countries resource");
            }

            var states = from s in statesAndCountries.Descendants("State")
                         select new { Name = s.Attribute("Name").Value, Code = s.Attribute("Code").Value };

            foreach (var state in states)
                States.Add(new NameValuePair(state.Name, state.Code));


            var countries = from s in statesAndCountries.Descendants("Country")
                            let hasZip = s.Attribute("HasZip")
                            let hasStates = s.Attribute("HasStates")
                            select new 
                            { 
                                Name = s.Attribute("Name").Value, 
                                Code = s.Attribute("Code").Value, 
                                // By default all countries have Zip codes, but don't have states
                                HasStates = hasStates != null && hasStates.Value == "true", 
                                HasZip = hasZip == null || hasZip.Value == "true" 
                            };
            
            var csb = new StringBuilder();
            
            var cList = new List<string>();
            foreach (var country in countries)
            {
                Countries.Add(new NameValuePair(country.Name, country.Code));

                // It is usual for country to have zip but no states. So we don't include 
                // this country in javascript array.
                if (!country.HasStates && country.HasZip) 
                    continue;

                var cn = string.Format("{{'Code':'{0}', 'Name':'{1}', 'HasZip':{2}, 'HasStates':{3}}}", 
                    country.Code.ToUpper(), country.Name.ToUpper(),  
                    country.HasZip.ToString().ToLower(), country.HasStates.ToString().ToLower());
                cList.Add(cn);
            }
            csb.Append(string.Join(",", cList));

            _countriesArray =  string.Format(_countriesArray, csb);
        }

        #endregion

        private bool isValidating;
        public TextBox Company { get; set; }
        public TextBox Line1 { get; set; }
        public TextBox Line2 { get; set; }
        public TextBox PostalCode { get; set; }
        public TextBox City { get; set; }
        public TextBox State { get; set; }
        public TextBox Country { get; set; }
        public RadComboBox StateComboBox { get; set; }
        public RadComboBox CountryComboBox { get; set; }
        public HiddenField County { get; set; }
        public HiddenField CongressionalDistrict { get; set; }
        public HiddenField CASSCertificationDate { get; set; }
        public HiddenField CarrierRoute { get; set; }
        public HiddenField DeliveryPointCode { get; set; }
        public HiddenField DeliveryPointCheckDigit { get; set; }
        public HiddenField GeocodeLat { get; set; }
        public HiddenField GeocodeLong { get; set; }
        public HiddenField LastGeocodeDate { get; set; }
        public Label ValidationStatus { get; set; }
        public LinkButton ValidationButton { get; set; }
        public Label LoadingLabel { get; set; }

        public string AddressName
        {
            get { return (string)ViewState["Name"]; }
            set { ViewState["Name"] = value; }
        }
     
        public bool RenderStateAsComboBox;
        
        public bool RenderCountryAsComboBox;

        private WebControl GetStateControl()
        {
            return RenderStateAsComboBox ? (WebControl) StateComboBox : State; 
        }

        private WebControl GetCountryControl()
        {
            return RenderCountryAsComboBox ? (WebControl)CountryComboBox : Country;
        }

        public Address Address
        {
            get
            {
                var a = new Address();
                a.Company = StringUtil.IsNullOrEmpty(Company.Text) ? null : Company.Text;
                a.Line1 = StringUtil.IsNullOrEmpty(Line1.Text) ? null : Line1.Text;
                a.Line2 = StringUtil.IsNullOrEmpty(Line2.Text) ? null : Line2.Text;
                a.PostalCode = StringUtil.IsNullOrEmpty(PostalCode.Text) ? null : PostalCode.Text;
                a.City = StringUtil.IsNullOrEmpty(City.Text) ? null : City.Text;

                if (RenderStateAsComboBox)
                    a.State = _getComboValue( StateComboBox );
                else 
                    a.State = State.Text;

                if (RenderCountryAsComboBox)
                    a.Country = _getComboValue( CountryComboBox ) ;
                else 
                    a.Country = Country.Text;


                //if ( String.IsNullOrWhiteSpace( a.Country ) )
                //    switch (Currency.Current)
                //    {
                //        case "CAD":
                //            a.Country = "CA";
                //            break;

                //        default:
                //            a.Country = "US";
                //            break;
                //    }

                // hidden fields
                a.County = StringUtil.IsNullOrEmpty(County.Value) ? null : County.Value;
                a.CongressionalDistrict = StringUtil.IsNullOrEmpty(CongressionalDistrict.Value)
                                              ? null
                                              : CongressionalDistrict.Value;
                DateTime dt;
                if (DateTime.TryParse(CASSCertificationDate.Value, out dt))
                    a.CASSCertificationDate = dt;
                else
                    a.CASSCertificationDate = null;
                a.CarrierRoute = StringUtil.IsNullOrEmpty(CarrierRoute.Value) ? null : CarrierRoute.Value;
                a.DeliveryPointCode = StringUtil.IsNullOrEmpty(DeliveryPointCode.Value) ? null : DeliveryPointCode.Value;
                a.DeliveryPointCheckDigit = StringUtil.IsNullOrEmpty(DeliveryPointCheckDigit.Value)
                                                ? null
                                                : DeliveryPointCheckDigit.Value;
                //MS-3287
                Decimal dLat,dLong;
                if (Decimal.TryParse(GeocodeLat.Value, out dLat))
                    a.GeocodeLat = dLat;
                else
                    a.GeocodeLat = null;
                if (Decimal.TryParse(GeocodeLat.Value, out dLong))
                    a.GeocodeLong = dLong;
                else
                    a.GeocodeLong = null;
                DateTime dt2;
                if (DateTime.TryParse(LastGeocodeDate.Value, out dt2))
                    a.LastGeocodeDate = dt;
                else
                    a.LastGeocodeDate = null;


                return a;
            }
            set
            {
                EnsureChildControls();
                Company.Text = value != null ? value.Company : null;
                Line1.Text = value != null ? value.Line1 : null;
                Line2.Text = value != null ? value.Line2 : null;
                City.Text = value != null ? value.City : null;

                var stateValue = value != null ? value.State : null;
                if (RenderStateAsComboBox)
                    _setComboValue( StateComboBox, stateValue, null  );
                else 
                    State.Text = stateValue;

                var countryValue = value != null ? value.Country : null;
                if (RenderCountryAsComboBox)
                    _setComboValue(CountryComboBox, countryValue, "US");
                else
                    Country.Text = countryValue;
                 

                PostalCode.Text = value != null ? value.PostalCode : null;

                // hidden fields
                County.Value = value != null ? value.County : null;
                CongressionalDistrict.Value = value != null ? value.CongressionalDistrict : null;
                CASSCertificationDate.Value = value != null ? value.CASSCertificationDate.ToString() : null;
                CarrierRoute.Value = value != null ? value.CarrierRoute : null;
                DeliveryPointCheckDigit.Value = value != null ? value.DeliveryPointCheckDigit : null;
                DeliveryPointCode.Value = value != null ? value.DeliveryPointCode : null;
                GeocodeLat.Value = value != null ? value.GeocodeLat.ToString() : null;
                GeocodeLong.Value = value != null ? value.GeocodeLong.ToString() : null;
                LastGeocodeDate.Value = value != null ? value.LastGeocodeDate.ToString() : null;
            }
        }

        public bool AutomaticallyValidate
        {
            get
            {
                if (!EnableValidation)
                    return false;

                object o = ViewState["AutomaticallyValidate"];
                if (o != null)
                    return (bool)o;
                return true;
            }
            set { ViewState["AutomaticallyValidate"] = value; }
        }

        public bool EnableValidation
        {
            get
            {
                object o = ViewState["EnableValidation"];
                if (o != null)
                    return (bool)o;
                return true;
            }
            set { ViewState["EnableValidation"] = value; }
        }

        private void _setComboValue(RadComboBox cb, string valueToSet, string defaultVal)
        {
            RadComboBoxItem item = cb.FindItemByValue(valueToSet);

            if (valueToSet == null || StringUtil.IsNullOrEmpty(valueToSet))
                valueToSet = defaultVal;

            if (item != null)
                item.Selected = true;
            else
                cb.Text = valueToSet;
        }

        private string _getComboValue(RadComboBox cb)
        {
            string retVal = null;

            if (cb.SelectedItem != null)
                retVal = cb.SelectedItem.Value;
            else
                retVal = cb.Text;

            if (StringUtil.IsNullOrEmpty(retVal))
                return null;
            return retVal;
        }

        public bool ShowCompany { get; set; }

        private static string _countriesArray = "var countriesWithStateAndZip = [{0}];";

        private void RegisterStartupScript()
        {
            var location = string.Format("{0}.ValidateCountry.js", GetType().Namespace);

            Page.ClientScript.RegisterArrayDeclaration("addresssControlArray", string.Format("{{'CountryId':'{0}', 'StateId':'{1}', 'ZipId':'{2}', 'IsRequired':'{3}'}}", 
                RenderCountryAsComboBox ? CountryComboBox.ClientID : Country.ClientID,
                RenderStateAsComboBox ? StateComboBox.ClientID : State.ClientID,
                PostalCode.ClientID,
                IsRequired.ToString().ToLower()));

            if (Page.ClientScript.IsClientScriptBlockRegistered(GetType(), "LookupCountry"))
                return;

            var script = string.Concat(_countriesArray,
                                       Environment.NewLine,
                                       EmbeddedResource.LoadAsString(location, Assembly.GetAssembly(GetType())));
            
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "LookupCountry", script, true);
        }

        protected override void CreateChildControls()
        {            
            if (TabIndex > 0)
            {
                hasTabIndex = true;
                _privateTabIndex = TabIndex;
                TabIndex = default(short);
            }
            base.CreateChildControls();

            Company = new TextBox { ID = "Company", CssClass = "inputText" };
            Line1 = new TextBox { ID = "Line1", CssClass = "inputText" };
            Line2 = new TextBox { ID = "Line2", CssClass = "inputText" };
            City = new TextBox { ID = "City", CssClass = "inputText" };

            if (RenderStateAsComboBox)
            {                
                StateComboBox = new RadComboBox
                                    {
                                        ID = "State",                                        
                                        AllowCustomText = false,
                                        MarkFirstMatch = true,
                                        ShowDropDownOnTextboxClick = false,
                                        AppendDataBoundItems = true,
                                        MaxHeight = Unit.Pixel ( COMBOXBOX_DROPDOWN_HEIGHT )
                                    };
            }
            else
                State = new TextBox
                {                    
                    ID = "State",
                    CssClass = "inputText", 
                    Width = Unit.Pixel(50)
                };

            if (RenderCountryAsComboBox)
            {
                CountryComboBox = new RadComboBox
                {
                    ID = "Country",
                    AllowCustomText = false,
                    MarkFirstMatch = true,
                    ShowDropDownOnTextboxClick = false,
                    AppendDataBoundItems = true,
                    MaxHeight = Unit.Pixel(COMBOXBOX_DROPDOWN_HEIGHT)
                };                
            }
            else
            {
                Country = new TextBox
                {
                    ID = "Country",
                    CssClass = "inputText", 
                    Width = Unit.Pixel(50)
                };
            }

            PostalCode = new TextBox
            {
                ID = "PostalCode",
                CssClass = "inputText"
            };
            ValidationButton = new LinkButton { ID = "ValidateButton", CausesValidation = false, Text = "Validate Address", CssClass = "addressValidate" };
            LoadingLabel = new Label { ID = "LoadingLabel" };
            LoadingLabel.Text =
                "<font color=blue>Please wait... validating the address... <img src='/images/loading1.gif'/></font>";
            LoadingLabel.CssClass = "addressValidate";
            LoadingLabel.Attributes["style"] = "display: none;";

            var pnl = new RadAjaxPanel();

            Controls.Add(pnl);
            // RadAjaxLoadingPanel pnlLoading = new RadAjaxLoadingPanel() { ID = "LoadingPanel" };
            // pnlLoading.IsSticky = false;
            //// Controls.Add(pnlLoading);
            //// pnl.LoadingPanelID = pnlLoading.ID;
            // pnlLoading.Transparency = 10;
            // pnlLoading.Controls.Add(new Image() { ImageUrl = "/images/loading1.gif" });

            var t = new Table();
            t.CssClass = "killPadding";
            t.Attributes["style"] = "padding: 0px 0px 0px 0px !important; width: 310px !important";

            pnl.Controls.Add(t);

            // line1
            if ( ShowCompany ) 
                CreateRowAndAdd(t, "Company", Company, false);
            CreateRowAndAdd(t, "Line 1", Line1, IsRequired);
            CreateRowAndAdd(t, "Line 2", Line2, false);           

            if (EnableValidation) // put the zip code first
            {
                CreateRowAndAdd(t, "Postal Code", PostalCode, IsRequired);
                CreateRowAndAdd(t, "City", City, IsRequired);
                CreateRowAndAdd(t, "State/Province",  GetStateControl(), IsRequired);
            }
            else
            {
                CreateRowAndAdd(t, "City", City, IsRequired);
                CreateRowAndAdd(t, "State/Province", GetStateControl(), IsRequired);
                CreateRowAndAdd(t, "Postal Code", PostalCode, IsRequired);
            }
            var countryControl = GetCountryControl();
            CreateRowAndAdd(t, "Country", countryControl, IsRequired);
            if (IsRequired)
            {
                RegisterStartupScript();
            }

            // let's add states and countries
            // I'm purposely not doing a databind here, b/c I don't want the page.databind to interfere
            if (StateComboBox != null)
            {
                // MS-5596
                //StateComboBox.Items.Add(new RadComboBoxItem("--- Select a State ---", ""));
                foreach (NameValuePair state in States)
                    StateComboBox.Items.Add(new RadComboBoxItem(state.Name, (string) state.Value));               
            }

            if (CountryComboBox != null)
            {
                foreach (NameValuePair country in Countries)
                    CountryComboBox.Items.Add(new RadComboBoxItem(country.Name, (string) country.Value));
                CountryComboBox.SelectedValue = "US"; // default - stars and stripes!
            }

            // add the hidden fields
            County = new HiddenField { ID = "County" };
            CongressionalDistrict = new HiddenField { ID = "CongressionalDistrict" };
            CASSCertificationDate = new HiddenField { ID = "CASSCertificationDate" };
            CarrierRoute = new HiddenField { ID = "CarrierRoute" };
            DeliveryPointCheckDigit = new HiddenField { ID = "DeliveryPointCheckDigit" };
            DeliveryPointCode = new HiddenField { ID = "DeliveryPointCode" };
            GeocodeLat = new HiddenField { ID = "GeocodeLat" };
            GeocodeLong = new HiddenField { ID = "GeocodeLong" };
            LastGeocodeDate = new HiddenField { ID = "LastGeocodeDate" };

            ValidationStatus = new Label { ID = "ValidationStatus" };
            ValidationStatus.Attributes["style"] = "display:none";

            ValidationStatus.CssClass = "addressValidate rightAlign";
            ValidationButton.CssClass = "addressValidate rightAlign";
            LoadingLabel.CssClass = "addressValidate rightAlign";

            pnl.Controls.Add(County);
            pnl.Controls.Add(CongressionalDistrict);
            pnl.Controls.Add(CASSCertificationDate);
            pnl.Controls.Add(CarrierRoute);
            pnl.Controls.Add(DeliveryPointCheckDigit);
            pnl.Controls.Add(DeliveryPointCode);

            var tr = new TableRow();
            var tc = new TableCell();
            tc.ColumnSpan = 2;
            tc.Attributes["style"] = "text-align: center";

            tc.Controls.Add(ValidationStatus);
            tc.Controls.Add(ValidationButton);
            tc.Controls.Add(LoadingLabel);

            tr.Cells.Add(tc);
            t.Rows.Add(tr);

            if (AutomaticallyValidate)
            {
                PostalCode.AutoPostBack = true;
                PostalCode.TextChanged += AddressComponentChanged;
            }
            ValidationButton.Click += AddressComponentChanged;

            ValidationButton.Visible = EnableValidation;

            string jsHideStatus =
                String.Format("document.getElementById('{0}').style.display = 'none'; document.getElementById('{1}').style.display ='';",
                              ValidationStatus.ClientID, ValidationButton.ClientID);

            if (EnableValidation)
            {
                Company.Attributes["onchange"] = jsHideStatus;
                Line1.Attributes["onchange"] = jsHideStatus;
                Line2.Attributes["onchange"] = jsHideStatus;
                City.Attributes["onchange"] = jsHideStatus;
                if ( State != null ) State.Attributes["onchange"] = jsHideStatus;
                //if (StateComboBox != null) StateComboBox.OnClientSelectedIndexChanged = jsHideStatus ;
                PostalCode.Attributes["onchange"] = jsHideStatus;
                if (Country != null)  Country.Attributes["onchange"] = jsHideStatus;
                //if (CountryComboBox != null) CountryComboBox.OnClientSelectedIndexChanged = jsHideStatus ;

                // build the disable script
                var sbDisableControls = new StringBuilder();
                //setDisabledScript(sbDisableControls, Company, true);
                setDisabledScript(sbDisableControls, Line1, true);
                setDisabledScript(sbDisableControls, Line2, true);
                setDisabledScript(sbDisableControls, City, true);
                if ( State != null ) setDisabledScript(sbDisableControls, State, true);
                setDisabledScript(sbDisableControls, PostalCode, true);
                if (Country != null) setDisabledScript(sbDisableControls, Country, true);
                if (StateComboBox != null ) setDisabledScript(sbDisableControls, StateComboBox, true);
                if (CountryComboBox != null) setDisabledScript(sbDisableControls, CountryComboBox, true);


                sbDisableControls.AppendFormat("document.getElementById('{0}').style.display='';", LoadingLabel.ClientID);
                sbDisableControls.AppendFormat("document.getElementById('{0}').style.display='none';",
                                               ValidationButton.ClientID);
                pnl.ClientEvents.OnRequestStart = sbDisableControls.ToString();

                // now, build the enabled script
                var sbEnableControls = new StringBuilder();
                //setDisabledScript(sbEnableControls, Company, false);
                setDisabledScript(sbEnableControls, Line1, false);
                setDisabledScript(sbEnableControls, Line2, false);
                setDisabledScript(sbEnableControls, City, false);
                if ( State != null ) setDisabledScript(sbEnableControls, State, false);
                if (Country != null) setDisabledScript(sbEnableControls, Country, false);
                if (StateComboBox != null) setDisabledScript(sbEnableControls, StateComboBox, false);
                if (CountryComboBox != null) setDisabledScript(sbEnableControls, CountryComboBox, false);
                setDisabledScript(sbEnableControls, PostalCode, false);
                sbEnableControls.AppendFormat("document.getElementById('{0}').style.display='none';", LoadingLabel.ClientID);
                pnl.ClientEvents.OnResponseEnd = sbEnableControls.ToString();
            }
        }

        private void setDisabledScript(StringBuilder sbClient, WebControl c, bool disabled)
        {
            sbClient.AppendFormat("document.getElementById('{0}').disabled = {1}; ", c.ClientID,
                                  disabled ? "true" : "false");
        }

        private void AddressComponentChanged(object sender, EventArgs e)
        {
            if (isValidating)
                return;

            isValidating = true;

            if (Host == null)
                return;

            var a = Address;
            if (a == null || String.IsNullOrWhiteSpace(a.PostalCode) || !Regex.IsMatch(a.PostalCode, RegularExpressions.PostalCodeRegex, RegexOptions.Compiled))  // bad postal code, forget it
                return;
            try
            {
                using (var api = Host.GetServiceAPIProxy())
                {
                    if (!String.IsNullOrWhiteSpace(a.PostalCode) && String.IsNullOrWhiteSpace(a.Line1) && String.IsNullOrWhiteSpace(a.City) && String.IsNullOrWhiteSpace(a.State))
                    {
                        var apiResult = api.PopulateCityStateFromPostalCode(a);
                        var result = apiResult.ResultValue;
                        Address = result;
                    }

                    else
                    {
                        var apiResult = api.ValidateAddress(a);
                        var result = apiResult.ResultValue;
                        if (result != null)
                        {
                            Address = result; // set the address 
                            notifyUserValidationSucceeded();
                        }
                        else
                            notifyUserValidationFailed(apiResult.AddressValidationErrorMessage);
                    }
                }
            }
            catch
            {
                notifyUserValidationFailed("Unknown error");
            }

            isValidating = false;
            if (sender == PostalCode) // postal code sent us
                Page.SetFocus(City);
        }

        private void notifyUserValidationFailed(string errMsg)
        {
            ValidationStatus.Attributes["style"] = "";
            ValidationButton.Attributes["style"] = "display: none";

            var sbError =
                new StringBuilder(
                    "<font color=red><img src='/images/flag_red.gif'/>Address validation failed. ");
            if (!String.IsNullOrEmpty(errMsg))
                sbError.AppendFormat("Error: {0}", errMsg);
            else
                sbError.Append("Please check the address.");

            sbError.Append("</font>");
            ValidationStatus.Text = sbError.ToString();
        }

        private void notifyUserValidationSucceeded()
        {
            ValidationStatus.Attributes["style"] = "";
            ValidationButton.Attributes["style"] = "display: none";
            ValidationStatus.Text =
                "<font color=green><span class='plainButtonSpan iconBtnCheck_hover'>Address validated successfully.</span></font>";
        }

        short _privateTabIndex = 0 ;
        bool hasTabIndex = false;
        private void CreateRowAndAdd(Table t, string text, WebControl control, bool shouldBeRequired)
        {
            var tr = new TableRow();
            t.Controls.Add(tr);

            // add the left, text column
            var tcLabel = new TableCell();
            if (text != null)
                tcLabel.Text = String.Format("{0} {1}:", AddressName, text);

            if (hasTabIndex)
                control.TabIndex = _privateTabIndex++;

            tcLabel.Attributes["style"] = "padding: 0px; ";
            tr.Cells.Add(tcLabel);

            // add the right column
            var tc = new TableCell();
            tc.Attributes["style"] = "padding-left: 0px !important; padding-right: 0px !important";
            tc.Controls.Add(control);
            tr.Cells.Add(tc);

            if (shouldBeRequired)
            {
                var rfv = new RequiredFieldValidator();
                rfv.ID = "validator_for_" + control.ClientID;
                rfv.ClientIDMode = ClientIDMode.Static;
                rfv.ControlToValidate = control.ID;
                rfv.ErrorMessage = "You have not entered a value for " + tcLabel.Text;
                tcLabel.Text += " <span class=\"requiredField\">*</span>";
                rfv.Display = ValidatorDisplay.None;
                tc.Controls.Add(rfv);                
            }
        }

        #region IHostedControl Members

        public IControlHost Host { get; set; }

        public bool IsRequired { get; set; }

        #endregion
    }
}