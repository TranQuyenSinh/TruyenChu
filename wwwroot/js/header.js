function RetrieveTheme(contentSelector) {
    var theme =
        getCookie('theme') != '' ? JSON.parse(atob(getCookie('theme'))) : {ClassTheme: 'dark-theme', UrlBgImage: 'https://static.8cache.com/img/bg_dark.gif'}
    var fontFamily = getCookie('font-family') || 'san-serif'
    var fontSize = getCookie('font-size') || '32px'
    var lineHeight = getCookie('line-height') || '1.4em'

    if (theme != null) {
        setTheme(theme.ClassTheme, theme.UrlBgImage)
    }

    contentSelector.css('font-family', fontFamily)
    contentSelector.css('font-size', fontSize)
    contentSelector.css('line-height', lineHeight)
}

function setTheme(theme, UrlBg) {
    $('body')
        .removeClass('light-theme dark-theme')
        .addClass(theme)
        .css('background-image', 'url(' + UrlBg + ')')
}
function setEventSelectChange(selectSelector, contentSelector) {
    $(selectSelector).change(function () {
        var prop = $(this).prop('name')
        var val = $(this).val()

        if (prop == 'maunen') {
            var base64Str = $(this).val()
            var themeObject = JSON.parse(atob(base64Str))
            setTheme(themeObject.ClassTheme, themeObject.UrlBgImage)
            setCookie('theme', base64Str, 30)
        } else {
            contentSelector.css(prop, val)
            setCookie(prop, val, 30)
        }
    })
}
