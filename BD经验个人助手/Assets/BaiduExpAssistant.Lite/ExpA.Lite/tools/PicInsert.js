console.log("========== PicInsert.js ==========");
setTimeout(function () {
    "use strict";
    function* autoinsertPictures(jmpCount) {
        var imgs = document.getElementsByClassName("ga-list");//("step-media-item item-media-image");
        var holds = document.getElementsByClassName("step-gallery-list gallery-list"); //("step-gallery-list gallery-list cf ui-sortable");
        var totalNum = imgs.length;
        var currentNum = 0;
        try {
            document.body.style.position = "fixed";
            for (let i = 0; i < imgs.length; i++) {
                //imgs[i].addEventListener("mousemove", (e) => {console.log("img[", i, "] event:", e);})
                if (parseInt(i) + parseInt(jmpCount) < holds.length) {
                    var rt = imgs[i].getBoundingClientRect();
                    var event = new MouseEvent('mousedown', { bubbles: true, cancelable: true, clientX: rt.left, clientY: rt.top });
                    imgs[i].dispatchEvent(event);
                    yield;
                    //var rt2pre = holds[parseInt(i) + parseInt(jmpCount)].getBoundingClientRect();
                    //window.scrollTo(rt2pre.left, rt2pre.top + pageYOffset - 60);
                    //yield;
                    let rt2 = holds[parseInt(i) + parseInt(jmpCount)].getBoundingClientRect();
                    let event11 = new MouseEvent('mousemove', { 
                        bubbles: true, 
                        clientX: rt2.left + 10, 
                        clientY: rt2.top + 10,
                        cancelable: true });
                    imgs[i].dispatchEvent(event11);
                    yield;
                    var event3 = new MouseEvent('mouseup', { bubbles: true, cancelable: true });
                    holds[parseInt(i) + parseInt(jmpCount)].dispatchEvent(event3);
                    yield;
                    currentNum += 1;
                }
            }
            document.body.style.position = "";
        }
        catch (e) {
            document.body.style.position = "";
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
    
    let autoinsertPictureInstantMode = true;
    function autoinsertPictures0() {
        let gen = autoinsertPictures(0);
        function stepGen() {
            let result = gen.next();
            if(result.done) return;
            else {
                if (autoinsertPictureInstantMode) {
                    stepGen();
                }else{
                    setTimeout(stepGen, 50);
                }
            }
            
        }
        setTimeout(stepGen, 0);
    }
    function autoinsertPictures1() {
        let gen = autoinsertPictures(1);
        function stepGen() {
            let result = gen.next();
            if(result.done) return;
            else {
                if (autoinsertPictureInstantMode) {
                    stepGen();
                }else{
                    setTimeout(stepGen, 50);
                }
            }
        }
        setTimeout(stepGen, 0);
    }
    
    (function () {
        if (document.getElementById("pic-insert-panel")) return;
        var picker = document.getElementById("gallery-bar");
        var insertPanel = document.createElement("div");
        insertPanel.id = "pic-insert-panel";
        insertPanel.style.position = "absolute";
        insertPanel.style.top = "80px";
        insertPanel.style.zIndex = "-1000";
        insertPanel.innerHTML = `
        <div style="left: -100%; position: relative; z-index: 1000;">
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
    
        // if(window._EXTENSION_IS_CHROME_) {
        //     var upld = document.getElementsByClassName("webuploader-element-invisible")[0];
        //     upld.ondragover = function () { };
        //     upld.ondragend = function () { };
        //     upld.style.clip = "unset";
        //     upld.style.display = "inherit";
        //     upld.style.position = "static";
        //     upld.style.fontSize = "22px";
        //     upld.style.width = "105px";
        //     upld.style.top = "-35px";
        //     upld.parentElement.style.overflow = "visible";
        // }
    })();
}, 1000);
console.log("^^^^^^^^^^ PicInsert.js ^^^^^^^^^^");