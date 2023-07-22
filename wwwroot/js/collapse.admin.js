$(document).ready(function () {

    var active_item = $("#accordionSidebar .collapse-item.active")

    var collapse = active_item.parents('.collapse')
    collapse.addClass('show')

    var collapseTitle = collapse.parents('.nav-item').first()
    collapseTitle.addClass('active')
})
