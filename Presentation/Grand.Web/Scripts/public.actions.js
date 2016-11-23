/*
** grandnode actions
*/
var CustomerAction = {
    UrlActive: false,
    UrlBanner: false,
    RemoveUrl: false,

    init: function (activeurl, bannerurl, removeUrl) {
        this.UrlActive = activeurl;
        this.UrlBanner = bannerurl;
        this.RemoveUrl = removeUrl;
    },

    checkActiveBanner: function () {
        $.ajax({
            type: "GET",
            cache: false,
            url: this.UrlBanner
        }).then(function (success) {
            CustomerAction.nextStep(success)
        }).fail(function (failureResponse) {
            CustomerAction.ajaxFailure(failureResponse)
        });
    },

    checkActiveUrl: function (_curl, _purl) {
        $.ajax({
            type: "GET",
            cache: false,
            url: this.UrlActive,
            data: {"curl": _curl, "purl": _purl },
        }).complete(function () {
            CustomerAction.checkActiveBanner();
        }).fail(function (failureResponse) {
            CustomerAction.ajaxFailure(failureResponse)
        });
    },

    nextStep: function (response) {
        if(response.Id)
        {
            $('#action-body').html(response.Body);
            window.setTimeout(function () {
                $('.popup-action-form').magnificPopup('open');                
            }, 100);
            CustomerAction.removeAction(response.Id);
        }
    },
    removeAction: function (id) {
        $.ajax({
            type: "post",
            cache: false,
            url: this.RemoveUrl,
            data: { "Id": id }
        }).fail(function (failureResponse) {
            CustomerAction.ajaxFailure(failureResponse)
        })
    },
    ajaxFailure: function (failureResponse) {
        console.log('Error: ' + failureResponse.responseText);
    },

}



$(document).ready(function () {

    $('.popup-action-form').magnificPopup({        
        type: 'inline',
        fixedContentPos: false,
        fixedBgPos: true,
        overflowY: 'auto',
        closeBtnInside: true,
        closeOnBgClick:false,
        preloader: false,
        midClick: true,
        mainClass: 'my-mfp-zoom-in'
    });

});
