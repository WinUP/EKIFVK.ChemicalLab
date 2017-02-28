"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var core_1 = require("@angular/core");
var material_1 = require("@angular/material");
var application_component_1 = require("./application.component");
require("hammerjs");
var ApplicationModule = (function () {
    function ApplicationModule() {
    }
    return ApplicationModule;
}());
ApplicationModule = __decorate([
    core_1.NgModule({
        imports: [material_1.MaterialModule.forRoot()],
        providers: [],
        declarations: [application_component_1.ApplicationComponent],
        bootstrap: [application_component_1.ApplicationComponent]
    }),
    __metadata("design:paramtypes", [])
], ApplicationModule);
exports.ApplicationModule = ApplicationModule;
//# sourceMappingURL=application.module.js.map