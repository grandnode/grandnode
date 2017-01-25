function setLocation(url) {
    window.location.href = url;
}

function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    winprops = 'resizable=1, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}

function showThrobber(message) {
    $('.throbber-header').html(message);
    window.setTimeout(function () {
        $(".throbber").show();
    }, 1000);
}

$(document).ready(function () {
    $('.multi-store-override-option').each(function (k, v) {
        checkOverriddenStoreValue(v, $(v).attr('data-for-input-selector'));
    });
});

function checkAllOverriddenStoreValue(item) {
    $('.multi-store-override-option').each(function (k, v) {
        $(v).attr('checked', item.checked);
        checkOverriddenStoreValue(v, $(v).attr('data-for-input-selector'));
    });
}

function checkOverriddenStoreValue(obj, selector) {
    var elementsArray = selector.split(",");
    if (!$(obj).is(':checked')) {
        $(selector).attr('disabled', true);
        //Kendo UI elements are enabled/disabled some other way
        $.each(elementsArray, function(key, value) {
            var kenoduiElement = $(value).data("kendoNumericTextBox");
            if (kenoduiElement !== undefined && kenoduiElement !== null) {
                kenoduiElement.enable(false);
            }
        }); 
    }
    else {
        $(selector).removeAttr('disabled');
        //Kendo UI elements are enabled/disabled some other way
        $.each(elementsArray, function(key, value) {
            var kenoduiElement = $(value).data("kendoNumericTextBox");
            if (kenoduiElement !== undefined && kenoduiElement !== null) {
                kenoduiElement.enable();
            }
        });
    };
}

function tabstrip_on_tab_select(e) {
    //we use this function to store selected tab index into HML input
    //this way we can persist selected tab between HTTP requests
    $("#selected-tab-index").val($(e.item).index());
}

function display_kendoui_grid_error(e) {
    if (e.errors) {
        if ((typeof e.errors) == 'string') {
            //single error
            //display the message
            alert(e.errors);
        } else {
            //array of errors
            //source: http://docs.kendoui.com/getting-started/using-kendo-with/aspnet-mvc/helpers/grid/faq#how-do-i-display-model-state-errors?
            var message = "The following errors have occurred:";
            //create a message containing all errors.
            $.each(e.errors, function (key, value) {
                if (value.errors) {
                    message += "\n";
                    message += value.errors.join("\n");
                }
            });
            //display the message
            alert(message);
        }
    } else if (e.errorThrown) {
        alert('Error happened');
    }
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


// Ajax activity indicator bound to ajax start/stop document events
$(document).ajaxStart(function () {
    $('#ajaxBusy').show();
}).ajaxStop(function () {
    $('#ajaxBusy').hide();
});
