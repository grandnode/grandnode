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
    define('kendo.mobile.modalview', [
        'kendo.mobile.shim',
        'kendo.mobile.view'
    ], f);
}(function () {
    var __meta__ = {
        id: 'mobile.modalview',
        name: 'ModalView',
        category: 'mobile',
        description: 'The Kendo ModalView is used to present self-contained functionality in the context of the current task.',
        depends: [
            'mobile.shim',
            'mobile.view'
        ]
    };
    (function ($, undefined) {
        var kendo = window.kendo, ui = kendo.mobile.ui, Shim = ui.Shim, Widget = ui.Widget, BEFORE_OPEN = 'beforeOpen', OPEN = 'open', CLOSE = 'close', INIT = 'init', WRAP = '<div class="km-modalview-wrapper" />';
        var ModalView = ui.View.extend({
            init: function (element, options) {
                var that = this;
                Widget.fn.init.call(that, element, options);
                that._id();
                that._wrap();
                that._shim();
                if (!this.options.$angular) {
                    that._layout();
                    that._scroller();
                    that._model();
                }
                that.element.css('display', '');
                that.trigger(INIT);
            },
            events: [
                INIT,
                BEFORE_OPEN,
                OPEN,
                CLOSE
            ],
            options: {
                name: 'ModalView',
                modal: true,
                width: null,
                height: null
            },
            destroy: function () {
                Widget.fn.destroy.call(this);
                this.shim.destroy();
            },
            open: function (target) {
                var that = this;
                that.target = $(target);
                that.shim.show();
                that._invokeNgController();
                that.trigger('show', { view: that });
            },
            openFor: function (target) {
                if (!this.trigger(BEFORE_OPEN, { target: target })) {
                    this.open(target);
                    this.trigger(OPEN, { target: target });
                }
            },
            close: function () {
                if (this.element.is(':visible') && !this.trigger(CLOSE)) {
                    this.shim.hide();
                }
            },
            _wrap: function () {
                var that = this, element = that.element, options = that.options, width, height;
                width = element[0].style.width || 'auto';
                height = element[0].style.height || 'auto';
                element.addClass('km-modalview').wrap(WRAP);
                that.wrapper = element.parent().css({
                    width: options.width || width || 300,
                    height: options.height || height || 300
                }).addClass(height == 'auto' ? ' km-auto-height' : '');
                element.css({
                    width: '',
                    height: ''
                });
            },
            _shim: function () {
                var that = this;
                that.shim = new Shim(that.wrapper, {
                    modal: that.options.modal,
                    position: 'center center',
                    align: 'center center',
                    effect: 'fade:in',
                    className: 'km-modalview-root',
                    hide: function (e) {
                        if (that.trigger(CLOSE)) {
                            e.preventDefault();
                        }
                    }
                });
            }
        });
        ui.plugin(ModalView);
    }(window.kendo.jQuery));
    return window.kendo;
}, typeof define == 'function' && define.amd ? define : function (a1, a2, a3) {
    (a3 || a2)();
}));