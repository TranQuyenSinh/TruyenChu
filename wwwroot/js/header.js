function RetrieveTheme() {
    var theme =
        getCookie("theme") != null ? JSON.parse(atob(getCookie("theme"))) : null;

    if (theme != null) {
        $("body")
            .removeClass("dark-theme light-theme")
            .addClass(theme.ClassTheme);
        $("body").css("background-image", "url(" + theme.UrlBgImage + ")");
    }
    if (getCookie("fontchu") != null) {
        $(".chapter-content p").css("font-family", getCookie("fontchu"));
    }

    if (getCookie("sizechu") != null) {
        $(".chapter-content p").css("font-size", getCookie("sizechu") + "px");
    }

    if (getCookie("chieucaodong") != null) {
        $(".chapter-content p").css(
            "line-height",
            getCookie("chieucaodong") + "em"
        );
    }
}

function setTheme(theme, UrlBg) {
    $("body").removeClass("light-theme dark-theme").addClass(theme).css("background-image", "url(" + UrlBg + ")");
}
function setEventSelectChange() {
    $(".dropdown-menu.story-setting select").change(function () {
        var name = $(this).prop("name");
        var val = $(this).val();

        switch (name) {
            case "maunen":
                var base64Str=$(this).val()
                var themeObject = JSON.parse(atob(base64Str))
                setTheme(themeObject.ClassTheme, themeObject.UrlBgImage)
                setCookie("theme", base64Str, 30);
                break;
            case "fontchu":
                $(".chapter-content p").css("font-family", val);
                setCookie("fontchu", val, 30);
                break;
            case "sizechu":
                $(".chapter-content p").css("font-size", val + "px");
                setCookie("sizechu", val, 30);
                break;
            case "chieucaodong":
                $(".chapter-content p").css("line-height", val + "em");
                setCookie("chieucaodong", val, 30);
                break;
        }
    });
}