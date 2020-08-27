$(".welcome-box").hover(
    function() {
        $(this).css({
            "background": "rgb(34, 31, 39)",
            "transition": "200ms",
        });
    }, function() {
        $(this).css({
            "background": "rgba(44, 51, 69, 0.6)",
            "transition": "200ms",          
        });
    }
);

$(".welcome-box").click(function () {
    /**if ($(this).attr("data-ischar") === "true") {
        $("#play-char").html(translate.play);
    } else {
        $("#play-char").html(translate.playNew);
    }**/
    // browser-side JS
fetch(`https://${GetParentResourceName()}/LoadGame`, {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json; charset=UTF-8',
    }
}).then(resp => resp.json()).then(resp => console.log(resp));
    Kashacter.CloseUI();
});

(() => {
    Kashacter = {};

    Kashacter.ShowUI = function(data) {
        $('body').css({"display":"block"});
        $('.main-container').css({"display":"block"});
    };

    Kashacter.CloseUI = function() {
        $('body').css({"display":"none"});
        $('.main-container').css({"display":"none"});
        $(".welcome-box").removeClass('active-char');
        $("#delete").css({"display":"none"});
    };
    window.onload = function(e) {
        window.addEventListener('message', function(event) {
            switch(event.data.action) {
                case 'openui':
                    Kashacter.ShowUI(event.data);
                    break;
            }
        })
        window.addEventListener('message', function(event) {
            switch(event.data.action) {
                case 'closeui':
                    Kashacter.CloseUI();
                    break;
            }
        })
    }

})();