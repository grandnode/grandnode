/*
** custom js functions
*/

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
        container = $('#dialog-error');
    }
    else if (messagetype == 'error') {
        //error
        container = $('#dialog-error');
    }
    else {
        //other
        container = $('#dialog-error');
    }

    //we do not encode displayed message
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p>' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p>' + message[i] + '</p>';
        }
    }
    container.append(htmlcode);
    $('#generalModal').modal('show');
}

$('#ModalAddToCart .modal-dialog').on('click tap', function (e) {
    if ($(e.target).hasClass('modal-dialog')) {
        $('.modal').modal('hide');
    }
})

function displayPopupAddToCart(html) {
    $('#ModalAddToCart').html(html).modal('show');
    $("body.modal-open").removeAttr("style");
    $(".navUp").removeAttr("style");
}
function displayPopupQuickView(html) {
    $('#ModalQuickView').html(html).modal('show');
    $("body.modal-open").removeAttr("style");
    $(".navUp").removeAttr("style");
    $('.sp-wrap').smoothproducts();
}

var barNotificationTimeout;
function displayBarNotification(message, messagetype, timeout) {
    clearTimeout(barNotificationTimeout);

    //types: success, error
    var cssclass = 'success';
    if (messagetype == 'success') {
        cssclass = 'card-success';
    }
    else if (messagetype == 'error') {
        cssclass = 'card-danger';
    }
    //remove previous CSS classes and notifications
    $('#bar-notification')
        .removeClass('card-success')
        .removeClass('card-danger');
    $('#bar-notification .content').remove();

    //add new notifications
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p class="content">' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p class="content">' + message[i] + '</p>';
        }
    }
    $('#bar-notification').append(htmlcode)
        .addClass(cssclass)
        .fadeIn('slow')
        .mouseenter(function () {
            clearTimeout(barNotificationTimeout);
        });

    $('#bar-notification .close').unbind('click').click(function () {
        $('#bar-notification').fadeOut('slow');
    });

    //timeout (if set)
    if (timeout > 0) {
        barNotificationTimeout = setTimeout(function () {
            $('#bar-notification').fadeOut('slow');
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



$(document).ready(function () {
    $('.block').on('click', function () {
        var windowWidth = $(window).width();
        if (windowWidth <= 992) {
            var elemId = this.id;
            $("#" + elemId).toggleClass("active");
            $("#" + elemId).siblings(".block").removeClass("active");
            $("#" + elemId).find(".viewBox").slideToggle();
            $("#" + elemId).siblings(".block").find(".viewBox").slideUp();

            $("#" + elemId).find(".fa").toggleClass("rotate");
            $("#" + elemId).siblings(".block").find(".fa").removeClass("rotate");
        };
    });

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
});

function productStarProgress5(procent, elem) {
    if (procent) {
        if (procent == 0) { productRatingStarFinal(0, 0, 5, elem); }
        else if (procent > 0 && procent <= 20) { productRatingStarFinal(1, 0, 4, elem); }
        else if (procent > 20 && procent <= 40) { productRatingStarFinal(2, 0, 3, elem); }
        else if (procent > 40 && procent <= 60) { productRatingStarFinal(3, 0, 2, elem); }
        else if (procent > 60 && procent <= 80) { productRatingStarFinal(4, 0, 1, elem); }
        else if (procent > 80 && procent <= 100) { productRatingStarFinal(5, 0, 0, elem); }
        else { productRatingStarFinal(5, 0, 0, elem); }
    } else {
        productRatingStarFinal(0, 0, 0);
    };
};
function productStarProgress10(procent, elem) {
    if (procent) {
        if (procent == 0) { productRatingStarFinal(0, 0, 5, elem); }
        else if (procent > 0 && procent < 10) { productRatingStarFinal(0, 1, 4, elem); }
        else if (procent >= 10 && procent <= 20) { productRatingStarFinal(1, 0, 4, elem); }
        else if (procent > 20 && procent <= 30) { productRatingStarFinal(1, 1, 3, elem); }
        else if (procent > 30 && procent <= 40) { productRatingStarFinal(2, 0, 3, elem); }
        else if (procent > 40 && procent <= 50) { productRatingStarFinal(2, 1, 2, elem); }
        else if (procent > 50 && procent <= 60) { productRatingStarFinal(3, 0, 2, elem); }
        else if (procent > 60 && procent <= 70) { productRatingStarFinal(3, 1, 1, elem); }
        else if (procent > 70 && procent <= 80) { productRatingStarFinal(4, 0, 1, elem); }
        else if (procent > 80 && procent < 100) { productRatingStarFinal(4, 1, 0, elem); }
        else { productRatingStarFinal(5, 0, 0, elem); }
    } else {
        productRatingStarFinal(0, 0, 0, elem);
    };
};


function productRatingStarFinal(whole, half, empty, elem) {
    var stars = [],
        fullStarr = "<i class=\"fas fa-star pr-1\"></i>",
        halfStarr = "<i class=\"fas fa-star-half pr-1\"></i><i class=\"far fa-star fix-star\"></i>",
        emptyStarr = "<i class=\"far fa-star pr-1\"></i>",
        id = "";

    if (whole == 0 && half == 0 && empty == 0) {
        stars = "";
    } else {
        for (var i = 0; i < whole; i++) { stars.push(fullStarr); };
        for (var i = 0; i < half; i++) { stars.push(halfStarr); };
        for (var i = 0; i < empty; i++) { stars.push(emptyStarr); };
        stars = stars.join("");
    };
    $("." + elem + "").html(stars);

};

// required asterisk position
$(".form-group .required").each(function () {
    var label_req = $(this).siblings("label");
    $(this).insertAfter(label_req);
});

// mobile collapsing menu
$(document).ready(function () {

    if ($(window).width() < 991) {
        $(".mainNav .navbar-nav").insertAfter("#mobile-collapsing-menu .options-container");
        $(".header-links .tax-list-mobile").insertAfter(".header-links-append");
        $(".header-links .currency-list-mobile").insertAfter(".header-links-append");
        $(".header-links .language-list-mobile").insertAfter(".header-links-append");
        $(".header-links .store-list-mobile").insertAfter(".header-links-append");
        $(".header-logo h1").insertAfter(".logo-mobile-holder");
        if ($(window).width() < 650) {
            $('#mobile-collapsing-menu li.dropdown > .dropdown-toggle').click(function () {
                window.location = $(this).attr('href');
            });
            $('#mobile-collapsing-menu .sub-cat-name').click(function () {
                window.location = $(this).attr('href');
            });
        }
    }
    $(window).resize(function () {
        if ($(window).width() < 991) {
            $(".mainNav .navbar-nav").insertAfter("#mobile-collapsing-menu .options-container");
            $(".header-links .tax-list-mobile").insertAfter(".header-links-append");
            $(".header-links .currency-list-mobile").insertAfter(".header-links-append");
            $(".header-links .language-list-mobile").insertAfter(".header-links-append");
            $(".header-links .store-list-mobile").insertAfter(".header-links-append");
            $(".header-logo h1").insertAfter(".logo-mobile-holder");
            if ($(window).width() < 650) {
                $('#mobile-collapsing-menu li.dropdown > .dropdown-toggle').click(function () {
                    window.location = $(this).attr('href');
                });
                $('#mobile-collapsing-menu .sub-cat-name').click(function () {
                    window.location = $(this).attr('href');
                });
            }
        }
        else {
            $(".logo-mobile h1").insertAfter(".logo-desktop-holder");
            $("#mobile-collapsing-menu .navbar-nav").insertAfter(".mainNav #menu_standing");
            $(".mobile-header-container .tax-list-mobile").insertAfter(".dropdowns-container .generalDropDown");
            $(".mobile-header-container .currency-list-mobile").insertAfter(".dropdowns-container .generalDropDown");
            $(".mobile-header-container .language-list-mobile").insertAfter(".dropdowns-container .generalDropDown");
        }
    });

    $("#mobile-menu-opener").click(function () {
        $("#mobile-collapsing-menu").toggleClass("show");
        $("#mobile-menu-opener").toggleClass("show");
        if ($("#back-to-top").hasClass("show")) {
            $("#back-to-top").removeClass("show");
        }
        $("#mobile-collapsing-menu .dropdown-menu").removeClass("show");
        $("body").toggleClass("noscroll");
    });

    $("#mobile-collapsing-menu .nav-item.dropdown .fa-angle-down").click(function () {
        $(this).parent().find(".dropdown-menu:first").toggleClass("show");
        $(".option-list-mobile ul").removeClass("show");
        var sub_name = $(this).parent().find(".nav-link.dropdown-toggle:first").text();
        var sub_value = $(this).parent().find(".nav-link.dropdown-toggle:first").attr("href");
        $(this).parent().find(".sub-cat-name:first").html(sub_name);
        $(this).parent().find(".sub-cat-name:first").attr("href", sub_value);
        $(".currency-button.icon-change").removeClass("icon-change");
        $(".language-button.icon-change").removeClass("icon-change");
        $(".tax-button.icon-change").removeClass("icon-change");
        $(".store-button.icon-change").removeClass("icon-change");
    });

    $("#mobile-collapsing-menu .fa-times").click(function () {
        $(this).parent().removeClass("show");
    });

    // mobile: currency, language, tax

    if ($(".tax-list-mobile").length > 0) {
    }
    else {
        $(".tax-button").hide();
    }
    if ($(".currency-list-mobile").length > 0) {
    }
    else {
        $(".currency-button").hide();
    }
    if ($(".language-list-mobile").length > 0) {
    }
    else {
        $(".language-button").hide();
    }
    if ($(".store-list-mobile").length > 0) {
    }
    else {
        $(".store-button").hide();
    }

    $(".currency-button").click(function () {
        $(".currency-list-mobile ul").toggleClass("show");
        $(".language-list-mobile ul").removeClass("show");
        $(".tax-list-mobile ul").removeClass("show");
        $(".store-list-mobile ul").removeClass("show");
        $(this).toggleClass("icon-change");
        $(".language-button.icon-change").removeClass("icon-change");
        $(".tax-button.icon-change").removeClass("icon-change");
        $(".store-button.icon-change").removeClass("icon-change");
    });

    $(".language-button").click(function () {
        $(".language-list-mobile ul").toggleClass("show");
        $(".tax-list-mobile ul").removeClass("show");
        $(".currency-list-mobile ul").removeClass("show");
        $(".store-list-mobile ul").removeClass("show");
        $(this).toggleClass("icon-change");
        $(".currency-button.icon-change").removeClass("icon-change");
        $(".tax-button.icon-change").removeClass("icon-change");
        $(".store-button.icon-change").removeClass("icon-change");
    });

    $(".tax-button").click(function () {
        $(".tax-list-mobile ul").toggleClass("show");
        $(".currency-list-mobile ul").removeClass("show");
        $(".language-list-mobile ul").removeClass("show");
        $(".store-list-mobile ul").removeClass("show");
        $(this).toggleClass("icon-change");
        $(".currency-button.icon-change").removeClass("icon-change");
        $(".language-button.icon-change").removeClass("icon-change");
        $(".store-button.icon-change").removeClass("icon-change");
    });

    $(".store-button").click(function () {
        $(".store-list-mobile ul").toggleClass("show");
        $(".currency-list-mobile ul").removeClass("show");
        $(".tax-list-mobile ul").removeClass("show");
        $(".language-list-mobile ul").removeClass("show");
        $(this).toggleClass("icon-change");
        $(".currency-button.icon-change").removeClass("icon-change");
        $(".tax-button.icon-change").removeClass("icon-change");
        $(".language-button.icon-change").removeClass("icon-change");
    });

    $("#mobile-collapsing-menu .currency-list-mobile li.active").insertBefore("#mobile-collapsing-menu .currency-list-mobile li:first");
    $("#mobile-collapsing-menu .store-list-mobile li.active").insertBefore("#mobile-collapsing-menu .store-list-mobile li:first");
    $("#mobile-collapsing-menu .tax-list-mobile li.active").insertBefore("#mobile-collapsing-menu .tax-list-mobile li:first");
    $("#mobile-collapsing-menu .language-list-mobile li img.selected").parent().parent().addClass("active").insertBefore("#mobile-collapsing-menu .language-list-mobile li:first");
    $("#mobile-collapsing-menu .language-list-mobile li.active").insertBefore("#mobile-collapsing-menu .language-list-mobile li:first");

    $(document).ready(function () {
        $(".mobile-search").click(function () {
            $("#small-search-box-form").appendTo("#searchModal .modal-content");
        });
    });
    $("#searchModal").on("hidden.bs.modal", function () {
        $("#small-search-box-form").appendTo(".formSearch");
    });

    $(".nav-item .fa").on('click', function () {
        var x = $(this).siblings("a").attr('href');
        $('a[href$="' + x + '"]').siblings(".first-level").toggleClass('open');
        $('a[href$="' + x + '"]').siblings(".next-level").toggleClass('open');
        $('a[href$="' + x + '"]').parent().siblings().children(".first-level").removeClass("open");
        $('a[href$="' + x + '"]').siblings(".fa").toggleClass("rotate");
        $('a[href$="' + x + '"]').parent(".nav-item").siblings().find(".fa").removeClass("rotate");
    });

    $(".general-opener").on('click', function () {
        $("#generalDropDown").toggle();
    });
});

$(document).ready(function () {
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        if ($(window).width() < 1050 & $(window).width() > 991) {
            $("a.dropdown-toggle").click(function (e) {
                e.preventDefault();
                $(this).unbind(e);
            });
        };
    };

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

});

// discount ribbons engine

$(document).ready(function () {
    $(".card.gen, .prod-list, .product-details-page .item-container").each(function () {
        if ($(this).find(".old-price").text().length > 0) {
            var actual_p = $(this).find(".actual-price").text().replace(",", "").replace(/^\D+/g, '');
            var old_p = $(this).find(".old-price").text().replace(",", "").replace(/^\D+/g, '');
            var promo = (Math.round((1 - actual_p / old_p) * 100));
            if (isNaN(promo)) {
                $(this).parent().parent().find(".change-percent").remove();
            } else {
                if (parseInt(promo) <= 0) {
                    $(this).find(".new-product-label").addClass("no-promo");
                }
                else {
                    var promo = "-" + (Math.round((1 - actual_p / old_p) * 100)) + "%";
                    $(this).parent().find(".change-percent").text(promo).css("opacity", "1");
                    $(this).find(".new-product-label").addClass("with-promo");
                }
            }
        }
        else {
            $(this).find(".change-percent").remove();
            $(this).find(".new-product-label").addClass("no-promo");
        }
    });
});

// mini shoppingcart engine

$(document).ready(function () {
    $("#topcartlink .btn-group").click(function () {
        if ($(this).parent().is(".fly-in")) {
            console.log(this);
            $("#topcartlink").removeClass("fly-in");
            $(".shadow-layer").removeClass("fly-in");
            $("body").removeClass("fly-in");
            $(".navUp .container").removeClass("fly-in");
            $(".dropdowns-container").removeClass("fly-in");
        }
        else {
            $(this).parent().addClass("fly-in");
            $(".shadow-layer").addClass("fly-in");
            $("body").addClass("fly-in");
            $(".navUp .container").addClass("fly-in");
            $(".dropdowns-container").addClass("fly-in");
        }
        if ($(".fly-cart-container .flyout-cart").length > 0) {
        } else {
            $(".shadow-layer").attr("style", "display:none!important;");
            $("#topcartlink").removeClass("fly-in");
            $(".shadow-layer").removeClass("fly-in");
            $("body").removeClass("fly-in");
            $(".navUp .container").removeClass("fly-in");
            $(".dropdowns-container").removeClass("fly-in");
        }
    });
});


function minicartclose() {
    $("#topcartlink").removeClass("fly-in");
    $(".shadow-layer").removeClass("fly-in");
    $("body").removeClass("fly-in");
    $(".navUp .container").removeClass("fly-in");
    $(".dropdowns-container").removeClass("fly-in");
}

$(document).mouseup(function (e) {
    var container = $("#topcartlink");
    if (!container.is(e.target) && container.has(e.target).length === 0) {
        $("#topcartlink").removeClass("fly-in");
        $(".shadow-layer").removeClass("fly-in");
        $("body").removeClass("fly-in");
        $(".navUp .container").removeClass("fly-in");
        $(".dropdowns-container").removeClass("fly-in");
    }
    $('.shadow-layer').bind("touchstart", function (e) {
        $("#topcartlink").removeClass("fly-in");
        $(".shadow-layer").removeClass("fly-in");
        $("body").removeClass("fly-in");
        $(".navUp .container").removeClass("fly-in");
        $(".dropdowns-container").removeClass("fly-in");
    });
});

// mini shoppingcart turned off

$(document).ready(function () {
    if ($(".fly-cart-container .flyout-cart").length > 0) {
    } else {
        $(".shadow-layer").attr("style", "display:none!important;");
        $("#topcartlink .dropdown-toggle").attr("href", "/cart");
    }
});

// ipad orientation-change mini shoppingcart / cart

$(document).ready(function () {
    if ($(window).width() < 991) {
        $("#topcartlink .dropdown-toggle").attr("href", "/cart");
    }
    $(window).resize(function () {
        if ($(window).width() < 991) {
            $("#topcartlink .dropdown-toggle").attr("href", "/cart");
            $(".shadow-layer").attr("style", "display:none!important;");
            $("#topcartlink").removeClass("fly-in");
            $(".shadow-layer").removeClass("fly-in");
            $("body").removeClass("fly-in");
            $(".navUp .container").removeClass("fly-in");
            $(".dropdowns-container").removeClass("fly-in");
        } else {
            $("#topcartlink .dropdown-toggle").removeAttr("href");
        }
    });
});

$(document).ready(function () {
    if (navigator.platform == "iPad") {
        $('.mainNav li.dropdown > .dropdown-toggle').click(function () {
            if ($(this).parent().hasClass("show")) {
                window.location = $(this).attr('href');
            }
        });
    }
    else {
        $('.mainNav li.dropdown > .dropdown-toggle').click(function () {
            window.location = $(this).attr('href');
        });
    }
});

$(document).mouseup(function (e) {
    var container = $(".mainNav .nav-item.dropdown");
    if (!container.is(e.target) && container.has(e.target).length === 0) {
        $(".mainNav .nav-item .dropdown-menu").removeClass("show");
    }
    $('body').bind("touchstart", function (e) {
        $(".mainNav .nav-item .dropdown-menu").removeClass("show");
    });
});
