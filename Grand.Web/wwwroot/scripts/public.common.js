/*
** custom js functions
*/
$(document).ready(function () {
    $('body').addClass('is-ready');
    $(".categoryGrid .flex-grid, .manufacturerGrid .flex-grid, .customGrid .flex-grid").scrollLeft(30);
});

// meno scrolls up and down

function MenuScroll() {
    var body = document.body;
    var scrollUp = "scroll-up";
    var scrollDown = "scroll-down";
    var onTop = "onTop";
    let lastScroll = 0;

    $(document).scroll(function () {
        var currentScroll = window.pageYOffset;
        if ($(this).scrollTop() <= 120) {
            $(body).addClass(onTop);
        } else {
            $(body).removeClass(onTop);
        }
        if (currentScroll > lastScroll && !$(body).hasClass(scrollDown)) {
            // down
            $(body).removeClass(scrollUp);
            if (lastScroll != 0) {
                $(body).addClass(scrollDown);
                return;
            } else {
                $(body).removeClass(scrollDown);
            }
        } else if (currentScroll < lastScroll && $(body).hasClass(scrollDown)) {
            // up
            $(body).removeClass(scrollDown);
            $(body).addClass(scrollUp);
        }
        lastScroll = currentScroll;
    });
}

// offcanvas

$(function () {
    $(document).bind("beforecreate.offcanvas", function (e) {
        var dataOffcanvas = $(e.target).data('offcanvas-component');
    });
    $(document).bind("create.offcanvas", function (e) {
        var dataOffcanvas = $(e.target).data('offcanvas-component');
        //console.log(dataOffcanvas);
        dataOffcanvas.onOpen = function () {
            //console.log('Callback onOpen');
        };
        dataOffcanvas.onClose = function () {
            //console.log('Callback onClose');
        };

    });
    $(document).bind("clicked.offcanvas-trigger clicked.offcanvas", function (e) {
        var dataBtnText = $(e.target).text();
        //console.log(e.type + '.' + e.namespace + ': ' + dataBtnText);
    });
    $(document).bind("open.offcanvas", function (e) {
        var dataOffcanvasID = $(e.target).attr('id');
        //console.log(e.type + ': #' + dataOffcanvasID);
    });
    $(document).bind("resizing.offcanvas", function (e) {
        var dataOffcanvasID = $(e.target).attr('id');
        //console.log(e.type + ': #' + dataOffcanvasID);
    });
    $(document).bind("close.offcanvas", function (e) {
        var dataOffcanvasID = $(e.target).attr('id');
        //console.log(e.type + ': #' + dataOffcanvasID);
    });
    $(document).trigger("enhance");
});

// main menu system

function mainMenuReplace() {
    if (window.matchMedia('(max-width: 991px)').matches) {
        $('.mainNav .navbar-collapse').prependTo('#mobile_menu');
        Popper.Defaults.modifiers.computeStyle.enabled = false;
        $("#mobile_menu .nav-item.dropdown .dropdown-toggle").each(function () {
            $(this).on("click", function (e) {
                e.preventDefault();
                $(this).parent().addClass("show");
                $(this).parent().find(".dropdown-menu:first").addClass("show");
            });
        });
        $("#mobile_menu .nav-item.cat-back").each(function () {
            $(this).on("click", function () {
                $(this).parents(".dropdown:first").removeClass("show");
                $(this).parents(".dropdown-menu:first").removeClass("show");
            });
        });
    }
    else {
        $('#mobile_menu .navbar-collapse').prependTo('.mainNav');
        Popper.Defaults.modifiers.computeStyle.enabled = true;
    }
}

// search box

function searchReplace() {
    if (window.matchMedia('(max-width: 991px)').matches) {
        if ($("#searchModal #small-search-box-form").length < 1) {
            $('#small-search-box-form').prependTo('#searchModal');
        }
    }
    else {
        if ($(".formSearch #small-search-box-form").length < 1) {
            $('#small-search-box-form').prependTo('.formSearch');
        }
    }
}

