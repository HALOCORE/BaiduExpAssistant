/**
 * Created by HP on 2018/8/15.
 */


/*
 var data = [
 {value: 'one', label: 'one'},
 {value: 'two', label: 'two'},
 {value: 'three', label: 'three'},
 {value: 'four', label: 'four'},
 {value: 'ok,thats it', label: 'ok,thats it'},
 {value: 'Visual Studio 2015', label: 'Visual Studio 2015'}
 ];
 */

function InitAutoComplete(myData) {

    //debug.  jQuery('input')[1].value = myData;
    var data = null;
    if (myData) {
        try {
            data = eval(myData);
            //debug.  jQuery('input')[2].value = data;
        }
        catch (e) {
            data = null;
        }
    }
    if (!data) {
        data = [
            { value: 'Visual Studio 2015', label: 'Visual Studio 2015' },
            { value: 'windows 10', label: 'windows 10' },
            { value: 'Photoshop CC', label: 'Photoshop CC' },
            { value: '首先，点击下方箭头指出的按钮', label: '首先，点击下方箭头指出的按钮' },
            { value: '最终效果如图所示。', label: '最终效果如图所示。' },
            { value: '点击右上角如图指出的菜单按钮', label: '点击右上角如图指出的菜单按钮' },
            { value: '打开首选项设置面板', label: '打开首选项设置面板' },
            { value: 'WebView', label: 'WebView' },
            { value: 'CentOS', label: 'CentOS' },
            { value: 'Firefox', label: 'Firefox' }
        ];
    }
    var mydiv = document.createElement('div');
    mydiv.style.width = "auto";//"200px";
    mydiv.style.height = "auto";//"40px";
    mydiv.style.padding = "3px";
    mydiv.style.borderRadius = "4px";
    mydiv.style.background = "rgba(120,180,180,0.4)";
    mydiv.style.position = "absolute";
    mydiv.id = "ac-wrapper";
    mydiv.style.zIndex = 99999;
    document.body.appendChild(mydiv);
    jQuery(mydiv).hide();

    var myinput = document.createElement('input');
    myinput.id = 'ac-input';
    myinput.style.fontSize = "18px";
    myinput.style.opacity = 0.8;
    mydiv.appendChild(myinput);

    jQuery(myinput).autocompleter({
        limit: 6,
        template: '{{ label }}',   //'<span>({{ value }})</span>',
        empty: true,
        highlightMatches: true,
        source: data,
        focusOpen: false
    });

    function DataAddEntry(text) {
        var ifExist = false;
        for (d in data) {
            if (text == data[d].value) {
                ifExist = true;
                return;
            }
        }
        if (!ifExist) {
            data.push({ value: text, label: text });
            jQuery(myinput).autocompleter('option', { source: data });
        }
    }

    jQuery(myinput).keydown(function (e) {
        var acdiv = jQuery("#ac-wrapper");

        if (e.keyCode == 9) //tab
        {

            if (acdiv.find('.autocompleter-closed').length > 0) {
                //closed
                //do nothing
            }
            else if (acdiv.find('.autocompleter-item-selected').length == 0) {
                //not select any one
                //contain item
                var liList = acdiv.find('li');
                if (liList.length > 0) {
                    var first = liList[0].attributes['data-value'].value;
                    myinput.value = first;
                }
            }
        }
        if (e.keyCode == 13 || e.keyCode == 9) //tab or enter
        {
            e.preventDefault();
            acdiv.hide();
            focusBack(myinput.value);
            DataAddEntry(myinput.value);
        }
        //console.log(e.keyCode);
    });

    var currentFocus = null;
    var currentAnchorNode = null; //not used
    var currentAnchorOffset = null; //not used
    var currentFocusNode = null;
    var currentFocusOffset = null;

    function focusBack(text) {
        if (!currentFocus) return;
        if (!currentFocusNode) return;

        jQuery(currentFocus).trigger({ type: 'focus', target: currentFocus });
        if (currentFocusNode.tagName != 'INPUT') {
            try {
                currentSelection.setBaseAndExtent(
                    currentFocusNode,
                    currentFocusNode.textContent.length,
                    currentFocusNode,
                    currentFocusNode.textContent.length);

                currentFocusNode.textContent =
                    currentFocusNode.textContent.substr(
                        0, currentFocusOffset)
                    + text
                    + currentFocusNode.textContent.substr(
                        currentFocusOffset);
            }
            catch (e) {
                currentFocusNode.textContent = currentFocusNode.textContent + text;
            }
        }
        else {
            try {
                //must error
                currentSelection.setBaseAndExtent(
                    currentFocusNode,
                    currentFocusNode.value.length,
                    currentFocusNode,
                    currentFocusNode.value.length);

                currentFocusNode.value =
                    currentFocusNode.value.substr(
                        0, currentFocusOffset)
                    + text
                    + currentFocusNode.value.substr(
                        currentFocusOffset);
            }
            catch (e) {
                currentFocusNode.value = currentFocusNode.value + text;
            }
        }

        try {
            currentSelection.setBaseAndExtent(
                currentFocusNode,
                currentFocusOffset + text.length,
                currentFocusNode,
                currentFocusOffset + text.length);
        } catch (e) {
            // try to move.
            currentSelection.selectAllChildren(currentFocusNode);
            currentSelection.collapseToEnd();
        }
    }

    function UpdateAcPosition(zone) {
        currentSelection = getSelection();
        currentAnchorNode = currentSelection.anchorNode;
        currentAnchorOffset = currentSelection.anchorOffset;
        currentFocusNode = currentSelection.focusNode;
        currentFocusOffset = currentSelection.focusOffset;

        var node = currentSelection.focusNode;
        var poffx = jQuery(node.parentNode).offset().left;
        var poffy = jQuery(node.parentNode).offset().top;
        var toffx = jQuery(node.parentNode).width() - jQuery(mydiv).width();
        var toffy = 0;//jQuery(node).offset().top;
        SetAcPosition(poffx + toffx, poffy + toffy);
    }

    function SetAcPosition(x, y) {
        var acdiv = jQuery("#ac-wrapper")[0];
        if (!acdiv) return;
        acdiv.style.left = x + "px";
        acdiv.style.top = y + "px";
    }

    function ACKeyDown(e) {
        if (e.keyCode == '9') {
            e.preventDefault();
        }
        CheckAcKey(e.target, e);
    }

    function ACMouseDown(e) {
        //spare.
    }

    jQuery(document).click(ACAdd);
    jQuery(document).keydown(ACAdd);

    function ACAdd() {
        if (
            document.activeElement.className.indexOf("edui-body-container") >= 0
            //|| (document.activeElement.tagName == "INPUT" && document.activeElement.id != "ac-input") //disabled this
        ) {
            if (document.activeElement.className.indexOf("ac-addin") < 0) {
                document.activeElement.className += " ac-addin";
                jQuery(document.activeElement).keydown(ACKeyDown);
                jQuery(document.activeElement).mousedown(ACMouseDown);
            }
        }
    }


    function CheckAcKey(zone, e) {
        if (e.key == 'Tab') {
            //e.preventDefault(); # already prevent in keydown
            UpdateAcPosition(zone);
            var acdiv = jQuery("#ac-wrapper");
            acdiv.show();
            var acinput = acdiv.find('input');
            acinput[0].value = "";
            acinput.trigger('focus');
            currentFocus = zone;
        }
    }

    jQuery("#submit").mouseenter(saveACData);
    function saveACData() {
        window.external.notify("DATA: " + JSON.stringify(data));
    }

    //"<button class="preview-btn" type="button" style="background:orange; color:white">保存自动补全</button>"
    try {
        var info0 = document.createElement('p');
        info0.style.color = 'Darkorange';
        info0.style.fontSize = '18px';
        info0.innerText = "[自动补全(Beta)] 已经加载";
        var info1 = document.createElement('p');
        info1.style.color = "brown";
        info1.style.fontSize = '14px';
        info1.innerText = "在简介或者步骤当中，使用Tab键呼出自动补全框，在补全框中输入的同时会检索。";
        var info2 = document.createElement('p');
        info2.style.color = "brown";
        info2.style.fontSize = '14px';
        info2.innerText = "Tab键：    检索结果存在时，Tab插入第一项；检索结果不存在时，Tab键插入补全框中内容并记忆。";
        var info3 = document.createElement('p');
        info3.style.color = "brown";
        info3.style.fontSize = '14px';
        info3.innerText = "Enter键：  不论检索结果是否存在，Enter键总是插入补全框中内容。";
        var info4 = document.createElement('p');
        info4.style.color = "Darkorange";
        info4.style.fontSize = '14px';
        info4.innerText = "连续自动补全bug已修复。另外不再监听ctrl按键，全选/复制/粘贴可以正常使用了。";
        //var info5 = document.createElement('p');
        //info5.style.color = "Darkorange";
        //info5.style.fontSize = '14px';
        //info5.innerText = "在单行文本输入框中（标题，工具，注意事项），输入机制不同，暂时不能使用自动补全。";
        var bs = jQuery("#brief-section")[0];
        //bs.prepend(info5);
        bs.prepend(info4);
        bs.prepend(info3);
        bs.prepend(info2);
        bs.prepend(info1);
        bs.prepend(info0);
    }
    catch (e) {
    }
}


