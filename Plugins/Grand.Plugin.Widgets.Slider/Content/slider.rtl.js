$(document).ready(function () {
    SliderPosition();
    $("#GrandCarousel .carousel-indicators:first, #GrandCarousel .carousel-inner .carousel-item:first").addClass("active");
    $("#GrandCarousel .carousel-indicators li").each(function () {
        var car_ind = $(this).index();
        $(this).attr("data-slide-to", car_ind);
    });
});
$(window).resize(function () {
    SliderPosition();
});
$("#next").click(function () {
    $('#grandCarousel').carousel('next');
});

$("#prev").click(function () {
    $('#grandCarousel').carousel('prev');
});

$(".carousel").on("touchstart", function (event) {
    var xClick = event.originalEvent.touches[0].pageX;
    $(this).one("touchmove", function (event) {
        var xMove = event.originalEvent.touches[0].pageX;
        if (Math.floor(xClick - xMove) > 5) {
            $(".carousel").carousel('next');
        }
        else if (Math.floor(xClick - xMove) < -5) {
            $(".carousel").carousel('prev');
        }
    });
    $(".carousel").on("touchend", function () {
        $(this).off("touchmove");
    });
});

function SliderPosition() {
    var FixSliderPos = $('.custom-container').position().left + 'px';
    $('#GrandCarousel').css('right', '-' + FixSliderPos);
}