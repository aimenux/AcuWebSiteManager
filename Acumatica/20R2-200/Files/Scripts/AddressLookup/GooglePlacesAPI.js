var addressLookupAPI = (function () {

var autocomplete;
var map;
var infowindow= null;
var marker;

function _GetUrl(address, queryprefix) {
	var _url = "http://maps.google.com/maps?hl=" + queryprefix;
	_url = _url + encodeURIComponent(address);
	return _url;
}

function _geocodeQuery(query) {
	if ((query == null) || (query == "")) {
		return;
	}
	var geocoder = new google.maps.Geocoder();
	geocoder.geocode({ 'address': query }, function (results, status) {
		if (status === 'OK') {
			var place = results[0].geometry;
			if (!marker) {
				marker = new google.maps.Marker({
					map: map,
					position: place.location
				});
			} else {
				marker.setPosition(place.location);
			}
			var url = _GetUrl(addressLookupPanel.GetFormattedAddress(),
				"&q=")
			showPlace(place.location, addressLookupPanel.GetFormattedAddress(), "", url);
		}
	});
}

function showPlace(location, text, name, url) {
	if (text == null || text == "") {
		return;
	}
	addressLookupVars.setSearchQuery("");
	marker.setVisible(false);
	map.setCenter(location);
	map.setZoom(13);

	marker.setPosition(location);
	marker.setVisible(true);
	if (url == "") {
		url = _GetUrl();
	}

	var contentString =
		'<div id="content-custom">' +
		text +
		'</div>' +
		'<div class="view-link">' +
		'<a target="_blank" href="' + url + '"> <span> View on Google Maps </span> </a>' +
		'</div > ';
	if (typeof infowindow == 'undefined' || infowindow == null) {
		infowindow = new google.maps.InfoWindow({
			content: contentString,
			zIndex: 13
		});
	} else {
		infowindow.close();
		infowindow.setContent(contentString);
	}
	infowindow.open(map, marker);
	google.maps.event.clearListeners(marker, 'click');
	google.maps.event.addListener(marker, 'click', function () {
		infowindow.open(map, marker);
	});
}

function _bindAutocompleteSearchControl(ctrl) {
	address_element = ctrl.element.firstElementChild;
	if (typeof address_element == 'undefined') {
		alert("Control not found:" + control_name);
	}
	autocomplete = new google.maps.places.Autocomplete(address_element);
	autocomplete.setFields(['name', 'url', 'address_component', 'geometry', 'adr_address', 'formatted_address']);

	if ((typeof componentRestrictions_country != 'undefined') && componentRestrictions_country != null) {
		try {
			autocomplete.setComponentRestrictions({ 'country': componentRestrictions_country });
		} catch (e) {
			alert("Can't apply countries component restriction.");
		}
	}

	var mapCtrl = document.querySelector("[id$='addressautocompletemap']");
	if (typeof mapCtrl == 'undefined') {
		return;
	}

	var _styles = [
		{
			"featureType": "poi",
			"stylers": [
				{
					"visibility": "off"
				}
			]
		}
	];

	map = new google.maps.Map(mapCtrl, {
		center: { lat: -33.8688, lng: 151.2195 },
		zoom: 13,
		fullscreenControl: false,
		streetViewControl: false,
		mapTypeControl: true,
		mapTypeControlOptions: {
			style: google.maps.MapTypeControlStyle.HORIZONTAL_BAR,
			position: google.maps.ControlPosition.TOP_RIGHT
		},
		styles: _styles 
	});

	marker = new google.maps.Marker({
		map: map,
		anchorPoint: new google.maps.Point(0, -29)
	});

	autocomplete.addListener('place_changed', function () {
		var place = autocomplete.getPlace();
		if (!place.geometry) {
			return;
		}
		showPlace(place.geometry.location, place.formatted_address, place.name, place.url);
	});
	px_callback.addHandler(addressLookupPanel.addHhandleCallback);
}

var componentForm = [
	{ adr: "street-address", placeField: 'street_number', field: 'AddressLine1', type: 'long_name' },
	// { adr: "extended-address", placeField: 'route', field: 'AddressLine2', type: 'long_name' },
	{ adr: "locality", placeField: 'locality', field: 'City', type: 'long_name' },
	{ adr: "postal-code", placeField: 'postal_code', field: 'PostalCode', type: 'short_name' },
	{ adr: "", placeField: 'administrative_area_level_1', field: 'State', type: 'short_name' },
	{ adr: "", placeField: 'country', field: 'Country', type: 'short_name' },
	{ adr: "", placeField: 'Latitude', field: 'Latitude', type: 'location' },
	{ adr: "", placeField: 'Longitude', field: 'Longitude', type: 'location' }
];

function _fillInAddress() {
	var place = autocomplete.getPlace();
	const parser = new DOMParser();
	const doc = parser.parseFromString(place.adr_address, 'text/html');
	addressLookupPanel.CleanSearchResponseValues();

	for (var i = 0; i < componentForm.length; i++) {
		var val = "";
		if (componentForm[i].placeField == "Latitude") {
			val = place.geometry.location.lat();
		} else if (componentForm[i].placeField == "Longitude") {
			val = place.geometry.location.lng();
		} else if (componentForm[i].adr != "") {
			var vtmp = doc.getElementsByClassName(componentForm[i].adr);
			if (vtmp != null && vtmp != undefined && vtmp.length > 0) {
				val = vtmp[0].innerHTML.toString();
			}
		} else {
			var placeItem = place.address_components.filter(function (item) { return item.types[0] === componentForm[i].placeField; });
			if (placeItem.length < 1) {
				continue;
			}
			val = placeItem[0][componentForm[i].type];
		}
		var ctrlName = "SearchResponse" + componentForm[i].field;
		px_alls[ctrlName].updateValue(val);
	}
}

function _disableInfoWindows() {
	if (infowindow) {
		infowindow.close();
	}
	if (marker) {
		marker.setVisible(false);
	}
}

	return {
		fillInAddress: _fillInAddress,
		bindAutocompleteSearchControl: _bindAutocompleteSearchControl,
		geocodeQuery: _geocodeQuery,
		disableInfoWindows: _disableInfoWindows
	}
})();