// back to top

function BackToTop() {
    if ($('#back-to-top').length) {
        var scrollTrigger = 100, // px
            backToTop = function () {
                var scrollTop = $(window).scrollTop();
                if (scrollTop > scrollTrigger) {
                    $('#back-to-top').addClass('show');
                } else {
                    $('#back-to-top').removeClass('show');
                }
            };
        backToTop();
        $(window).on('scroll', function () {
            backToTop();
        });
        $('#back-to-top').on('click', function (e) {
            e.preventDefault();
            $('html,body').animate({
                scrollTop: 0
            }, 1000);
        });
    }
}
function edgeFix() {
    if (document.documentMode || /Edge/.test(navigator.userAgent)) {
        if (typeof ($.fn.popover) != 'undefined') {
            $("body").addClass("IE-ready");
        }
    }
}

// auctions

function dataCountdown() {
    $('[data-countdown]').each(function () {
        var $this = $(this), finalDate = $(this).data('countdown');
        $this.countdown(finalDate, function (event) {
            if (event.strftime('%D') > 0) {
                $this.html(event.strftime('%D days %H:%M:%S'));
            }
            else {
                $this.html(event.strftime('%H:%M:%S'));
            }
        });
    });
}

// flyCart on Cart fix 

function CartFix() {
    var pathname = window.location.pathname;
    if (pathname === '/cart') {
        $("#topcartlink").css("pointer-events", "none");
    }
}

// left-side canvas

function LeftSide() {
    if ($(window).width() < 991) {
        $('.generalLeftSide').prependTo('#leftSide');
    }
    else {
        $('.generalLeftSide').insertBefore('.generalSideRight');
    }
}

// productbox info

function productInfo() {
    $('.product-box').each(function () {
        var PB_bottom_h = $('.product-info .bottom', this).height();
        $('.box-unvisible', this).css('margin-bottom', - PB_bottom_h);
    });
}

// tooltips

$(function () {
    $('.product-box [data-tooltip="title"]').tooltip();
});

$(document).ready(function () {

    MenuScroll();
    edgeFix();
    CartFix();
    mainMenuReplace();
    searchReplace();
    LeftSide();
    itemsStatistics();
    dataCountdown();
    BackToTop();
    productInfo(); 

    $(window).resize(function () {
        mainMenuReplace();
        searchReplace();
        LeftSide();
        productInfo();
    });

    function newsletter_subscribe(subscribe) {
        var subscribeProgress = $("#subscribe-loading-progress");
        subscribeProgress.show();
        var postData = {
            subscribe: subscribe,
            email: $("#newsletter-email").val()
        };
        var href = $("#newsletterbox").closest('[data-href]').data('href');
        $.ajax({
            cache: false,
            type: "POST",
            url: href,
            data: postData,
            success: function (data) {
                subscribeProgress.hide();
                $("#newsletter-result-block").html(data.Result);
                if (data.Success) {
                    $('.newsletter-button-container, #newsletter-email, .newsletter-subscribe-unsubscribe').hide();
                    $('#newsletter-result-block').addClass("d-block").show().css("bottom", "unset");
                    if (data.Showcategories) {
                        $('#nc_modal_form').html(data.ResultCategory);
                        window.setTimeout(function () {
                            $('.nc-action-form').magnificPopup('open');
                        }, 100);
                    }
                } else {
                    $('#newsletter-result-block').fadeIn("slow").delay(2000).fadeOut("slow");
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to subscribe.');
                subscribeProgress.hide();
            }
        });
    }
    $('#newsletter-subscribe-button').click(function () {
        var allowToUnsubscribe = $("#newsletterbox").data('allowtounsubscribe').toLowerCase();
        if (allowToUnsubscribe == 'true') {
            if ($('#newsletter_subscribe').is(':checked')) {
                newsletter_subscribe('true');
            }
            else {
                newsletter_subscribe('false');
            }
        }
        else {
            newsletter_subscribe('true');
        }
    });

    $("#newsletter-email").keydown(function (event) {
        if (event.keyCode == 13) {
            $("#newsletter-subscribe-button").trigger("click")
            return false;
        }
    });

    $('#small-searchterms').blur(function () {
        if ($(this).val().length === 0) {
            $(".advanced-search-results").removeClass("open");
        }
    });

    $('#small-searchterms').on('keydown', function () {
        var key = event.keyCode || event.charCode;

        if (key == 8 || key == 46)
            $(".advanced-search-results").removeClass("open");
    });

    $('.product-standard .review-scroll-button').on('click', function (e) {
        var el = $("#review-tab");
        var elOffset = el.offset().top;
        var elHeight = el.height();
        var windowHeight = $(window).height();
        var offset;
        if (elHeight < windowHeight) {
            offset = elOffset - ((windowHeight / 2) - (elHeight / 2));
        }
        else {
            offset = elOffset;
        }
        $.smoothScroll({ speed: 300 }, offset);
        $("#review-tab").click();
        return false;
    });

    $('#ModalQuickView').on('hide.bs.modal', function (e) {
        $('#ModalQuickView').empty();
    });

    $('#ModalAddToCart .modal-dialog').on('click tap', function (e) {
        if ($(e.target).hasClass('modal-dialog')) {
            $('.modal').modal('hide');
        }
    });
    $('#ModalPrivacyPreference').on('hide.bs.modal', function (e) {
        $('#ModalPrivacyPreference').empty();
    });
});

function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}

