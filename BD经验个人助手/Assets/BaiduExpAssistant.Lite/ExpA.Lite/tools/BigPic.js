console.log("========== BigPic.js ==========");
(function AddMyImg() {
    myoldbox = document.getElementById("my-img-preview");
    if (myoldbox) {
        myoldbox.parentNode.removeChild(myoldbox);
    }
    var box = document.createElement("div");
    // box.className = "img-preview";
    box.id = "my-img-preview";
    box.style.left = 200.183 + "px";
    box.style.top = 270.717 + "px";
    box.style.display = "block";
    box.style.position = "fixed";
    box.style.width = 400 + 12 + "px";
    box.style.minHeight = "190px";
    box.style.boxShadow = "1px 3px 6px 1px darkgrey";
    box.style.zIndex = 99999;
    box.style.background = "floralwhite";
    box.style.padding = "2px";
    box.innerHTML = '<div class="my-img-wrapper" style="width:400px; min-height: 178px; user-select: none; -webkit-user-select: none; -ms-user-select: none;">' +
        '<div id="imgbox-help" style="margin:20px; float:left;">'
        + '<b>1. \u6B64\u6846\u53EF\u81EA\u7531\u62D6\u62FD</b>'
        + '<p>2. \u5C06\u9F20\u6807\u653E\u5728\u67D0\u4E2A\u5C0F\u56FE\u4E0A\uFF0C\u7B49\u51FA\u73B0\u5F39\u51FA\u56FE\u7247</p>'
        + '<p>3. \u9F20\u6807\u7ECF\u8FC7\u5F39\u51FA\u56FE\u7247\u6846\uFF0C\u56FE\u7247\u81EA\u52A8\u663E\u793A\u5728\u6B64</p>'
        + '<p>4. \u56FE\u7247\u663E\u793A\u540E\uFF0C\u70B9\u51FB + - \u8C03\u6574\u5927\u5C0F</p>'
        + '<p style="color:darkgreen">5. 点击 × 隐藏，下次鼠标经过预览大图再出现</p>'
        + '</div>'
        + '<div class="pr" style="width:400px; float:left;"><img id="my-bigimg" style="width: 100%; height:100%;  left: 0px; top: -19.2414px;"></div>'
        + '</div>'
        + '<div id="myimg-buttons" style="position:absolute; opacity: 0.7; left: 0px; top: 0px; display: none; user-select: none;">'
        + '<div id="myimg-bigger" style="width:35px; height:35px; background:rgba(200,200,200,100); text-align:center; line-height:35px; border-radius:20px;  display:inline-block">+</div>'
        + '<div id="myimg-smaller" style="width:35px; height:35px; background:rgba(200,200,200,100); text-align:center; line-height:35px; border-radius:20px;  display:inline-block;">-</div>'
        + '<div id="myimg-close" style="width:35px; height:35px; background:orange; text-align:center; line-height:35px; border-radius:20px;  display:inline-block;">×</div>'
        + '</div>';

    document.body.appendChild(box);
    document.querySelector("#myimg-bigger").addEventListener("click", function (e) {
        e.preventDefault();
        var wp = box.getElementsByClassName('my-img-wrapper')[0];
        var width = parseInt(wp.style.width);
        wp.style.width = (width * 1.1) + 'px';
        var pr = box.getElementsByClassName('pr')[0];
        pr.style.width = wp.style.width;
        updateSize();
    });
    document.querySelector("#myimg-smaller").addEventListener("click", function (e) {
        e.preventDefault();
        var wp = box.getElementsByClassName('my-img-wrapper')[0];
        var width = parseInt(wp.style.width);
        wp.style.width = (width * 0.91) + 'px';
        var pr = box.getElementsByClassName('pr')[0];
        pr.style.width = wp.style.width;
        updateSize();
    });
    document.querySelector("#myimg-close").addEventListener("click", function (e) {
        e.preventDefault();
        box.style.display = "none";
    });

    function updateSize() {
        var myimg = document.getElementById("my-bigimg");
        var imgheight = myimg.height;
        var imgwidth = myimg.width;

        var mybox = document.getElementById("my-img-preview");
        mybox.style.height = imgheight + 10 + "px";
        mybox.style.width = imgwidth + 12 + "px";
        mybox.children[0].style.height = imgheight.toString() + "px";
    }

    document.querySelector("#img-preview-box").addEventListener("mouseenter", function () {
        var helper = document.getElementById("imgbox-help");
        if (!helper) return;
        helper.style.display = "none";
        box.style.display = "block";
        var myimg = document.getElementById("my-bigimg");
        var oribox = document.getElementById("img-preview-box");
        if (oribox.childElementCount > 0){
            let bgurl = oribox.children[0].style.background.split("url(\"")[1].split("\")")[0];
            console.log("# bgurl:", bgurl);
            myimg.src = bgurl;
        }
        updateSize();
    });

    document.querySelector("#my-img-preview").addEventListener("mouseenter", function () {
        document.getElementById("myimg-buttons").style.display = 'block';
    });

    document.querySelector("#my-img-preview").addEventListener("mouseleave", function () {
        document.getElementById("myimg-buttons").style.display = 'none';
    });

    document.getElementById("my-bigimg").addEventListener("mousedown", function (e) {
        e.preventDefault();
    });

    var drag = document.getElementById("my-img-preview");
    var isDown = false;
    var diffX = 0;
    var diffY = 0;

    drag.addEventListener("mousedown", function (e) {
        diffX = e.clientX - drag.offsetLeft;
        diffY = e.clientY - drag.offsetTop;
        isDown = true;
    });

    document.addEventListener("mousemove", function (e) {
        if (isDown === false) return;
        var left = e.clientX - diffX;
        var top = e.clientY - diffY;

        if (left < 0) {
            left = 0;
        } else if (left > window.innerWidth - drag.offsetWidth) {
            left = window.innerWidth - drag.offsetWidth;
        }
        if (top < 0) {
            top = 0;
        } else if (top > window.innerHeight - drag.offsetHeight) {
            top = window.innerHeight - drag.offsetHeight;
        }
        drag.style.left = left + 'px';
        drag.style.top = top + 'px';
    });

    document.addEventListener("mouseup", function (e) {
        isDown = false;
    });
})();
console.log("^^^^^^^^^^ BigPic.js ^^^^^^^^^^");