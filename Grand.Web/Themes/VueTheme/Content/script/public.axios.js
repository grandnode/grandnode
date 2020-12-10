
/*
** axios cart implementation
*/
var AxiosCart = {
    loadWaiting: false,
    topcartselector: '',
    topwishlistselector: '',
    flyoutcartselector: '',

    init: function (topcartselector, topwishlistselector, flyoutcartselector) {
        this.loadWaiting = false;
        this.topcartselector = topcartselector;
        this.topwishlistselector = topwishlistselector;
        this.flyoutcartselector = flyoutcartselector;
    },

    setLoadWaiting: function (display) {
        //displayAxiosLoading(display);
        this.loadWaiting = display;
    },

    quickview_product: function (quickviewurl) {
        axios({
            url: quickviewurl,
            method: 'post',
        }).then(function (response) {
            this.AxiosCart.success_process(response);
        }).catch(function (error) {
            error.axiosFailure;
        }).then(function (response) {
            this.AxiosCart.resetLoadWaiting();
        });  
    },

    //add a product to the cart/wishlist from the catalog pages
    addproducttocart_catalog: function (urladd, showqty, productid) {
        if (showqty.toLowerCase() == 'true') {
            var qty = document.querySelector('#addtocart_' + productid + '_EnteredQuantity').value;
            if (urladd.indexOf("forceredirection") != -1) {
                urladd += '&quantity=' + qty;
            }
            else {
                urladd += '?quantity=' + qty;
            }
        }
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        axios({
            url: urladd,
            method: 'post',
        }).then(function (response) {
            this.AxiosCart.success_process(response);
        }).catch(function (error) {
            error.axiosFailure;
        }).then(function (response) {
            this.AxiosCart.resetLoadWaiting();
        });  
    },

    //add a product to the cart/wishlist from the product details page
    addproducttocart_details: function (urladd, formselector) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);
        if (document.querySelector("#ModalQuickView")) {
            var form = document.querySelector('#ModalQuickView #product-details-form');
        } else {
            var form = document.querySelector('.product-standard #product-details-form');
        }
        var data = new FormData(form);
        axios({
            url: urladd,
            data: data,
            method: 'post',
        }).then(function (response) {
            this.AxiosCart.success_process(response); 
        }).catch(function (error) {
            error.axiosFailure;
        }).then(function () {
            this.AxiosCart.resetLoadWaiting();
        });  
    },

    //add bid
    addbid: function (urladd, formselector) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);
        var form = document.querySelector(formselector);
        var data = new FormData(form);
        axios({
            url: urladd,
            data: data,
            method: 'post',
        })
        .then(function (response) {
            this.AxiosCart.success_process(response);
        }).catch(function (error) {
            error.axiosFailure;
        }).then(function () {
            this.AxiosCart.resetLoadWaiting();
        });  
    },
    //add a product to compare list
    addproducttocomparelist: function (urladd) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        axios({
            url: urladd,
            method: 'post'
        }).then(function (response) {
            this.AxiosCart.success_process(response);
        }).catch(function (error) {
            error.axiosFailure;
        }).then(function () {
            this.AxiosCart.resetLoadWaiting();
        });  
    },

    success_process: function (response) {
        if (response.data.updatetopwishlistsectionhtml) {
            document.querySelector(AxiosCart.topwishlistselector).innerHTML = response.data.updatetopwishlistsectionhtml;
        }
        if (response.data.flyoutshoppingcartmodel) {
            var newfly = response.data.flyoutshoppingcartmodel;
            this.flycart = newfly;
            this.flycartitems = newfly.Items;
            this.flycartindicator = newfly.TotalProducts;
            vm.flycart = newfly;
            vm.flycartitems = newfly.Items;
            vm.flycartindicator = newfly.TotalProducts;

        }
        if (response.data.comparemessage) {
            if (response.data.success == true) {
                displayBarNotification(response.data.comparemessage, 'success', 3500);
            }
            else {
                displayBarNotification(response.data.comparemessage, 'error', 3500);
            }
            return false;
        }
        if (response.data.product) {
            if (response.data.success == true) {
                displayPopupQuickView(response.data.html);
            }
        }
        if (response.data.message) {
            //display notification
            if (response.data.success == true) {
                //success
                //$("#ModalQuickView .close").click();
                displayPopupAddToCart(response.data.html);

                if (response.data.refreshreservation == true) {
                    var param = "";
                    if ($("#parameterDropdown").val() != null) {
                        param = $("#parameterDropdown").val();
                    }

                    Reservation.fillAvailableDates(Reservation.currentYear, Reservation.currentMonth, param, true);
                }

            }
            else {
                //error
                displayBarNotification(response.data.message, 'error', 3500);
            }
            return false;
        }
        if (response.data.redirect) {
            location.href = response.data.redirect;
            return true;
        }
        return false;
    },

    resetLoadWaiting: function () {
        AxiosCart.setLoadWaiting(false);
    },

    axiosFailure: function () {
        alert('Failed to add the product. Please refresh the page and try one more time.');
    }
};