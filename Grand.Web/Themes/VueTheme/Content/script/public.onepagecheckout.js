/*
** one page checkout
*/

var Checkout = {
    loadWaiting: false,
    failureUrl: false,

    init: function (failureUrl) {
        this.loadWaiting = false;
        this.failureUrl = failureUrl;
        //Accordion.disallowAccessToNextSections = true;
    },

    axiosFailure: function () {
        location = Checkout.failureUrl;
    },

    _disableEnableAll: function (element, isDisabled) {
        var descendants = element.querySelectorAll('*');
        descendants.forEach(function(d) {
            if (isDisabled) {
                d.setAttribute('disabled', 'disabled');
            } else {
                d.removeAttribute('disabled');
            }
        });

        if (isDisabled) {
                element.setAttribute('disabled', 'disabled');
            } else {
                element.removeAttribute('disabled');
            }
    },

    setLoadWaiting: function (step, keepDisabled) {
        if (step) {
            if (this.loadWaiting) {
                this.setLoadWaiting(false);
            }
            var container = document.querySelector('#' + step + '-buttons-container');
            container.classList.add('disabled');
            container.style.opacity = '0.5';
            this._disableEnableAll(container, true);
            document.querySelector('#' + step + '-please-wait').style.display = 'block';
        } else {
            if (this.loadWaiting) {
                var container = document.querySelector('#' + this.loadWaiting + '-buttons-container');
                var isDisabled = (keepDisabled ? true : false);
                if (!isDisabled) {
                    container.classList.remove('disabled');
                    container.style.opacity = '1';
                }
                this._disableEnableAll(container, isDisabled);
                document.querySelector('#' + this.loadWaiting + '-please-wait').style.display = 'none';
            }
        }
        this.loadWaiting = step;
    },

    gotoSection: function (section) {
        section = document.querySelector('#button-' + section);
        section.classList.add("allow");
    },

    back: function () {
        if (this.loadWaiting) return;
    },

    setStepResponse: function (response) {
        if (response.data.update_section.name) {
            if (response.data.goto_section == "shipping") {
                var model = response.data.update_section.model;
                vmorder.ShippingAllowPickUpInStore = model.AllowPickUpInStore;
                vmorder.ShippingAllowPickUpInStore = model.AllowPickUpInStore;
                vmorder.ShippingExistingAddresses = model.ExistingAddresses;
                vmorder.ShippingNewAddress = model.NewAddress;
                vmorder.ShippingNewAddressPreselected = model.NewAddressPreselected;
                vmorder.ShippingPickUpInStore = model.PickUpInStore;
                vmorder.ShippingPickUpInStoreOnly = model.PickUpInStoreOnly;
                vmorder.ShippingPickupPoints = model.PickupPoints;
                vmorder.ShippingWarnings = model.Warnings;
                vmorder.ShippingAddress = true;
            }
            if (response.data.goto_section == "shipping_method") {
                var model = response.data.update_section.model;
                vmorder.NotifyCustomerAboutShippingFromMultipleLocations = model.NotifyCustomerAboutShippingFromMultipleLocations;
                vmorder.ShippingMethods = model.ShippingMethods;
                vmorder.ShippingMethodWarnings = model.Warnings;
                vmorder.ShippingMethod = true;
                if (model.ShippingMethods.length > 0) {
                    var elem = model.ShippingMethods[0].Name + '___' + model.ShippingMethods[0].ShippingRateComputationMethodSystemName;
                    loadPartialView(elem);
                }
                this.updateOrderTotal();
            }
            if (response.data.goto_section == "payment_method") {
                var model = response.data.update_section.model;
                vmorder.DisplayRewardPoints = model.DisplayRewardPoints;
                vmorder.PaymentMethods = model.PaymentMethods;
                vmorder.RewardPointsAmount = model.RewardPointsAmount;
                vmorder.RewardPointsBalance = model.RewardPointsBalance;
                vmorder.RewardPointsEnoughToPayForOrder = model.RewardPointsEnoughToPayForOrder;
                vmorder.UseRewardPoints = model.UseRewardPoints;
                vmorder.PaymentMethod = true;

                this.updateOrderTotal();

            }
            if (response.data.goto_section == "payment_info") {
                var model = response.data.update_section.model;
                vmorder.DisplayOrderTotals = model.DisplayOrderTotals;
                vmorder.PaymentViewComponentName = model.PaymentViewComponentName;
                vmorder.PaymentInfo = true;

                axios({
                    baseURL: '/Component/Index?Name=' + model.PaymentViewComponentName,
                    method: 'get',
                    data: null,
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json',
                    }
                }).then(response => {
                    var html = response.data;
                    document.querySelector('.payment-info .info').innerHTML = html;
                }).then(function () {
                    if (document.querySelector('.script-tag-info')) {
                        runScripts(document.querySelector('.script-tag-info'))
                    }
                })

                if (model.DisplayOrderTotals) {
                    this.updateOrderSummary(false);
                }

                this.updateOrderTotal();
                
            }
            if (response.data.goto_section == "confirm_order") {
                var model = response.data.update_section.model;
                vmorder.MinOrderTotalWarning = model.MinOrderTotalWarning;
                vmorder.ConfirmWarnings = model.Warnings;

                vmorder.Confirm = true;

                setTimeout(function () {
                    var c_back = document.getElementById('back-confirm_order').getAttribute('onclick');
                    document.getElementById('new-back-confirm_order').setAttribute('onclick', c_back);
                }, 300);

                this.updateOrderSummary(true);
                this.updateOrderTotal();
            }

            if (!response.data.wrong_billing_address) {
                if (!(document.querySelector("#opc-confirm-order").classList.contains('show'))) {
                    vm.$root.$emit('bv::toggle::collapse', 'opc-' + response.data.update_section.name)
                    resetSteps(document.querySelector('#opc-' + response.data.update_section.name));
                }
            }
        }
        if (response.data.allow_sections) {
            response.data.allow_sections.forEach(function (e) {
                document.querySelector('#button-' + e).classList.add('allow');
            });
        }
        
        if (document.querySelector("#billing-address-select")) {
            Billing.newAddress(!document.querySelector('#billing-address-select').value);
        }
        if (document.querySelector("#shipping-address-select")) {
            Shipping.newAddress(!document.querySelector('#shipping-address-select').value);
        }

        if (response.data.update_section) {
            Checkout.gotoSection(response.data.update_section.name);
            return true;
        }
        if (response.data.redirect) {
            location.href = response.data.redirect;
            return true;
        }
        return false;
    },
    updateOrderSummary: function (displayOrderReviewData) {
        axios({
            baseURL: '/Component/Index?Name=OrderSummary',
            method: 'post',
            data: {
                prepareAndDisplayOrderReviewData: displayOrderReviewData,
            },
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'X-Response-View': 'Json'
            }
        }).then(response => {
            vmorder.cart.OrderReviewData = response.data.OrderReviewData
        });
    },

    updateOrderTotal: function () {
        axios({
            baseURL: '/Component/Index?Name=OrderTotals',
            method: 'get',
            data: null,
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'X-Response-View': 'Json'
            }
        }).then(response => {
            vmorder.totals = response.data;
        });
    },
};





