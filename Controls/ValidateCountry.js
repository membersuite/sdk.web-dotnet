function pageLoad() {
    for (var i = 0; i < addresssControlArray.length; i++) {
        var countryId = addresssControlArray[i].CountryId;
        var stateId = addresssControlArray[i].StateId;
        var zipId = addresssControlArray[i].ZipId;
        var isRequired = addresssControlArray[i].IsRequired;
        var cboCountry = $find(countryId);
        var closure;
        var countryVal;
        if (cboCountry) {
            // combobox
            // close around stateId and zipId values
            closure = (function(s, z) {
                            return function (sender, eventArgs) {
                                        var item = eventArgs.get_item();
                                        var country = item.get_text().toUpperCase();
                                        enableDisableControls(country, s, z);
                                    }
            })(stateId, zipId);

            cboCountry.add_selectedIndexChanged(closure);
            cboCountry.add_textChange(closure);
            countryVal = cboCountry.get_selectedItem().get_value();
        } else {
            // textbox
            // close around stateId and zipId values
            closure = (function(s, z) {
                            return function() {
                                var country = $(this).val();
                                enableDisableControls(country, s, z);
                            }
            })(stateId, zipId);

            countryVal = $('#' + countryId).change(closure).val();
        }

        if (isRequired) {
            enableDisableControls(countryVal, stateId, zipId);
        }        
    }

    function enableDisableControls(country, stateId, zipId) {        
        if (country && country.length > 0) {
            var countryInfo = lookupCountry(country.toUpperCase());
            if (countryInfo) {
                enableStateAndZipControls(stateId, zipId, countryInfo.HasStates, countryInfo.HasZip);
            } else {
                enableStateAndZipControls(stateId, zipId, false, true);
            }
        } else {
            enableStateAndZipControls(stateId, zipId, true, true);
        }
    }

    function lookupCountry(country) {
        for (i = 0; i < countriesWithStateAndZip.length; i++) {
            if (countriesWithStateAndZip[i].Code == country || countriesWithStateAndZip[i].Name == country) {
                return countriesWithStateAndZip[i];
            }
        }
        return null;
    }

    function enableStateAndZipControls(stateId, zipId, hasStates, hasZip) {
        var cboStates = $find(stateId);
        if (cboStates) {
            comboBoxEnableDisable(cboStates, hasStates);
        } else {
            textBoxEnableDisable($('#' + stateId), hasStates);
        }
        var validator = $('#validator_for_' + stateId);
        if (validator && validator.length > 0) {
            ValidatorEnable(validator[0], hasStates);
        }
        
        textBoxEnableDisable($('#' + zipId), hasZip);
        validator = $('#validator_for_' + zipId);
        if (validator && validator.length > 0) {
            ValidatorEnable(validator[0], hasZip);
        }

        // very slow. need to find faster way.
        $('#' + stateId).closest('tr').find('span.requiredField').css('opacity', hasStates ? '1' : '0');
        $('#' + zipId).closest('tr').find('span.requiredField').css('opacity', hasZip ? '1' : '0');
    }

    function comboBoxEnableDisable(comboBox, enabled) {
        if (enabled) {
            comboBox.enable();
        } else {
            comboBox.clearSelection();
            comboBox.disable();
        }
    }

    function textBoxEnableDisable(textBox, enabled) {
        var disabled = enabled == false;
        textBox.prop('disabled', disabled);
        if (disabled) {
            textBox.val('');
        }
    }
}