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

    checkActiveUrl: function (_curl, _purl) {
        $.ajax({
            type: "GET",
            url: this.UrlActive,
            data: {"curl": _curl, "purl": _purl }
        }).complete(() =>
            this.checkActiveBanner())
        .fail((failureResponse) =>
            ajaxFailure(failureResponse)
        );
    },

    checkActiveBanner: function () {
        $.ajax({
                type: "GET",
                url: this.UrlBanner
        }).then((success) =>
            this.nextStep(success)
        ).fail((failureResponse) =>
            ajaxFailure(failureResponse)
        );
    },
    nextStep: function (response) {
        if(response.Id)
        {
            $('#action-body').html(response.Body);
            window.setTimeout(function () {
                $('.popup-action-form').magnificPopup('open');
            }, 100);
            this.removeAction(response.Id);
        }
    },
    removeAction: function (id) {
        $.ajax({
            type: "post",
            url: this.RemoveUrl,
            data: { "Id": id }
        }).fail((failureResponse) =>
            this.ajaxFailure(failureResponse)
        )
    },
    ajaxFailure: function (failureResponse) {
        alert('Error: ' + failureResponse.responseText);
    },

}



$(document).ready(function () {

    $('.popup-action-form').magnificPopup({        
        type: 'inline',
        fixedContentPos: false,
        fixedBgPos: true,
        overflowY: 'auto',
        closeBtnInside: true,
        preloader: false,
        midClick: true,
        mainClass: 'my-mfp-zoom-in'
    });

});
