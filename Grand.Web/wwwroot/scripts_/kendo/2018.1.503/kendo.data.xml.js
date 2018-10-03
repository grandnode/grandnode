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
    define('kendo.data.xml', ['kendo.core'], f);
}(function () {
    var __meta__ = {
        id: 'data.xml',
        name: 'XML',
        category: 'framework',
        depends: ['core'],
        hidden: true
    };
    (function ($, undefined) {
        var kendo = window.kendo, isArray = $.isArray, isPlainObject = $.isPlainObject, map = $.map, each = $.each, extend = $.extend, getter = kendo.getter, Class = kendo.Class;
        var XmlDataReader = Class.extend({
            init: function (options) {
                var that = this, total = options.total, model = options.model, parse = options.parse, errors = options.errors, serialize = options.serialize, data = options.data;
                if (model) {
                    if (isPlainObject(model)) {
                        var base = options.modelBase || kendo.data.Model;
                        if (model.fields) {
                            each(model.fields, function (field, value) {
                                if (isPlainObject(value) && value.field) {
                                    if (!$.isFunction(value.field)) {
                                        value = extend(value, { field: that.getter(value.field) });
                                    }
                                } else {
                                    value = { field: that.getter(value) };
                                }
                                model.fields[field] = value;
                            });
                        }
                        var id = model.id;
                        if (id) {
                            var idField = {};
                            idField[that.xpathToMember(id, true)] = { field: that.getter(id) };
                            model.fields = extend(idField, model.fields);
                            model.id = that.xpathToMember(id);
                        }
                        model = base.define(model);
                    }
                    that.model = model;
                }
                if (total) {
                    if (typeof total == 'string') {
                        total = that.getter(total);
                        that.total = function (data) {
                            return parseInt(total(data), 10);
                        };
                    } else if (typeof total == 'function') {
                        that.total = total;
                    }
                }
                if (errors) {
                    if (typeof errors == 'string') {
                        errors = that.getter(errors);
                        that.errors = function (data) {
                            return errors(data) || null;
                        };
                    } else if (typeof errors == 'function') {
                        that.errors = errors;
                    }
                }
                if (data) {
                    if (typeof data == 'string') {
                        data = that.xpathToMember(data);
                        that.data = function (value) {
                            var result = that.evaluate(value, data), modelInstance;
                            result = isArray(result) ? result : [result];
                            if (that.model && model.fields) {
                                modelInstance = new that.model();
                                return map(result, function (value) {
                                    if (value) {
                                        var record = {}, field;
                                        for (field in model.fields) {
                                            record[field] = modelInstance._parse(field, model.fields[field].field(value));
                                        }
                                        return record;
                                    }
                                });
                            }
                            return result;
                        };
                    } else if (typeof data == 'function') {
                        that.data = data;
                    }
                }
                if (typeof parse == 'function') {
                    var xmlParse = that.parse;
                    that.parse = function (data) {
                        var xml = parse.call(that, data);
                        return xmlParse.call(that, xml);
                    };
                }
                if (typeof serialize == 'function') {
                    that.serialize = serialize;
                }
            },
            total: function (result) {
                return this.data(result).length;
            },
            errors: function (data) {
                return data ? data.errors : null;
            },
            serialize: function (data) {
                return data;
            },
            parseDOM: function (element) {
                var result = {}, parsedNode, node, nodeType, nodeName, member, attribute, attributes = element.attributes, attributeCount = attributes.length, idx;
                for (idx = 0; idx < attributeCount; idx++) {
                    attribute = attributes[idx];
                    result['@' + attribute.nodeName] = attribute.nodeValue;
                }
                for (node = element.firstChild; node; node = node.nextSibling) {
                    nodeType = node.nodeType;
                    if (nodeType === 3 || nodeType === 4) {
                        result['#text'] = node.nodeValue;
                    } else if (nodeType === 1) {
                        parsedNode = this.parseDOM(node);
                        nodeName = node.nodeName;
                        member = result[nodeName];
                        if (isArray(member)) {
                            member.push(parsedNode);
                        } else if (member !== undefined) {
                            member = [
                                member,
                                parsedNode
                            ];
                        } else {
                            member = parsedNode;
                        }
                        result[nodeName] = member;
                    }
                }
                return result;
            },
            evaluate: function (value, expression) {
                var members = expression.split('.'), member, result, length, intermediateResult, idx;
                while (member = members.shift()) {
                    value = value[member];
                    if (isArray(value)) {
                        result = [];
                        expression = members.join('.');
                        for (idx = 0, length = value.length; idx < length; idx++) {
                            intermediateResult = this.evaluate(value[idx], expression);
                            intermediateResult = isArray(intermediateResult) ? intermediateResult : [intermediateResult];
                            result.push.apply(result, intermediateResult);
                        }
                        return result;
                    }
                }
                return value;
            },
            parse: function (xml) {
                var documentElement, tree, result = {};
                documentElement = xml.documentElement || $.parseXML(xml).documentElement;
                tree = this.parseDOM(documentElement);
                result[documentElement.nodeName] = tree;
                return result;
            },
            xpathToMember: function (member, raw) {
                if (!member) {
                    return '';
                }
                member = member.replace(/^\//, '').replace(/\//g, '.');
                if (member.indexOf('@') >= 0) {
                    return member.replace(/\.?(@.*)/, raw ? '$1' : '["$1"]');
                }
                if (member.indexOf('text()') >= 0) {
                    return member.replace(/(\.?text\(\))/, raw ? '#text' : '["#text"]');
                }
                return member;
            },
            getter: function (member) {
                return getter(this.xpathToMember(member), true);
            }
        });
        $.extend(true, kendo.data, {
            XmlDataReader: XmlDataReader,
            readers: { xml: XmlDataReader }
        });
    }(window.kendo.jQuery));
    return window.kendo;
}, typeof define == 'function' && define.amd ? define : function (a1, a2, a3) {
    (a3 || a2)();
}));