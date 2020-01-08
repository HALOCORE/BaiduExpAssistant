var BACheckList = [
    {
        "name": "标题",
        "status": () => {
            if ($(".main-title")[0].value.trim() != "") return [true, ""];
            else return [false, "未填写"];
        }
    },
    {
        "name": "简介",
        "status": () => {
            if ($("#editor-brief")[0].innerText.trim() != "") return [true, ""];
            else return [false, "未填写"];
        }
    },
    {
        "name": "简介图片",
        "status": () => {
            if ($("#brief-section .gallery-list .ga-list").length > 0) return ["true", ""];
            else return [false, "无图片"];
        }
    },
    {
        "name": "注意事项",
        "status": () => {
            if ($("#notice-section input")[0].value.trim() != "") return [true, ""];
            else return [false, "未填写"];
        }
    },
    {
        "name": "原创",
        "status": () => {
            if ($("#is-origin")[0].checked == true) return [true, ""];
            else return [false, "未勾选"];
        }
    },
];

function BACheck() {
    var d = document.getElementById("ba-check-list");

    if (!d) {
        d = document.createElement("div");
        d.id = "ba-check-list";
        d.style.marginLeft = '84px';
        d.style.marginBottom = '10px';
        document.getElementById("main-content").appendChild(d);
    }
    else {
        d.innerHTML = "";
    }

    var checkResult = [];
    for (var i = 0; i < BACheckList.length; i++) {
        var checkObj = BACheckList[i];
        var name = checkObj['name'];
        var checker = checkObj['status'];
        var result = [false, "检查失败"];
        try {
            result = checker();
        } catch (e) { }
        checkResult.push(`<p><span>${name}: </span><span style='color:${result[0] ? 'green;' : 'red;'}'>${result[0] ? 'YES' : 'NO'}</span><span>${result[1]}</span></p>`);
    }
    console.log(checkResult);
    var fullResult = checkResult.join("\n");
    d.innerHTML = fullResult;
}

(function () {
    try {
        setInterval(BACheck, 1000);
    } catch (e) {
        var emm = document.createElement("div");
        emm.innerText = e.message;
        try {
            window.external.notify("LOG-EVENT: BACheck_LoadFailed_STRANGE");
            //window.external.notify("SHOW-DIALOG: 检查功能加载失败 | " + emm.innerText.replace("|", " I "));
        } catch (e2) { };
    }
})();

