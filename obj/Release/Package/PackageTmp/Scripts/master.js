function updateChart() {
    $("#bargraphresult").remove();
    dashboard('#topLoader', freqData);
}

function updateTitle(title) {
    var titleWidth = $("#lblTitle").width();
    var numberchar = Math.floor(titleWidth / 12) * 3;
    $("#lblTitle").text(title.substring(0, numberchar) + "...")
}

function updateResize() {    
    $(".slider").each(function () {
        var preOuterWidth = $(this).prev().outerWidth();
        var nextOuterWidth = $(this).next().outerWidth();
        var nextnextWidth = $(this).next().next().outerWidth();
        var colwidth = $("#col-1").width();

        $(this).outerWidth(colwidth - preOuterWidth - nextOuterWidth - nextnextWidth - 5 - 20);

    });
}

function ScrollView(id)
{
    var el = document.getElementById(id)
    if (el != null)
    {        
        el.scrollIntoView();
        el.focus();
    }
}

function cloneBlobInfo() {
    $("#txtAccountName_blob").val($("#txtAccountName").val());
    $("#txtAccountKey_blob").val($("#txtAccountKey").val());
    $("#txtContainer_blob").val($("#txtContainer").val());
};



function deleteAllCookies() {
    var cookies = document.cookie.split(";");

    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i];
        var eqPos = cookie.indexOf("=");
        var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
        document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    }
};

