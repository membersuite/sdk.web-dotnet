 
    function updateCascadingDropDown( parent)
    {
 
      switch( parent.id )
      {
        %parentDropDownSwitchConditions%
      }
      
    }
    function prepareChildDropDowns() {
        isInitializing = true;
        %dropDownsToPrepare%
        isInitializing = false;
    }
    
   
 