var Billing = {
    form: false,
    saveUrl: false,
    disableBillingAddressCheckoutStep: false,

    init: function (form, saveUrl, disableBillingAddressCheckoutStep) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.disableBillingAddressCheckoutStep = disableBillingAddressCheckoutStep;
    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            document.querySelector('#billing-new-address-form').style.display = 'block';
        } else {
            document.querySelector('#billing-new-address-form').style.display = 'none';
        }
    },

    resetSelectedAddress: function () {
        var selectElement = document.querySelector('#billing-address-select');
        if (selectElement) {
            selectElement.value = '';
        }
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('billing');

        var form = document.querySelector(this.form);
        var data = new FormData(form);
        axios({
            url: this.saveUrl,
            method: 'post',
            data: data,
        }).then(function (response) {
            if (!response.data.wrong_billing_address) {
                if (document.querySelector("#billing_card").style['display'] != 'none') {
                    if (document.querySelector('#back-' + response.data.goto_section)) {
                        document.querySelector('#back-' + response.data.goto_section).setAttribute('onclick', 'document.querySelector("#button-billing").click()');
                    }
                }
            }
            this.Billing.nextStep(response);
        }).catch(function (error) {
            alert(error);
        }).then(function () {
            this.Billing.resetLoadWaiting();
        }); 
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        //ensure that response.wrong_billing_address is set
        //if not set, "true" is the default value
        if (typeof response.data.wrong_billing_address == 'undefined') {
            response.data.wrong_billing_address = false;
        }
        if (Billing.disableBillingAddressCheckoutStep) {
            if (response.data.wrong_billing_address) {
                document.querySelector("#billing_card").style.display = "flex";
                document.getElementById('button-billing').classList.add('allow');
            } else {
                document.getElementById('button-billing').classList.remove('allow');
            }
        }


        if (response.data.error) {
            if ((typeof response.data.message) == 'string') {
                alert(response.data.message);
            } else {
                alert(response.data.message.join("\n"));
            }

            return false;
        }
        Checkout.setStepResponse(response);
    }
};



