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
    define('kendo.angular', ['kendo.core'], f);
}(function () {
    var __meta__ = {
        id: 'angular',
        name: 'AngularJS Directives',
        category: 'framework',
        description: 'Adds Kendo UI for AngularJS directives',
        depends: ['core'],
        defer: true
    };
    (function ($, angular, undefined) {
        'use strict';
        if (!angular || !angular.injector) {
            return;
        }
        var module = angular.module('kendo.directives', []), $injector = angular.injector(['ng']), $parse = $injector.get('$parse'), $timeout = $injector.get('$timeout'), $defaultCompile, $log = $injector.get('$log');
        function withoutTimeout(f) {
            var save = $timeout;
            try {
                $timeout = function (f) {
                    return f();
                };
                return f();
            } finally {
                $timeout = save;
            }
        }
        var OPTIONS_NOW;
        var createDataSource = function () {
            var types = {
                TreeList: 'TreeListDataSource',
                TreeView: 'HierarchicalDataSource',
                Scheduler: 'SchedulerDataSource',
                PivotGrid: 'PivotDataSource',
                PivotConfigurator: 'PivotDataSource',
                PanelBar: 'HierarchicalDataSource',
                Menu: '$PLAIN',
                ContextMenu: '$PLAIN'
            };
            var toDataSource = function (dataSource, type) {
                if (type == '$PLAIN') {
                    return dataSource;
                }
                return kendo.data[type].create(dataSource);
            };
            return function (scope, element, role, source) {
                var type = types[role] || 'DataSource';
                var current = scope.$eval(source);
                var ds = toDataSource(current, type);
                scope.$watch(source, function (mew) {
                    var widget = kendoWidgetInstance(element);
                    if (widget && typeof widget.setDataSource == 'function') {
                        if (mew !== current) {
                            var ds = toDataSource(mew, type);
                            widget.setDataSource(ds);
                            current = mew;
                        }
                    }
                });
                return ds;
            };
        }();
        var ignoredAttributes = {
            kDataSource: true,
            kOptions: true,
            kRebind: true,
            kNgModel: true,
            kNgDelay: true
        };
        var ignoredOwnProperties = {
            name: true,
            title: true,
            style: true
        };
        function createWidget(scope, element, attrs, widget, origAttr, controllers) {
            if (!(element instanceof jQuery)) {
                throw new Error('The Kendo UI directives require jQuery to be available before AngularJS. Please include jquery before angular in the document.');
            }
            var kNgDelay = attrs.kNgDelay, delayValue = scope.$eval(kNgDelay);
            controllers = controllers || [];
            var ngModel = controllers[0], ngForm = controllers[1];
            var ctor = $(element)[widget];
            if (!ctor) {
                window.console.error('Could not find: ' + widget);
                return null;
            }
            var parsed = parseOptions(scope, element, attrs, widget, ctor);
            var options = parsed.options;
            if (parsed.unresolved.length) {
                var promises = [];
                for (var i = 0, len = parsed.unresolved.length; i < len; i++) {
                    var unresolved = parsed.unresolved[i];
                    var promise = $.Deferred(function (d) {
                        var unwatch = scope.$watch(unresolved.path, function (newValue) {
                            if (newValue !== undefined) {
                                unwatch();
                                d.resolve();
                            }
                        });
                    }).promise();
                    promises.push(promise);
                }
                $.when.apply(null, promises).then(createIt);
                return;
            }
            if (kNgDelay && !delayValue) {
                var root = scope.$root || scope;
                var register = function () {
                    var unregister = scope.$watch(kNgDelay, function (newValue) {
                        if (newValue !== undefined) {
                            unregister();
                            element.removeAttr(attrs.$attr.kNgDelay);
                            kNgDelay = null;
                            $timeout(createIt);
                        }
                    });
                };
                if (/^\$(digest|apply)$/.test(root.$$phase)) {
                    register();
                } else {
                    scope.$apply(register);
                }
                return;
            } else {
                return createIt();
            }
            function createIt() {
                var originalElement;
                if (attrs.kRebind) {
                    originalElement = $($(element)[0].cloneNode(true));
                }
                options = parseOptions(scope, element, attrs, widget, ctor).options;
                if (element.is('select')) {
                    (function (options) {
                        if (options.length > 0) {
                            var first = $(options[0]);
                            if (!/\S/.test(first.text()) && /^\?/.test(first.val())) {
                                first.remove();
                            }
                            for (var i = 0; i < options.length; i++) {
                                $(options[i]).off('$destroy');
                            }
                        }
                    }(element[0].options));
                }
                var object = ctor.call(element, OPTIONS_NOW = options).data(widget);
                exposeWidget(object, scope, attrs, widget, origAttr);
                scope.$emit('kendoWidgetCreated', object);
                var destroyRegister = destroyWidgetOnScopeDestroy(scope, object);
                if (attrs.kRebind) {
                    setupRebind(object, scope, element, originalElement, attrs.kRebind, destroyRegister, attrs);
                }
                if (attrs.kNgDisabled) {
                    var kNgDisabled = attrs.kNgDisabled;
                    var isDisabled = scope.$eval(kNgDisabled);
                    if (isDisabled) {
                        object.enable(!isDisabled);
                    }
                    bindToKNgDisabled(object, scope, element, kNgDisabled);
                }
                if (attrs.kNgReadonly) {
                    var kNgReadonly = attrs.kNgReadonly;
                    var isReadonly = scope.$eval(kNgReadonly);
                    if (isReadonly) {
                        object.readonly(isReadonly);
                    }
                    bindToKNgReadonly(object, scope, element, kNgReadonly);
                }
                if (attrs.kNgModel) {
                    bindToKNgModel(object, scope, attrs.kNgModel);
                }
                if (ngModel) {
                    bindToNgModel(object, scope, element, ngModel, ngForm);
                }
                if (object) {
                    propagateClassToWidgetWrapper(object, element);
                }
                return object;
            }
        }
        function parseOptions(scope, element, attrs, widget, ctor) {
            var role = widget.replace(/^kendo/, '');
            var unresolved = [];
            var optionsPath = attrs.kOptions || attrs.options;
            var optionsValue = scope.$eval(optionsPath);
            if (optionsPath && optionsValue === undefined) {
                unresolved.push({
                    option: 'options',
                    path: optionsPath
                });
            }
            var options = angular.extend({}, attrs.defaultOptions, optionsValue);
            function addOption(name, value) {
                var scopeValue = angular.copy(scope.$eval(value));
                if (scopeValue === undefined) {
                    unresolved.push({
                        option: name,
                        path: value
                    });
                } else {
                    options[name] = scopeValue;
                }
            }
            var widgetOptions = ctor.widget.prototype.options;
            var widgetEvents = ctor.widget.prototype.events;
            $.each(attrs, function (name, value) {
                if (name === 'source' || name === 'kDataSource' || name === 'kScopeField' || name === 'scopeField') {
                    return;
                }
                var dataName = 'data' + name.charAt(0).toUpperCase() + name.slice(1);
                if (name.indexOf('on') === 0) {
                    var eventKey = name.replace(/^on./, function (prefix) {
                        return prefix.charAt(2).toLowerCase();
                    });
                    if (widgetEvents.indexOf(eventKey) > -1) {
                        options[eventKey] = value;
                    }
                }
                if (widgetOptions.hasOwnProperty(dataName)) {
                    addOption(dataName, value);
                } else if (widgetOptions.hasOwnProperty(name) && !ignoredOwnProperties[name]) {
                    addOption(name, value);
                } else if (!ignoredAttributes[name]) {
                    var match = name.match(/^k(On)?([A-Z].*)/);
                    if (match) {
                        var optionName = match[2].charAt(0).toLowerCase() + match[2].slice(1);
                        if (match[1] && name != 'kOnLabel') {
                            options[optionName] = value;
                        } else {
                            if (name == 'kOnLabel') {
                                optionName = 'onLabel';
                            }
                            addOption(optionName, value);
                        }
                    }
                }
            });
            var dataSource = attrs.kDataSource || attrs.source;
            if (dataSource) {
                options.dataSource = createDataSource(scope, element, role, dataSource);
            }
            options.$angular = [scope];
            return {
                options: options,
                unresolved: unresolved
            };
        }
        function bindToKNgDisabled(widget, scope, element, kNgDisabled) {
            if (kendo.ui.PanelBar && widget instanceof kendo.ui.PanelBar || kendo.ui.Menu && widget instanceof kendo.ui.Menu) {
                $log.warn('k-ng-disabled specified on a widget that does not have the enable() method: ' + widget.options.name);
                return;
            }
            scope.$watch(kNgDisabled, function (newValue, oldValue) {
                if (newValue != oldValue) {
                    widget.enable(!newValue);
                }
            });
        }
        function bindToKNgReadonly(widget, scope, element, kNgReadonly) {
            if (typeof widget.readonly != 'function') {
                $log.warn('k-ng-readonly specified on a widget that does not have the readonly() method: ' + widget.options.name);
                return;
            }
            scope.$watch(kNgReadonly, function (newValue, oldValue) {
                if (newValue != oldValue) {
                    widget.readonly(newValue);
                }
            });
        }
        function exposeWidget(widget, scope, attrs, kendoWidget, origAttr) {
            if (attrs[origAttr]) {
                var set = $parse(attrs[origAttr]).assign;
                if (set) {
                    set(scope, widget);
                } else {
                    throw new Error(origAttr + ' attribute used but expression in it is not assignable: ' + attrs[kendoWidget]);
                }
            }
        }
        function formValue(element) {
            if (/checkbox|radio/i.test(element.attr('type'))) {
                return element.prop('checked');
            }
            return element.val();
        }
        var formRegExp = /^(input|select|textarea)$/i;
        function isForm(element) {
            return formRegExp.test(element[0].tagName);
        }
        function bindToNgModel(widget, scope, element, ngModel, ngForm) {
            if (!widget.value) {
                return;
            }
            var value;
            var haveChangeOnElement = false;
            if (isForm(element)) {
                value = function () {
                    return formValue(element);
                };
            } else {
                value = function () {
                    return widget.value();
                };
            }
            var viewRender = function () {
                var val = ngModel.$viewValue;
                if (val === undefined) {
                    val = ngModel.$modelValue;
                }
                if (val === undefined) {
                    val = null;
                }
                haveChangeOnElement = true;
                setTimeout(function () {
                    haveChangeOnElement = false;
                    if (widget) {
                        var kNgModel = scope[widget.element.attr('k-ng-model')];
                        if (kNgModel) {
                            val = kNgModel;
                        }
                        if (widget.options.autoBind === false && !widget.listView.bound()) {
                            if (val) {
                                widget.value(val);
                            }
                        } else {
                            widget.value(val);
                        }
                    }
                }, 0);
            };
            ngModel.$render = viewRender;
            setTimeout(function () {
                if (ngModel.$render !== viewRender) {
                    ngModel.$render = viewRender;
                    ngModel.$render();
                }
            });
            if (isForm(element)) {
                element.on('change', function () {
                    haveChangeOnElement = true;
                });
            }
            var onChange = function (pristine) {
                return function () {
                    var formPristine;
                    if (haveChangeOnElement && !element.is('select')) {
                        return;
                    }
                    if (pristine && ngForm) {
                        formPristine = ngForm.$pristine;
                    }
                    ngModel.$setViewValue(value());
                    if (pristine) {
                        ngModel.$setPristine();
                        if (formPristine) {
                            ngForm.$setPristine();
                        }
                    }
                    digest(scope);
                };
            };
            widget.first('change', onChange(false));
            widget.first('spin', onChange(false));
            if (!(kendo.ui.AutoComplete && widget instanceof kendo.ui.AutoComplete)) {
                widget.first('dataBound', onChange(true));
            }
            var currentVal = value();
            if (!isNaN(ngModel.$viewValue) && currentVal != ngModel.$viewValue) {
                if (!ngModel.$isEmpty(ngModel.$viewValue)) {
                    widget.value(ngModel.$viewValue);
                } else if (currentVal != null && currentVal !== '' && currentVal != ngModel.$viewValue) {
                    ngModel.$setViewValue(currentVal);
                }
            }
            ngModel.$setPristine();
        }
        function bindToKNgModel(widget, scope, kNgModel) {
            if (typeof widget.value != 'function') {
                $log.warn('k-ng-model specified on a widget that does not have the value() method: ' + widget.options.name);
                return;
            }
            var form = $(widget.element).parents('form');
            var ngForm = kendo.getter(form.attr('name'), true)(scope);
            var getter = $parse(kNgModel);
            var setter = getter.assign;
            var updating = false;
            var valueIsCollection = kendo.ui.MultiSelect && widget instanceof kendo.ui.MultiSelect;
            var length = function (value) {
                return value && valueIsCollection ? value.length : 0;
            };
            var currentValueLength = length(getter(scope));
            widget.$angular_setLogicValue(getter(scope));
            var watchHandler = function (newValue, oldValue) {
                if (newValue === undefined) {
                    newValue = null;
                }
                if (updating || newValue == oldValue && length(newValue) == currentValueLength) {
                    return;
                }
                currentValueLength = length(newValue);
                widget.$angular_setLogicValue(newValue);
            };
            if (valueIsCollection) {
                scope.$watchCollection(kNgModel, watchHandler);
            } else {
                scope.$watch(kNgModel, watchHandler);
            }
            var changeHandler = function () {
                updating = true;
                if (ngForm && ngForm.$pristine) {
                    ngForm.$setDirty();
                }
                digest(scope, function () {
                    setter(scope, widget.$angular_getLogicValue());
                    currentValueLength = length(getter(scope));
                });
                updating = false;
            };
            widget.first('change', changeHandler);
            widget.first('spin', changeHandler);
        }
        function destroyWidgetOnScopeDestroy(scope, widget) {
            var deregister = scope.$on('$destroy', function () {
                deregister();
                if (widget) {
                    kendo.destroy(widget.element);
                    widget = null;
                }
            });
            return deregister;
        }
        function propagateClassToWidgetWrapper(widget, element) {
            if (!(window.MutationObserver && widget.wrapper)) {
                return;
            }
            var prevClassList = [].slice.call($(element)[0].classList);
            var mo = new MutationObserver(function (changes) {
                suspend();
                if (!widget) {
                    return;
                }
                changes.forEach(function (chg) {
                    var w = $(widget.wrapper)[0];
                    switch (chg.attributeName) {
                    case 'class':
                        var currClassList = [].slice.call(chg.target.classList);
                        currClassList.forEach(function (cls) {
                            if (prevClassList.indexOf(cls) < 0) {
                                w.classList.add(cls);
                                if (kendo.ui.ComboBox && widget instanceof kendo.ui.ComboBox) {
                                    widget.input[0].classList.add(cls);
                                }
                            }
                        });
                        prevClassList.forEach(function (cls) {
                            if (currClassList.indexOf(cls) < 0) {
                                w.classList.remove(cls);
                                if (kendo.ui.ComboBox && widget instanceof kendo.ui.ComboBox) {
                                    widget.input[0].classList.remove(cls);
                                }
                            }
                        });
                        prevClassList = currClassList;
                        break;
                    case 'disabled':
                        if (typeof widget.enable == 'function' && !widget.element.attr('readonly')) {
                            widget.enable(!$(chg.target).attr('disabled'));
                        }
                        break;
                    case 'readonly':
                        if (typeof widget.readonly == 'function' && !widget.element.attr('disabled')) {
                            widget.readonly(!!$(chg.target).attr('readonly'));
                        }
                        break;
                    }
                });
                resume();
            });
            function suspend() {
                mo.disconnect();
            }
            function resume() {
                mo.observe($(element)[0], { attributes: true });
            }
            resume();
            widget.first('destroy', suspend);
        }
        function setupRebind(widget, scope, element, originalElement, rebindAttr, destroyRegister, attrs) {
            var unregister = scope.$watch(rebindAttr, function (newValue, oldValue) {
                if (!widget._muteRebind && newValue !== oldValue) {
                    unregister();
                    if (attrs._cleanUp) {
                        attrs._cleanUp();
                    }
                    var templateOptions = WIDGET_TEMPLATE_OPTIONS[widget.options.name];
                    if (templateOptions) {
                        templateOptions.forEach(function (name) {
                            var templateContents = scope.$eval(attrs['k' + name]);
                            if (templateContents) {
                                originalElement.append($(templateContents).attr(kendo.toHyphens('k' + name), ''));
                            }
                        });
                    }
                    var _wrapper = $(widget.wrapper)[0];
                    var _element = $(widget.element)[0];
                    var isUpload = widget.options.name === 'Upload';
                    if (isUpload) {
                        element = $(_element);
                    }
                    var compile = element.injector().get('$compile');
                    widget._destroy();
                    if (destroyRegister) {
                        destroyRegister();
                    }
                    widget = null;
                    if (_element) {
                        if (_wrapper) {
                            _wrapper.parentNode.replaceChild(_element, _wrapper);
                        }
                        $(element).replaceWith(originalElement);
                    }
                    compile(originalElement)(scope);
                }
            }, true);
            digest(scope);
        }
        function bind(f, obj) {
            return function (a, b) {
                return f.call(obj, a, b);
            };
        }
        function setTemplate(key, value) {
            this[key] = kendo.stringify(value);
        }
        module.factory('directiveFactory', [
            '$compile',
            function (compile) {
                var kendoRenderedTimeout;
                var RENDERED = false;
                $defaultCompile = compile;
                var create = function (role, origAttr) {
                    return {
                        restrict: 'AC',
                        require: [
                            '?ngModel',
                            '^?form'
                        ],
                        scope: false,
                        controller: [
                            '$scope',
                            '$attrs',
                            '$element',
                            function ($scope, $attrs) {
                                this.template = bind(setTemplate, $attrs);
                                $attrs._cleanUp = bind(function () {
                                    this.template = null;
                                    $attrs._cleanUp = null;
                                }, this);
                            }
                        ],
                        link: function (scope, element, attrs, controllers) {
                            var $element = $(element);
                            var roleattr = role.replace(/([A-Z])/g, '-$1');
                            $element.attr(roleattr, $element.attr('data-' + roleattr));
                            $element[0].removeAttribute('data-' + roleattr);
                            var widget = createWidget(scope, element, attrs, role, origAttr, controllers);
                            if (!widget) {
                                return;
                            }
                            if (kendoRenderedTimeout) {
                                clearTimeout(kendoRenderedTimeout);
                            }
                            kendoRenderedTimeout = setTimeout(function () {
                                scope.$emit('kendoRendered');
                                if (!RENDERED) {
                                    RENDERED = true;
                                    $('form').each(function () {
                                        var form = $(this).controller('form');
                                        if (form) {
                                            form.$setPristine();
                                        }
                                    });
                                }
                            });
                        }
                    };
                };
                return { create: create };
            }
        ]);
        var TAGNAMES = {
            Editor: 'textarea',
            NumericTextBox: 'input',
            DatePicker: 'input',
            DateTimePicker: 'input',
            TimePicker: 'input',
            AutoComplete: 'input',
            ColorPicker: 'input',
            MaskedTextBox: 'input',
            MultiSelect: 'input',
            Upload: 'input',
            Validator: 'form',
            Button: 'button',
            MobileButton: 'a',
            MobileBackButton: 'a',
            MobileDetailButton: 'a',
            ListView: 'ul',
            MobileListView: 'ul',
            PanelBar: 'ul',
            TreeView: 'ul',
            Menu: 'ul',
            ContextMenu: 'ul',
            ActionSheet: 'ul'
        };
        var SKIP_SHORTCUTS = [
            'MobileView',
            'MobileDrawer',
            'MobileLayout',
            'MobileSplitView',
            'MobilePane',
            'MobileModalView'
        ];
        var MANUAL_DIRECTIVES = [
            'MobileApplication',
            'MobileView',
            'MobileModalView',
            'MobileLayout',
            'MobileActionSheet',
            'MobileDrawer',
            'MobileSplitView',
            'MobilePane',
            'MobileScrollView',
            'MobilePopOver'
        ];
        angular.forEach([
            'MobileNavBar',
            'MobileButton',
            'MobileBackButton',
            'MobileDetailButton',
            'MobileTabStrip',
            'MobileScrollView',
            'MobileScroller'
        ], function (widget) {
            MANUAL_DIRECTIVES.push(widget);
            widget = 'kendo' + widget;
            module.directive(widget, function () {
                return {
                    restrict: 'A',
                    link: function (scope, element, attrs) {
                        createWidget(scope, element, attrs, widget, widget);
                    }
                };
            });
        });
        function createDirectives(klass, isMobile) {
            function make(directiveName, widgetName) {
                module.directive(directiveName, [
                    'directiveFactory',
                    function (directiveFactory) {
                        return directiveFactory.create(widgetName, directiveName);
                    }
                ]);
            }
            var name = isMobile ? 'Mobile' : '';
            name += klass.fn.options.name;
            var className = name;
            var shortcut = 'kendo' + name.charAt(0) + name.substr(1).toLowerCase();
            name = 'kendo' + name;
            var dashed = name.replace(/([A-Z])/g, '-$1');
            if (SKIP_SHORTCUTS.indexOf(name.replace('kendo', '')) == -1) {
                var names = name === shortcut ? [name] : [
                    name,
                    shortcut
                ];
                angular.forEach(names, function (directiveName) {
                    module.directive(directiveName, function () {
                        return {
                            restrict: 'E',
                            replace: true,
                            template: function (element, attributes) {
                                var tag = TAGNAMES[className] || 'div';
                                var scopeField = attributes.kScopeField || attributes.scopeField;
                                return '<' + tag + ' ' + dashed + (scopeField ? '="' + scopeField + '"' : '') + '>' + element.html() + '</' + tag + '>';
                            }
                        };
                    });
                });
            }
            if (MANUAL_DIRECTIVES.indexOf(name.replace('kendo', '')) > -1) {
                return;
            }
            make(name, name);
            if (shortcut != name) {
                make(shortcut, name);
            }
        }
        function kendoWidgetInstance(el) {
            el = $(el);
            return kendo.widgetInstance(el, kendo.ui) || kendo.widgetInstance(el, kendo.mobile.ui) || kendo.widgetInstance(el, kendo.dataviz.ui);
        }
        function digest(scope, func) {
            var root = scope.$root || scope;
            var isDigesting = /^\$(digest|apply)$/.test(root.$$phase);
            if (func) {
                if (isDigesting) {
                    func();
                } else {
                    root.$apply(func);
                }
            } else if (!isDigesting) {
                root.$digest();
            }
        }
        function destroyScope(scope, el) {
            scope.$destroy();
            if (el) {
                $(el).removeData('$scope').removeData('$$kendoScope').removeData('$isolateScope').removeData('$isolateScopeNoTemplate').removeClass('ng-scope');
            }
        }
        var pendingPatches = [];
        function defadvice(klass, methodName, func) {
            if ($.isArray(klass)) {
                return angular.forEach(klass, function (klass) {
                    defadvice(klass, methodName, func);
                });
            }
            if (typeof klass == 'string') {
                var a = klass.split('.');
                var x = kendo;
                while (x && a.length > 0) {
                    x = x[a.shift()];
                }
                if (!x) {
                    pendingPatches.push([
                        klass,
                        methodName,
                        func
                    ]);
                    return false;
                }
                klass = x.prototype;
            }
            var origMethod = klass[methodName];
            klass[methodName] = function () {
                var self = this, args = arguments;
                return func.apply({
                    self: self,
                    next: function () {
                        return origMethod.apply(self, arguments.length > 0 ? arguments : args);
                    }
                }, args);
            };
            return true;
        }
        kendo.onWidgetRegistered(function (entry) {
            pendingPatches = $.grep(pendingPatches, function (args) {
                return !defadvice.apply(null, args);
            });
            createDirectives(entry.widget, entry.prefix == 'Mobile');
        });
        defadvice([
            'ui.Widget',
            'mobile.ui.Widget'
        ], 'angular', function (cmd, arg) {
            var self = this.self;
            if (cmd == 'init') {
                if (!arg && OPTIONS_NOW) {
                    arg = OPTIONS_NOW;
                }
                OPTIONS_NOW = null;
                if (arg && arg.$angular) {
                    self.$angular_scope = arg.$angular[0];
                    self.$angular_init(self.element, arg);
                }
                return;
            }
            var scope = self.$angular_scope;
            if (scope) {
                withoutTimeout(function () {
                    var x = arg(), elements = x.elements, data = x.data;
                    if (elements.length > 0) {
                        switch (cmd) {
                        case 'cleanup':
                            angular.forEach(elements, function (el) {
                                var itemScope = $(el).data('$$kendoScope');
                                if (itemScope && itemScope !== scope && itemScope.$$kendoScope) {
                                    destroyScope(itemScope, el);
                                }
                            });
                            break;
                        case 'compile':
                            var injector = self.element.injector();
                            var compile = injector ? injector.get('$compile') : $defaultCompile;
                            angular.forEach(elements, function (el, i) {
                                var itemScope;
                                if (x.scopeFrom) {
                                    itemScope = x.scopeFrom;
                                } else {
                                    var vars = data && data[i];
                                    if (vars !== undefined) {
                                        itemScope = $.extend(scope.$new(), vars);
                                        itemScope.$$kendoScope = true;
                                    } else {
                                        itemScope = scope;
                                    }
                                }
                                $(el).data('$$kendoScope', itemScope);
                                compile(el)(itemScope);
                            });
                            digest(scope);
                            break;
                        }
                    }
                });
            }
        });
        defadvice('ui.Widget', '$angular_getLogicValue', function () {
            return this.self.value();
        });
        defadvice('ui.Widget', '$angular_setLogicValue', function (val) {
            this.self.value(val);
        });
        defadvice('ui.Select', '$angular_getLogicValue', function () {
            var item = this.self.dataItem(), valueField = this.self.options.dataValueField;
            if (item) {
                if (this.self.options.valuePrimitive) {
                    if (!!valueField) {
                        return item[valueField];
                    } else {
                        return item;
                    }
                } else {
                    return item.toJSON();
                }
            } else {
                return null;
            }
        });
        defadvice('ui.Select', '$angular_setLogicValue', function (val) {
            var self = this.self;
            var options = self.options;
            var valueField = options.dataValueField;
            var text = options.text || '';
            if (val === undefined) {
                val = '';
            }
            if (valueField && !options.valuePrimitive && val) {
                text = val[options.dataTextField] || '';
                val = val[valueField || options.dataTextField];
            }
            if (self.options.autoBind === false && !self.listView.bound()) {
                if (!text && val && options.valuePrimitive) {
                    self.value(val);
                } else {
                    self._preselect(val, text);
                }
            } else {
                self.value(val);
            }
        });
        defadvice('ui.MultiSelect', '$angular_getLogicValue', function () {
            var value = this.self.dataItems().slice(0);
            var valueField = this.self.options.dataValueField;
            if (valueField && this.self.options.valuePrimitive) {
                value = $.map(value, function (item) {
                    return item[valueField];
                });
            }
            return value;
        });
        defadvice('ui.MultiSelect', '$angular_setLogicValue', function (val) {
            if (val == null) {
                val = [];
            }
            var self = this.self;
            var options = self.options;
            var valueField = options.dataValueField;
            var data = val;
            if (valueField && !options.valuePrimitive) {
                val = $.map(val, function (item) {
                    return item[valueField];
                });
            }
            if (options.autoBind === false && !options.valuePrimitive && !self.listView.bound()) {
                self._preselect(data, val);
            } else {
                self.value(val);
            }
        });
        defadvice('ui.Widget', '$angular_init', function (element, options) {
            var self = this.self;
            if (options && !$.isArray(options)) {
                var scope = self.$angular_scope;
                for (var i = self.events.length; --i >= 0;) {
                    var event = self.events[i];
                    var handler = options[event];
                    if (handler && typeof handler == 'string') {
                        options[event] = self.$angular_makeEventHandler(event, scope, handler);
                    }
                }
            }
        });
        defadvice('ui.Widget', '$angular_makeEventHandler', function (event, scope, handler) {
            handler = $parse(handler);
            return function (e) {
                digest(scope, function () {
                    handler(scope, { kendoEvent: e });
                });
            };
        });
        defadvice([
            'ui.Grid',
            'ui.ListView',
            'ui.TreeView',
            'ui.PanelBar'
        ], '$angular_makeEventHandler', function (event, scope, handler) {
            if (event != 'change') {
                return this.next();
            }
            handler = $parse(handler);
            return function (ev) {
                var widget = ev.sender;
                var options = widget.options;
                var cell, multiple, locals = { kendoEvent: ev }, elems, items, columns, colIdx;
                if (angular.isString(options.selectable)) {
                    cell = options.selectable.indexOf('cell') !== -1;
                    multiple = options.selectable.indexOf('multiple') !== -1;
                }
                if (widget._checkBoxSelection) {
                    multiple = true;
                }
                elems = locals.selected = this.select();
                items = locals.data = [];
                columns = locals.columns = [];
                for (var i = 0; i < elems.length; i++) {
                    var item = cell ? elems[i].parentNode : elems[i];
                    var dataItem = widget.dataItem(item);
                    if (cell) {
                        if (angular.element.inArray(dataItem, items) < 0) {
                            items.push(dataItem);
                        }
                        colIdx = angular.element(elems[i]).index();
                        if (angular.element.inArray(colIdx, columns) < 0) {
                            columns.push(colIdx);
                        }
                    } else {
                        items.push(dataItem);
                    }
                }
                if (!multiple) {
                    locals.dataItem = locals.data = items[0];
                    locals.angularDataItem = kendo.proxyModelSetters(locals.dataItem);
                    locals.selected = elems[0];
                }
                digest(scope, function () {
                    handler(scope, locals);
                });
            };
        });
        defadvice('ui.Grid', '$angular_init', function (element, options) {
            this.next();
            if (options.columns) {
                var settings = $.extend({}, kendo.Template, options.templateSettings);
                angular.forEach(options.columns, function (col) {
                    if (col.field && !col.template && !col.format && !col.values && (col.encoded === undefined || col.encoded)) {
                        col.template = '<span ng-bind=\'' + kendo.expr(col.field, 'dataItem') + '\'>#: ' + kendo.expr(col.field, settings.paramName) + '#</span>';
                    }
                });
            }
        });
        {
            defadvice('mobile.ui.ButtonGroup', 'value', function (mew) {
                var self = this.self;
                if (mew != null) {
                    self.select(self.element.children('li.km-button').eq(mew));
                    self.trigger('change');
                    self.trigger('select', { index: self.selectedIndex });
                }
                return self.selectedIndex;
            });
            defadvice('mobile.ui.ButtonGroup', '_select', function () {
                this.next();
                this.self.trigger('change');
            });
        }
        module.directive('kendoMobileApplication', function () {
            return {
                terminal: true,
                link: function (scope, element, attrs) {
                    createWidget(scope, element, attrs, 'kendoMobileApplication', 'kendoMobileApplication');
                }
            };
        }).directive('kendoMobileView', function () {
            return {
                scope: true,
                link: {
                    pre: function (scope, element, attrs) {
                        attrs.defaultOptions = scope.viewOptions;
                        attrs._instance = createWidget(scope, element, attrs, 'kendoMobileView', 'kendoMobileView');
                    },
                    post: function (scope, element, attrs) {
                        attrs._instance._layout();
                        attrs._instance._scroller();
                    }
                }
            };
        }).directive('kendoMobileDrawer', function () {
            return {
                scope: true,
                link: {
                    pre: function (scope, element, attrs) {
                        attrs.defaultOptions = scope.viewOptions;
                        attrs._instance = createWidget(scope, element, attrs, 'kendoMobileDrawer', 'kendoMobileDrawer');
                    },
                    post: function (scope, element, attrs) {
                        attrs._instance._layout();
                        attrs._instance._scroller();
                    }
                }
            };
        }).directive('kendoMobileModalView', function () {
            return {
                scope: true,
                link: {
                    pre: function (scope, element, attrs) {
                        attrs.defaultOptions = scope.viewOptions;
                        attrs._instance = createWidget(scope, element, attrs, 'kendoMobileModalView', 'kendoMobileModalView');
                    },
                    post: function (scope, element, attrs) {
                        attrs._instance._layout();
                        attrs._instance._scroller();
                    }
                }
            };
        }).directive('kendoMobileSplitView', function () {
            return {
                terminal: true,
                link: {
                    pre: function (scope, element, attrs) {
                        attrs.defaultOptions = scope.viewOptions;
                        attrs._instance = createWidget(scope, element, attrs, 'kendoMobileSplitView', 'kendoMobileSplitView');
                    },
                    post: function (scope, element, attrs) {
                        attrs._instance._layout();
                    }
                }
            };
        }).directive('kendoMobilePane', function () {
            return {
                terminal: true,
                link: {
                    pre: function (scope, element, attrs) {
                        attrs.defaultOptions = scope.viewOptions;
                        createWidget(scope, element, attrs, 'kendoMobilePane', 'kendoMobilePane');
                    }
                }
            };
        }).directive('kendoMobileLayout', function () {
            return {
                link: {
                    pre: function (scope, element, attrs) {
                        createWidget(scope, element, attrs, 'kendoMobileLayout', 'kendoMobileLayout');
                    }
                }
            };
        }).directive('kendoMobileActionSheet', function () {
            return {
                restrict: 'A',
                link: function (scope, element, attrs) {
                    element.find('a[k-action]').each(function () {
                        $(this).attr('data-' + kendo.ns + 'action', $(this).attr('k-action'));
                    });
                    createWidget(scope, element, attrs, 'kendoMobileActionSheet', 'kendoMobileActionSheet');
                }
            };
        }).directive('kendoMobilePopOver', function () {
            return {
                terminal: true,
                link: {
                    pre: function (scope, element, attrs) {
                        attrs.defaultOptions = scope.viewOptions;
                        createWidget(scope, element, attrs, 'kendoMobilePopOver', 'kendoMobilePopOver');
                    }
                }
            };
        }).directive('kendoViewTitle', function () {
            return {
                restrict: 'E',
                replace: true,
                template: function (element) {
                    return '<span data-' + kendo.ns + 'role=\'view-title\'>' + element.html() + '</span>';
                }
            };
        }).directive('kendoMobileHeader', function () {
            return {
                restrict: 'E',
                link: function (scope, element) {
                    element.addClass('km-header').attr('data-role', 'header');
                }
            };
        }).directive('kendoMobileFooter', function () {
            return {
                restrict: 'E',
                link: function (scope, element) {
                    element.addClass('km-footer').attr('data-role', 'footer');
                }
            };
        }).directive('kendoMobileScrollViewPage', function () {
            return {
                restrict: 'E',
                replace: true,
                template: function (element) {
                    return '<div data-' + kendo.ns + 'role=\'page\'>' + element.html() + '</div>';
                }
            };
        });
        angular.forEach([
            'align',
            'icon',
            'rel',
            'transition',
            'actionsheetContext'
        ], function (attr) {
            var kAttr = 'k' + attr.slice(0, 1).toUpperCase() + attr.slice(1);
            module.directive(kAttr, function () {
                return {
                    restrict: 'A',
                    priority: 2,
                    link: function (scope, element, attrs) {
                        element.attr(kendo.attr(kendo.toHyphens(attr)), scope.$eval(attrs[kAttr]));
                    }
                };
            });
        });
        var WIDGET_TEMPLATE_OPTIONS = {
            'TreeMap': ['Template'],
            'MobileListView': [
                'HeaderTemplate',
                'Template'
            ],
            'MobileScrollView': [
                'EmptyTemplate',
                'Template'
            ],
            'Grid': [
                'AltRowTemplate',
                'DetailTemplate',
                'RowTemplate'
            ],
            'ListView': [
                'EditTemplate',
                'Template',
                'AltTemplate'
            ],
            'Pager': [
                'SelectTemplate',
                'LinkTemplate'
            ],
            'PivotGrid': [
                'ColumnHeaderTemplate',
                'DataCellTemplate',
                'RowHeaderTemplate'
            ],
            'Scheduler': [
                'AllDayEventTemplate',
                'DateHeaderTemplate',
                'EventTemplate',
                'MajorTimeHeaderTemplate',
                'MinorTimeHeaderTemplate'
            ],
            'PanelBar': ['Template'],
            'TreeView': ['Template'],
            'Validator': ['ErrorTemplate']
        };
        (function () {
            var templateDirectives = {};
            angular.forEach(WIDGET_TEMPLATE_OPTIONS, function (templates, widget) {
                angular.forEach(templates, function (template) {
                    if (!templateDirectives[template]) {
                        templateDirectives[template] = [];
                    }
                    templateDirectives[template].push('?^^kendo' + widget);
                });
            });
            angular.forEach(templateDirectives, function (parents, directive) {
                var templateName = 'k' + directive;
                var attrName = kendo.toHyphens(templateName);
                module.directive(templateName, function () {
                    return {
                        restrict: 'A',
                        require: parents,
                        terminal: true,
                        compile: function ($element, $attrs) {
                            if ($attrs[templateName] !== '') {
                                return;
                            }
                            $element.removeAttr(attrName);
                            var template = $element[0].outerHTML;
                            return function (scope, element, attrs, controllers) {
                                var controller;
                                while (!controller && controllers.length) {
                                    controller = controllers.shift();
                                }
                                if (!controller) {
                                    $log.warn(attrName + ' without a matching parent widget found. It can be one of the following: ' + parents.join(', '));
                                } else {
                                    controller.template(templateName, template);
                                    element.remove();
                                }
                            };
                        }
                    };
                });
            });
        }());
    }(window.kendo.jQuery, window.angular));
    return window.kendo;
}, typeof define == 'function' && define.amd ? define : function (a1, a2, a3) {
    (a3 || a2)();
}));