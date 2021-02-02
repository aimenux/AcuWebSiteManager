var addressLookupAPI = (function () {

var map, searchManager, infobox, pin;
var bingSuggestedAddress = "";
var latitude = "";
var longitude = "";

function _GetUrl(address, queryprefix) {
	var _url = "https://www.bing.com/maps?where1=" + queryprefix;
	_url = _url + encodeURIComponent(address);
	return _url;
}

function _geocodeQuery(query) {
	if ((query == null) || (query == "")) {
		return;
	}

	if (!searchManager) {
		setTimeout(function () {
			Microsoft.Maps.loadModule('Microsoft.Maps.Search', function () {
				searchManager = new Microsoft.Maps.Search.SearchManager(map);
				_geocodeQuery(query);
			});
		}, 2000);
	}
	var searchRequest = {
		where: query,
		callback: function (r) {
			if (r && r.results && r.results.length > 0) {
				showPlace(r.results[0].location, addressLookupPanel.GetFormattedAddress(), "");
			}
		},
		errorCallback: function (e) {
			return;
		}
	};
	searchManager.geocode(searchRequest);
}

function _bindAutocompleteSearchControl(ctrl) {
	if (!map) {
		map = new Microsoft.Maps.Map('#addressautocompletemap', {});

		Microsoft.Maps.loadModule('Microsoft.Maps.AutoSuggest', function () {
			var options = { maxResults: 5, businessSuggestions: true, map: map };
			if (typeof countryCodeSettings != 'undefined' && countryCodeSettings != null) {
				options = { maxResults: 5, businessSuggestions: true, map: map, countryCode: countryCodeSettings };
			}
			var manager = new Microsoft.Maps.AutosuggestManager(options);
			manager.attachAutosuggest('#' + ctrl.ID, '#searchBoxContainer', _selectedSuggestion);
		});
	}
	px_callback.addHandler(addressLookupPanel.addHhandleCallback);
}

function _selectedSuggestion(result) {
	map.entities.clear();

	bingSuggestedAddress = result.address;
	_fillInAddress();
	_addr = addressLookupPanel.GetFormattedAddress();
	showPlace(result.location, _addr, _GetUrl(_addr, ""));

	bingSuggestedAddress = result.address;
	latitude = result.location.latitude;
	longitude = result.location.longitude;
}

function showPlace(location, text, url) {
	if (text == null || text == "") {
		return;
	}
	if (!pin) {
		pin = new Microsoft.Maps.Pushpin(location);
		map.entities.push(pin);
	}
	pin.setLocation(location);
	addressLookupVars.setSearchQuery("");

	map.setView({ center: location });
	var center = map.getCenter();
	if (url == "") {
		if(!px_alls.SearchResponseAddressLine1.getValue() && 
			!px_alls.SearchResponseAddressLine2.getValue())
		{
			url = "https://bing.com/maps/default.aspx?cp=" + location.latitude + "~" + location.longitude + "&lvl=16&dir=0&sty=o&sp=point."+ location.latitude + "_" + location.longitude + "_You%20are%20here";
		} else {
			url = _GetUrl(addressLookupPanel.GetFormattedAddress(),"");
		}
	}

	if (url != "") {
		text = text + '<div class="view-link">' +
			'<a target="_blank" href="' + url + '"> <span> View on Bing Map </span> </a>' +
			'</div > ';
	}
	if (!infobox) {
		infobox = new Microsoft.Maps.Infobox(center, {
			location: location,
			title: "",
			description: text,
			visible: true
		});
		infobox.setMap(map);
	}
	infobox.setOptions({
		location: location,
		title: "",
		description: text,
		visible: true
	});
	bingSuggestedAddress = null;
}

var componentForm = [
	{ placeField: 'addressLine', field: 'AddressLine1' },
	//{ placeField: 'district', field: 'AddressLine2'},
	{ placeField: 'locality', field: 'City' },
	{ placeField: 'postalCode', field: 'PostalCode' },
	{ placeField: 'adminDistrict', field: 'State' },
	{ placeField: 'countryRegionISO2', field: 'Country' },
	{ placeField: 'latitude', field: 'Latitude' },
	{ placeField: 'longitude', field: 'Longitude' }
];

function _fillInAddress() {
	if ((typeof bingSuggestedAddress == 'undefined') || bingSuggestedAddress === null) {
		result;
	}
	addressLookupPanel.CleanSearchResponseValues();
	var place = bingSuggestedAddress;
	for (var i = 0; i < componentForm.length; i++) {
		var placeItem = place[componentForm[i].placeField];
		if ((typeof placeItem == 'undefined') || placeItem === null || placeItem.length < 1) {
			if (componentForm[i].placeField == 'latitude') {
				placeItem = latitude;
			} else if (componentForm[i].placeField == 'longitude') {
				placeItem = longitude;
			} else {
				placeItem = "";
			}
		}
		var val = placeItem;
		var ctrlName = "SearchResponse" + componentForm[i].field;
		px_alls[ctrlName].updateValue(val);
	}
	bingSuggestedAddress = "";
}

function _disableInfoWindows() {
	if (pin) {
		pin.setOptions({ visible: false });
	}
	if (infobox) {
		infobox.setOptions({ visible: false });
	}
	bingSuggestedAddress = "";
}
	return {
		fillInAddress: _fillInAddress, 
		bindAutocompleteSearchControl: _bindAutocompleteSearchControl, 
		geocodeQuery: _geocodeQuery,
		disableInfoWindows: _disableInfoWindows
	}
})();
