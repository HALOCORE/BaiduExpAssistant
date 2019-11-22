function autoinsertPictures(jmpCount) {
    var imgs = document.getElementsByClassName("step-media-item item-media-image");
    var holds = document.getElementsByClassName("step-gallery-list gallery-list cf ui-sortable");
    var totalNum = imgs.length;
    var currentNum = 0;
    try {
        for (var i in imgs) {
            if (parseInt(i) + parseInt(jmpCount) < holds.length) {
                var rt = imgs[i].getBoundingClientRect();
                var rt2 = holds[parseInt(i) + parseInt(jmpCount)].getBoundingClientRect();
                var event = new MouseEvent('mousedown', { bubbles: true, cancelable: true, clientX: rt.left, clientY: rt.top });
                imgs[i].dispatchEvent(event);
                var event11 = new MouseEvent('mousemove', { bubbles: true, clientX: rt2.left + 10, clientY: rt2.top + 10, cancelable: true });
                imgs[i].dispatchEvent(event11);
                var event3 = new MouseEvent('mouseup', { bubbles: true, cancelable: true });
                holds[parseInt(i) + parseInt(jmpCount)].dispatchEvent(event3);
                currentNum += 1;
            }
        }
    }
    catch (e) {
        var d1 = document.createElement("div");
        d1.innerText = e.message;
        window.external.notify("NOTIFY: 插入失败  | " + d1.innerText + " | WARN");
        return;
    }
    var msg1 = "插入了" + (currentNum).toString() + "张图片";
    var msg2 = "所有图片已插入";
    if (currentNum < totalNum) {
        msg2 = "还剩 " + (totalNum - currentNum).toString() + " 张图片未插入";
    }
    try {
        window.external.notify("NOTIFY: " + msg1 + " | " + msg2 + " | OK");
    }
    catch (e) {
        alert(msg1 + "\n" + msg2);
    }
}

function autoinsertPictures0() {
    autoinsertPictures(0);
}
function autoinsertPictures1() {
    autoinsertPictures(1);
}

(function () {
    if (document.getElementById("pic-insert-panel")) return;
    var picker = document.getElementById("picker");
    var insertPanel = document.createElement("div");
    insertPanel.id = "pic-insert-panel";
    insertPanel.style.position = "relative";
    insertPanel.innerHTML = `
    <div style="left: -82%; position: relative;">
    <button id="insert-pics-button0" style="padding: 10px 10px; line-height: 1; display: block;">
    插入图片<br><span style="font-size: 10px;">包括简介位置</span>
    </button>
    <button id="insert-pics-button1" style="padding: 10px 10px; line-height: 1; display: block;">
    插入图片<br><span style="font-size: 10px;">跳过简介</span>
    </button>
    </div>
    `;
    picker.appendChild(insertPanel);
    document.getElementById("insert-pics-button0").addEventListener("click", autoinsertPictures0);
    document.getElementById("insert-pics-button1").addEventListener("click", autoinsertPictures1);

    //var upld = document.getElementsByClassName("webuploader-element-invisible")[0];
    //upld.ondragover = function () { };
    //upld.ondragend = function () { };
    //upld.style.clip = "unset";
    //upld.style.display = "inherit";
    //upld.style.position = "static";
    //upld.style.fontSize = "22px";
    //upld.style.width = "195px";
    //upld.parentElement.style.overflow = "visible";
})();
