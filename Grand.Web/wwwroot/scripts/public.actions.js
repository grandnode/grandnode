/*
** grandnode actions
*/
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
    $('.nc-action-form').magnificPopup({
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

});
