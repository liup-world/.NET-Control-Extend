
function textBox_ondblclick(obj) {
    var ddl = obj.nextSibling;
    if (ddl != null) {
    }
}

function dropDownList_onchange(obj) {
    var txt = obj.previousSibling;
    if (txt != null) {
        txt.value = obj.value;
    }
}
