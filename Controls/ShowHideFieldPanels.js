function showAppropriatePanelsForDisplayType() {
    var ddlData = document.getElementById('%dataTypeDropDownID%');
    var ddlDisplay = document.getElementById('%displayTypeDropDownID%');
    var pnlAcceptableValues = document.getElementById('S_AcceptableValues');
    var pnlReference = document.getElementById('S_Reference');
    var pnlMonthYear = document.getElementById('S_MonthYear');

    var selectedDataType = ddlData.options[ddlData.selectedIndex].value;
    var selectedDisplayType = ddlDisplay.options[ddlDisplay.selectedIndex].value;

    if (pnlAcceptableValues != null)
        pnlAcceptableValues.style.display = 'none';

    if (pnlReference != null)
        pnlReference.style.display = 'none';

    if (pnlMonthYear != null)
        pnlMonthYear.style.display = 'none';


    switch (selectedDataType) {
        //Temporary to treat lookup tables like a reference until 
        //large amounts of acceptable values is redesigned
        case 'Text':
            if (selectedDisplayType == 'AjaxComboBox')
                pnlAcceptableValues.style.display = '';
            break;

        case 'Reference':
            if (pnlReference != null)
                pnlReference.style.display = '';
            break;
    }

   
    switch (selectedDisplayType) {
        case 'CheckBoxes':
        case 'RadioButtons':
        case 'DropDownList':
        case 'ListBox':
        case 'DualListBox':
        case 'ComboBox':
        case 'MultiSelectListBox':
            if (pnlAcceptableValues != null && selectedDataType != 'Reference')
                pnlAcceptableValues.style.display = '';

            break;

        case 'MonthYearPicker':
            pnlMonthYear.style.display = '';
            break;
            
       
    }
}