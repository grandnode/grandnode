﻿/*!
 * BootstrapVueIcons 2.20.1
 *
 * @link https://bootstrap-vue.org
 * @source https://github.com/bootstrap-vue/bootstrap-vue
 * @copyright (c) 2016-2020 BootstrapVue
 * @license MIT
 * https://github.com/bootstrap-vue/bootstrap-vue/blob/master/LICENSE
 */
var a, l;
(a = this),
    (l = function (a) {
        "use strict";
        function l(a) {
            return a && "object" == typeof a && "default" in a ? a : { default: a };
        }
        var e = l(a);
        function h(a) {
            return (h =
                "function" == typeof Symbol && "symbol" == typeof Symbol.iterator
                    ? function (a) {
                        return typeof a;
                    }
                    : function (a) {
                        return a && "function" == typeof Symbol && a.constructor === Symbol && a !== Symbol.prototype ? "symbol" : typeof a;
                    })(a);
        }
        function d(a, l, e) {
            return l in a ? Object.defineProperty(a, l, { value: e, enumerable: !0, configurable: !0, writable: !0 }) : (a[l] = e), a;
        }
        function v(a, l) {
            var e = Object.keys(a);
            if (Object.getOwnPropertySymbols) {
                var h = Object.getOwnPropertySymbols(a);
                l &&
                    (h = h.filter(function (l) {
                        return Object.getOwnPropertyDescriptor(a, l).enumerable;
                    })),
                    e.push.apply(e, h);
            }
            return e;
        }
        function o(a) {
            for (var l = 1; l < arguments.length; l++) {
                var e = null != arguments[l] ? arguments[l] : {};
                l % 2
                    ? v(Object(e), !0).forEach(function (l) {
                        d(a, l, e[l]);
                    })
                    : Object.getOwnPropertyDescriptors
                        ? Object.defineProperties(a, Object.getOwnPropertyDescriptors(e))
                        : v(Object(e)).forEach(function (l) {
                            Object.defineProperty(a, l, Object.getOwnPropertyDescriptor(e, l));
                        });
            }
            return a;
        }
        function z(a) {
            return (
                (function (a) {
                    if (Array.isArray(a)) return n(a);
                })(a) ||
                (function (a) {
                    if ("undefined" != typeof Symbol && Symbol.iterator in Object(a)) return Array.from(a);
                })(a) ||
                (function (a, l) {
                    if (a) {
                        if ("string" == typeof a) return n(a, l);
                        var e = Object.prototype.toString.call(a).slice(8, -1);
                        return "Object" === e && a.constructor && (e = a.constructor.name), "Map" === e || "Set" === e ? Array.from(a) : "Arguments" === e || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(e) ? n(a, l) : void 0;
                    }
                })(a) ||
                (function () {
                    throw new TypeError("Invalid attempt to spread non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method.");
                })()
            );
        }
        function n(a, l) {
            (null == l || l > a.length) && (l = a.length);
            for (var e = 0, h = new Array(l); e < l; e++) h[e] = a[e];
            return h;
        }
        var c = function () {
            return (c =
                Object.assign ||
                function (a) {
                    for (var l, e = 1, h = arguments.length; e < h; e++) for (var d in (l = arguments[e])) Object.prototype.hasOwnProperty.call(l, d) && (a[d] = l[d]);
                    return a;
                }).apply(this, arguments);
        },
            i = /-(\w)/g,
            r = /:(.*)/,
            t = /;(?![^(]*\))/g;
        function M(a, l) {
            return l ? l.toUpperCase() : "";
        }
        function V(a) {
            for (var l, e = {}, h = 0, d = a.split(t); h < d.length; h++) {
                var v = d[h].split(r),
                    o = v[0],
                    z = v[1];
                (o = o.trim()) && ("string" == typeof z && (z = z.trim()), (e[((l = o), l.replace(i, M))] = z));
            }
            return e;
        }
        function p() {
            for (var a, l, e = {}, h = arguments.length; h--;)
                for (var d = 0, v = Object.keys(arguments[h]); d < v.length; d++)
                    switch ((a = v[d])) {
                        case "class":
                        case "style":
                        case "directives":
                            if ((Array.isArray(e[a]) || (e[a] = []), "style" === a)) {
                                var o = void 0;
                                o = Array.isArray(arguments[h].style) ? arguments[h].style : [arguments[h].style];
                                for (var z = 0; z < o.length; z++) {
                                    var n = o[z];
                                    "string" == typeof n && (o[z] = V(n));
                                }
                                arguments[h].style = o;
                            }
                            e[a] = e[a].concat(arguments[h][a]);
                            break;
                        case "staticClass":
                            if (!arguments[h][a]) break;
                            void 0 === e[a] && (e[a] = ""), e[a] && (e[a] += " "), (e[a] += arguments[h][a].trim());
                            break;
                        case "on":
                        case "nativeOn":
                            e[a] || (e[a] = {});
                            for (var i = 0, r = Object.keys(arguments[h][a] || {}); i < r.length; i++) (l = r[i]), e[a][l] ? (e[a][l] = [].concat(e[a][l], arguments[h][a][l])) : (e[a][l] = arguments[h][a][l]);
                            break;
                        case "attrs":
                        case "props":
                        case "domProps":
                        case "scopedSlots":
                        case "staticStyle":
                        case "hook":
                        case "transition":
                            e[a] || (e[a] = {}), (e[a] = c({}, arguments[h][a], e[a]));
                            break;
                        case "slot":
                        case "key":
                        case "ref":
                        case "tag":
                        case "show":
                        case "keepAlive":
                        default:
                            e[a] || (e[a] = arguments[h][a]);
                    }
            return e;
        }
        var H,
            u,
            m,
            A,
            f = "undefined" != typeof window,
            B = "undefined" != typeof document,
            L = "undefined" != typeof navigator,
            I = f && B && L,
            F = f ? window : {},
            s = L ? navigator : {},
            C = (s.USER_AGENT || "").toLowerCase(),
            k = C.indexOf("jsdom") > 0,
            S =
                (/msie|trident/.test(C),
                    (function () {
                        if (I)
                            try {
                                var a = {
                                    get passive() {
                                        !0;
                                    },
                                };
                                F.addEventListener("test", a, a), F.removeEventListener("test", a, a);
                            } catch (a) {
                                !1;
                            }
                    })(),
                    I && "IntersectionObserver" in F && "IntersectionObserverEntry" in F && F.IntersectionObserverEntry.prototype,
                    /\B([A-Z])/g),
            g = /-(\w)/g,
            w = /^BIcon/,
            E = function (a) {
                return void 0 === a;
            },
            y = function (a) {
                return (
                    E(a) ||
                    (function (a) {
                        return null === a;
                    })(a)
                );
            },
            D = function (a) {
                return (
                    "function" ===
                    (function (a) {
                        return h(a);
                    })(a)
                );
            },
            P = function (a) {
                return Array.isArray(a);
            },
            x = function (a) {
                return null !== a && "object" === h(a);
            },
            T = function (a) {
                return "[object Object]" === Object.prototype.toString.call(a);
            },
            R = function (a) {
                return Object.keys(a);
            },
            b = function (a, l) {
                return R(a)
                    .filter(function (a) {
                        return -1 === l.indexOf(a);
                    })
                    .reduce(function (l, e) {
                        return o(o({}, l), {}, d({}, e, a[e]));
                    }, {});
            },
            U = function a(l) {
                var e = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : l;
                return P(l)
                    ? l.reduce(function (l, e) {
                        return [].concat(z(l), [a(e, e)]);
                    }, [])
                    : T(l)
                        ? R(l).reduce(function (e, h) {
                            return o(o({}, e), {}, d({}, h, a(l[h], l[h])));
                        }, {})
                        : e;
            },
            q = function (a) {
                return a;
            },
            O = function (a) {
                var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : null,
                    e = ("undefined" != typeof process && process && process.env) || {};
                return a ? e[a] || l : e;
            },
            G = function () {
                return O("BOOTSTRAP_VUE_NO_WARN") || "production" === O("NODE_ENV");
            },
            j =
                ((H = !1),
                    (u = ["Multiple instances of Vue detected!", "You may need to set up an alias for Vue in your bundler config.", "See: https://bootstrap-vue.org/docs#using-module-bundlers"].join("\n")),
                    function (a) {
                        H ||
                            e.default === a ||
                            k ||
                            (function (a) {
                                var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : null;
                                G() || console.warn("[BootstrapVue warn]: ".concat(l ? "".concat(l, " - ") : "").concat(a));
                            })(u),
                            (H = !0);
                    }),
            W = function () {
                var a = arguments.length > 0 && void 0 !== arguments[0] ? arguments[0] : {},
                    l = a.components,
                    e = a.directives,
                    h = a.plugins,
                    d = function a(d) {
                        a.installed || ((a.installed = !0), j(d), Q(d, l), K(d, e), N(d, h));
                    };
                return (d.installed = !1), d;
            },
            X = function () {
                var a = arguments.length > 0 && void 0 !== arguments[0] ? arguments[0] : {},
                    l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : {};
                return o(o({}, l), {}, { install: W(a) });
            },
            N = function (a) {
                var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : {};
                for (var e in l) e && l[e] && a.use(l[e]);
            },
            J = function (a, l, e) {
                a && l && e && a.component(l, e);
            },
            Q = function (a) {
                var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : {};
                for (var e in l) J(a, e, l[e]);
            },
            Z = function (a, l, e) {
                a && l && e && a.directive(l.replace(/^VB/, "B"), e);
            },
            K = function (a) {
                var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : {};
                for (var e in l) Z(a, e, l[e]);
            },
            $ = void 0,
            _ = Boolean,
            Y = Number,
            aa = String,
            la = [Y, aa],
            ea = e.default.prototype,
            ha = function (a) {
                var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : void 0,
                    e = ea.$bvConfig;
                return e ? e.getConfigValue(a, l) : U(l);
            },
            da = function (a) {
                return a.replace(S, "-$1").toLowerCase();
            },
            va = function (a) {
                return (
                    (a = da(a).replace(g, function (a, l) {
                        return l ? l.toUpperCase() : "";
                    }))
                        .charAt(0)
                        .toUpperCase() + a.slice(1)
                );
            },
            oa = function (a) {
                return (function (a) {
                    var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : 2;
                    return y(a) ? "" : P(a) || (T(a) && a.toString === Object.prototype.toString) ? JSON.stringify(a, null, l) : String(a);
                })(a).trim();
            },
            za = function () {
                var a = arguments.length > 0 && void 0 !== arguments[0] ? arguments[0] : $,
                    l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : void 0,
                    e = arguments.length > 2 && void 0 !== arguments[2] ? arguments[2] : void 0,
                    h = arguments.length > 3 && void 0 !== arguments[3] ? arguments[3] : void 0,
                    d = !0 === e;
                return (
                    (h = d ? h : e),
                    o(
                        o(
                            o({}, a ? { type: a } : {}),
                            d
                                ? { required: d }
                                : E(l)
                                    ? {}
                                    : {
                                        default: x(l)
                                            ? function () {
                                                return l;
                                            }
                                            : l,
                                    }
                        ),
                        E(h) ? {} : { validator: h }
                    )
                );
            },
            na = function (a, l, e) {
                return o(
                    o({}, U(a)),
                    {},
                    {
                        default: function () {
                            var h = (function (a) {
                                var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : null,
                                    e = arguments.length > 2 && void 0 !== arguments[2] ? arguments[2] : void 0;
                                return l ? ha("".concat(a, ".").concat(l), e) : ha(a, {});
                            })(e, l, a.default);
                            return D(h) ? h() : h;
                        },
                    }
                );
            },
            ca = function (a, l) {
                return R(a).reduce(function (e, h) {
                    return o(o({}, e), {}, d({}, h, na(a[h], h, l)));
                }, {});
            },
            ia = (na({}, "", "").default.name, "BIcon"),
            ra = "BIconstack",
            ta = Math.max,
            Ma = function (a) {
                var l = arguments.length > 1 && void 0 !== arguments[1] ? arguments[1] : NaN,
                    e = parseFloat(a);
                return isNaN(e) ? l : e;
            },
            Va = { viewBox: "0 0 16 16", width: "1em", height: "1em", focusable: "false", role: "img", "aria-label": "icon" },
            pa = { width: null, height: null, focusable: null, role: null, "aria-label": null },
            Ha = {
                animation: za(aa),
                content: za(aa),
                flipH: za(_, !1),
                flipV: za(_, !1),
                fontScale: za(la, 1),
                rotate: za(la, 0),
                scale: za(la, 1),
                shiftH: za(la, 0),
                shiftV: za(la, 0),
                stacked: za(_, !1),
                title: za(aa),
                variant: za(aa),
            },
            ua = e.default.extend({
                name: "BIconBase",
                functional: !0,
                props: Ha,
                render: function (a, l) {
                    var e,
                        h = l.data,
                        v = l.props,
                        o = l.children,
                        z = v.animation,
                        n = v.content,
                        c = v.flipH,
                        i = v.flipV,
                        r = v.stacked,
                        t = v.title,
                        M = v.variant,
                        V = ta(Ma(v.fontScale, 1), 0) || 1,
                        H = ta(Ma(v.scale, 1), 0) || 1,
                        u = Ma(v.rotate, 0),
                        m = Ma(v.shiftH, 0),
                        A = Ma(v.shiftV, 0),
                        f = c || i || 1 !== H,
                        B = f || u,
                        L = m || A,
                        I = !y(n),
                        F = a(
                            "g",
                            {
                                attrs: {
                                    transform:
                                        [B ? "translate(8 8)" : null, f ? "scale(".concat((c ? -1 : 1) * H, " ").concat((i ? -1 : 1) * H, ")") : null, u ? "rotate(".concat(u, ")") : null, B ? "translate(-8 -8)" : null]
                                            .filter(q)
                                            .join(" ") || null,
                                },
                                domProps: I ? { innerHTML: n || "" } : {},
                            },
                            o
                        );
                    L && (F = a("g", { attrs: { transform: "translate(".concat((16 * m) / 16, " ").concat((-16 * A) / 16, ")") } }, [F])), r && (F = a("g", {}, [F]));
                    var s = t ? a("title", t) : null;
                    return a(
                        "svg",
                        p(
                            { staticClass: "b-icon bi", class: ((e = {}), d(e, "text-".concat(M), M), d(e, "b-icon-animation-".concat(z), z), e), attrs: Va, style: r ? {} : { fontSize: 1 === V ? null : "".concat(100 * V, "%") } },
                            h,
                            r ? { attrs: pa } : {},
                            { attrs: { xmlns: r ? null : "http://www.w3.org/2000/svg", fill: "currentColor" } }
                        ),
                        [s, F]
                    );
                },
            }),
            ma = function (a, l) {
                var h = da(a),
                    d = "BIcon".concat(va(a)),
                    v = "bi-".concat(h),
                    z = h.replace(/-/g, " "),
                    n = oa(l || "");
                return e.default.extend({
                    name: d,
                    functional: !0,
                    props: o(o({}, b(Ha, ["content", "stacked"])), {}, { stacked: za(_, !1) }),
                    render: function (a, l) {
                        var e = l.data,
                            h = l.props;
                        return a(ua, p({ props: { title: z }, attrs: { "aria-label": z } }, e, { staticClass: v, props: o(o({}, h), {}, { content: n }) }));
                    },
                });
            },
            Aa = ma("Blank", ""),
            td = ma(
                "Calendar2Check",
                '<path fill-rule="evenodd" d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5zM2 2a1 1 0 0 0-1 1v11a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V3a1 1 0 0 0-1-1H2z"/><path d="M2.5 4a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5H3a.5.5 0 0 1-.5-.5V4z"/><path fill-rule="evenodd" d="M10.854 8.146a.5.5 0 0 1 0 .708l-3 3a.5.5 0 0 1-.708 0l-1.5-1.5a.5.5 0 0 1 .708-.708L7.5 10.793l2.646-2.647a.5.5 0 0 1 .708 0z"/>'
            ),
            fm = ma(
                "Search",
                '<path fill-rule="evenodd" d="M10.442 10.442a1 1 0 0 1 1.415 0l3.85 3.85a1 1 0 0 1-1.414 1.415l-3.85-3.85a1 1 0 0 1 0-1.415z"/><path fill-rule="evenodd" d="M6.5 12a5.5 5.5 0 1 0 0-11 5.5 5.5 0 0 0 0 11zM13 6.5a6.5 6.5 0 1 1-13 0 6.5 6.5 0 0 1 13 0z"/>'
            ),
            mB = ma(
                "Trash",
                '<path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/><path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4L4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/>'
            ),
            Uc = ma(
                "Envelope",
                '<path fill-rule="evenodd" d="M0 4a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V4zm2-1a1 1 0 0 0-1 1v.217l7 4.2 7-4.2V4a1 1 0 0 0-1-1H2zm13 2.383l-4.758 2.855L15 11.114v-5.73zm-.034 6.878L9.271 8.82 8 9.583 6.728 8.82l-5.694 3.44A1 1 0 0 0 2 13h12a1 1 0 0 0 .966-.739zM1 11.114l4.758-2.876L1 5.383v5.73z"/>'
            ),
            PM = ma(
                "HandThumbsDown",
                '<path fill-rule="evenodd" d="M6.956 14.534c.065.936.952 1.659 1.908 1.42l.261-.065a1.378 1.378 0 0 0 1.012-.965c.22-.816.533-2.512.062-4.51.136.02.285.037.443.051.713.065 1.669.071 2.516-.211.518-.173.994-.68 1.2-1.272a1.896 1.896 0 0 0-.234-1.734c.058-.118.103-.242.138-.362.077-.27.113-.568.113-.857 0-.288-.036-.585-.113-.856a2.094 2.094 0 0 0-.16-.403c.169-.387.107-.82-.003-1.149a3.162 3.162 0 0 0-.488-.9c.054-.153.076-.313.076-.465a1.86 1.86 0 0 0-.253-.912C13.1.757 12.437.28 11.5.28v1c.563 0 .901.272 1.066.56.086.15.121.3.121.416 0 .12-.035.165-.04.17l-.354.353.353.354c.202.202.407.512.505.805.104.312.043.44-.005.488l-.353.353.353.354c.043.043.105.141.154.315.048.167.075.37.075.581 0 .212-.027.415-.075.582-.05.174-.111.272-.154.315l-.353.353.353.354c.353.352.373.714.267 1.021-.122.35-.396.593-.571.651-.653.218-1.447.224-2.11.164a8.907 8.907 0 0 1-1.094-.17l-.014-.004H9.62a.5.5 0 0 0-.595.643 8.34 8.34 0 0 1 .145 4.725c-.03.112-.128.215-.288.255l-.262.066c-.306.076-.642-.156-.667-.519-.075-1.081-.239-2.15-.482-2.85-.174-.502-.603-1.267-1.238-1.977C5.597 8.926 4.715 8.23 3.62 7.93 3.226 7.823 3 7.534 3 7.28V3.279c0-.26.22-.515.553-.55 1.293-.138 1.936-.53 2.491-.869l.04-.024c.27-.165.495-.296.776-.393.277-.096.63-.163 1.14-.163h3.5v-1H8c-.605 0-1.07.08-1.466.217a4.823 4.823 0 0 0-.97.485l-.048.029c-.504.308-.999.61-2.068.723C2.682 1.815 2 2.434 2 3.279v4c0 .851.685 1.433 1.357 1.616.849.232 1.574.787 2.132 1.41.56.626.914 1.28 1.039 1.638.199.575.356 1.54.428 2.591z"/>'
            ),
            xM = ma(
                "HandThumbsUp",
                '<path fill-rule="evenodd" d="M6.956 1.745C7.021.81 7.908.087 8.864.325l.261.066c.463.116.874.456 1.012.965.22.816.533 2.511.062 4.51a9.84 9.84 0 0 1 .443-.051c.713-.065 1.669-.072 2.516.21.518.173.994.681 1.2 1.273.184.532.16 1.162-.234 1.733.058.119.103.242.138.363.077.27.113.567.113.856 0 .289-.036.586-.113.856-.039.135-.09.273-.16.404.169.387.107.819-.003 1.148a3.163 3.163 0 0 1-.488.901c.054.152.076.312.076.465 0 .305-.089.625-.253.912C13.1 15.522 12.437 16 11.5 16v-1c.563 0 .901-.272 1.066-.56a.865.865 0 0 0 .121-.416c0-.12-.035-.165-.04-.17l-.354-.354.353-.354c.202-.201.407-.511.505-.804.104-.312.043-.441-.005-.488l-.353-.354.353-.354c.043-.042.105-.14.154-.315.048-.167.075-.37.075-.581 0-.211-.027-.414-.075-.581-.05-.174-.111-.273-.154-.315L12.793 9l.353-.354c.353-.352.373-.713.267-1.02-.122-.35-.396-.593-.571-.652-.653-.217-1.447-.224-2.11-.164a8.907 8.907 0 0 0-1.094.171l-.014.003-.003.001a.5.5 0 0 1-.595-.643 8.34 8.34 0 0 0 .145-4.726c-.03-.111-.128-.215-.288-.255l-.262-.065c-.306-.077-.642.156-.667.518-.075 1.082-.239 2.15-.482 2.85-.174.502-.603 1.268-1.238 1.977-.637.712-1.519 1.41-2.614 1.708-.394.108-.62.396-.62.65v4.002c0 .26.22.515.553.55 1.293.137 1.936.53 2.491.868l.04.025c.27.164.495.296.776.393.277.095.63.163 1.14.163h3.5v1H8c-.605 0-1.07-.081-1.466-.218a4.82 4.82 0 0 1-.97-.484l-.048-.03c-.504-.307-.999-.609-2.068-.722C2.682 14.464 2 13.846 2 13V9c0-.85.685-1.432 1.357-1.615.849-.232 1.574-.787 2.132-1.41.56-.627.914-1.28 1.039-1.639.199-.575.356-1.539.428-2.59z"/>'
            ),
            cV = ma(
                "HouseDoor",
                '<path fill-rule="evenodd" d="M7.646 1.146a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1 .146.354v7a.5.5 0 0 1-.5.5H9.5a.5.5 0 0 1-.5-.5v-4H7v4a.5.5 0 0 1-.5.5H2a.5.5 0 0 1-.5-.5v-7a.5.5 0 0 1 .146-.354l6-6zM2.5 7.707V14H6v-4a.5.5 0 0 1 .5-.5h3a.5.5 0 0 1 .5.5v4h3.5V7.707L8 2.207l-5.5 5.5z"/><path fill-rule="evenodd" d="M13 2.5V6l-2-2V2.5a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5z"/>'
            ),
            Cp = ma(
                "List",
                '<path fill-rule="evenodd" d="M2.5 11.5A.5.5 0 0 1 3 11h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5zm0-4A.5.5 0 0 1 3 7h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5zm0-4A.5.5 0 0 1 3 3h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5z"/>'
            ),
            IM = ma(
                "Grid3x2Gap",
                '<path fill-rule="evenodd" d="M4 4H2v2h2V4zm1 7V9a1 1 0 0 0-1-1H2a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1h2a1 1 0 0 0 1-1zm0-5V4a1 1 0 0 0-1-1H2a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1h2a1 1 0 0 0 1-1zm5 5V9a1 1 0 0 0-1-1H7a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1h2a1 1 0 0 0 1-1zm0-5V4a1 1 0 0 0-1-1H7a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1h2a1 1 0 0 0 1-1zM9 4H7v2h2V4zm5 0h-2v2h2V4zM4 9H2v2h2V9zm5 0H7v2h2V9zm5 0h-2v2h2V9zm-3-5a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1V4zm1 4a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1h2a1 1 0 0 0 1-1V9a1 1 0 0 0-1-1h-2z"/>'
            ),
            FM = ma(
                "Grid3x2GapFill",
                '<path d="M1 4a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1V4zm5 0a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V4zm5 0a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1V4zM1 9a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1V9zm5 0a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V9zm5 0a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1V9z"/>'
            ),
            CL = ma(
                "XCircleFill",
                '<path fill-rule="evenodd" d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/>'
            ),
            jc = ma("Exclamation", '<path d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z"/>'),
            Iz = ma(
                "ClipboardPlus",
                '<path fill-rule="evenodd" d="M4 1.5H3a2 2 0 0 0-2 2V14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V3.5a2 2 0 0 0-2-2h-1v1h1a1 1 0 0 1 1 1V14a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3.5a1 1 0 0 1 1-1h1v-1z"/><path fill-rule="evenodd" d="M9.5 1h-3a.5.5 0 0 0-.5.5v1a.5.5 0 0 0 .5.5h3a.5.5 0 0 0 .5-.5v-1a.5.5 0 0 0-.5-.5zm-3-1A1.5 1.5 0 0 0 5 1.5v1A1.5 1.5 0 0 0 6.5 4h3A1.5 1.5 0 0 0 11 2.5v-1A1.5 1.5 0 0 0 9.5 0h-3zM8 7a.5.5 0 0 1 .5.5V9H10a.5.5 0 0 1 0 1H8.5v1.5a.5.5 0 0 1-1 0V10H6a.5.5 0 0 1 0-1h1.5V7.5A.5.5 0 0 1 8 7z"/>'
            ),
            Lm = ma(
                "Server",
                '<path fill-rule="evenodd" d="M1.333 2.667C1.333 1.194 4.318 0 8 0s6.667 1.194 6.667 2.667V4C14.665 5.474 11.68 6.667 8 6.667 4.318 6.667 1.333 5.473 1.333 4V2.667zm0 3.667v3C1.333 10.805 4.318 12 8 12c3.68 0 6.665-1.193 6.667-2.665V6.334c-.43.32-.931.58-1.458.79C11.81 7.684 9.967 8 8 8c-1.967 0-3.81-.317-5.21-.876a6.508 6.508 0 0 1-1.457-.79zm13.334 5.334c-.43.319-.931.578-1.458.789-1.4.56-3.242.876-5.209.876-1.967 0-3.81-.316-5.21-.876a6.51 6.51 0 0 1-1.457-.79v1.666C1.333 14.806 4.318 16 8 16s6.667-1.194 6.667-2.667v-1.665z"/>'
            ),
            FL = ma(
                "X",
                '<path fill-rule="evenodd" d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z"/>'
            ),
            ZM = ma(
                "Heart",
                '<path fill-rule="evenodd" d="M8 2.748l-.717-.737C5.6.281 2.514.878 1.4 3.053c-.523 1.023-.641 2.5.314 4.385.92 1.815 2.834 3.989 6.286 6.357 3.452-2.368 5.365-4.542 6.286-6.357.955-1.886.838-3.362.314-4.385C13.486.878 10.4.28 8.717 2.01L8 2.748zM8 15C-7.333 4.868 3.279-3.04 7.824 1.143c.06.055.119.112.176.171a3.12 3.12 0 0 1 .176-.17C12.72-3.042 23.333 4.867 8 15z"/>'
            ),
            EM = ma(
                "Hammer",
                '<path d="M9.812 1.952a.5.5 0 0 1-.312.89c-1.671 0-2.852.596-3.616 1.185L4.857 5.073V6.21a.5.5 0 0 1-.146.354L3.425 7.853a.5.5 0 0 1-.708 0L.146 5.274a.5.5 0 0 1 0-.706l1.286-1.29a.5.5 0 0 1 .354-.146H2.84C4.505 1.228 6.216.862 7.557 1.04a5.009 5.009 0 0 1 2.077.782l.178.129z"/><path fill-rule="evenodd" d="M6.012 3.5a.5.5 0 0 1 .359.165l9.146 8.646A.5.5 0 0 1 15.5 13L14 14.5a.5.5 0 0 1-.756-.056L4.598 5.297a.5.5 0 0 1 .048-.65l1-1a.5.5 0 0 1 .366-.147z"/>'
            ),
            Nm = ma(
                "Shuffle",
                '<path fill-rule="evenodd" d="M0 3.5A.5.5 0 0 1 .5 3H1c2.202 0 3.827 1.24 4.874 2.418.49.552.865 1.102 1.126 1.532.26-.43.636-.98 1.126-1.532C9.173 4.24 10.798 3 13 3v1c-1.798 0-3.173 1.01-4.126 2.082A9.624 9.624 0 0 0 7.556 8a9.624 9.624 0 0 0 1.317 1.918C9.828 10.99 11.204 12 13 12v1c-2.202 0-3.827-1.24-4.874-2.418A10.595 10.595 0 0 1 7 9.05c-.26.43-.636.98-1.126 1.532C4.827 11.76 3.202 13 1 13H.5a.5.5 0 0 1 0-1H1c1.798 0 3.173-1.01 4.126-2.082A9.624 9.624 0 0 0 6.444 8a9.624 9.624 0 0 0-1.317-1.918C4.172 5.01 2.796 4 1 4H.5a.5.5 0 0 1-.5-.5z"/><path d="M13 5.466V1.534a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384l-2.36 1.966a.25.25 0 0 1-.41-.192zm0 9v-3.932a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384l-2.36 1.966a.25.25 0 0 1-.41-.192z"/>'
            ),
            gB = ma(
                "Truck",
                '<path fill-rule="evenodd" d="M0 3.5A1.5 1.5 0 0 1 1.5 2h9A1.5 1.5 0 0 1 12 3.5V5h1.02a1.5 1.5 0 0 1 1.17.563l1.481 1.85a1.5 1.5 0 0 1 .329.938V10.5a1.5 1.5 0 0 1-1.5 1.5H14a2 2 0 1 1-4 0H5a2 2 0 1 1-3.998-.085A1.5 1.5 0 0 1 0 10.5v-7zm1.294 7.456A1.999 1.999 0 0 1 4.732 11h5.536a2.01 2.01 0 0 1 .732-.732V3.5a.5.5 0 0 0-.5-.5h-9a.5.5 0 0 0-.5.5v7a.5.5 0 0 0 .294.456zM12 10a2 2 0 0 1 1.732 1h.768a.5.5 0 0 0 .5-.5V8.35a.5.5 0 0 0-.11-.312l-1.48-1.85A.5.5 0 0 0 13.02 6H12v4zm-9 1a1 1 0 1 0 0 2 1 1 0 0 0 0-2zm9 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2z"/>'
            ),
            ju = ma(
                "QuestionCircle",
                '<path fill-rule="evenodd" d="M8 15A7 7 0 1 0 8 1a7 7 0 0 0 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/><path d="M5.255 5.786a.237.237 0 0 0 .241.247h.825c.138 0 .248-.113.266-.25.09-.656.54-1.134 1.342-1.134.686 0 1.314.343 1.314 1.168 0 .635-.374.927-.965 1.371-.673.489-1.206 1.06-1.168 1.987l.003.217a.25.25 0 0 0 .25.246h.811a.25.25 0 0 0 .25-.25v-.105c0-.718.273-.927 1.01-1.486.609-.463 1.244-.977 1.244-2.056 0-1.511-1.276-2.241-2.673-2.241-1.267 0-2.655.59-2.75 2.286zm1.557 5.763c0 .533.425.927 1.01.927.609 0 1.028-.394 1.028-.927 0-.552-.42-.94-1.029-.94-.584 0-1.009.388-1.009.94z"/>'
            ),
            ac = ma("DoorClosed", '<path fill-rule="evenodd" d="M3 2a1 1 0 0 1 1-1h8a1 1 0 0 1 1 1v13h1.5a.5.5 0 0 1 0 1h-13a.5.5 0 0 1 0-1H3V2zm1 13h8V2H4v13z"/><path d="M9 9a1 1 0 1 0 2 0 1 1 0 0 0-2 0z"/>'),
            eM = ma(
                "Gear",
                '<path fill-rule="evenodd" d="M8.837 1.626c-.246-.835-1.428-.835-1.674 0l-.094.319A1.873 1.873 0 0 1 4.377 3.06l-.292-.16c-.764-.415-1.6.42-1.184 1.185l.159.292a1.873 1.873 0 0 1-1.115 2.692l-.319.094c-.835.246-.835 1.428 0 1.674l.319.094a1.873 1.873 0 0 1 1.115 2.693l-.16.291c-.415.764.42 1.6 1.185 1.184l.292-.159a1.873 1.873 0 0 1 2.692 1.116l.094.318c.246.835 1.428.835 1.674 0l.094-.319a1.873 1.873 0 0 1 2.693-1.115l.291.16c.764.415 1.6-.42 1.184-1.185l-.159-.291a1.873 1.873 0 0 1 1.116-2.693l.318-.094c.835-.246.835-1.428 0-1.674l-.319-.094a1.873 1.873 0 0 1-1.115-2.692l.16-.292c.415-.764-.42-1.6-1.185-1.184l-.291.159A1.873 1.873 0 0 1 8.93 1.945l-.094-.319zm-2.633-.283c.527-1.79 3.065-1.79 3.592 0l.094.319a.873.873 0 0 0 1.255.52l.292-.16c1.64-.892 3.434.901 2.54 2.541l-.159.292a.873.873 0 0 0 .52 1.255l.319.094c1.79.527 1.79 3.065 0 3.592l-.319.094a.873.873 0 0 0-.52 1.255l.16.292c.893 1.64-.902 3.434-2.541 2.54l-.292-.159a.873.873 0 0 0-1.255.52l-.094.319c-.527 1.79-3.065 1.79-3.592 0l-.094-.319a.873.873 0 0 0-1.255-.52l-.292.16c-1.64.893-3.433-.902-2.54-2.541l.159-.292a.873.873 0 0 0-.52-1.255l-.319-.094c-1.79-.527-1.79-3.065 0-3.592l.319-.094a.873.873 0 0 0 .52-1.255l-.16-.292c-.892-1.64.902-3.433 2.541-2.54l.292.159a.873.873 0 0 0 1.255-.52l.094-.319z"/><path fill-rule="evenodd" d="M8 5.754a2.246 2.246 0 1 0 0 4.492 2.246 2.246 0 0 0 0-4.492zM4.754 8a3.246 3.246 0 1 1 6.492 0 3.246 3.246 0 0 1-6.492 0z"/>'
            ),
            IL = ma(
                "Wrench",
                '<path fill-rule="evenodd" d="M.102 2.223A3.004 3.004 0 0 0 3.78 5.897l6.341 6.252A3.003 3.003 0 0 0 13 16a3 3 0 1 0-.851-5.878L5.897 3.781A3.004 3.004 0 0 0 2.223.1l2.141 2.142L4 4l-1.757.364L.102 2.223zm13.37 9.019L13 11l-.471.242-.529.026-.287.445-.445.287-.026.529L11 13l.242.471.026.529.445.287.287.445.529.026L13 15l.471-.242.529-.026.287-.445.445-.287.026-.529L15 13l-.242-.471-.026-.529-.445-.287-.287-.445-.529-.026z"/>'
            ),
            Wv = ma(
                "Cart",
                '<path fill-rule="evenodd" d="M0 1.5A.5.5 0 0 1 .5 1H2a.5.5 0 0 1 .485.379L2.89 3H14.5a.5.5 0 0 1 .491.592l-1.5 8A.5.5 0 0 1 13 12H4a.5.5 0 0 1-.491-.408L2.01 3.607 1.61 2H.5a.5.5 0 0 1-.5-.5zM3.102 4l1.313 7h8.17l1.313-7H3.102zM5 12a2 2 0 1 0 0 4 2 2 0 0 0 0-4zm7 0a2 2 0 1 0 0 4 2 2 0 0 0 0-4zm-7 1a1 1 0 1 0 0 2 1 1 0 0 0 0-2zm7 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2z"/>'
            ),
            vo = ma(
                "CashStack",
                '<path d="M14 3H1a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1h-1z"/><path fill-rule="evenodd" d="M15 5H1v8h14V5zM1 4a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h14a1 1 0 0 0 1-1V5a1 1 0 0 0-1-1H1z"/><path d="M13 5a2 2 0 0 0 2 2V5h-2zM3 5a2 2 0 0 1-2 2V5h2zm10 8a2 2 0 0 1 2-2v2h-2zM3 13a2 2 0 0 0-2-2v2h2zm7-4a2 2 0 1 1-4 0 2 2 0 0 1 4 0z"/>'
            ),
            Qv = ma(
                "CartCheck",
                '<path fill-rule="evenodd" d="M0 1.5A.5.5 0 0 1 .5 1H2a.5.5 0 0 1 .485.379L2.89 3H14.5a.5.5 0 0 1 .491.592l-1.5 8A.5.5 0 0 1 13 12H4a.5.5 0 0 1-.491-.408L2.01 3.607 1.61 2H.5a.5.5 0 0 1-.5-.5zM3.102 4l1.313 7h8.17l1.313-7H3.102zM5 12a2 2 0 1 0 0 4 2 2 0 0 0 0-4zm7 0a2 2 0 1 0 0 4 2 2 0 0 0 0-4zm-7 1a1 1 0 1 0 0 2 1 1 0 0 0 0-2zm7 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2z"/><path fill-rule="evenodd" d="M11.354 5.646a.5.5 0 0 1 0 .708l-3 3a.5.5 0 0 1-.708 0l-1.5-1.5a.5.5 0 1 1 .708-.708L8 8.293l2.646-2.647a.5.5 0 0 1 .708 0z"/>'
            ),
            YH = ma(
                "Person",
                '<path fill-rule="evenodd" d="M10 5a2 2 0 1 1-4 0 2 2 0 0 1 4 0zM8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm6 5c0 1-1 1-1 1H3s-1 0-1-1 1-4 6-4 6 3 6 4zm-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.289 10 8 10c-2.29 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664h10z"/>'
            ),
            Hr = ma(
                "FileEarmarkRichtext",
                '<path d="M4 0h5.5v1H4a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h1V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2z"/><path d="M9.5 3V0L14 4.5h-3A1.5 1.5 0 0 1 9.5 3z"/><path fill-rule="evenodd" d="M4.5 12.5A.5.5 0 0 1 5 12h3a.5.5 0 0 1 0 1H5a.5.5 0 0 1-.5-.5zm0-2A.5.5 0 0 1 5 10h6a.5.5 0 0 1 0 1H5a.5.5 0 0 1-.5-.5zm1.639-3.708l1.33.886 1.854-1.855a.25.25 0 0 1 .289-.047l1.888.974V8.5a.5.5 0 0 1-.5.5H5a.5.5 0 0 1-.5-.5V8s1.54-1.274 1.639-1.208zM6.25 6a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5z"/>'
            ),
            Oi = ma(
                "FileEarmarkEasel",
                '<path d="M4 0h5.5v1H4a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h1V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2z"/><path d="M9.5 3V0L14 4.5h-3A1.5 1.5 0 0 1 9.5 3zm-1 3a.5.5 0 1 0-1 0h1zm1.527 5H8.973l.553 1.658a.5.5 0 1 0 .948-.316L10.027 11zM8.5 11h-1v1a.5.5 0 0 0 1 0v-1zm-1.473 0H5.973l-.447 1.342a.5.5 0 1 0 .948.316L7.027 11z"/><path fill-rule="evenodd" d="M4 7.5A1.5 1.5 0 0 1 5.5 6h5A1.5 1.5 0 0 1 12 7.5v2a1.5 1.5 0 0 1-1.5 1.5h-5A1.5 1.5 0 0 1 4 9.5v-2zM5.5 7a.5.5 0 0 0-.5.5v2a.5.5 0 0 0 .5.5h5a.5.5 0 0 0 .5-.5v-2a.5.5 0 0 0-.5-.5h-5z"/>'
            ),
            Ni = ma(
                "FileEarmarkFont",
                '<path d="M4 0h5.5v1H4a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h1V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2z"/><path d="M9.5 3V0L14 4.5h-3A1.5 1.5 0 0 1 9.5 3zm1.443 3H5.057L5 8h.5c.18-1.096.356-1.192 1.694-1.235l.293-.01v5.09c0 .47-.1.582-.898.655v.5H9.41v-.5c-.803-.073-.903-.184-.903-.654V6.755l.298.01c1.338.043 1.514.14 1.694 1.235h.5l-.057-2z"/>'
            ),
            xi = ma(
                "FileEarmarkCheck",
                '<path d="M4 0h5.5v1H4a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h1V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2z"/><path d="M9.5 3V0L14 4.5h-3A1.5 1.5 0 0 1 9.5 3z"/><path fill-rule="evenodd" d="M10.854 7.146a.5.5 0 0 1 0 .708l-3 3a.5.5 0 0 1-.708 0l-1.5-1.5a.5.5 0 1 1 .708-.708L7.5 9.793l2.646-2.647a.5.5 0 0 1 .708 0z"/>'
            ),
            Vl = ma(
                "ArrowReturnLeft",
                '<path fill-rule="evenodd" d="M14.5 1.5a.5.5 0 0 1 .5.5v4.8a2.5 2.5 0 0 1-2.5 2.5H2.707l3.347 3.346a.5.5 0 0 1-.708.708l-4.2-4.2a.5.5 0 0 1 0-.708l4-4a.5.5 0 1 1 .708.708L2.707 8.3H12.5A1.5 1.5 0 0 0 14 6.8V2a.5.5 0 0 1 .5-.5z"/>'
            ),
            xz = ma(
                "CloudDownload",
                '<path fill-rule="evenodd" d="M4.406 1.342A5.53 5.53 0 0 1 8 0c2.69 0 4.923 2 5.166 4.579C14.758 4.804 16 6.137 16 7.773 16 9.569 14.502 11 12.687 11H10a.5.5 0 0 1 0-1h2.688C13.979 10 15 8.988 15 7.773c0-1.216-1.02-2.228-2.313-2.228h-.5v-.5C12.188 2.825 10.328 1 8 1a4.53 4.53 0 0 0-2.941 1.1c-.757.652-1.153 1.438-1.153 2.055v.448l-.445.049C2.064 4.805 1 5.952 1 7.318 1 8.785 2.23 10 3.781 10H6a.5.5 0 0 1 0 1H3.781C1.708 11 0 9.366 0 7.318c0-1.763 1.266-3.223 2.942-3.593.143-.863.698-1.723 1.464-2.383z"/><path fill-rule="evenodd" d="M7.646 15.854a.5.5 0 0 0 .708 0l3-3a.5.5 0 0 0-.708-.708L8.5 14.293V5.5a.5.5 0 0 0-1 0v8.793l-2.146-2.147a.5.5 0 0 0-.708.708l3 3z"/>'
            ),
            lA = ma(
                "SkipBackward",
                '<path fill-rule="evenodd" d="M.5 3.5A.5.5 0 0 1 1 4v3.248l6.267-3.636c.52-.302 1.233.043 1.233.696v2.94l6.267-3.636c.52-.302 1.233.043 1.233.696v7.384c0 .653-.713.998-1.233.696L8.5 8.752v2.94c0 .653-.713.998-1.233.696L1 8.752V12a.5.5 0 0 1-1 0V4a.5.5 0 0 1 .5-.5zm7 1.133L1.696 8 7.5 11.367V4.633zm7.5 0L9.196 8 15 11.367V4.633z"/>'
            ),
            tz = ma("ChevronLeft", '<path fill-rule="evenodd" d="M11.354 1.646a.5.5 0 0 1 0 .708L5.707 8l5.647 5.646a.5.5 0 0 1-.708.708l-6-6a.5.5 0 0 1 0-.708l6-6a.5.5 0 0 1 .708 0z"/>'),
            Pm = ma(
                "ShieldLock",
                '<path fill-rule="evenodd" d="M5.443 1.991a60.17 60.17 0 0 0-2.725.802.454.454 0 0 0-.315.366C1.87 7.056 3.1 9.9 4.567 11.773c.736.94 1.533 1.636 2.197 2.093.333.228.626.394.857.5.116.053.21.089.282.11A.73.73 0 0 0 8 14.5c.007-.001.038-.005.097-.023.072-.022.166-.058.282-.111.23-.106.525-.272.857-.5a10.197 10.197 0 0 0 2.197-2.093C12.9 9.9 14.13 7.056 13.597 3.159a.454.454 0 0 0-.315-.366c-.626-.2-1.682-.526-2.725-.802C9.491 1.71 8.51 1.5 8 1.5c-.51 0-1.49.21-2.557.491zm-.256-.966C6.23.749 7.337.5 8 .5c.662 0 1.77.249 2.813.525a61.09 61.09 0 0 1 2.772.815c.528.168.926.623 1.003 1.184.573 4.197-.756 7.307-2.367 9.365a11.191 11.191 0 0 1-2.418 2.3 6.942 6.942 0 0 1-1.007.586c-.27.124-.558.225-.796.225s-.526-.101-.796-.225a6.908 6.908 0 0 1-1.007-.586 11.192 11.192 0 0 1-2.417-2.3C2.167 10.331.839 7.221 1.412 3.024A1.454 1.454 0 0 1 2.415 1.84a61.11 61.11 0 0 1 2.772-.815z"/><path d="M9.5 6.5a1.5 1.5 0 0 1-1 1.415l.385 1.99a.5.5 0 0 1-.491.595h-.788a.5.5 0 0 1-.49-.595l.384-1.99a1.5 1.5 0 1 1 2-1.415z"/>'
            ),
            kB = ma(
                "Trophy",
                '<path fill-rule="evenodd" d="M2.5.5A.5.5 0 0 1 3 0h10a.5.5 0 0 1 .5.5c0 .538-.012 1.05-.034 1.536a3 3 0 1 1-1.133 5.89c-.79 1.865-1.878 2.777-2.833 3.011v2.173l1.425.356c.194.048.377.135.537.255L13.3 15.1a.5.5 0 0 1-.3.9H3a.5.5 0 0 1-.3-.9l1.838-1.379c.16-.12.343-.207.537-.255L6.5 13.11v-2.173c-.955-.234-2.043-1.146-2.833-3.012a3 3 0 1 1-1.132-5.89A33.076 33.076 0 0 1 2.5.5zm.099 2.54a2 2 0 0 0 .72 3.935c-.333-1.05-.588-2.346-.72-3.935zm10.083 3.935a2 2 0 0 0 .72-3.935c-.133 1.59-.388 2.885-.72 3.935zM3.504 1c.007.517.026 1.006.056 1.469.13 2.028.457 3.546.87 4.667C5.294 9.48 6.484 10 7 10a.5.5 0 0 1 .5.5v2.61a1 1 0 0 1-.757.97l-1.426.356a.5.5 0 0 0-.179.085L4.5 15h7l-.638-.479a.501.501 0 0 0-.18-.085l-1.425-.356a1 1 0 0 1-.757-.97V10.5A.5.5 0 0 1 9 10c.516 0 1.706-.52 2.57-2.864.413-1.12.74-2.64.87-4.667.03-.463.049-.952.056-1.469H3.504z"/>'
            ),
            vu = ma(
                "PersonCircle",
                '<path d="M13.468 12.37C12.758 11.226 11.195 10 8 10s-4.757 1.225-5.468 2.37A6.987 6.987 0 0 0 8 15a6.987 6.987 0 0 0 5.468-2.63z"/><path fill-rule="evenodd" d="M8 9a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"/><path fill-rule="evenodd" d="M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zM0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8z"/>'
            ),
            ct = ma(
                "FileRuled",
                '<path fill-rule="evenodd" d="M2 2a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2zm2-1a1 1 0 0 0-1 1v4h10V2a1 1 0 0 0-1-1H4zm9 6H6v2h7V7zm0 3H6v2h7v-2zm0 3H6v2h6a1 1 0 0 0 1-1v-1zm-8 2v-2H3v1a1 1 0 0 0 1 1h1zm-2-3h2v-2H3v2zm0-3h2V7H3v2z"/>'
            ),
            Wm = ma(
                "Shop",
                '<path fill-rule="evenodd" d="M2.97 1.35A1 1 0 0 1 3.73 1h8.54a1 1 0 0 1 .76.35l2.609 3.044A1.5 1.5 0 0 1 16 5.37v.255a2.375 2.375 0 0 1-4.25 1.458A2.371 2.371 0 0 1 9.875 8 2.37 2.37 0 0 1 8 7.083 2.37 2.37 0 0 1 6.125 8a2.37 2.37 0 0 1-1.875-.917A2.375 2.375 0 0 1 0 5.625V5.37a1.5 1.5 0 0 1 .361-.976l2.61-3.045zm1.78 4.275a1.375 1.375 0 0 0 2.75 0 .5.5 0 0 1 1 0 1.375 1.375 0 0 0 2.75 0 .5.5 0 0 1 1 0 1.375 1.375 0 1 0 2.75 0V5.37a.5.5 0 0 0-.12-.325L12.27 2H3.73L1.12 5.045A.5.5 0 0 0 1 5.37v.255a1.375 1.375 0 0 0 2.75 0 .5.5 0 0 1 1 0zM1.5 8.5A.5.5 0 0 1 2 9v6h1v-5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v5h6V9a.5.5 0 0 1 1 0v6h.5a.5.5 0 0 1 0 1H.5a.5.5 0 0 1 0-1H1V9a.5.5 0 0 1 .5-.5zM4 15h3v-5H4v5zm5-5a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1v-3zm3 0h-2v3h2v-3z"/>'
            ),
            _A = ma(
                "Star",
                '<path fill-rule="evenodd" d="M2.866 14.85c-.078.444.36.791.746.593l4.39-2.256 4.389 2.256c.386.198.824-.149.746-.592l-.83-4.73 3.523-3.356c.329-.314.158-.888-.283-.95l-4.898-.696L8.465.792a.513.513 0 0 0-.927 0L5.354 5.12l-4.898.696c-.441.062-.612.636-.283.95l3.523 3.356-.83 4.73zm4.905-2.767l-3.686 1.894.694-3.957a.565.565 0 0 0-.163-.505L1.71 6.745l4.052-.576a.525.525 0 0 0 .393-.288l1.847-3.658 1.846 3.658a.525.525 0 0 0 .393.288l4.052.575-2.906 2.77a.564.564 0 0 0-.163.506l.694 3.957-3.686-1.894a.503.503 0 0 0-.461 0z"/>'
            ),
            YA = ma(
                "StarFill",
                '<path d="M3.612 15.443c-.386.198-.824-.149-.746-.592l.83-4.73L.173 6.765c-.329-.314-.158-.888.283-.95l4.898-.696L7.538.792c.197-.39.73-.39.927 0l2.184 4.327 4.898.696c.441.062.612.636.283.95l-3.523 3.356.83 4.73c.078.443-.36.79-.746.592L8 13.187l-4.389 2.256z"/>'
            ),
            af = ma(
                "StarHalf",
                '<path fill-rule="evenodd" d="M5.354 5.119L7.538.792A.516.516 0 0 1 8 .5c.183 0 .366.097.465.292l2.184 4.327 4.898.696A.537.537 0 0 1 16 6.32a.55.55 0 0 1-.17.445l-3.523 3.356.83 4.73c.078.443-.36.79-.746.592L8 13.187l-4.389 2.256a.519.519 0 0 1-.146.05c-.341.06-.668-.254-.6-.642l.83-4.73L.173 6.765a.55.55 0 0 1-.171-.403.59.59 0 0 1 .084-.302.513.513 0 0 1 .37-.245l4.898-.696zM8 12.027c.08 0 .16.018.232.056l3.686 1.894-.694-3.957a.564.564 0 0 1 .163-.505l2.906-2.77-4.052-.576a.525.525 0 0 1-.393-.288L8.002 2.223 8 2.226v9.8z"/>'
            ),
            iu = ma(
                "PersonPlus",
                '<path fill-rule="evenodd" d="M8 5a2 2 0 1 1-4 0 2 2 0 0 1 4 0zM6 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm6 5c0 1-1 1-1 1H1s-1 0-1-1 1-4 6-4 6 3 6 4zm-1-.004c-.001-.246-.154-.986-.832-1.664C9.516 10.68 8.289 10 6 10c-2.29 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664h10zM13.5 5a.5.5 0 0 1 .5.5V7h1.5a.5.5 0 0 1 0 1H14v1.5a.5.5 0 0 1-1 0V8h-1.5a.5.5 0 0 1 0-1H13V5.5a.5.5 0 0 1 .5-.5z"/>'
            ),
            TM = ma(
                "Handbag",
                '<path fill-rule="evenodd" d="M8 1a2 2 0 0 0-2 2v2h4V3a2 2 0 0 0-2-2zm3 4V3a3 3 0 1 0-6 0v2H3.361a1.5 1.5 0 0 0-1.483 1.277L.85 13.13A2.5 2.5 0 0 0 3.322 16h9.356a2.5 2.5 0 0 0 2.472-2.87l-1.028-6.853A1.5 1.5 0 0 0 12.64 5H11zm-1 1v1.5a.5.5 0 0 0 1 0V6h1.639a.5.5 0 0 1 .494.426l1.028 6.851A1.5 1.5 0 0 1 12.678 15H3.322a1.5 1.5 0 0 1-1.483-1.723l1.028-6.851A.5.5 0 0 1 3.36 6H5v1.5a.5.5 0 0 0 1 0V6h4z"/>'
            ),
            Dp = ma(
                "Lock",
                '<path fill-rule="evenodd" d="M11.5 8h-7a1 1 0 0 0-1 1v5a1 1 0 0 0 1 1h7a1 1 0 0 0 1-1V9a1 1 0 0 0-1-1zm-7-1a2 2 0 0 0-2 2v5a2 2 0 0 0 2 2h7a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-7zm0-3a3.5 3.5 0 1 1 7 0v3h-1V4a2.5 2.5 0 0 0-5 0v3h-1V4z"/>'
            ),
            Pm = ma(
                "ShieldLock",
                '<path fill-rule="evenodd" d="M5.443 1.991a60.17 60.17 0 0 0-2.725.802.454.454 0 0 0-.315.366C1.87 7.056 3.1 9.9 4.567 11.773c.736.94 1.533 1.636 2.197 2.093.333.228.626.394.857.5.116.053.21.089.282.11A.73.73 0 0 0 8 14.5c.007-.001.038-.005.097-.023.072-.022.166-.058.282-.111.23-.106.525-.272.857-.5a10.197 10.197 0 0 0 2.197-2.093C12.9 9.9 14.13 7.056 13.597 3.159a.454.454 0 0 0-.315-.366c-.626-.2-1.682-.526-2.725-.802C9.491 1.71 8.51 1.5 8 1.5c-.51 0-1.49.21-2.557.491zm-.256-.966C6.23.749 7.337.5 8 .5c.662 0 1.77.249 2.813.525a61.09 61.09 0 0 1 2.772.815c.528.168.926.623 1.003 1.184.573 4.197-.756 7.307-2.367 9.365a11.191 11.191 0 0 1-2.418 2.3 6.942 6.942 0 0 1-1.007.586c-.27.124-.558.225-.796.225s-.526-.101-.796-.225a6.908 6.908 0 0 1-1.007-.586 11.192 11.192 0 0 1-2.417-2.3C2.167 10.331.839 7.221 1.412 3.024A1.454 1.454 0 0 1 2.415 1.84a61.11 61.11 0 0 1 2.772-.815z"/><path d="M9.5 6.5a1.5 1.5 0 0 1-1 1.415l.385 1.99a.5.5 0 0 1-.491.595h-.788a.5.5 0 0 1-.49-.595l.384-1.99a1.5 1.5 0 1 1 2-1.415z"/>'
            ),
            lo = ma(
                "CartX",
                '<path fill-rule="evenodd" d="M0 1.5A.5.5 0 0 1 .5 1H2a.5.5 0 0 1 .485.379L2.89 3H14.5a.5.5 0 0 1 .491.592l-1.5 8A.5.5 0 0 1 13 12H4a.5.5 0 0 1-.491-.408L2.01 3.607 1.61 2H.5a.5.5 0 0 1-.5-.5zM3.102 4l1.313 7h8.17l1.313-7H3.102zM5 12a2 2 0 1 0 0 4 2 2 0 0 0 0-4zm7 0a2 2 0 1 0 0 4 2 2 0 0 0 0-4zm-7 1a1 1 0 1 0 0 2 1 1 0 0 0 0-2zm7 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2z"/><path fill-rule="evenodd" d="M6.646 5.646a.5.5 0 0 1 .708 0L8.5 6.793l1.146-1.147a.5.5 0 0 1 .708.708L9.207 7.5l1.147 1.146a.5.5 0 0 1-.708.708L8.5 8.207 7.354 9.354a.5.5 0 1 1-.708-.708L7.793 7.5 6.646 6.354a.5.5 0 0 1 0-.708z"/>'
            ),
            Xv = ma(
                "Cart2",
                '<path fill-rule="evenodd" d="M0 2.5A.5.5 0 0 1 .5 2H2a.5.5 0 0 1 .485.379L2.89 4H14.5a.5.5 0 0 1 .485.621l-1.5 6A.5.5 0 0 1 13 11H4a.5.5 0 0 1-.485-.379L1.61 3H.5a.5.5 0 0 1-.5-.5zM3.14 5l1.25 5h8.22l1.25-5H3.14zM5 13a1 1 0 1 0 0 2 1 1 0 0 0 0-2zm-2 1a2 2 0 1 1 4 0 2 2 0 0 1-4 0zm9-1a1 1 0 1 0 0 2 1 1 0 0 0 0-2zm-2 1a2 2 0 1 1 4 0 2 2 0 0 1-4 0z"/>'
            ),
            ip = ma(
                "LayoutSidebarInset",
                '<path fill-rule="evenodd" d="M14 2H2a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V3a1 1 0 0 0-1-1zM2 1a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V3a2 2 0 0 0-2-2H2z"/><path d="M3 4a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V4z"/>'
            ),
            Oa = ma(
                "ArrowClockwise",
                '<path fill-rule="evenodd" d="M8 3a5 5 0 1 0 4.546 2.914.5.5 0 0 1 .908-.417A6 6 0 1 1 8 2v1z"/><path d="M8 4.466V.534a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384L8.41 4.658A.25.25 0 0 1 8 4.466z"/>'
            ),
            $i = ma(
                "FileEarmarkLock2",
                '<path fill-rule="evenodd" d="M8 6a1 1 0 0 0-1 1v1h2V7a1 1 0 0 0-1-1zm2 2.076V7a2 2 0 1 0-4 0v1.076c-.54.166-1 .597-1 1.224v2.4c0 .816.781 1.3 1.5 1.3h3c.719 0 1.5-.484 1.5-1.3V9.3c0-.627-.46-1.058-1-1.224z"/><path d="M4 0h5.5v1H4a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h1V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2z"/><path d="M9.5 3V0L14 4.5h-3A1.5 1.5 0 0 1 9.5 3z"/>'
            ),
            mr = ma(
                "FileEarmarkRuled",
                '<path fill-rule="evenodd" d="M5 10H3V9h10v1H6v2h7v1H6v2H5v-2H3v-1h2v-2z"/><path d="M4 0h5.5v1H4a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h1V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2z"/><path d="M9.5 3V0L14 4.5h-3A1.5 1.5 0 0 1 9.5 3z"/>'
            ),
            Wl = ma(
                "AspectRatio",
                '<path fill-rule="evenodd" d="M0 3.5A1.5 1.5 0 0 1 1.5 2h13A1.5 1.5 0 0 1 16 3.5v9a1.5 1.5 0 0 1-1.5 1.5h-13A1.5 1.5 0 0 1 0 12.5v-9zM1.5 3a.5.5 0 0 0-.5.5v9a.5.5 0 0 0 .5.5h13a.5.5 0 0 0 .5-.5v-9a.5.5 0 0 0-.5-.5h-13z"/><path fill-rule="evenodd" d="M2 4.5a.5.5 0 0 1 .5-.5h3a.5.5 0 0 1 0 1H3v2.5a.5.5 0 0 1-1 0v-3zm12 7a.5.5 0 0 1-.5.5h-3a.5.5 0 0 1 0-1H13V8.5a.5.5 0 0 1 1 0v3z"/>'
            ),
            lH = ma("Moon", '<path fill-rule="evenodd" d="M14.53 10.53a7 7 0 0 1-9.058-9.058A7.003 7.003 0 0 0 8 15a7.002 7.002 0 0 0 6.53-4.47z"/>'),
            sf = ma(
                "Sun",
                '<path d="M3.5 8a4.5 4.5 0 1 1 9 0 4.5 4.5 0 0 1-9 0z"/><path fill-rule="evenodd" d="M8.202.28a.25.25 0 0 0-.404 0l-.91 1.255a.25.25 0 0 1-.334.067L5.232.79a.25.25 0 0 0-.374.155l-.36 1.508a.25.25 0 0 1-.282.19l-1.532-.245a.25.25 0 0 0-.286.286l.244 1.532a.25.25 0 0 1-.189.282l-1.509.36a.25.25 0 0 0-.154.374l.812 1.322a.25.25 0 0 1-.067.333l-1.256.91a.25.25 0 0 0 0 .405l1.256.91a.25.25 0 0 1 .067.334L.79 10.768a.25.25 0 0 0 .154.374l1.51.36a.25.25 0 0 1 .188.282l-.244 1.532a.25.25 0 0 0 .286.286l1.532-.244a.25.25 0 0 1 .282.189l.36 1.508a.25.25 0 0 0 .374.155l1.322-.812a.25.25 0 0 1 .333.067l.91 1.256a.25.25 0 0 0 .405 0l.91-1.256a.25.25 0 0 1 .334-.067l1.322.812a.25.25 0 0 0 .374-.155l.36-1.508a.25.25 0 0 1 .282-.19l1.532.245a.25.25 0 0 0 .286-.286l-.244-1.532a.25.25 0 0 1 .189-.282l1.508-.36a.25.25 0 0 0 .155-.374l-.812-1.322a.25.25 0 0 1 .067-.333l1.256-.91a.25.25 0 0 0 0-.405l-1.256-.91a.25.25 0 0 1-.067-.334l.812-1.322a.25.25 0 0 0-.155-.374l-1.508-.36a.25.25 0 0 1-.19-.282l.245-1.532a.25.25 0 0 0-.286-.286l-1.532.244a.25.25 0 0 1-.282-.189l-.36-1.508a.25.25 0 0 0-.374-.155l-1.322.812a.25.25 0 0 1-.333-.067L8.203.28zM8 2.5a5.5 5.5 0 1 0 0 11 5.5 5.5 0 0 0 0-11z"/>'
            ),
            TL = function a(l, e) {
                return l ? (l.$options || {}).components[e] || a(l.$parent, e) : null;
            },
            RL = ca(
                ((m = o(o({}, b(Ha, ["content", "stacked"])), {}, { icon: za(aa), stacked: za(_, !1) })),
                    R(m)
                        .sort()
                        .reduce(function (a, l) {
                            return o(o({}, a), {}, d({}, l, m[l]));
                        }, {})),
                ia
            ),
            bL = e.default.extend({
                name: ia,
                functional: !0,
                props: RL,
                render: function (a, l) {
                    var e = l.data,
                        h = l.props,
                        d = l.parent,
                        v = va(oa(h.icon || "")).replace(w, "");
                    return a((v && TL(d, "BIcon".concat(v))) || Aa, p(e, { props: o(o({}, h), {}, { icon: null }) }));
                },
            }),
            UL = ca(b(Ha, ["content", "stacked"]), ra),
            qL = X(
                {
                    plugins: {
                        IconsPlugin: X({
                            components: {
                                BIcon: bL,
                                BIconstack: e.default.extend({
                                    name: ra,
                                    functional: !0,
                                    props: UL,
                                    render: function (a, l) {
                                        var e = l.data,
                                            h = l.props,
                                            d = l.children;
                                        return a(ua, p(e, { staticClass: "b-iconstack", props: o(o({}, h), {}, { stacked: !1 }) }), d);
                                    },
                                }),
                                BIconBlank: Aa,
                                BIconCalendar2Check: td,
                                BIconSearch: fm,
                                BIconTrash: mB,
                                BIconEnvelope: Uc,
                                BIconHandThumbsDown: PM,
                                BIconHandThumbsUp: xM,
                                BIconHouseDoor: cV,
                                BIconList: Cp,
                                BIconGrid3x2Gap: IM,
                                BIconGrid3x2GapFill: FM,
                                BIconXCircleFill: CL,
                                BIconExclamation: jc,
                                BIconClipboardPlus: Iz,
                                BIconServer: Lm,
                                BIconX: FL,
                                BIconHeart: ZM,
                                BIconHammer: EM,
                                BIconShuffle: Nm,
                                BIconTruck: gB,
                                BIconQuestionCircle: ju,
                                BIconDoorClosed: ac,
                                BIconGear: eM,
                                BIconWrench: IL,
                                BIconCart: Wv,
                                BIconCashStack: vo,
                                BIconCartCheck: Qv,
                                BIconPerson: YH,
                                BIconFileEarmarkRichtext: Hr,
                                BIconFileEarmarkEasel: Oi,
                                BIconFileEarmarkFont: Ni,
                                BIconFileEarmarkCheck: xi,
                                BIconArrowReturnLeft: Vl,
                                BIconCloudDownload: xz,
                                BIconSkipBackward: lA,
                                BIconChevronLeft: tz,
                                BIconShieldLock: Pm,
                                BIconTrophy: kB,
                                BIconPersonCircle: vu,
                                BIconFileRuled: ct,
                                BIconShop: Wm,
                                BIconStar: _A,
                                BIconStarFill: YA,
                                BIconStarHalf: af,
                                BIconPersonPlus: iu,
                                BIconHandbag: TM,
                                BIconLock: Dp,
                                BIconShieldLock: Pm,
                                BIconCartX: lo,
                                BIconCart2: Xv,
                                BIconLayoutSidebarInset: ip,
                                BIconArrowClockwise: Oa,
                                BIconFileEarmarkLock2: $i,
                                BIconFileEarmarkRuled: mr,
                                BIconAspectRatio: Wl,
                                BIconMoon: lH,
                                BIconSun: sf,
                            },
                        }),
                    },
                },
                { NAME: "BootstrapVueIcons" }
            );
        return (A = qL), f && window.Vue && window.Vue.use(A), f && A.NAME && (window[A.NAME] = A), qL;
    }),
    "object" == typeof exports && "undefined" != typeof module
        ? (module.exports = l(require("vue")))
        : "function" == typeof define && define.amd
            ? define(["vue"], l)
            : ((a = "undefined" != typeof globalThis ? globalThis : a || self).bootstrapVueIcons = l(a.Vue));
//# sourceMappingURL=bootstrap-vue-icons.min.js.map
