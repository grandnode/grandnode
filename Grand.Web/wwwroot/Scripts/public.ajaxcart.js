/*
** ajax cart implementation
*/
var AjaxCart = {
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
        displayAjaxLoading(display);
        this.loadWaiting = display;
    },

    quickview_product: function (quickviewurl) {
        this.setLoadWaiting(true);
        $.ajax({
            cache: false,
            url: quickviewurl,
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        })
    },

    //add a product to the cart/wishlist from the catalog pages
    addproducttocart_catalog: function (urladd, showqty, productid) {
        if (showqty.toLowerCase() == 'true') {
            var qty = $('#addtocart_' + productid + '_EnteredQuantity').val();
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

        $.ajax({
            cache: false,
            url: urladd,
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //add a product to the cart/wishlist from the product details page
    addproducttocart_details: function (urladd, formselector) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);
        $.ajax({
            cache: false,
            url: urladd,
            data: $(formselector).serialize(),
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //add bid
    addbid: function (urladd, formselector) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);
        $.ajax({
            cache: false,
            url: urladd,
            data: $(formselector).serialize(),
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    //add a product to compare list
    addproducttocomparelist: function (urladd) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urladd,
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    success_process: function (response) {
        if (response.updatetopcartsectionhtml) {
            $(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
        }
        if (response.updatetopwishlistsectionhtml) {
            $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
        }
        if (response.updateflyoutcartsectionhtml) {
            $(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
        }
        if (response.comparemessage) {
            if (response.success == true) {
                displayBarNotification(response.comparemessage, 'success', 3500);
            }
            else {
                displayBarNotification(response.comparemessage, 'error', 3500);
            }
            return false;
        }
        if (response.product) {
            if (response.success == true) {
                $("#ModalQuickView .product-quickview").remove();
                displayPopupQuickView(response.html);
            }
        }


        if (response.message) {
            //display notification
            if (response.success == true) {
                //success
                $("#ModalQuickView .close").click();
                displayPopupAddToCart(response.html);

                if (response.refreshreservation == true) {
                    var param = "";
                    if ($("#parameterDropdown").val() != null) {
                        param = $("#parameterDropdown").val();
                    }

                    Reservation.fillAvailableDates(Reservation.currentYear, Reservation.currentMonth, param, true);
                }

            }
            else {
                //error
                displayBarNotification(response.message, 'error', 3500);
            }
            return false;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    },

    resetLoadWaiting: function () {
        AjaxCart.setLoadWaiting(false);
    },

    ajaxFailure: function () {
        alert('Failed to add the product. Please refresh the page and try one more time.');
    }
};