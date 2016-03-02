/*
** grandnode actions
*/
var CustomerAction = {
    Url: false,
    RemoveUrl: false,

    init: function(url, removeUrl){
        this.Url = url;
        this.RemoveUrl = removeUrl;
    },
    checkActiveBanner: function () {
        $.ajax({
                type: "GET",
                url: this.Url
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
            }, 800);
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
