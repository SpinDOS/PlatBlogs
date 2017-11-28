// Write your Javascript code.

function platBlogsAjaxHandler(form, mainFieldName, onTrue, onFalse, onError) {
    form.submit(function (e) {
        e.preventDefault();

        var submit = form.find(":submit");
        submit.prop("disabled", true);

        var url = form.attr("action");

        $.ajax({
            type: form.attr("method"),
            url: url,
            data: form.serialize(), // serializes the form's elements.
            success: function (data) {
                if (data.error) {
                    alert(data.error);
                } else {
                    if (data[mainFieldName]) {
                        onTrue(data.warning);
                    } else {
                        onFalse(data.warning);
                    }

                    if (data.warning) {
                        alert(data.warning);
                    } 
                }
            },
            error: function () {
                onError();
            },
            complete: function () {
                submit.prop("disabled", false);
            }
        });

    });
};

function handleLoadMore() {
    var form = $(this);
    form.submit(function (e) {
        e.preventDefault();
        var submit = form.find(":submit");
        submit.prop("disabled", true);

        var url = form.attr("action");

        $.ajax({
            type: form.attr("method"),
            url: "/Api/" + url,
            data: form.serialize(), // serializes the form's elements.
            success: function (data) {
                var received = $(data);
                received.filter(".load-more-form").each(handleLoadMore);
                form.replaceWith(received);
            },
            error: function () {
                alert("Cannot load more data");
                submit.prop("disabled", false);
            }
        });

    });
}

$(document).ready(function () {
    $(".load-more-form").each(handleLoadMore);
    (function() {
        var followersCountBlock = $("#followers-count");
        var form = $("#follow-form");
        var submit = form.find(":submit");
        platBlogsAjaxHandler(form, "followed",
            function (withWarning) {
                form.attr("action", form.attr("action").replace("Follow", "Unfollow"));
                submit.text("Unfollow");
                if (!withWarning) {
                    var followersCount = parseInt(followersCountBlock.text());
                    followersCountBlock.text(followersCount + 1);
                }
            },
            function (withWarning) {
                form.attr("action", form.attr("action").replace("Unfollow", "Follow"));
                submit.text("Follow");
                if (!withWarning) {
                    var followersCount = parseInt(followersCountBlock.text());
                    followersCountBlock.text(followersCount - 1);
                }
            },
            function() {
                alert("Error sending follow request");
            });
    })();

    $(".post-like-form").each(function() {
        var form = $(this);
        var submit = form.find(":submit");
        platBlogsAjaxHandler(form, "liked",
            function (withWarning) {
                form.attr("action", form.attr("action").replace("Like", "Unlike"));
                submit.addClass("liked-img").removeClass("not-liked-img");
                if (!withWarning) {
                    submit.text(parseInt(submit.text()) + 1);
                }
            },
            function (withWarning) {
                form.attr("action", form.attr("action").replace("Unlike", "Like"));
                submit.addClass("not-liked-img").removeClass("liked-img");
                if (!withWarning) {
                    submit.text(parseInt(submit.text()) - 1);
                }
            },
            function () {
                alert("Error sending like request");
            });
    });
});

