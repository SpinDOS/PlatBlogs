// Write your Javascript code.

$(document).ready(function () {
    var followersCountBlock = $("#followers-count");
    $("#follow-form").submit(function (e) {
        e.preventDefault();

        var form = $(this);
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
                    var diff;
                    if (data.followed) {
                        form.attr("action", url.replace("Follow", "Unfollow"));
                        submit.text("Unfollow");
                        diff = 1;
                    } else {
                        form.attr("action", url.replace("Unfollow", "Follow"));
                        submit.text("Follow");
                        diff = -1;
                    }

                    if (data.warning) {
                        alert(data.warning);
                    } else {
                        var followersCount = parseInt(followersCountBlock.text());
                        followersCountBlock.text(followersCount + diff);
                    }
                }
            },
            error: function() {
                alert("Error sending follow request");
            },
            complete: function() {
                submit.prop("disabled", false);
            }
        });

    });
});

