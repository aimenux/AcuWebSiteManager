"use strict";
var Logger = /** @class */ (function () {
    function Logger() {
    }
    Logger.initLogger = function (url, user, company, apiKey) {
        if (apiKey) {
            Logger.apiKey = apiKey;
        }
        Logger.url = url + "/" + company + "/" + user;
    };
    Logger.logEventToServer = function (event, payload) {
        if (!Logger.url) {
            return;
        }
        var r = new XMLHttpRequest();
        r.open("POST", Logger.url);
        if (Logger.apiKey) {
            r.setRequestHeader("X-Api-Key", Logger.apiKey);
        }
        var data = {
            event: event,
            details: payload
        };
        r.send(JSON.stringify(data));
    };
    Logger.apiKey = '';
    return Logger;
}());