function setLocation(url) {
    window.location.href = url;
}

function displayAjaxLoading(display) {
    if (display) {
        $('.ajax-loading-block-window').show();
    }
    else {
        $('.ajax-loading-block-window').hide('slow');
    }
}

function displayPopupNotification(message, messagetype, modal) {
    //types: success, error
    var container;
    if (messagetype == 'success') {
        //success
        container = $('#dialog_success');
        $('#dialog_error').html('');
    }
    else {
        //error
        container = $('#dialog_error');
        $('#dialog_success').html('');
    }

    //we do not encode displayed message
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<div class="p-3"><h5 class="text-center">' + message + '</h5></div>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p>' + message[i] + '</p>';
        }
    }
    container.html(htmlcode);
    $('#generalModal').modal('show');
}

function closeOffcanvas() {
    var dataOffcanvas = $('#right').data('offcanvas-component');
    dataOffcanvas.close();
}

function displayPopupAddToCart(html) {
    $('#ModalAddToCart').html(html).modal('show');
    $("body.modal-open").removeAttr("style");
    $(".navUp").removeAttr("style");
}

function displayPopupQuickView(html) {
    $('#ModalQuickView').html(html).modal('show');
    $("body.modal-open").removeAttr("style");
    $(".navUp").removeAttr("style");
    dataCountdown();
}

function displayPopupPrivacyPreference(html) {
    $('#ModalPrivacyPreference').html(html).modal('show');
    $("body.modal-open").removeAttr("style");
    $(".navUp").removeAttr("style");
}

var barNotificationTimeout;
function displayBarNotification(message, messagetype, timeout) {
    clearTimeout(barNotificationTimeout);

    //types: success, error
    var cssclass = 'success';
    if (messagetype == 'success') {
        cssclass = 'success';
    }
    else if (messagetype == 'error') {
        cssclass = 'danger';
    }
    //remove previous CSS classes and notifications
    $('#bar-notification')
        .removeClass('success')
        .removeClass('danger');
    $('#bar-notification .toast').remove();

    //add new notifications
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<div class="toast show"><span class="close"><span class="mdi mdi-close" aria-hidden="true"></span></span><div class="content">' + message + '</div></div>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<div class="toast show"><span class="close"><span class="mdi mdi-close" aria-hidden="true"></span></span><div class="content">' + message[i] + '</div></div>';
        }
    }
    $('#bar-notification').append(htmlcode)
        .addClass(cssclass)
        .mouseenter(function () {
            clearTimeout(barNotificationTimeout);
        });

    $('#bar-notification .close').unbind('click touchstart').click(function () {
        $(this).parents(".toast").remove();
    });

    //timeout (if set)
    if (timeout > 0) {
        barNotificationTimeout = setTimeout(function () {
            $('#bar-notification .toast').removeClass('show');
        }, timeout);
    }
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}


