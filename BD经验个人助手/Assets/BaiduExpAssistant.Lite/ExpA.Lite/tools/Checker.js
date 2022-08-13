console.log("========== Checker.js ==========");
(function () {
    var BACheckList = [
        {
            "name": "标题",
            "status": () => {
                if (document.querySelector(".main-title").value.trim() != "") return [true, ""];
                else return [false, "未填写"];
            }
        },
        {
            "name": "简介",
            "status": () => {
                if (document.querySelector("#editor-brief").innerText.trim() != "") return [true, ""];
                else return [false, "未填写"];
            }
        },
        {
            "name": "简介图片",
            "status": () => {
                if (document.querySelector("#brief-section .gallery-list .ga-list") !== null) return ["true", ""];
                else return [false, "无图片"];
            }
        },
        {
            "name": "注意事项",
            "status": () => {
                if (document.querySelector("#notice-section input").value.trim() != "") return [true, ""];
                else return [false, "未填写"];
            }
        },
        {
            "name": "原创",
            "status": () => {
                if (document.querySelector("#is-origin").checked == true) return [true, ""];
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
            } catch (e) { console.error(e); }
            checkResult.push(`<p><span>${name}: </span><span style='color:${result[0] ? 'green;' : 'red;'}'>${result[0] ? 'YES' : 'NO'}</span><span>${result[1]}</span></p>`);
        }
        //console.log(checkResult);
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
})();
console.log("^^^^^^^^^^ Checker.js ^^^^^^^^^^");