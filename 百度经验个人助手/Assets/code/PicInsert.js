function autoinsertPictures() {
    var imgs = document.getElementsByClassName("step-media-item item-media-image");
    var holds = document.getElementsByClassName("step-gallery-list gallery-list cf ui-sortable");
    var totalNum = imgs.length;
    var currentNum = 0;
    for (var i in imgs) {
        if (i < holds.length) {
            var rt = imgs[i].getBoundingClientRect();
            var rt2 = holds[i].getBoundingClientRect();
            var event = new MouseEvent('mousedown', { bubbles: true, cancelable: true, clientX: rt.left, clientY: rt.top });
            imgs[i].dispatchEvent(event);
            var event11 = new MouseEvent('mousemove', { bubbles: true, clientX: rt2.left + 10, clientY: rt2.top + 10, cancelable: true });
            imgs[i].dispatchEvent(event11);
            var event3 = new MouseEvent('mouseup', { bubbles: true, cancelable: true });
            holds[i].dispatchEvent(event3);
            currentNum += 1;
        }
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

(function () {
    var picker = document.getElementById("picker");
    var insertPanel = document.createElement("div");
    insertPanel.style.position = "relative";
    insertPanel.innerHTML = `
    <div style="left: -100%; top: -100%; position: relative;">
    <button id="insert-pics-button" style="padding: 10px 18px;" onclick="autoinsertPictures()">
    插入图片<br><span style="font-size: 10px;">到各个步骤</span>
    </button>
    </div>
    `;
    picker.appendChild(insertPanel);
})();