// CSRF (XSRF) security
function addAntiForgeryToken(data) {
    //if the object is undefined, create a new one.
    if (!data) {
        data = {};
    }
    //add token
    var tokenInput = $('input[name=__RequestVerificationToken]');
    if (tokenInput.length) {
        data.__RequestVerificationToken = tokenInput.val();
    }
    return data;
};

function sendcontactusform(urladd) {
    if ($("#product-details-form").valid()) {
        var contactData = {
            AskQuestionEmail: $('#AskQuestionEmail').val(),
            AskQuestionFullName: $('#AskQuestionFullName').val(),
            AskQuestionPhone: $('#AskQuestionPhone').val(),
            AskQuestionMessage: $('#AskQuestionMessage').val(),
            Id: $('#AskQuestionProductId').val(),
            'g-recaptcha-response-value': $("textarea[id^='g-recaptcha-response']").val()
        };
        addAntiForgeryToken(contactData);
        $.ajax({
            cache: false,
            url: urladd,
            data: contactData,
            type: 'post',
            success: function (successprocess) {
                if (successprocess.success) {
                    $('#contact-us-product').hide();
                    $('.product-contact-error').hide();
                    $('.product-contact-send .card-body').html(successprocess.message);
                    $('.product-contact-send').show();
                }
                else {
                    $('.product-contact-error .card-body').html(successprocess.message);
                    $('.product-contact-error').show();
                }
            },
            error: function (error) {
                alert('Error: ' + error);
            }
        });
    }
}


function GetPrivacyPreference(href) {
    $.ajax({
        cache: false,
        url: href,
        success: function (data) {
            displayPopupPrivacyPreference(data.html)
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to get privacy preference.');
        }
    });
}
function SavePrivacyPreference(href) {
    $.ajax({
        cache: false,
        url: href,
        data: $('#frmPrivacyPreference').serialize(),
        type: 'post',
        success: function (data) {
            $('#ModalPrivacyPreference').modal('hide');
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to get privacy preference.');
        }
    });
}
function newAddress(isNew) {
    if (isNew) {
        this.resetSelectedAddress();
        $('#pickup-new-address-form').show();
    } else {
        $('#pickup-new-address-form').hide();
    }
}

function resetSelectedAddress() {
    var selectElement = $('#pickup-address-select');
    if (selectElement) {
        selectElement.val('');
    }
}

function deletecartitem(href) {
    var flyoutcartselector = AjaxCart.flyoutcartselector;
    var topcartselector = AjaxCart.topcartselector;
    $.ajax({
        cache: false,
        type: "POST",
        url: href,
        success: function (data) {
            var flyoutcart = $(flyoutcartselector, $(data.flyoutshoppingcart));
            $(flyoutcartselector).replaceWith(flyoutcart);
            $(topcartselector).html(data.totalproducts);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Failed to retrieve Flyout Shopping Cart.');
        }
    });
    return false;
}

function itemsStatistics() {
    if ($('#items_statistics').length) {
        var totalItems = parseInt($('#items_statistics .items-total').text());
        var perPageFinal = parseInt($('.items-page-size').text());
        var currentPaggingSite = 0;
        if ($('.pagination').length) {
            currentPaggingSite = parseInt($('.pagination .current-page .page-link').text());
        } else {
            currentPaggingSite = 1;
        }
        if (totalItems < currentPaggingSite * perPageFinal) {
            $('#items_statistics .items-per-page .number').text(currentPaggingSite * perPageFinal - perPageFinal + 1 + ' - ' + totalItems);
        }
        else {
            $('#items_statistics .items-per-page .number').text(currentPaggingSite * perPageFinal - perPageFinal + 1 + ' - ' + currentPaggingSite * perPageFinal);
        }
    }
}