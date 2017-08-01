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
        fullStarr = "<i class=\"fa fa-star pr-1\"></i>",
        halfStarr = "<i class=\"fa fa-star-half-o pr-1\"></i>",
        emptyStarr = "<i class=\"fa fa-star-o pr-1\"></i>",
        id = "";

    if(whole == 0 && half == 0 && empty == 0 ){
        stars = "";
    } else {
        for(var i=0; i<whole; i++){ stars.push( fullStarr ); };
        for(var i=0; i<half; i++){ stars.push( halfStarr ); };
        for(var i=0; i<empty; i++){ stars.push( emptyStarr ); };
        stars = stars.join("");
    };
    $("."+elem+"").html(stars);

};
