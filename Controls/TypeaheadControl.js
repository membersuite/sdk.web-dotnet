 
$(document).ready(function () {
    $('#{SearchPlaceholderID}').membersuiteTypeahead({
        algolia_app_id: '{AlgoliaApplicationID}',
        algolia_api_id: '{AlgoliaAPIKey}',
        algolia_init_idx: '{AlgoliaIndex}',
        hits_per_page: %HitsPerPage%,
        input_id: '{PrimaryInputID}',
        input_placeholder_text: '{PlaceholderText}',
        input_width: '{Width}',
        input_position_right: 300,
        input_position_top: 0,
        hidden_input_client_id: '{SearchItemControlID}',
        hidden_input_unique_id: '{SelectedSearchItemControlID}',
        dropdown_menu_height: 253,
        dropdown_menu_alignment: '{Alignment}',
        noResultsText: '{NoResultsText}',
        dropdown_menu_content: [
            %DropDownMenuContent%
        ],
        guid: %SelectedValue%,
        all_records_index: '*',
        tag_filters: %TagFilters%,
        onselecteditem: function() {
            %ClientOnSelectedItemChanged%
        },
        onbeforerender: function() {
            ;
        },
        onafterrender: function() {
            ;
        },
        onerror: function() {
            %ClientOnSearchError%
        }
    });
       
});
     