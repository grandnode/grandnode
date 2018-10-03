/** 
 * Copyright 2018 Telerik AD                                                                                                                                                                            
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
    define('kendo.mobile.shim', ['kendo.popup'], f);
}(function () {
    var __meta__ = {
        id: 'mobile.shim',
        name: 'Shim',
        category: 'mobile',
        description: 'Mobile Shim',
        depends: ['popup'],
        hidden: true
    };
    (function ($, undefined) {
        var kendo = window.kendo, ui = kendo.mobile.ui, Popup = kendo.ui.Popup, SHIM = '<div class="km-shim"/>', HIDE = 'hide', Widget = ui.Widget;
        var Shim = Widget.extend({
            init: function (element, options) {
                var that = this, app = kendo.mobile.application, os = kendo.support.mobileOS, osname = app ? app.os.name : os ? os.name : 'ios', ioswp = osname === 'ios' || osname === 'wp' || (app ? app.os.skin : false), bb = osname === 'blackberry', align = options.align || (ioswp ? 'bottom center' : bb ? 'center right' : 'center center'), position = options.position || (ioswp ? 'bottom center' : bb ? 'center right' : 'center center'), effect = options.effect || (ioswp ? 'slideIn:up' : bb ? 'slideIn:left' : 'fade:in'), shim = $(SHIM).handler(that).hide();
                Widget.fn.init.call(that, element, options);
                that.shim = shim;
                element = that.element;
                options = that.options;
                if (options.className) {
                    that.shim.addClass(options.className);
                }
                if (!options.modal) {
                    that.shim.on('down', '_hide');
                }
                (app ? app.element : $(document.body)).append(shim);
                that.popup = new Popup(that.element, {
                    anchor: shim,
                    modal: true,
                    appendTo: shim,
                    origin: align,
                    position: position,
                    animation: {
                        open: {
                            effects: effect,
                            duration: options.duration
                        },
                        close: { duration: options.duration }
                    },
                    close: function (e) {
                        var prevented = false;
                        if (!that._apiCall) {
                            prevented = that.trigger(HIDE);
                        }
                        if (prevented) {
                            e.preventDefault();
                        }
                        that._apiCall = false;
                    },
                    deactivate: function () {
                        shim.hide();
                    },
                    open: function () {
                        shim.show();
                    }
                });
                kendo.notify(that);
            },
            events: [HIDE],
            options: {
                name: 'Shim',
                modal: false,
                align: undefined,
                position: undefined,
                effect: undefined,
                duration: 200
            },
            show: function () {
                this.popup.open();
            },
            hide: function () {
                this._apiCall = true;
                this.popup.close();
            },
            destroy: function () {
                Widget.fn.destroy.call(this);
                this.shim.kendoDestroy();
                this.popup.destroy();
                this.shim.remove();
            },
            _hide: function (e) {
                if (!e || !$.contains(this.shim.children().children('.k-popup')[0], e.target)) {
                    this.popup.close();
                }
            }
        });
        ui.plugin(Shim);
    }(window.kendo.jQuery));
    return window.kendo;
}, typeof define == 'function' && define.amd ? define : function (a1, a2, a3) {
    (a3 || a2)();
}));