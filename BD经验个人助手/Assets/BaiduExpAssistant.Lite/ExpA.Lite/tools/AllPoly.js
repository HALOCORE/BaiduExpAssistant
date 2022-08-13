console.log("========== AllPoly.js ==========");
window._EXPLITE_EXTENSION_ENV_ = null;
try {
    window.external.notify("TEST_NOTIFY");
    console.log("百度经验个人助手环境");
    window._EXPLITE_EXTENSION_ENV_ = "assistant";
} catch(e) {
    console.log("浏览器环境");
    window._EXPLITE_EXTENSION_ENV_ = "browser";
}

////// both
console.logerr = (err) => {
    var hiddenErrB = document.createElement('div');
    hiddenErrB.innerText = err.message;
    console.error("# LOGERR:" + hiddenErrB.innerText);
}

console.log("# hacking pic uploader...");
let criticalElem = document.getElementsByClassName("webuploader-element-invisible")[0];
function cloneNodeGen(clone) {
    return function() {
        var ret = clone.apply(this, arguments);
        console.log("# clone: " + ret.tagName.toLowerCase());
        ret.cloneNode = cloneNodeGen(ret.cloneNode);
        ret.addEventListener = function(adder) {
            return function() {
                var ret = adder.apply(this, arguments);
                console.log("# addEventListener.", arguments);
                if(arguments[0] === "change") {
                    console.log("# change EventListener found.", arguments[1]);
                    window._onUploadFileChange = arguments[1];
                }
                return ret;
            }
        }(ret.addEventListener);
        return ret;
    };
}
criticalElem.cloneNode = cloneNodeGen(criticalElem.cloneNode);
criticalElem.dispatchEvent(new CustomEvent("change"));

////// assistant only
if(window._EXPLITE_EXTENSION_ENV_ === "assistant") {
    window.external._getImageDict = {};
    window.external.getImage = (url, onSucceed, onFailed) => {
        url = url.trim();
        console.log("# externalGetImage: " + url);
        window.external._getImageDict[url] = {};
        window.external._getImageDict[url]['onSucceed'] = onSucceed;
        window.external._getImageDict[url]['onFailed'] = onFailed;
        try {
          window.external.notify("IMAGE-GET: " + url);
        }
        catch (e) { console.error(e); }
    }
    window.external_getImageSucceed = (url, dataUrl) => {
        console.log("# external_getImageSucceed: " + url);
        try {
            window.external._getImageDict[url]['onSucceed'](dataUrl);
        } catch(e) { console.error(e); }
    }
    window.external_getImageFailed = (url, message) => {
        console.log("# external_getImageFailed: " + url);
        try {
            window.external._getImageDict[url]['onFailed'](message);
        } catch(e) { console.error(e); }
    }

    window.external_getUploadPicSucceed = null;
    window.external_getUploadPicFailed = null;
    window.external.getUploadPic = (onSucceed, onFailed) => {
        console.log("# external.getUploadPic called.");
        window.external.notify("GET-PIC-FOR-UPLOAD: " + 600 / 240 + " | 600 | 240");
        window.external_getUploadPicSucceed = onSucceed;
        if(onFailed) window.external_getUploadPicFailed = onFailed;
        else window.external_getUploadPicFailed = (message) => {console.error("# external_getUploadPicFailed: " + message)};
    }

    window.dataURLtoFile = (dataurl, filename) => {
        var arr = dataurl.split(','), mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);
        while(n--){
            u8arr[n] = bstr.charCodeAt(n);
        }
        let file = new Blob([u8arr], { type: mime });
        file.lastModifiedDate = new Date();
        file.name = filename;
        return file;
    };
    //window._onUploadFileChange({type:"change", target:{"files": [fakeFile]}})

    window.external_confirmCallbackYes = null;
    window.external_confirmCallbackNo = null;
    window.external.cofirmDialog = (title, body, callbackYes, callbackNo) => {
        console.log("# window.external.cofirmDialog called.");
        window.external_confirmCallbackYes = callbackYes;
        if(callbackNo) window.external_confirmCallbackNo = callbackNo;
        else window.external_confirmCallbackNo = () => console.warn("# external_confirmCallbackNo.");
        window.external.notify("CONFIRM: " + title + " | " + body);
    }
} 

////// browser only
if(window._EXPLITE_EXTENSION_ENV_ === "browser") {
    window.external.notify = function (msg) {
        console.log("[window.external.notify]", msg);
    }

    window.dataURLtoFile = (dataurl, filename) => {
        var arr = dataurl.split(','), mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);
        while(n--){
            u8arr[n] = bstr.charCodeAt(n);
        }
        let file =new File([u8arr], filename, {type:mime});
        return file;
    }
}

// let testRoot = document.createElement("div");
// document.body.appendChild(testRoot);
// ReactDOM.render(RNE_Test, testRoot);
// console.log("# ReactDOM.render called.");

console.log("^^^^^^^^^^ AllPoly.js ^^^^^^^^^^");