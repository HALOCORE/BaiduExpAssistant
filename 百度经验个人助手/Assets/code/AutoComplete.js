function InitAutoComplete(myData) {
    "use strict";
    var hinterData = null;
    if (myData) {
        try {
            hinterData = JSON.parse(myData);
        }
        catch (e) {
            hinterData = null;
        }
    }
    if (!hinterData) {
        hinterData = [
            { label: 'Visual Studio 2015' },
            { label: 'windows 10' },
            { label: 'win+R打开运行窗口' },
            { label: '打开首选项设置面板' },
            { label: 'WebView' },
            { label: 'CentOS' },
            { label: 'Chrome' }
        ];
    }

    let hinterValueDict = {};
    let hinterDataDict = {};
    let inputTextBefore = "";
    let selectedHinterIndex = 0;
    let hintersResult = [];
    let selectionText = "";
    let selectionRect = null;
    let isSelectionChanged = false;
    let isHintersShowup = false;
    let isDirty = false;

    function initHinterData() {
        for (let i = 0; i < hinterData.length; i++) {
            let hd = hinterData[i];
            let val = 'value' in hd ? hd['value'] : hd['label'];
            hinterValueDict[val] = hd;
        }
        for (let i = 0; i < hinterData.length; i++) {
            let hi = hinterData[i]['label'];
            for (let j = hi.length - 2; j >= 0; j--) {
                let curdict = hinterDataDict;
                for (let k = j; k >= 0; k--) {
                    if (!(hi[k] in curdict)) {
                        curdict[hi[k]] = {};
                    }
                    curdict = curdict[hi[k]];
                }
                if (!("_FNS_" in curdict)) {
                    curdict["_FNS_"] = [];
                }
                curdict["_FNS_"].push(hinterData[i]);
            }
        }
    }

    function addHinterData(label, value = "") {
        if (label.length === 0) return;
        let data = { "label": label };
        let val = label;
        if (value.length > 0) {
            data["value"] = value;
            val = value;
        }
        if (val in hinterValueDict) return;

        isDirty = true;
        console.log("<<<addHinterData>>>", label, value);
        hinterData.push(data);
        hinterValueDict[val] = data;
        let hi = data['label'];
        for (let j = hi.length - 2; j >= 0; j--) {
            let curdict = hinterDataDict;
            for (let k = j; k >= 0; k--) {
                if (!(hi[k] in curdict)) {
                    curdict[hi[k]] = {};
                }
                curdict = curdict[hi[k]];
            }
            if (!("_FNS_" in curdict)) {
                curdict["_FNS_"] = [];
            }
            curdict["_FNS_"].push(data);
        }
    }

    function deleteHinterData(label) {
        if (label.length === 0) return;

        isDirty = true;
        console.log("<<<deleteHinterData>>>", label);
        let isEntryFound = false;
        for (let i = 0; i < hinterData.length; i++) {
            if (hinterData[i].label === label) {
                isEntryFound = true;
                let val = hinterData[i].label;
                if ("value" in hinterData[i]) val = hinterData[i]["value"];
                console.log("value is:", val);
                delete hinterValueDict[val];
                hinterData.splice(i, 1);
                break;
            }
        }
        if (isEntryFound === false) {
            console.error("delete entry not found. label=", label);
        }
        let hi = label;
        for (let j = hi.length - 2; j >= 0; j--) {
            let curdict = hinterDataDict;
            let isNormal = true;
            for (let k = j; k >= 0; k--) {
                if (!(hi[k] in curdict)) {
                    console.error("delete entry not found. label=", label);
                    isNormal = false;
                    break;
                }
                curdict = curdict[hi[k]];
            }
            if(!isNormal) break;
            if (!("_FNS_" in curdict)) {
                console.error("_FNS_ not exist. label=", label);
            }
            let fnslist = curdict["_FNS_"];
            let isfnFound = false;
            for (let i = 0; i < fnslist.length; i++) {
                if (fnslist[i].label === label) {
                    isfnFound = true;
                    fnslist.splice(i, 1);
                    break;
                }
            }
            if(!isfnFound) console.error("delete entry not in _FNS_. label=", label);
        }
    }

    function askUserDeleteHinterData(label) {
        console.log("<<<askUserDeleteHinterData>>>");
        alertify.confirm("确定删除条目？ " + label, function (e) {
            if (e) {
                deleteHinterData(label);
                fullUpdate(true);
            } else {
                console.log("askUserDeleteHinterData canceled.");
            }
        });
        
    }

    function updateMatchedHinters() {
        let textBefore = inputTextBefore;
        console.log("getMatchedHinters called: ", textBefore);
        selectedHinterIndex = 0;
        let curdict = hinterDataDict;
        let tempResults = [];
        for (let i = textBefore.length - 1; i >= 0; i--) {
            let cr = textBefore[i];
            let restCount = textBefore.length - i;
            if (cr in curdict) {
                curdict = curdict[cr];
                if (("_FNS_" in curdict)) {
                    for (let k = 0; k < curdict["_FNS_"].length; k++) {
                        tempResults.push({
                            "label_hit": restCount,
                            "entry": curdict["_FNS_"][k]
                        });
                    }
                }
            } else {
                break;
            }
        }
        hintersResult = [];
        for (let i = tempResults.length - 1; i >= 0; i--) {
            hintersResult.push(tempResults[i]);
        }
    }

    function isChildOfClass(elem, className) {
        if (!elem) return false;
        if (!elem.className) return isChildOfClass(elem.parentElement, className);
        if (elem.className.indexOf(className) >= 0) return true;
        if (elem === document.body) return false;
        return isChildOfClass(elem.parentElement, className);
    }

    function addOneHinterSimple() {
        addHinterData(selectionText);
    }

    function updateHinterDataAdder() {
        if (!selectionRect) return;
        //console.log("<<<updateHinterDataAdder>>>");
        let hinterAdder = document.querySelector("#wb-hinter-adder");
        if (!hinterAdder) {
            hinterAdder = document.createElement("div");
            hinterAdder.id = "wb-hinter-adder";
            hinterAdder.style.position = "absolute";
            hinterAdder.style.zIndex = 2000;
            hinterAdder.style.boxShadow = "rgb(234, 234, 234) 3px 3px 5px";
            hinterAdder.style.borderRadius = "50px";
            let button = document.createElement("button");
            button.style.padding = "2px 10px";
            button.style.borderRadius = "50px";
            button.style.fontSize = "14px";
            button.style.background = "linear-gradient(#d6ffa4, #97ffc2)";
            button.classList.add("wb-hinter-adder-button");
            button.addEventListener("click", addOneHinterSimple);
            button.addEventListener("mouseup", () => {
                button.style.background = "linear-gradient(#d6ffa4, #97ffc2)";
            });
            button.addEventListener("mouseleave", () => {
                button.style.background = "linear-gradient(#d6ffa4, #97ffc2)";
            });
            button.addEventListener("mousedown", () => {
                button.style.background = "linear-gradient(#a6bf84, #67cfa2)";
            });
            hinterAdder.appendChild(button);
            document.body.appendChild(hinterAdder);
        }
        if (selectionText.length !== 0) {
            let boundingRect = selectionRect;
            let bodyRect = document.body.getBoundingClientRect();
            hinterAdder.style.display = "";
            hinterAdder.style.left = (boundingRect.left - bodyRect.left) + "px";
            hinterAdder.style.top = (boundingRect.top - bodyRect.top + 24) + "px";
            let button = hinterAdder.getElementsByTagName("button")[0];
            if (selectionText in hinterValueDict) {
                button.disabled = true;
                button.textContent = "√ 已记忆";
            } else if (selectionText.length < 42) {
                button.disabled = false;
                button.textContent = "+ 记忆";
            } else {
                button.disabled = true;
                button.textContent = "⚠ 记忆条目太长";
            }
            //window.external.notify("NOTIFY: ddd | ddd | OK")
        } else {
            hinterAdder.style.display = "none";
            //window.external.notify("NOTIFY: FFF | FFF | OK")
        }
    }

    function updateTextHinter() {
        //console.log("<<<updateTextHinter>>>");
        isHintersShowup = false;
        let hinter = document.querySelector("#wb-text-hinter");
        if (!hinter) {
            hinter = document.createElement("div");
            hinter.id = "wb-text-hinter";
            hinter.style.position = "absolute";
            hinter.style.zIndex = 2000;
            hinter.style.boxShadow = "2px 2px 7px #888888";
            hinter.style.borderRadius = "2px";
            hinter.style.padding = "2px";
            hinter.style.background = "oldlace";
            hinter.style.marginLeft = "6px";
            document.body.appendChild(hinter);
        }
        if (!selectionRect || selectionText.length > 0) {
            hinter.style.display = "none";
            return;
        }
        let boundingRect = selectionRect;
        let bodyRect = document.body.getBoundingClientRect();
        hinter.innerHTML = "";
        updateMatchedHinters();
        if (hintersResult.length == 0) {
            hinter.style.display = "none";
            return;
        }
        hinter.style.display = "";
        for (let i = 0; i < hintersResult.length; i++) {
            let elem = document.createElement("div");
            elem.style.padding = "0px 4px";
            elem.style.borderRadius = "3px";
            let ent = hintersResult[i]["entry"];
            let text = ent["label"];
            if ("value" in ent) text = ent["value"];
            let labelhit = hintersResult[i]["label_hit"];
            let grayText = text.substr(0, labelhit);
            let normalText = text.substr(labelhit);
            let grayNode = document.createElement("span");
            grayNode.innerText = grayText;
            grayNode.style.color = "burlywood";
            grayNode.style.userSelect = "none";
            let normalNode = document.createElement("span");
            normalNode.innerText = normalText;
            normalNode.style.color = "black";
            normalNode.style.userSelect = "none";
            let deleteClickable = document.createElement("button");
            deleteClickable.innerText = "×";
            deleteClickable.style.color = "black";
            deleteClickable.style.background = "white";
            deleteClickable.style.opacity = 0.45;
            deleteClickable.style.float = "right";
            deleteClickable.style.borderRadius = "50px";
            deleteClickable.style.borderWidth = "0px";
            deleteClickable.style.marginTop = "2.5px";
            deleteClickable.style.fontSize = "18px";
            deleteClickable.style.lineHeight = "20px";
            deleteClickable.style.textAlign = "center";
            deleteClickable.style.width = "18px";
            deleteClickable.style.height = "18px";
            deleteClickable.style.marginLeft = "10px";
            deleteClickable.style.display = "inline-block";
            deleteClickable.addEventListener("click", () => askUserDeleteHinterData(text));
            elem.appendChild(grayNode);
            elem.appendChild(normalNode);
            elem.appendChild(deleteClickable);
            hinter.appendChild(elem);
        }
        hinter.style.left = (boundingRect.left - bodyRect.left) + "px";
        hinter.style.top = (boundingRect.top - bodyRect.top) + "px";
        isHintersShowup = true;
        updateTextHinterSelection();
    }

    function updateTextHinterSelection() {
        let hinter = document.querySelector("#wb-text-hinter");
        let cCount = hinter.children.length;
        for (let i = 0; i < cCount; i++) {
            let elem = hinter.children.item(i);
            if (i === selectedHinterIndex) {
                elem.style.background = "steelblue";
                elem.children.item(1).style.color = "white";
            }
            else {
                elem.style.background = "transparent";
                elem.children.item(1).style.color = "black";
            }
        }
    }

    function insertSelectedHinter() {
        if (hintersResult.length === 0) return;
        let labelHit = hintersResult[selectedHinterIndex]['label_hit'];
        let enterHinter = hintersResult[selectedHinterIndex]['entry'];
        let toEnter = 'value' in enterHinter ? enterHinter['value'] : enterHinter['label'];
        toEnter = toEnter.substr(labelHit);
        console.log("<<<insertSelectedHinter>>>", toEnter);
        let sel = null;
        try {
            sel = window.getSelection().getRangeAt(0);
        } catch (e) { }
        if (!sel) return;
        let textNode = document.createTextNode(toEnter);
        sel.insertNode(textNode);
        setCaretPosition(textNode, toEnter.length);
    }

    function setCaretPosition(ctrl, pos) {
        document.getSelection().removeAllRanges();
        let newRange = document.createRange();
        newRange.setStart(ctrl, pos);
        newRange.setEnd(ctrl, pos);
        document.getSelection().addRange(newRange);
    }

    let _oldStartContainer = null;
    let _oldStartOffset = null;
    let _oldEndContainer = null;
    let _oldEndOffset = null;
    function updateSelectionInfo() {
        let sel = null;
        try {
            sel = window.getSelection().getRangeAt(0);
        } catch (e) { }
        if (!sel) return;
        isSelectionChanged = false;
        if (_oldStartContainer !== sel.startContainer) isSelectionChanged = true;
        if (_oldStartOffset !== sel.startOffset) isSelectionChanged = true;
        if (_oldEndContainer !== sel.endContainer) isSelectionChanged = true;
        if (_oldEndOffset !== sel.endOffset) isSelectionChanged = true;
        if (isSelectionChanged) {
            _oldStartContainer = sel.startContainer;
            _oldStartOffset = sel.startOffset;
            _oldEndContainer = sel.endContainer;
            _oldEndOffset = sel.endOffset;
        }
        let contents = sel.cloneContents();
        selectionText = contents.textContent.trim();
        selectionRect = sel.getBoundingClientRect();
        if (isChildOfClass(sel.startContainer, "edui-body-container")) {
            let inputTextsBefore = [];
            let curtext = sel.startContainer.textContent.substr(0, sel.startOffset);
            inputTextsBefore.push(curtext);
            let commonParent = sel.startContainer.parentNode;
            let previousSibling = sel.startContainer.previousSibling;
            while (previousSibling && previousSibling.parentNode === commonParent) {
                inputTextsBefore.push(previousSibling.textContent);
                previousSibling = previousSibling.previousSibling;
            }
            inputTextBefore = "";
            for (let i = inputTextsBefore.length - 1; i >= 0; i--) {
                inputTextBefore = inputTextBefore + inputTextsBefore[i];
            }
            if (inputTextBefore.length > 60) {
                inputTextBefore = inputTextBefore.substr(inputTextBefore.length - 60);
            }
        } else {
            inputTextBefore = "";
        }
    }

    function fullUpdate() {
        updateSelectionInfo();
        updateTextHinter();
    }

    function fullIntervalUpdate() {
        updateSelectionInfo();
        if (isSelectionChanged) {
            console.log("isSelectionChanged. updateTextHinter...");
            updateTextHinter();
        }
        updateHinterDataAdder();
    }

    function hinterInitialize() {
        initHinterData();
        window.addEventListener("keydown", (e) => {
            console.log("key:", e.key, " code:", e.code);
            if (e.key === 'Tab') {
                e.preventDefault();
                insertSelectedHinter();
                setTimeout(fullUpdate, 30);
                return;
            }
            if (isHintersShowup) {
                if (e.key === "ArrowUp") {
                    e.preventDefault();
                    selectedHinterIndex--;
                    if (selectedHinterIndex < 0) selectedHinterIndex = 0;
                    updateTextHinterSelection();
                    return;
                }
                if (e.key === "ArrowDown") {
                    e.preventDefault();
                    selectedHinterIndex++;
                    if (selectedHinterIndex >= hintersResult.length) selectedHinterIndex = hintersResult.length - 1;
                    updateTextHinterSelection();
                    return;
                }
            }
            setTimeout(fullUpdate, 50);
        });
        //window.addEventListener("change", (e) => console.log(e));
        setInterval(fullIntervalUpdate, 250);
    }
    hinterInitialize();


    jQuery("#submit").mouseenter(saveACData);
    function saveACData() {
        if (isDirty === true) {
            window.external.notify("DATA: " + JSON.stringify(hinterData));
            window.external.notify("NOTIFY: 自动补全数据更新 | 本次新增数据已存储(总计" + hinterData.length + "条) | OK");
            isDirty = false;
        }
    }

    //"<button class="preview-btn" type="button" style="background:orange; color:white">保存自动补全</button>"
    try {
        var info0 = document.createElement('p');
        info0.style.color = 'Darkorange';
        info0.style.fontSize = '18px';
        info0.innerText = "[自动补全] 已加载（支持简介和步骤）";
        var info1 = document.createElement('p');
        info1.style.color = "brown";
        info1.style.fontSize = '14px';
        info1.innerText = "快捷输入：Tab键输入自动补全项，区分大小写（例子：尝试输入“windows 10”）";
        var info2 = document.createElement('p');
        info2.style.color = "brown";
        info2.style.fontSize = '14px';
        info2.innerText = "增加条目：选中一段文字，点击“+记忆”";
        var info3 = document.createElement('p');
        info3.style.color = "brown";
        info3.style.fontSize = '14px';
        info3.innerText = "删除条目：点击条目右侧的“×”并确认";
        var bs = jQuery("#brief-section")[0];
        bs.prepend(info3);
        bs.prepend(info2);
        bs.prepend(info1);
        bs.prepend(info0);
    }
    catch (e) {
    }
}


