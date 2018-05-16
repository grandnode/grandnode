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
    define('kendo.data.signalr', ['kendo.data'], f);
}(function () {
    var __meta__ = {
        id: 'data.signalr',
        name: 'SignalR',
        category: 'framework',
        depends: ['data'],
        hidden: true
    };
    (function ($) {
        var transport = kendo.data.RemoteTransport.extend({
            init: function (options) {
                var signalr = options && options.signalr ? options.signalr : {};
                var promise = signalr.promise;
                if (!promise) {
                    throw new Error('The "promise" option must be set.');
                }
                if (typeof promise.done != 'function' || typeof promise.fail != 'function') {
                    throw new Error('The "promise" option must be a Promise.');
                }
                this.promise = promise;
                var hub = signalr.hub;
                if (!hub) {
                    throw new Error('The "hub" option must be set.');
                }
                if (typeof hub.on != 'function' || typeof hub.invoke != 'function') {
                    throw new Error('The "hub" option is not a valid SignalR hub proxy.');
                }
                this.hub = hub;
                kendo.data.RemoteTransport.fn.init.call(this, options);
            },
            push: function (callbacks) {
                var client = this.options.signalr.client || {};
                if (client.create) {
                    this.hub.on(client.create, callbacks.pushCreate);
                }
                if (client.update) {
                    this.hub.on(client.update, callbacks.pushUpdate);
                }
                if (client.destroy) {
                    this.hub.on(client.destroy, callbacks.pushDestroy);
                }
            },
            _crud: function (options, type) {
                var hub = this.hub;
                var server = this.options.signalr.server;
                if (!server || !server[type]) {
                    throw new Error(kendo.format('The "server.{0}" option must be set.', type));
                }
                var args = [server[type]];
                var data = this.parameterMap(options.data, type);
                if (!$.isEmptyObject(data)) {
                    args.push(data);
                }
                this.promise.done(function () {
                    hub.invoke.apply(hub, args).done(options.success).fail(options.error);
                });
            },
            read: function (options) {
                this._crud(options, 'read');
            },
            create: function (options) {
                this._crud(options, 'create');
            },
            update: function (options) {
                this._crud(options, 'update');
            },
            destroy: function (options) {
                this._crud(options, 'destroy');
            }
        });
        $.extend(true, kendo.data, { transports: { signalr: transport } });
    }(window.kendo.jQuery));
    return window.kendo;
}, typeof define == 'function' && define.amd ? define : function (a1, a2, a3) {
    (a3 || a2)();
}));