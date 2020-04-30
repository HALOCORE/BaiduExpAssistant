function AddMyImg() {
    myoldbox = document.getElementById("my-img-preview");
    if (myoldbox) {
        myoldbox.parentNode.removeChild(myoldbox);
    }
    var box = document.createElement("div");
    box.className = "img-preview";
    box.id = "my-img-preview";
    box.style.left = 200.183 + "px";
    box.style.bottom = 140.717 + "px";
    box.style.display = "block";
    box.style.position = "fixed";
    box.style.width = 400 + 12 + "px";
    box.style.minHeight = "190px";
    box.innerHTML = '<div class="img-wrapper" style="width:400px; min-height: 178px;">' +
        '<div id="imgbox-help" style="margin:20px; float:left;">'
        + '<b>1. \u6B64\u6846\u53EF\u81EA\u7531\u62D6\u62FD</b>'
        + '<p>2. \u5C06\u9F20\u6807\u653E\u5728\u67D0\u4E2A\u5C0F\u56FE\u4E0A\uFF0C\u7B49\u51FA\u73B0\u5F39\u51FA\u56FE\u7247</p>'
        + '<p>3. \u9F20\u6807\u7ECF\u8FC7\u5F39\u51FA\u56FE\u7247\u6846\uFF0C\u56FE\u7247\u81EA\u52A8\u663E\u793A\u5728\u6B64</p>'
        + '<p>4. \u56FE\u7247\u663E\u793A\u540E\uFF0C\u70B9\u51FB + - \u8C03\u6574\u5927\u5C0F</p>'
        + '</div>'
        + '<div class="pr" style="width:400px; float:left;"><img id="my-bigimg" style="width: 100%; height:100%;  left: 0px; top: -19.2414px;"></div>'
        + '</div>'
        + '<div id="myimg-buttons" style="position:absolute; opacity: 0.7; left: 0px; top: 0px; display: none;">'
        + '<div id="myimg-bigger" style="width:35px; height:35px; background:rgba(200,200,200,100); text-align:center; line-height:35px; border-radius:20px;  display:inline-block">+</div>'
        + '<div id="myimg-smaller" style="width:35px; height:35px; background:rgba(200,200,200,100); text-align:center; line-height:35px; border-radius:20px;  display:inline-block;">-</div>'
        + '<div id="myimg-close" style="width:35px; height:35px; background:orange; text-align:center; line-height:35px; border-radius:20px;  display:inline-block;">×</div>'
        + '</div>';

    document.body.appendChild(box);
    jQuery("#myimg-bigger").click(function (e) {
        e.preventDefault();
        var wp = jQuery(box).find('.img-wrapper')[0];
        var width = parseInt(wp.style.width);
        wp.style.width = (width * 1.1) + 'px';
        var pr = jQuery(box).find('.pr')[0];
        pr.style.width = wp.style.width;
        updateSize();
    });
    jQuery("#myimg-smaller").click(function (e) {
        e.preventDefault();
        var wp = jQuery(box).find('.img-wrapper')[0];
        var width = parseInt(wp.style.width);
        wp.style.width = (width * 0.91) + 'px';
        var pr = jQuery(box).find('.pr')[0];
        pr.style.width = wp.style.width;
        updateSize();
    });
    jQuery("#myimg-close").click(function (e) {
        e.preventDefault();
        document.body.removeChild(box);
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

    jQuery("#img-preview-box").hover(function () {
        var helper = document.getElementById("imgbox-help");
        if (!helper) return;
        helper.style.display = "none";

        var myimg = document.getElementById("my-bigimg");
        var oribox = document.getElementById("img-preview-box");
        if (oribox.childElementCount > 0)
            myimg.src = oribox.children[0].src;
        updateSize();

        var tryBriefInput = document.getElementById("brief-background-bigbox");
        if (tryBriefInput) {
            tryBriefInput.value = oribox.children[0].src;
        }
    });

    jQuery("#my-img-preview").mouseenter(function () {
        document.getElementById("myimg-buttons").style.display = 'block';
    });

    jQuery("#my-img-preview").mouseleave(function () {
        document.getElementById("myimg-buttons").style.display = 'none';
    });

    document.getElementById("my-bigimg").onmousedown = function (e) {
        e.preventDefault();
    };

    var drag = document.getElementById("my-img-preview");
    var isDown = false;
    var diffX = 0;
    var diffY = 0;

    drag.onmousedown = function (e) {
        diffX = e.clientX - drag.offsetLeft;
        diffY = e.clientY - drag.offsetTop;
        isDown = true;
    };

    document.onmousemove = function (e) {
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
    };

    document.onmouseup = function (e) {
        isDown = false;
    };
}
