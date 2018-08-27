var Admin = Admin || {};

Admin.Search = (function () {
    var itemTemplate = function (data) {
        var result = '<div id="user-selection">' +
            "<h5>" + data.title + "</h5>";

        result = result + data.source

        result = result + "</div>";
        return result;
    };

    return {
        init: function (url) {
            var $input = $("#searchInput");
            $input.blur(function (e) { e.preventDefault(); e.stopPropagation(); });
            $input.typeahead({ minLength: 1, highlight: true, hint: false },
                {
                    name: "pages",
                    displayKey: "name",
                    templates: {
                        suggestion: itemTemplate
                    },
                    source: function (q, sync, async) {

                        var postData = {
                            searchTerm: $("#searchInput").val(),
                        };

                        addAntiForgeryToken(postData);

                        $.ajax({
                            cache: false,
                            type: "POST",
                            url: url,
                            dataType: 'json',
                            data: postData,
                            success: function (data) {
                                async(data);
                            },
                            error: function () {
                                alert('Error');
                            }
                        });
                    },
                    limit: 10
                });

            var navigateTo = function (item) {
                $input.typeahead("val", "");
                Admin.Navigation.open(item.link);
            };

            $input.on("typeahead:selected", function (e, item) {
                navigateTo(item);
            });
        }
    };
})();

var Admin = Admin || {};
Admin.Navigation = (function () {
    var events = {};
    return {
        open: function (url) {
            if (events["open"]) {
                var event = $.Event("open", { url: url });
                events["open"].fire(event);
                if (event.isDefaultPrevented())
                    return;
            }
            window.location.href = url;
        }
    };
})();
