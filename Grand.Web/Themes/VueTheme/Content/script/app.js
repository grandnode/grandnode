﻿var vm = new Vue({
    el: '#app',
    data: function() {
        return {
            show: false,
            fluid: false,
            hover: false,
            darkMode: false,
            active: false,
            NextDropdownVisible: false,
            value: 5,
            searchitems: null,
            searchcategories: null,
            searchmanufacturers: null,
            searchblog: null,
            searchproducts: null,
        }
    },
    props: {
        flycart: null,
        flycartitems: null,
        flycartindicator: null,
    },
    mounted: function () {
        if (localStorage.fluid == "true") this.fluid = "fluid";
        if (localStorage.fluid == "fluid") this.fluid = "fluid";
        if (localStorage.fluid == "") this.fluid = "false";
        if (localStorage.darkMode == "true") this.darkMode = true;
        this.updateFly();
    },
    watch: {
        fluid: function (newName) {
            localStorage.fluid = newName;
        },
        darkMode: function (newValue) {
            localStorage.darkMode = newValue;
        },
    },
    methods: {
        updateFly: function () {
            axios({
                baseURL: '/Component/Index?Name=FlyoutShoppingCart',
                method: 'get',
                data: null,
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'X-Response-View': 'Json'
                }
            }).then(response => (
                this.flycart = response.data,
                this.flycartitems = response.data.Items,
                this.flycartindicator = response.data.TotalProducts
            ))
        },
        showModalBackInStock: function () {
            this.$refs['back-in-stock'].show()
        },
        productImage: function (event) {
            var Imagesrc = event.target.parentElement.getAttribute('data-href');
            function collectionHas(a, b) {
                for (var i = 0, len = a.length; i < len; i++) {
                    if (a[i] == b) return true;
                }
                return false;
            }
            function findParentBySelector(elm, selector) {
                var all = document.querySelectorAll(selector);
                var cur = elm.parentNode;
                while (cur && !collectionHas(all, cur)) {
                    cur = cur.parentNode;
                }
                return cur;
            }

            var yourElm = event.target
            var selector = ".product-box";
            var parent = findParentBySelector(yourElm, selector);
            var Image = parent.querySelectorAll(".main-product-img")[0];
            Image.setAttribute('src', Imagesrc);
        },
        validateBeforeSubmit: function (event) {
            this.$validator.validateAll().then((result) => {
                if (result) {
                    event.srcElement.submit();
                    return
                } else {
                    if (vm.$refs.selected !== undefined && vm.$refs.selected.checked) {
                        event.srcElement.submit();
                        return
                    }
                    if (vm.$refs.visible !== undefined && vm.$refs.visible.style.display == "none") {
                        event.srcElement.submit();
                        return
                    }
                }
            });
        },
        validateBeforeClick: function (event) {
            this.$validator.validateAll().then((result) => {
                if (result) {
                    var callFunction = event.srcElement.getAttribute('data-click');
                    eval(callFunction)
                    return
                }
            });
        },
        validateBeforeSubmitParam: function (event,param) {
            this.$validator.validateAll().then((result) => {
                if (result) {
                    var para = document.createElement("input");
                    para.name = param;
                    para.type = 'hidden';
                    event.srcElement.appendChild(para);
                    event.srcElement.submit();
                    return
                } else {
                    if ((vm.$refs.selected !== undefined && vm.$refs.selected.checked) ||
                        (vm.$refs.visible !== undefined && vm.$refs.visible.style.display == "none")) {
                        var para = document.createElement("input");
                        para.name = param;
                        para.type = 'hidden';
                        event.srcElement.appendChild(para);
                        event.srcElement.submit();
                        return
                    }
                }
            });
        },
        isMobile: function () {
            return (typeof window.orientation !== "undefined") || (navigator.userAgent.indexOf('IEMobile') !== -1);
        },
    }
});
