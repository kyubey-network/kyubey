//Hook up the tweet display

$(document).ready(function () {
    $(".countdown").countdown({
        date: time,
        format: "on"
    },
        function () {
        });
});	