var Shipping = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            document.querySelector('#shipping-new-address-form').style.display = 'block';
        } else {
            document.querySelector('#shipping-new-address-form').style.display = 'none';
        }
    },

    togglePickUpInStore: function (pickupInStoreInput) {
        if (pickupInStoreInput.checked) {
            document.querySelector('#shipping-addresses-form').style.display = 'none';
            document.querySelector('#pickup-points-form').style.display = 'block';
        }
        else {
            document.querySelector('#shipping-addresses-form').style.display = 'block';
            document.querySelector('#pickup-points-form').style.display = 'none';
        }
    },

    resetSelectedAddress: function () {
        var selectElement = document.querySelector('#shipping-address-select');
        if (selectElement) {
            selectElement.value = '';
        }
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;
        Checkout.setLoadWaiting('shipping');

        var form = document.querySelector(this.form);
        var data = new FormData(form);
        axios({
            url: this.saveUrl,
            method: 'post',
            data: data,
        }).then(function (response) {
            if (response.data.goto_section !== undefined) {
                if (!(response.data.update_section.name == "shipping")) {
                    this.Shipping.nextStep(response);
                }
                document.querySelector('#back-' + response.data.goto_section).setAttribute('onclick', 'document.querySelector("#button-shipping").click()');
            }
        }).catch(function (error) {
            error.axiosFailure;
        }).then(function () {
            this.Billing.resetLoadWaiting();
        }); 
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.data.error) {
            if ((typeof response.data.message) == 'string') {
                alert(response.data.message);
            } else {
                alert(response.data.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var ShippingMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    validate: function() {
        var methods = document.getElementsByName('shippingoption');
        if (methods.length==0) {
            alert('Your order cannot be completed at this time as there is no shipping methods available for it. Please make necessary changes in your shipping address.');
            return false;
        }

        for (var i = 0; i< methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify shipping method.');
        return false;
    },
    
    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        if (this.validate()) {
            Checkout.setLoadWaiting('shipping-method');
        
            var form = document.querySelector(this.form);
            var data = new FormData(form);
            axios({
                url: this.saveUrl,
                method: 'post',
                data: data,
            }).then(function (response) {
                this.ShippingMethod.nextStep(response);
                document.querySelector('#back-' + response.data.goto_section).setAttribute('onclick', 'document.querySelector("#button-shipping-method").click()');
            }).catch(function (error) {
                error.axiosFailure;
            }).then(function () {
                this.ShippingMethod.resetLoadWaiting();
            }); 
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.data.error) {
            if ((typeof response.data.message) == 'string') {
                alert(response.data.message);
            } else {
                alert(response.data.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    toggleUseRewardPoints: function (useRewardPointsInput) {
        if (useRewardPointsInput.checked) {
            document.querySelector('#payment-method-block').style.display = 'none';
        }
        else {
            document.querySelector('#payment-method-block').style.display = 'block';
        }
    },

    validate: function () {
        var methods = document.getElementsByName('paymentmethod');
        if (methods.length == 0) {
            alert('Your order cannot be completed at this time as there is no payment methods available for it.');
            return false;
        }
        
        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify payment method.');
        return false;
    },
    
    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        if (this.validate()) {
            Checkout.setLoadWaiting('payment-method');
            var form = document.querySelector(this.form);
            var data = new FormData(form);
            axios({
                url: this.saveUrl,
                method: 'post',
                data: data,
            }).then(function (response) {
                if (response.data.goto_section !== undefined) {
                    this.PaymentMethod.nextStep(response);
                    document.querySelector('#back-' + response.data.goto_section).setAttribute('onclick', 'document.querySelector("#button-payment-method").click()');
                }
            }).catch(function (error) {
                error.axiosFailure;
            }).then(function () {
                this.PaymentMethod.resetLoadWaiting();
            }); 
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.data.error) {
            if ((typeof response.data.message) == 'string') {
                alert(response.data.message);
            } else {
                alert(response.data.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentInfo = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        Checkout.setLoadWaiting('payment-info');
        var form = document.querySelector(this.form);
        var data = new FormData(form);

        axios({
            url: this.saveUrl,
            method: 'post',
            data: data,
        }).then(function (response) {
            if (response.data.goto_section !== undefined) {
                this.PaymentInfo.nextStep(response);
                document.querySelector('#back-' + response.data.goto_section).setAttribute('onclick', 'document.querySelector("#button-payment-info").click()');
            }
            if (response.data.update_section !== undefined && response.data.update_section.name == 'payment-info') {
                var model = response.data.update_section.model;
                vm.DisplayOrderTotals = model.DisplayOrderTotals;
                vm.PaymentViewComponentName = model.PaymentViewComponentName,
                vm.PaymentInfo = true;

                axios({
                    baseURL: '/Component/Form?Name=' + model.PaymentViewComponentName,
                    method: 'post',
                    data: data,
                }).then(response => {
                    var html = response.data;
                    document.querySelector('.payment-info .info').innerHTML = html;
                })
                
            }

        }).catch(function (error) {
            error.axiosFailure;
        }).then(function () {
            this.PaymentInfo.resetLoadWaiting()
        }); 
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.data.error) {
            if ((typeof response.data.message) == 'string') {
                alert(response.data.message);
            } else {
                alert(response.data.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var ConfirmOrder = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (saveUrl, successUrl) {
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        // terms of service
        var termOfServiceOk = true;
        if (termOfServiceOk) {
            Checkout.setLoadWaiting('confirm-order');
            axios({
                url: this.saveUrl,
                method: 'post',
            }).then(function (response) {
                this.ConfirmOrder.nextStep(response);
            }).catch(function (error) {
                error.axiosFailure;
            }).then(function () {
                this.ConfirmOrder.resetLoadWaiting()
            }); 
        } else {
            return false;
        }
    },
    
    resetLoadWaiting: function (transport) {
        Checkout.setLoadWaiting(false, ConfirmOrder.isSuccess);
    },

    nextStep: function (response) {
        if (response.data.error) {
            if ((typeof response.data.message) == 'string') {
                alert(response.data.message);
            } else {
                alert(response.data.message.join("\n"));
            }

            return false;
        }
        
        if (response.data.redirect) {
            ConfirmOrder.isSuccess = true;
            location.href = response.data.redirect;
            return;
        }
        if (response.data.success) {
            ConfirmOrder.isSuccess = true;
            window.location = ConfirmOrder.successUrl;
        }
        Checkout.setStepResponse(response);
    }
};  