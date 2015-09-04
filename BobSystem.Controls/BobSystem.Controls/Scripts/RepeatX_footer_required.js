
function repeatX_txtPageNo_onkeydown(obj, clientID, itemID) {
    if (!((event.keyCode >= 48 && event.keyCode <= 57) // number 0-9
        //|| (event.keyCode >= 96 && event.keyCode <= 105)
        || (event.keyCode == 8)  // 退格
        || (event.keyCode == 46) // Del
        || (event.keyCode == 27) // ESC
        || (event.keyCode == 37) // 左
        || (event.keyCode == 39) // 右
        || (event.keyCode == 16) // shift
        || (event.keyCode == 9)  // Tab
        )) {
        event.returnValue = false;
    }
    if (event.keyCode == 13) {
        obj.blur();
        var btnGo = document.getElementById(clientID + "_btnGo"); // client id
        if (btnGo == null) {
            btnGo = document.getElementById(clientID + "_ctl" + itemID + "_btnGo"); // itemID
        }
        if (btnGo != null) {
            btnGo.click();
        }
    }
}

function repeatX_txtPageNo_onchange(obj, clientID, itemID) {
    if (obj.value != "") {
        var btnGo = document.getElementById(clientID + "_btnGo");
        var ref = "javascript:__doPostBack('" + clientID + "', 'go$" + obj.value + "')";
        if (btnGo != null) {
            btnGo.href = ref;
        }
        else {
            btnGo = document.getElementById(clientID + "_ctl" + itemID + "_btnGo");
            if (btnGo != null) {
                btnGo.href = ref;
            }
        }
    }
}

function repeatX_txtPageNo_onfocus(obj) {
    if (obj.select) {
        obj.select();
    }
    else if (obj.setSelectionRange) {
        obj.setSelectionRange(0, obj.value.length);
    }
}

function repeatX_txtPageNo_onpaste() {
    return !clipboardData.getData('text').match(/\D/);
}

function repeatX_btnGo_onclick(clientID, itemID) {
    var txtGo = document.getElementById(clientID + "_txtGo");
    if (txtGo == null) {
        txtGo = document.getElementById(clientID + "_ctl" + itemID + "_txtGo");
    }
    if (txtGo != null) {
        if (txtGo.value == "") {
            txtGo.focus();

            return false;
        }

        return true;
    }
}