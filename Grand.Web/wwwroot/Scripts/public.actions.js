/*
** grandnode actions
*/
var CustomerAction = {
    UrlActive: false,
    UrlPopup: false,
    RemoveUrl: false,

    init: function (activeurl, popupurl, removeUrl) {
        this.UrlActive = activeurl;
        this.UrlPopup = popupurl;
        this.RemoveUrl = removeUrl;
    },

    checkActivePopup: function () {
        $.ajax({
            type: "GET",
            cache: false,
            url: this.UrlPopup
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
            data: { "curl": _curl, "purl": _purl },
        }).complete(function () {
            CustomerAction.checkActivePopup();
        }).fail(function (failureResponse) {
            CustomerAction.ajaxFailure(failureResponse)
        });
    },

    nextStep: function (response) {
        if (response.Id) {
            if (response.PopupTypeId == 10) {
                $('#action-body-banner').html(response.Body);
                window.setTimeout(function () {
                    $('.popup-action-banner').magnificPopup('open');
                }, 100);
                CustomerAction.removeAction(response.Id);
            }
            if (response.PopupTypeId == 20) {
                $('#action-body-form').html(response.Body);
                window.setTimeout(function () {
                    $('.popup-action-form').magnificPopup('open');
                }, 100);
                CustomerAction.removeAction(response.Id);
            }

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
        closeOnBgClick: false,
        preloader: false,
        midClick: true,
        mainClass: 'my-mfp-zoom-in'
    });

    $('.popup-action-banner').magnificPopup({
        type: 'inline',
        fixedContentPos: false,
        fixedBgPos: true,
        overflowY: 'auto',
        closeBtnInside: true,
        closeOnBgClick: false,
        preloader: false,
        midClick: true,
        mainClass: 'my-mfp-zoom-in'
    });

    $("#interactive-form").submit(function (event) {
        event.preventDefault();
        var $form = $(this),
            url = $form.attr('action');
        var posting = $.post(url, $('#interactive-form').serialize());
        posting.done(function (result) {
            if (!result.success) {
                $('#errorMessages').empty();
                for (var error in result.errors) {
                    $('#errorMessages').append(result.errors[error] + '<br />');
                }
            }
            else {
                $('.popup-action-form').magnificPopup('close');
            }
        });
    });


});
