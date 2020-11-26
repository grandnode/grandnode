var Admin = Admin || {};

Admin.Search = (function () {
    var itemTemplate = function (data) {
        var path = function () {
            if (data.grandParent) {
                return $("<p/>").text([data.grandParent, data.parent].join(" > ")).html();
            }
            else {
                return data.parent;
            }
        };
        var result = '<div id="user-selection">' +
            "<h5>" + data.title + "</h5>";
        if (data.source) {
            result = result + data.source
        } else {
            if (path()) {
                result = result + path();
            }
        }
        result = result + "</div>";
        return result;
    };

    var substringMatcher = function (enumerate) {
        var byRateAndTitle = function (a, b) {
            if (a.rate < b.rate)
                return 1;
            if (a.rate > b.rate)
                return -1;
            if (a.title < b.title)
                return -1;
            if (a.title > b.title)
                return 1;
            return 0;
        };

        return function findMatches(q, cb) {
            var matches = [];
            var substrRegex = new RegExp(q, "i");
            enumerate(function (item) {
                var rate = item.rate || 0;
                var missKeyword = false;
                if (substrRegex.test(item.title)) {
                    rate += 10;
                }
                else if (item.node && substrRegex.test(item.node)) {
                    rate += 5;
                }
                else if (substrRegex.test(item.root)) {
                    rate += 1;
                } else {
                    missKeyword = true;
                }
                item.rate = rate;
                if (!missKeyword) {
                    matches.push(item);
                }
            });
            matches.sort(byRateAndTitle);
            return matches;
        };
    };

    return {
        init: function (url) {
            Admin.Navigation.initOnce();

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
                            foundMenuItems: substringMatcher(Admin.Navigation.enumerate)($("#searchInput").val())
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
                    limit: 100
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
    var buildMap = function () {
        var map = {};
        var linkElements = $("a.nav-link");
        linkElements.each(function () {
            var parents = $(this).parentsUntil(".page-sidebar-menu");
            var href;
            var title;
            var parent;
            var grandParent;
            switch (parents.length) {
                // items in level one
                case 1:
                    {
                        href = $(parents).find("a").attr("href");
                        title = $(parents).find("a").find("span").html();
                        map[href] = { title: title, link: href, parent: null, grandParent: null };
                        break;
                    }
                // items in level two, these items have parent but have not grand parent
                case 3:
                    {
                        href = $(parents).eq(0).find("a").attr("href");
                        title = $(parents).eq(0).find("a").find("span").html();
                        parent = $(parents).eq(2).find("a").find("span").html();
                        map[href] = { title: title, link: href, parent: parent, grandParent: null };
                        break;
                    }
                // items in level three, these items have both parent and grand parent
                case 5:
                    {
                        href = $(parents).eq(0).find("a").attr("href");
                        title = $(parents).eq(0).find("a").find("span").html();
                        parent = $(parents).eq(2).find("a").find("span").html();
                        grandParent = $(parents).eq(4).find("a").find("span").html();
                        map[href] = { title: title, link: href, parent: parent, grandParent: grandParent };
                        break;
                    }
                default: break;
            }
        });
        return map;
    };
    var map;
    var init = function () {
        map = buildMap();
    };
    var events = {};
    return {
        enumerate: function (callback) {
            for (var url in map) {
                var node = map[url];
                callback.call(node, node);
            }
        },
        open: function (url) {
            if (events["open"]) {
                var event = $.Event("open", { url: url });
                events["open"].fire(event);
                if (event.isDefaultPrevented())
                    return;
            }
            window.location.href = url;
        },
        initOnce: function () {
            if (!map)
                init();
        },
        init: init
    };
})();