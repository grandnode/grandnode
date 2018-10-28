/*
** grandnode search
*/
var SearchAction = {
    EnterSearchTerms: '',
    MinLength: 0,
    UrlProductSearch: '',
    ShowProductImagesInSearch: true,

    init: function (enterSearchTerms) {
        this.EnterSearchTerms = enterSearchTerms;

        $("#small-search-box-form").submit(function (event) {
            if ($("#small-searchterms").val() == "") {
                alert(SearchAction.EnterSearchTerms);
                $("#small-searchterms").focus();
                event.preventDefault();
            }
        });
    },

    autocomplete: function (minLength, urlProductSearch, showProductImagesInSearch) {
        this.MinLength = minLength;
        this.UrlProductSearch = urlProductSearch;
        this.ShowProductImagesInSearch = showProductImagesInSearch;

        $('#small-searchterms').autocomplete({
            delay: 300,
            minLength: SearchAction.MinLength,
            source: function (request, response) {
                var category = '';
                if ($("#SearchCategoryId").length > 0) {
                    category = $("#SearchCategoryId").val();
                }
                $.ajax({
                    url: SearchAction.UrlProductSearch,
                    dataType: "json",
                    data: {
                        term: request.term,
                        categoryId: category
                    },
                    success: function (data) {
                        response(data);
                        $(".advanced-search-results").addClass("open");
                        $('.advanced-search-results .list-group-item').each(function () {
                            $(this).remove();
                        });
                        $('.list-group-item[data-type="Product"]').each(function () {
                            $(this).prependTo(".advanced-search-results .products-container");
                        });
                        $('.list-group-item[data-type="Category"]').each(function () {
                            $(this).prependTo(".advanced-search-results .categories-container ul");
                        });
                        $('.list-group-item[data-type="Manufacturer"]').each(function () {
                            $(this).prependTo(".advanced-search-results .manufacturers-container ul");
                        });
                        $('.list-group-item[data-type="Blog"]').each(function () {
                            $(this).prependTo(".advanced-search-results .blog-container ul");
                        });
                        if ($(".categories-container ul .list-group-item").length) {
                            $(".categories-container .title").css("display", "block");
                            $(".categories-container .no-data").css("display", "none");
                        }
                        else {
                            $(".categories-container .no-data").css("display", "block");
                        }
                        if ($(".products-container .list-group-item").length) {
                            $(".products-title").css("display", "block");
                            $(".right-side .no-data").css("display", "none");
                            $(".right-side").addClass("col-md-6");
                            $(".left-side").css("display", "block");
                        }
                        else {
                            $(".right-side .no-data").css("display", "block");
                            $(".right-side").removeClass("col-md-6");
                            $(".left-side").css("display", "none");
                        }
                        if ($(".blog-container ul .list-group-item").length) {
                            $(".blog-container").css("display", "block");
                            $(".blog-container .title").css("display", "block");
                            $(".blog-container .no-data").css("display", "none");
                        }
                        else {
                            $(".blog-container .no-data").css("display", "block");
                            $(".blog-container").css("display", "none");
                        }
                        if ($(".manufacturers-container ul .list-group-item").length) {
                            $(".manufacturers-container .title").css("display", "block");
                            $(".manufacturers-container .no-data").css("display", "none");
                        }
                        else {
                            $(".manufacturers-container .no-data").css("display", "block");
                        }
                        $('.list-group-item img').each(function () {
                            if (!$(this).attr("src").length) {
                                $(this).remove();
                            }
                        });
                    }
                });
            },
            appendTo: '.search-box',
            select: function (event, ui) {
                $("#small-searchterms").val(ui.item.Label);
                setLocation(ui.item.producturl);
                return false;
            }
        })
            .data("ui-autocomplete")._renderItem = function (ul, item) {
                //html encode
                var term = this.element.val();
                regex = new RegExp('(' + term + ')', 'gi');
                t = item.Label.replace(regex, "<b>$&</b>");
                desc = item.Desc.replace(regex, "<b>$&</b>");
                tin = $('#small-searchterms').val();
                var image = '';
                if (SearchAction.ShowProductImagesInSearch) {
                    image = "<img class='d-inline-flex' src='" + item.PictureUrl + "'>";
                }
                return $("<li data-type='" + item.SearchType + "' class='list-group-item' ></li>")
                    .data("item.autocomplete", item)
                    .append("<a href='" + item.Url + "' class='generalImg row mx-0'>" + image + "<div class='container-off col px-0'><div class='product-in'></div><div class='in-separator'>in</div><div class='product-title'>" + t + "</div><div class='product-desc'>" + desc + "</div></div></a>")
                    .appendTo(ul)
                    .find(".product-in").text(tin)
            };

    }
}