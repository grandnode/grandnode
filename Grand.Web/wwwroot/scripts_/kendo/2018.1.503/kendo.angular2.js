/** 
 * Copyright 2017 Telerik AD                                                                                                                                                                            
 *                                                                                                                                                                                                      
 * Licensed under the Apache License, Version 2.0 (the "License");                                                                                                                                      
 * you may not use this file except in compliance with the License.                                                                                                                                     
 * You may obtain a copy of the License at                                                                                                                                                              
 *                                                                                                                                                                                                      
 *     http://www.apache.org/licenses/LICENSE-2.0                                                                                                                                                       
 *                                                                                                                                                                                                      
 * Unless required by applicable law or agreed to in writing, software                                                                                                                                  
 * distributed under the License is distributed on an "AS IS" BASIS,                                                                                                                                    
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.                                                                                                                             
 * See the License for the specific language governing permissions and                                                                                                                                  
 * limitations under the License.                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       
                                                                                                                                                                                                       

*/
(function (f, define) {
    define('kendo.angular2', [
        'kendo.core',
        'kendo.webcomponents'
    ], f);
}(function () {
    var __meta__ = {
        id: 'angular2',
        name: 'Angular 2',
        category: 'framework',
        description: 'Supports angular2 value accessors',
        depends: ['core']
    };
    (function (kendo, System) {
        if (!System || !System.register) {
            return;
        }
        var __decorate = this && this.__decorate || function (decorators, target, key, desc) {
            if (typeof Reflect === 'object' && typeof Reflect.decorate === 'function') {
                return Reflect.decorate(decorators, target, key, desc);
            }
            switch (arguments.length) {
            case 2:
                return decorators.reduceRight(function (o, d) {
                    return d && d(o) || o;
                }, target);
            case 3:
                return decorators.reduceRight(function (o, d) {
                    return d && d(target, key), void 0;
                }, void 0);
            case 4:
                return decorators.reduceRight(function (o, d) {
                    return d && d(target, key, o) || o;
                }, desc);
            }
        };
        var __metadata = this && this.__metadata || function (k, v) {
            if (typeof Reflect === 'object' && typeof Reflect.metadata === 'function') {
                return Reflect.metadata(k, v);
            }
        };
        System.register('kendo/angular2', ['angular2/angular2'], function (exports_1) {
            var angular2_1;
            var KendoValueAccessor;
            return {
                setters: [function (_angular2_1) {
                        angular2_1 = _angular2_1;
                    }],
                execute: function () {
                    KendoValueAccessor = function () {
                        function KendoValueAccessor(cd, elementRef) {
                            var _this = this;
                            this.elementRef = elementRef;
                            this.onChange = function (_) {
                            };
                            this.onTouched = function () {
                            };
                            this.element = elementRef.nativeElement;
                            this.element.addEventListener('change', function () {
                                _this.onChange(_this.element.value());
                            });
                            this.element.addEventListener('spin', function () {
                                _this.onChange(_this.element.value());
                            });
                            cd.valueAccessor = this;
                            this.cd = cd;
                            cd.valueAccessor = this;
                        }
                        KendoValueAccessor.prototype.writeValue = function (value) {
                            this.element.value(value);
                        };
                        KendoValueAccessor.prototype.registerOnChange = function (fn) {
                            this.onChange = fn;
                        };
                        KendoValueAccessor.prototype.registerOnTouched = function (fn) {
                            this.onTouched = fn;
                        };
                        KendoValueAccessor = __decorate([
                            angular2_1.Directive({ selector: kendo.webComponents.join(',') }),
                            __metadata('design:paramtypes', [
                                angular2_1.NgControl,
                                angular2_1.ElementRef
                            ])
                        ], KendoValueAccessor);
                        return KendoValueAccessor;
                    }();
                    exports_1('KendoValueAccessor', KendoValueAccessor);
                }
            };
        });
    }(window.kendo, window.System));
}, typeof define == 'function' && define.amd ? define : function (a1, a2, a3) {
    (a3 || a2)();
}));