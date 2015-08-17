$(document).ready(function () {
    footer();
    setCurrentDayTitle();
});

function footer() {
    var item = $(".item");
    item.on("click", function () {
        var pageName = $(this).children(".footer_text")[0].innerHTML.toLowerCase();
        moveToPage(pageName);
    });
}

function moveToPage(pageName) {
    switch (pageName) {
        case "timer":
            document.location = "wh.html";
            break;
        case "graph":
            document.location = "wh_graph.html";
            break;
        case "table":
            document.location = "wh_table.html";
            break;
        case "settings":
            document.localName = "wh_settings.html"
            break;
    }
}

arrDays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

//set div hour_title innerhtml
function setCurrentDayTitle() {
    var arrDivs = document.getElementsByClassName("hour_title");
    if (arrDivs && arrDivs.length == 1) {
        var div = arrDivs[0];
        var d = new Date();
        var dayName = arrDays[d.getDay()];
        var hours = d.getHours();
        var minutes = d.getMinutes();

        hours = hours < 10 ? ("0" + hours) : hours;
        minutes = minutes < 10 ? ("0" + minutes) : minutes;

        div.innerHTML = dayName.toUpperCase() + " <b>" + hours + ":" + minutes + "</b>";
    }


}