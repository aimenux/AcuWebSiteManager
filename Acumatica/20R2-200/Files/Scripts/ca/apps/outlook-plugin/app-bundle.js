var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
define('apps/outlook-plugin/app',["require", "exports", "aurelia-fetch-client", "aurelia-framework", "./services/screen-api-client", "./services/authentication-service"], function (require, exports, aurelia_fetch_client_1, aurelia_framework_1, screen_api_client_1, authentication_service_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.App = void 0;
    var App = (function () {
        function App(client, tasqQueue, apiManager, authenticationService) {
            this.client = client;
            this.tasqQueue = tasqQueue;
            this.apiManager = apiManager;
            this.authenticationService = authenticationService;
            this.initialized = false;
            var _this = this;
            var root = requirejs.s.contexts._.config.baseUrl.replace("/scripts/ca/", "/");
            this.apiType = 'marina';
            this.apiManager.setApiRoot('newui/look');
            this.outlookScreenId = "ou201000";
            this.client.configure(function (client) {
                client
                    .withBaseUrl(root)
                    .withDefaults({
                    headers: {
                        'Accept': 'application/json',
                        'X-Requested-With': 'Fetch'
                    },
                    credentials: 'same-origin',
                })
                    .withInterceptor({
                    response: function (r) {
                        return __awaiter(this, void 0, void 0, function () {
                            var response, contentType, x;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, r.clone()];
                                    case 1:
                                        response = _a.sent();
                                        contentType = response.headers.get("content-type");
                                        if (!(contentType && contentType.indexOf("application/json") !== -1)) return [3, 3];
                                        return [4, response.json()];
                                    case 2:
                                        x = _a.sent();
                                        if (x.diffType === "redirect") {
                                            if (x.mode === 1) {
                                                if (x.url.indexOf("Frames/Outlook/FirstRun.html") > -1) {
                                                    _this.authenticationService.lostAuth();
                                                    throw new Error('Not authenticated');
                                                }
                                            }
                                            else if (x.mode === 2) {
                                                Office.context.ui.displayDialogAsync(x.url);
                                            }
                                        }
                                        _a.label = 3;
                                    case 3: return [2, r];
                                }
                            });
                        });
                    }
                });
            });
        }
        App.prototype.apiTypeChanged = function (newValue) {
            if (newValue == 'marina') {
                this.apiManager.setApiRoot('newui/look');
                this.outlookScreenId = "ou201000";
            }
            else if (newValue == 'sasha') {
                this.apiManager.setApiRoot('newui/test');
                this.outlookScreenId = "OutlookPlugin";
            }
            this.tasqQueue.flushMicroTaskQueue();
        };
        App.prototype.signInWithToken = function () {
            return __awaiter(this, void 0, void 0, function () {
                var signedIn, failedToSignIn, result;
                var _this_1 = this;
                return __generator(this, function (_a) {
                    result = new Promise(function (resolve, reject) { signedIn = resolve; failedToSignIn = reject; });
                    Office.context.mailbox.getUserIdentityTokenAsync(function (asyncResult) {
                        _this_1.authenticationService.signInWithToken(asyncResult.value)
                            .then(function (x) {
                            signedIn();
                        })
                            .catch(function (x) {
                            failedToSignIn();
                        });
                    });
                    return [2, result];
                });
            });
        };
        App.prototype.attached = function () {
            return __awaiter(this, void 0, void 0, function () {
                var signalReady, appReady;
                var _this_1 = this;
                return __generator(this, function (_a) {
                    appReady = new Promise(function (r) { signalReady = r; });
                    Office.initialize = function () {
                        signalReady();
                        _this_1.signInWithToken()
                            .then(function () { _this_1.initialized = true; })
                            .catch(function (x) { _this_1.initialized = true; });
                    };
                    return [2, appReady];
                });
            });
        };
        App.prototype.logIn = function () {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4, this.authenticationService.logIn(this.username, this.password)];
                        case 1:
                            _a.sent();
                            if (this.authenticationService.authenticated) {
                                this.username = '';
                                this.password = '';
                            }
                            return [2];
                    }
                });
            });
        };
        __decorate([
            aurelia_framework_1.observable,
            __metadata("design:type", Object)
        ], App.prototype, "initialized", void 0);
        __decorate([
            aurelia_framework_1.observable,
            __metadata("design:type", String)
        ], App.prototype, "apiType", void 0);
        App = __decorate([
            aurelia_framework_1.autoinject,
            __metadata("design:paramtypes", [aurelia_fetch_client_1.HttpClient, aurelia_framework_1.TaskQueue, screen_api_client_1.ApiManager, authentication_service_1.AuthenticationService])
        ], App);
        return App;
    }());
    exports.App = App;
});
;
define('text!apps/outlook-plugin/app.css',[],function(){return ".login-form {\n  padding: 12px;\n}\n\n.login-form input,\n.login-form button {\n  box-sizing: border-box;\n\n  width: 100%;\n  margin: 6px 0;\n  color: rgba(0, 0, 0, 0.64);\n  font-size: 19px;\n  font-weight: 500;\n\n  padding: 6px 8px;\n  border: solid 1px RGBA(0, 0, 0, 0.12);\n  border-radius: 3px;\n}\n\n.login-form input::placeholder {\n  font-style: normal;\n}\n\n.login-form button {\n  color: #444;\n  cursor: pointer;\n  padding: 5px;\n  background-color: #fff;\n\n  border: solid 1px #027ACC;\n  border-radius: 5px;\n  transition: all .1s ease-in-out;\n  -webkit-appearance: none;\n}\n\n.login-form button:focus,\n.login-form button:hover {\n  color: white;\n  background-color: #027ACC;\n  border: solid 1px #027ACC;\n\n}\n\n.login-form button:disabled {\n  color: #444;\n  cursor: default;\n  background-color: #eee;\n  border: solid 1px #ccc;\n}\n";});;
define('text!apps/outlook-plugin/app.html',[],function(){return "<template>\n  <require from=./app.css></require>\n  <require from=./controls/containers/screen-container></require>\n  \n  <hr>\n  <div if.bind=\"initialized && apiType\">\n    <div if.bind=authenticationService.authenticated style=background-color:#fff>\n      <qp-screen-container config.bind={screenId:outlookScreenId}>\n      </qp-screen-container>\n    </div>\n    <div else class=login-form>\n      <input type=text name=username value.bind=username required placeholder=\"My Username\">\n      <input type=password value.bind=password required placeholder=\"My Password\">\n      <button click.delegate=logIn() disabled.bind=\"!username || !password\">Sign In</button>\n    </div>\n  </div>\n  \n</template>\n";});;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
define('apps/outlook-plugin/controls/containers/labeled-control',["require", "exports", "aurelia-framework"], function (require, exports, aurelia_framework_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.QpLabeledControlCustomElement = void 0;
    var QpLabeledControlCustomElement = (function () {
        function QpLabeledControlCustomElement() {
            this.controlType = undefined;
        }
        Object.defineProperty(QpLabeledControlCustomElement.prototype, "label", {
            get: function () {
                var _a, _b, _c;
                if ((_a = this.controlConfig) === null || _a === void 0 ? void 0 : _a.suppressLabel) {
                    return undefined;
                }
                if ((_b = this.controlConfig) === null || _b === void 0 ? void 0 : _b.action) {
                    return undefined;
                }
                return (_c = this.controlConfig) === null || _c === void 0 ? void 0 : _c.displayName;
            },
            enumerable: false,
            configurable: true
        });
        __decorate([
            aurelia_framework_1.bindable,
            __metadata("design:type", Object)
        ], QpLabeledControlCustomElement.prototype, "controlConfig", void 0);
        __decorate([
            aurelia_framework_1.bindable({ defaultBindingMode: aurelia_framework_1.bindingMode.twoWay }),
            __metadata("design:type", Object)
        ], QpLabeledControlCustomElement.prototype, "controlValue", void 0);
        __decorate([
            aurelia_framework_1.bindable,
            __metadata("design:type", Object)
        ], QpLabeledControlCustomElement.prototype, "controlType", void 0);
        __decorate([
            aurelia_framework_1.computedFrom('controlConfig.displayName', 'controlConfig.action', 'controlConfig.suppressLabel'),
            __metadata("design:type", Object),
            __metadata("design:paramtypes", [])
        ], QpLabeledControlCustomElement.prototype, "label", null);
        QpLabeledControlCustomElement = __decorate([
            aurelia_framework_1.customElement("qp-labeled-control")
        ], QpLabeledControlCustomElement);
        return QpLabeledControlCustomElement;
    }());
    exports.QpLabeledControlCustomElement = QpLabeledControlCustomElement;
});
;
define('text!apps/outlook-plugin/controls/containers/labeled-control.css',[],function(){return ".labeled-control__control .qp-text-editor {\n  width: 100%;\n}\n.labeled-control__control .qp-button{\n  width:100%;\n}\n";});;
define('text!apps/outlook-plugin/controls/containers/labeled-control.html',[],function(){return "<template>\n  <require from=./labeled-control.css></require>\n  <div style=display:flex;margin:5px if.bind=\"controlConfig.visible === undefined  || controlConfig.visible !== false\">\n    <div style=\"flex:0 0 100px\" if.bind=label class=labeled-control__label>\n      <label>${label}</label>\n    </div>\n    <div style=\"flex:1 0 180px\">\n      <enhanced-compose view-model.bind=controlType class=labeled-control__control value.two-way=controlValue config.bind=controlConfig>\n      </enhanced-compose>\n    </div>\n  </div>\n</template>\n";});;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
define('apps/outlook-plugin/controls/containers/screen-container',["require", "exports", "aurelia-framework", "apps/outlook-plugin/services/screen-data"], function (require, exports, aurelia_framework_1, screen_data_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.QpScreenContainerCustomElement = void 0;
    var QpScreenContainerCustomElement = (function () {
        function QpScreenContainerCustomElement(screenInfo, screenData) {
            this.screenInfo = screenInfo;
            this.screenData = screenData;
        }
        QpScreenContainerCustomElement.prototype.configChanged = function (newValue) {
        };
        QpScreenContainerCustomElement.prototype.attached = function () {
            return this.screenInfo.initScreen(this.config.screenId);
        };
        __decorate([
            aurelia_framework_1.bindable,
            __metadata("design:type", Object)
        ], QpScreenContainerCustomElement.prototype, "config", void 0);
        QpScreenContainerCustomElement = __decorate([
            aurelia_framework_1.customElement("qp-screen-container"),
            aurelia_framework_1.autoinject,
            __metadata("design:paramtypes", [screen_data_1.ScreenInfo, screen_data_1.ScreenData])
        ], QpScreenContainerCustomElement);
        return QpScreenContainerCustomElement;
    }());
    exports.QpScreenContainerCustomElement = QpScreenContainerCustomElement;
});
;
define('text!apps/outlook-plugin/controls/containers/screen-container.html',[],function(){return "<template>\n  <div>\n    <div repeat.for=\"con of screenInfo.controls\">\n      <template if.bind=con.controls>\n        <enhanced-compose controls.bind=con.controls view-model.bind=con.type config.bind=screenData.configs[con.id]></enhanced-compose>\n      </template>\n      <template else>\n        <qp-labeled-control control-config.bind=screenData.configs[con.id] control-value.two-way=screenData.values[con.view][con.field] control-type.bind=con.type></qp-labeled-control>\n      </template>\n    </div>\n  </div>\n</template>\n";});;
define('apps/outlook-plugin/controls/containers/vertical-stack',["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.QpVerticalStackContainerCustomElement = void 0;
    var QpVerticalStackContainerCustomElement = (function () {
        function QpVerticalStackContainerCustomElement() {
        }
        return QpVerticalStackContainerCustomElement;
    }());
    exports.QpVerticalStackContainerCustomElement = QpVerticalStackContainerCustomElement;
});
;
define('text!apps/outlook-plugin/controls/containers/vertical-stack.html',[],function(){return "<template>\n  \n</template>\n";});;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
define('apps/outlook-plugin/controls/outlook-data/outlook-data',["require", "exports", "aurelia-framework", "apps/outlook-plugin/services/screen-data", "aurelia-event-aggregator"], function (require, exports, aurelia_framework_1, screen_data_1, aurelia_event_aggregator_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.QpOutlookDataCustomElement = void 0;
    var QpOutlookDataCustomElement = (function () {
        function QpOutlookDataCustomElement(container, screenData, taskQueue, eventAggregator) {
            this.container = container;
            this.screenData = screenData;
            this.taskQueue = taskQueue;
            this.eventAggregator = eventAggregator;
            this.ran = false;
        }
        QpOutlookDataCustomElement.prototype.configChanged = function (n, o) {
        };
        QpOutlookDataCustomElement.prototype.attached = function () {
            var _this = this;
            this.taskQueue.queueTask(function () { _this.updateValue(); });
        };
        QpOutlookDataCustomElement.prototype.updateValue = function () {
            var _this = this;
            var message = Office.context.mailbox.item;
            this.serialized = JSON.stringify(message, null, 2);
            if (!message) {
                return;
            }
            if (this.config.fieldsMap) {
                if (this.config.fieldsMap.isIncome) {
                    this.screenData.setValue(this.config.fieldsMap.isIncome, true);
                }
                if (this.config.fieldsMap.emailAddress) {
                    this.screenData.setValue(this.config.fieldsMap.emailAddress, message.sender.emailAddress);
                }
                if (this.config.fieldsMap.displayName) {
                    this.screenData.setValue(this.config.fieldsMap.displayName, message.sender.displayName);
                }
                if (this.config.fieldsMap.newFirstName) {
                    this.screenData.setValue(this.config.fieldsMap.newFirstName, getFirstName(message.sender.displayName));
                }
                if (this.config.fieldsMap.newLastName) {
                    this.screenData.setValue(this.config.fieldsMap.newLastName, getLastName(message.sender.displayName));
                }
                if (message.sender.emailAddress == "" || message.sender.emailAddress == Office.context.mailbox.userProfile.emailAddress) {
                    if (this.config.fieldsMap.isIncome) {
                        this.screenData.setValue(this.config.fieldsMap.isIncome, false);
                    }
                    if (this.config.fieldsMap.displayName) {
                        this.screenData.setValue(this.config.fieldsMap.displayName, "");
                    }
                    if (this.config.fieldsMap.newFirstName) {
                        this.screenData.setValue(this.config.fieldsMap.newFirstName, "");
                    }
                    if (this.config.fieldsMap.newLastName) {
                        this.screenData.setValue(this.config.fieldsMap.newLastName, "");
                    }
                }
                if (this.config.fieldsMap.messageId) {
                    this.screenData.setValue(this.config.fieldsMap.messageId, message.internetMessageId);
                }
                if (this.config.fieldsMap.subject) {
                    this.screenData.setValue(this.config.fieldsMap.subject, message.subject);
                }
                if (this.config.fieldsMap.itemId) {
                    this.screenData.setValue(this.config.fieldsMap.itemId, message.itemId);
                }
                if (this.config.fieldsMap.to) {
                    this.screenData.setValue(this.config.fieldsMap.to, serializeAddressList(message.to));
                }
                if (this.config.fieldsMap.cC) {
                    this.screenData.setValue(this.config.fieldsMap.cC, serializeAddressList(message.cc));
                }
                if (this.config.fieldsMap.ewsUrl) {
                    this.screenData.setValue(this.config.fieldsMap.ewsUrl, Office.context.mailbox.ewsUrl);
                }
                Office.context.mailbox.getCallbackTokenAsync(function (asyncResult) {
                    if (asyncResult.status === "succeeded") {
                        if (_this.config.fieldsMap.apiToken) {
                            _this.screenData.setValue(_this.config.fieldsMap.apiToken, asyncResult.value);
                        }
                    }
                    _this.eventAggregator.publish("execute-command");
                });
            }
        };
        __decorate([
            aurelia_framework_1.bindable,
            __metadata("design:type", Object)
        ], QpOutlookDataCustomElement.prototype, "config", void 0);
        __decorate([
            aurelia_framework_1.bindable,
            __metadata("design:type", Object)
        ], QpOutlookDataCustomElement.prototype, "value", void 0);
        QpOutlookDataCustomElement = __decorate([
            aurelia_framework_1.autoinject,
            aurelia_framework_1.customElement('qp-outlook-data'),
            __metadata("design:paramtypes", [aurelia_framework_1.Container, screen_data_1.ScreenData, aurelia_framework_1.TaskQueue, aurelia_event_aggregator_1.EventAggregator])
        ], QpOutlookDataCustomElement);
        return QpOutlookDataCustomElement;
    }());
    exports.QpOutlookDataCustomElement = QpOutlookDataCustomElement;
    function serializeAddressList(list) {
        var msg = "";
        if (list != null)
            list.forEach(function (recip, index) {
                msg = msg + "\"" + recip.displayName + "\"" + " <" + recip.emailAddress + ">;";
            });
        return msg;
    }
    function getFirstName(displayName) {
        displayName = displayName.trim();
        while (displayName.indexOf("  ") > -1)
            displayName = displayName.replace("  ", " ");
        var names = displayName.split(" ");
        var firstName = names.length > 1 ? names[0] : null;
        return firstName;
    }
    function getLastName(displayName) {
        displayName = displayName.trim();
        while (displayName.indexOf("  ") > -1)
            displayName = displayName.replace("  ", " ");
        var names = displayName.split(" ");
        var lastName = names.length > 1 ? names[names.length - 1] : names[0];
        return lastName;
    }
});
;
define('text!apps/outlook-plugin/controls/outlook-data/outlook-data.html',[],function(){return "<template>\n\n</template>\n";});;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
define('apps/outlook-plugin/controls/qp-command-button',["require", "exports", "aurelia-framework", "../services/screen-data"], function (require, exports, aurelia_framework_1, screen_data_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.QpCommandButtonCustomElement = void 0;
    var QpCommandButtonCustomElement = (function () {
        function QpCommandButtonCustomElement(screenData) {
            this.screenData = screenData;
        }
        QpCommandButtonCustomElement.prototype.click = function () {
            this.screenData.executeCommand(this.config.command || this.config.action);
        };
        __decorate([
            aurelia_framework_1.bindable,
            __metadata("design:type", Object)
        ], QpCommandButtonCustomElement.prototype, "config", void 0);
        QpCommandButtonCustomElement = __decorate([
            aurelia_framework_1.customElement("qp-command-button"),
            aurelia_framework_1.autoinject,
            __metadata("design:paramtypes", [screen_data_1.ScreenInfo])
        ], QpCommandButtonCustomElement);
        return QpCommandButtonCustomElement;
    }());
    exports.QpCommandButtonCustomElement = QpCommandButtonCustomElement;
});
;
define('text!apps/outlook-plugin/controls/qp-command-button.html',[],function(){return "<template>\n  <button style=\"width:100%;color:#333;font-family:'Segoe UI Regular WestEuropean','Segoe UI','Segoe WP',Tahoma,Arial,sans-serif;font-size:14px;font-weight:400;box-sizing:border-box;margin:0;box-shadow:none;background-color:#f4f4f4;border:1px solid #f4f4f4;cursor:pointer;display:inline-block;height:2pc;min-width:5pc;padding:4px 20px 6px\" if.bind=config.visible click.delegate=click() disabled.bind=config.disabled>${config.DisplayName || config.displayName|| config.id}</button> \n</template>\n\n\n\n";});;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
define('apps/outlook-plugin/controls/qp-container',["require", "exports", "aurelia-framework", "../services/screen-data"], function (require, exports, aurelia_framework_1, screen_data_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.QpContainerCustomElement = void 0;
    var QpContainerCustomElement = (function () {
        function QpContainerCustomElement(screenData) {
            this.screenData = screenData;
        }
        __decorate([
            aurelia_framework_1.bindable,
            __metadata("design:type", Object)
        ], QpContainerCustomElement.prototype, "controls", void 0);
        QpContainerCustomElement = __decorate([
            aurelia_framework_1.autoinject,
            aurelia_framework_1.customElement("qp-container"),
            __metadata("design:paramtypes", [screen_data_1.ScreenData])
        ], QpContainerCustomElement);
        return QpContainerCustomElement;
    }());
    exports.QpContainerCustomElement = QpContainerCustomElement;
});
;
define('text!apps/outlook-plugin/controls/qp-container.html',[],function(){return "<template>\n  <div>\n    <div repeat.for=\"con of controls\">\n      <template if.bind=con.controls>\n\n        <enhanced-compose controls.bind=con.controls view-model.bind=con.type config.bind=screenData.configs[con.id]>\n        </enhanced-compose>\n\n      </template>\n      <template else>\n        <qp-labeled-control control-config.bind=screenData.configs[con.id] control-value.two-way=screenData.fieldValues[con.view][con.field] control-type.bind=con.type></qp-labeled-control>\n\n      </template>\n    </div>\n  </div>\n</template>\n";});;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
define('apps/outlook-plugin/enhanced-compose',["require", "exports", "aurelia-framework", "aurelia-logging"], function (require, exports, aurelia_framework_1, aurelia_logging_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.EnhancedComposeCustomElement = void 0;
    var logger = aurelia_logging_1.getLogger('enhanced-compose');
    var EnhancedComposeCustomElement = (function () {
        function EnhancedComposeCustomElement(container, viewResources, element, targetInstruction, compositionEngine) {
            this.container = container;
            this.viewResources = viewResources;
            this.element = element;
            this.targetInstruction = targetInstruction;
            this.compositionEngine = compositionEngine;
            this.expressions = this.targetInstruction.expressions;
        }
        EnhancedComposeCustomElement.prototype.bind = function (bindingContext, overrideContext) {
            this.bindingContext = bindingContext;
            this.overrideContext = overrideContext;
            this.viewModelChanged(this.viewModel);
        };
        EnhancedComposeCustomElement.prototype.getViewModelClassFromTagName = function (customElementTagName) {
            var r = this.viewResources.getElement(customElementTagName);
            if (r) {
                return r.target;
            }
            else {
                var r_1 = this.viewResources.getElement('qp-text-editor');
            }
        };
        EnhancedComposeCustomElement.prototype.viewModelChanged = function (viewModel) {
            return __awaiter(this, void 0, void 0, function () {
                var vmClass, scope_1;
                var _this = this;
                return __generator(this, function (_a) {
                    if (this.currViewModel || this.currViewModelPromise) {
                        return [2];
                    }
                    if (viewModel) {
                        vmClass = this.getViewModelClassFromTagName(viewModel);
                        if (!vmClass) {
                            logger.error("No client control exists for tag", viewModel);
                        }
                        scope_1 = {
                            bindingContext: this.bindingContext,
                            overrideContext: this.overrideContext,
                            viewModel: vmClass,
                            container: this.container,
                            host: this.element,
                            viewResources: this.container.get(aurelia_framework_1.ViewResources),
                            viewSlot: this.container.get(aurelia_framework_1.ViewSlot)
                        };
                        this.currentViewModelPromise = this.compositionEngine
                            .compose(scope_1)
                            .then(function (controller) {
                            _this.currentViewModel = controller.viewModel;
                            var behaviorResource = aurelia_framework_1.metadata.get(aurelia_framework_1.metadata.resource, controller.viewModel.constructor);
                            if (behaviorResource) {
                                var bindables = behaviorResource.properties;
                                var _loop_1 = function (property) {
                                    var expression = _this.expressions.find(function (exp) { return exp.targetProperty === property.name; });
                                    if (expression) {
                                        var b = expression.createBinding(controller.viewModel);
                                        controller.view.addBinding(b);
                                    }
                                };
                                for (var _i = 0, bindables_1 = bindables; _i < bindables_1.length; _i++) {
                                    var property = bindables_1[_i];
                                    _loop_1(property);
                                }
                            }
                            controller.bind(scope_1);
                        }, function (ex) {
                            logger.error("error composing", ex);
                        })
                            .then(function () {
                            _this.currentViewModelPromise = undefined;
                        });
                    }
                    return [2];
                });
            });
        };
        __decorate([
            aurelia_framework_1.bindable,
            __metadata("design:type", Object)
        ], EnhancedComposeCustomElement.prototype, "viewModel", void 0);
        EnhancedComposeCustomElement = __decorate([
            aurelia_framework_1.noView,
            aurelia_framework_1.customElement("enhanced-compose"),
            aurelia_framework_1.autoinject,
            __metadata("design:paramtypes", [aurelia_framework_1.Container, aurelia_framework_1.ViewResources, Element, aurelia_framework_1.TargetInstruction, aurelia_framework_1.CompositionEngine])
        ], EnhancedComposeCustomElement);
        return EnhancedComposeCustomElement;
    }());
    exports.EnhancedComposeCustomElement = EnhancedComposeCustomElement;
});
;
define('apps/outlook-plugin/home/copy',["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.Home = void 0;
    var Home = (function () {
        function Home() {
        }
        return Home;
    }());
    exports.Home = Home;
});
;
define('text!apps/outlook-plugin/home/copy.html',[],function(){return "<template>\n  <div class=\"au-animate animate-slide-in-right\">\n\n    Copy\n    пашел на \n  </div>\n</template>\n";});;
define('apps/outlook-plugin/home/index',["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.Home = void 0;
    var Home = (function () {
        function Home() {
        }
        return Home;
    }());
    exports.Home = Home;
});
;
define('text!apps/outlook-plugin/home/index.html',[],function(){return "<template>\n  <div class=\"au-animate animate-slide-in-right\">\n    Home\nиди на\n  </div>\n</template>\n";});;
define('apps/outlook-plugin/main',["require", "exports", "aurelia-framework", "../../environment", "./enhanced-compose", "./controls/outlook-data/outlook-data", "./controls/qp-command-button", "./controls/qp-container", "./controls/containers/labeled-control"], function (require, exports, aurelia_framework_1, environment_1, enhanced_compose_1, outlook_data_1, qp_command_button_1, qp_container_1, labeled_control_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.configure = void 0;
    function configure(aurelia) {
        var globalFeatures = aurelia_framework_1.PLATFORM.global.globalControlsModules;
        aurelia.use
            .basicConfiguration()
            .plugin('aurelia-dialog')
            .plugin('aurelia-ui-virtualization');
        ["apps/enhance/controls", "plugins/glue"].forEach(function (i) {
            aurelia.use.feature(i);
        });
        if (Array.isArray(globalFeatures)) {
            globalFeatures.forEach(function (x) { return aurelia.use.feature(x); });
        }
        aurelia.use.globalResources([enhanced_compose_1.EnhancedComposeCustomElement, outlook_data_1.QpOutlookDataCustomElement, qp_command_button_1.QpCommandButtonCustomElement, qp_container_1.QpContainerCustomElement, labeled_control_1.QpLabeledControlCustomElement]);
        aurelia.use.developmentLogging(environment_1.default.debug ? 'debug' : 'warn');
        if (environment_1.default.testing) {
            aurelia.use.plugin('aurelia-testing');
        }
        aurelia.start().then(function () { return aurelia.setRoot(); });
    }
    exports.configure = configure;
});
;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
define('apps/outlook-plugin/services/authentication-service',["require", "exports", "aurelia-framework", "aurelia-fetch-client"], function (require, exports, aurelia_framework_1, aurelia_fetch_client_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.AuthenticationService = void 0;
    var root = requirejs.s.contexts._.config.baseUrl.replace("/scripts/ca/", "/");
    var AuthenticationService = (function () {
        function AuthenticationService(client) {
            this.client = client;
            this.authenticated = false;
            var _this = this;
            this.client.configure(function (config) {
                config.withInterceptor({
                    response: function (response, request) {
                        var reqUrl = request.url.toLowerCase();
                        if (reqUrl.indexOf('authdock') || reqUrl.indexOf('login')) {
                            return response;
                        }
                        if (response.redirected && response.url.toLowerCase().indexOf('login') > -1) {
                            _this.authenticated = false;
                            throw new Error('not authenticated');
                        }
                        else {
                            return response;
                        }
                    }
                });
            });
        }
        AuthenticationService.prototype.signInWithToken = function (token) {
            return __awaiter(this, void 0, void 0, function () {
                var signedIn, failedToSignIn, result, formData;
                var _this_1 = this;
                return __generator(this, function (_a) {
                    this.outlookAuthToken = token;
                    result = new Promise(function (resolve, reject) { signedIn = resolve; failedToSignIn = reject; });
                    formData = new FormData();
                    formData.append('token', this.outlookAuthToken);
                    this.client.fetch("Frames/AuthDock.ashx?_returnUrl_=" + root + "success", {
                        method: 'POST',
                        body: formData
                    }).then(function (tokenAuthResult) { return __awaiter(_this_1, void 0, void 0, function () {
                        var target;
                        return __generator(this, function (_a) {
                            if (tokenAuthResult.redirected) {
                                if (tokenAuthResult.url.endsWith('success')) {
                                    this.authenticated = true;
                                    signedIn();
                                }
                                else {
                                    this.authenticated = false;
                                    if (tokenAuthResult.url.toLocaleLowerCase().indexOf('login') > -1) {
                                        target = new URL(tokenAuthResult.url);
                                        this.exId = target.searchParams.get('exceptionid');
                                    }
                                    failedToSignIn();
                                }
                            }
                            return [2];
                        });
                    }); });
                    return [2, result];
                });
            });
        };
        AuthenticationService.prototype.logIn = function (username, password) {
            return __awaiter(this, void 0, void 0, function () {
                var _a;
                var _this_1 = this;
                return __generator(this, function (_b) {
                    switch (_b.label) {
                        case 0:
                            if (!(!this.exId && this.outlookAuthToken)) return [3, 4];
                            _b.label = 1;
                        case 1:
                            _b.trys.push([1, 3, , 4]);
                            return [4, this.signInWithToken(this.outlookAuthToken)];
                        case 2:
                            _b.sent();
                            return [3, 4];
                        case 3:
                            _a = _b.sent();
                            return [3, 4];
                        case 4:
                            this.client.fetch("ui/Outlook/LogIn?exceptionid=" + this.exId, {
                                method: 'POST',
                                body: aurelia_fetch_client_1.json({ Login: username, Password: password })
                            })
                                .then(function (r) {
                                _this_1.exId = undefined;
                                if (r.ok) {
                                    _this_1.authenticated = true;
                                }
                                else {
                                    _this_1.authenticated = false;
                                }
                            });
                            return [2];
                    }
                });
            });
        };
        AuthenticationService.prototype.lostAuth = function () {
            this.authenticated = false;
        };
        AuthenticationService = __decorate([
            aurelia_framework_1.autoinject,
            __metadata("design:paramtypes", [aurelia_fetch_client_1.HttpClient])
        ], AuthenticationService);
        return AuthenticationService;
    }());
    exports.AuthenticationService = AuthenticationService;
});
;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
define('apps/outlook-plugin/services/screen-api-client',["require", "exports", "aurelia-framework", "aurelia-fetch-client"], function (require, exports, aurelia_framework_1, aurelia_fetch_client_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.ApiManager = exports.ScreenApiClient = void 0;
    var apiRoot = 'newui/look/schema';
    var ScreenApiClient = (function () {
        function ScreenApiClient(client) {
            this.client = client;
        }
        ScreenApiClient.prototype.getScreenConfig = function (screenId) {
            return this.client.fetch(apiRoot + "/" + screenId)
                .then(function (x) { return x.json(); });
        };
        ScreenApiClient.prototype.postScreenData = function (screenId, data) {
            return this.client.fetch(apiRoot + "/" + screenId, {
                method: 'POST',
                body: aurelia_fetch_client_1.json(data)
            }).then(function (x) { return x.json(); });
        };
        ScreenApiClient = __decorate([
            aurelia_framework_1.autoinject,
            __metadata("design:paramtypes", [aurelia_fetch_client_1.HttpClient])
        ], ScreenApiClient);
        return ScreenApiClient;
    }());
    exports.ScreenApiClient = ScreenApiClient;
    var ApiManager = (function () {
        function ApiManager() {
        }
        ApiManager.prototype.setApiRoot = function (newApiRoot) {
            apiRoot = newApiRoot;
        };
        return ApiManager;
    }());
    exports.ApiManager = ApiManager;
});
;
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
define('apps/outlook-plugin/services/screen-data',["require", "exports", "aurelia-framework", "aurelia-event-aggregator", "./screen-api-client"], function (require, exports, aurelia_framework_1, aurelia_event_aggregator_1, screen_api_client_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.ScreenInfo = exports.ScreenData = void 0;
    var RECORD_ID = '_RecordId';
    var ScreenData = (function () {
        function ScreenData() {
            this.fieldValues = {};
            this.configs = {};
        }
        ScreenData.prototype.setValue = function (id, value) {
            this.fieldValues[id.view][id.field] = value;
        };
        ScreenData = __decorate([
            aurelia_framework_1.singleton("screen-data")
        ], ScreenData);
        return ScreenData;
    }());
    exports.ScreenData = ScreenData;
    var ScreenInfo = (function () {
        function ScreenInfo(eventAggregator, screenData, screenApiClient, bindingEngine, tq) {
            var _this = this;
            this.eventAggregator = eventAggregator;
            this.screenData = screenData;
            this.screenApiClient = screenApiClient;
            this.bindingEngine = bindingEngine;
            this.tq = tq;
            this.originalValues = {};
            this.dirtyValues = {};
            this.eventAggregator.subscribe("execute-command", function (d) {
                _this.tq.flushMicroTaskQueue();
                _this.tq.queueTask(function () {
                    _this.updateScreen(d);
                });
            });
        }
        ScreenInfo.prototype.initScreen = function (screenId, blackBoxMatters) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    this.screenData.fieldValues = {};
                    this.screenData.configs = {};
                    this.controls = [];
                    this.screenId = screenId;
                    return [2, this.screenApiClient.getScreenConfig(screenId)
                            .then(function (x) {
                            _this.controls = x.controls;
                            _this.screenData.fieldValues = x.fieldValues;
                            _this.originalValues = JSON.parse(JSON.stringify(x.fieldValues));
                            _this.screenData.configs = x.configs;
                            for (var viewName in x.fieldValues) {
                                if (x.fieldValues.hasOwnProperty(viewName)) {
                                    var viewFields = x.fieldValues[viewName];
                                    for (var fieldName in viewFields) {
                                        if (viewFields.hasOwnProperty(fieldName)) {
                                            var autopostback = false;
                                            var ob = _this.generateValueObserver(viewName, fieldName, autopostback);
                                            _this.bindingEngine.propertyObserver(_this.screenData.fieldValues[viewName], fieldName).subscribe(ob);
                                        }
                                    }
                                }
                            }
                        })];
                });
            });
        };
        ScreenInfo.prototype.updateScreen = function (command) {
            return __awaiter(this, void 0, void 0, function () {
                var saveData, haveData, viewName, viewFields, fieldName, updates, newValues, viewName, viewFields, fieldName;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            this.saving = true;
                            saveData = Object.assign({}, { data: this.dirtyValues }, { command: command });
                            haveData = false;
                            for (viewName in saveData.data) {
                                if (saveData.data.hasOwnProperty(viewName)) {
                                    viewFields = saveData.data[viewName];
                                    for (fieldName in viewFields) {
                                        if (viewFields.hasOwnProperty(fieldName)) {
                                            haveData = true;
                                            saveData.data[viewName][RECORD_ID] = this.screenData.fieldValues[viewName][RECORD_ID];
                                            break;
                                        }
                                    }
                                }
                            }
                            if (!haveData && !command) {
                                return [2];
                            }
                            return [4, this.screenApiClient.postScreenData(this.screenId, saveData)];
                        case 1:
                            updates = _a.sent();
                            if (updates.diffType == undefined) {
                                newValues = updates.fieldValues;
                                for (viewName in newValues) {
                                    if (newValues.hasOwnProperty(viewName)) {
                                        viewFields = newValues[viewName];
                                        for (fieldName in viewFields) {
                                            if (viewFields.hasOwnProperty(fieldName)) {
                                                this.screenData.fieldValues[viewName][fieldName] = viewFields[fieldName];
                                            }
                                        }
                                    }
                                }
                                this.screenData.configs = updates.configs;
                            }
                            this.tq.flushMicroTaskQueue();
                            this.dirtyValues = {};
                            return [2];
                    }
                });
            });
        };
        ScreenInfo.prototype.executeCommand = function (command) {
            this.tq.flushMicroTaskQueue();
            this.eventAggregator.publish("execute-command", command);
        };
        ScreenInfo.prototype.generateValueObserver = function (viewName, fieldName, autoPostback) {
            var _this = this;
            return function (newValue, oldValue) {
                if (!_this.dirtyValues[viewName]) {
                    _this.dirtyValues[viewName] = {};
                }
                _this.dirtyValues[viewName][fieldName] = newValue;
                if (_this.dirtyValues[viewName][fieldName] === _this.originalValues[viewName][fieldName]) {
                    delete _this.dirtyValues[viewName][fieldName];
                }
            };
        };
        ScreenInfo = __decorate([
            aurelia_framework_1.autoinject,
            __metadata("design:paramtypes", [aurelia_event_aggregator_1.EventAggregator, ScreenData, screen_api_client_1.ScreenApiClient, aurelia_framework_1.BindingEngine, aurelia_framework_1.TaskQueue])
        ], ScreenInfo);
        return ScreenInfo;
    }());
    exports.ScreenInfo = ScreenInfo;
});

//# sourceMappingURL=app-bundle.js.map