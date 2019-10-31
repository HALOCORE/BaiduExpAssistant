using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace 百度经验个人助手
{
    public static class JSCodeString
    {
        #region Special Info During Navigation
        public static bool isCookieInsertSucceed = false;
        public static string planToGoUrl = "";
        #endregion

        #region WebView Set
        //每当一个Navigation完成看是否是主页（插入Cookie).
        private static async Task InsertCookieAndRefresh(
            WebView sender,
            WebViewNavigationCompletedEventArgs e)
        {
            string js0 =
                "var script = document.createElement(\"script\");\r\n"
                + " script.type = \"text/javascript\";\r\n"
                + "script.text = url;\r\n"
                + "document.body.appendChild(script);";

            // Utility.ShowMessageDialog("InsertCookieAndRefresh", sender.Source.AbsoluteUri);
            if (sender.Source.AbsoluteUri.EndsWith("jingyan.baidu.com/") || sender.Source.AbsoluteUri.EndsWith("jingyan.baidu.com"))
            {
                isCookieInsertSucceed = false; //assume false.

                if (string.IsNullOrEmpty(ExpManager.newMainUserName))
                {
                    Utility.ShowMessageDialog("请返回设置有效的Cookie（请勿在此登录）", "当前Cookie信息无效，无法保持登录。请返回设置Cookie。\n请勿在此登录！");
                    return;
                }
                string curCookie = await sender.InvokeScriptAsync(
                    "eval",
                    new string[]
                    {
                        "document.documentElement.outerHTML;"
                    });
                
                if (curCookie.Contains(ExpManager.newMainUserName))
                {
                    isCookieInsertSucceed = true;
                    App.currentMainPage.WebSetUpdate(true, true, true, true);
                    return;
                }


                App.currentMainPage.ShowLoading("添加Cookie...");

                string bduss = "";
                foreach (HttpCookiePairHeaderValue cp in ExpManager.client.DefaultRequestHeaders.Cookie)
                {
                    if (cp.Name == "BDUSS")
                    {
                        bduss = cp.Value.ToString();
                        break;
                    }
                }
                if (bduss == "") return;

                string js = ""; //构建脚本
                js += "var str = \"BDUSS\" + \"=\" + escape(\""
                      + bduss
                      + "\"); document.cookie = str;";

                await sender.InvokeScriptAsync("eval", new string[] { js });

                //相当于刷新页面了。e是事件参数。
                // still assume isCookieInsertSucceed = false. 
                // Make judgement when navigation complete and enter this function again
                sender.Navigate(e.Uri);
            }



        }

        public static void SetWebView(WebView webView)
        {

            webView.NavigationCompleted += async (view, args) =>
            {
                App.currentMainPage.HideLoading();
                await InsertCookieAndRefresh(view, args);

                // if plan to go somewhere, go. Currently include [the reward edit page]
                // await Utility.ShowMessageDialog("debug: " + isCookieInsertSucceed.ToString(), planToGoUrl);
                if (isCookieInsertSucceed && planToGoUrl != "")
                {
                    string url = planToGoUrl;
                    planToGoUrl = "";
                    App.currentMainPage.ShowLoading("前往 " + url + " ... ");
                    webView.Navigate(new Uri(url));
                }
                
                
            };


            webView.NewWindowRequested += (sender, args) =>
            {
                args.Handled = true;
                WebViewNewWindowRequestedEventArgs argss = args;
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, argss.Uri);
                req.Headers.Referer = argss.Referrer;


                ((WebView)sender).NavigateWithHttpRequestMessage(req);
            };

            webView.ScriptNotify += (o, args) =>
            {
                if (args.Value.StartsWith("DATA: "))
                {
                    string jsonData = args.Value.Substring(6);
                    StorageManager.SaveAutoCompleteData("", jsonData);
                    //Utility.ShowMessageDialog(o.ToString(), args.Value);
                }
                else
                {
                    Utility.ShowMessageDialog(o.ToString(), args.Value);
                }
            };
        }

        #endregion

        public static string JsAddPictureBox =
                "function AddMyImg() {\n    myoldbox = document.getElementById(\"my-img-preview\");\n    if (myoldbox) {\n        myoldbox.parentNode.removeChild(myoldbox);\n    }\n    var box = document.createElement(\"div\");\n    box.className = \"img-preview\";\n    box.id = \"my-img-preview\";\n    box.style.left = 200.183 + \"px\";\n    box.style.top = 100.717 + \"px\";\n    box.style.display = \"block\";\n    box.style.position = \"fixed\";\n    box.style.width = 400 + 12 + \"px\";\n    box.innerHTML = '<div class=\"img-wrapper\" style=\"width:400px;\">' +\n        '<div id=\"imgbox-help\" style=\"margin:20px; float:left;\">'\n        + '<p>1. \u5C06\u9F20\u6807\u653E\u5728\u67D0\u4E2A\u5C0F\u56FE\u4E0A\uFF0C\u7B49\u51FA\u73B0\u5F39\u51FA\u56FE\u7247</p>'\n        + '<p>2. \u9F20\u6807\u7ECF\u8FC7\u5F39\u51FA\u56FE\u7247\u6846\uFF0C\u56FE\u7247\u81EA\u52A8\u663E\u793A\u5728\u6B64</p>'\n        + '<p>3. \u56FE\u7247\u663E\u793A\u4EE5\u540E\uFF0C\u6B64\u6846\u5373\u53EF\u81EA\u7531\u62D6\u62FD</p>'\n        + '<p>4. \u56FE\u7247\u663E\u793A\u540E\uFF0C\u70B9\u51FB+-\u8C03\u6574\u5927\u5C0F</p>'\n        + '</div>'\n        + '<div class=\"pr\" style=\"width:400px; float:left;\"><img id=\"my-bigimg\" style=\"width: 100%; height:100%;  left: 0px; top: -19.2414px;\"></div>'\n        + '</div>'\n        + '<div id=\"myimg-buttons\" style=\"position:absolute; opacity: 0.7; left: 0px; top: 0px; display: none;\">'\n        + '<div id=\"myimg-bigger\" style=\"width:35px; height:35px; background:rgba(200,200,200,100); text-align:center; line-height:35px; border-radius:20px;  display:inline-block\">+</div>'\n        + '<div id=\"myimg-smaller\" style=\"width:35px; height:35px; background:rgba(200,200,200,100); text-align:center; line-height:35px; border-radius:20px;  display:inline-block;\">-</div>'\n        + '</div>';\n\n    document.body.appendChild(box);\n    jQuery(\"#myimg-bigger\").click(function (e) {\n        e.preventDefault();\n        var wp = jQuery(box).find('.img-wrapper')[0];\n        var width = parseInt(wp.style.width);\n        wp.style.width = (width * 1.1) + 'px';\n        var pr = jQuery(box).find('.pr')[0];\n        pr.style.width = wp.style.width;\n        updateSize();\n    });\n    jQuery(\"#myimg-smaller\").click(function (e) {\n        e.preventDefault();\n        var wp = jQuery(box).find('.img-wrapper')[0];\n        var width = parseInt(wp.style.width);\n        wp.style.width = (width * 0.91) + 'px';\n        var pr = jQuery(box).find('.pr')[0];\n        pr.style.width = wp.style.width;\n        updateSize();\n    });\n\n    function updateSize() {\n        var myimg = document.getElementById(\"my-bigimg\");\n        var imgheight = myimg.height;\n        var imgwidth = myimg.width;\n\n        var mybox = document.getElementById(\"my-img-preview\");\n        mybox.style.height = imgheight + 10 + \"px\";\n        mybox.style.width = imgwidth + 12 + \"px\";\n        mybox.children[0].style.height = imgheight.toString() + \"px\";\n    }\n\n    jQuery(\"#img-preview-box\").hover(function () {\n        var helper = document.getElementById(\"imgbox-help\");\n        helper.style.display = \"none\";\n\n        var myimg = document.getElementById(\"my-bigimg\");\n        var oribox = document.getElementById(\"img-preview-box\");\n        if (oribox.childElementCount > 0)\n            myimg.src = oribox.children[0].src;\n        updateSize();\n    });\n\n    jQuery(\"#my-img-preview\").mouseenter(function(){\n        document.getElementById(\"myimg-buttons\").style.display = 'block';\n    });\n\n    jQuery(\"#my-img-preview\").mouseleave(function(){\n        document.getElementById(\"myimg-buttons\").style.display = 'none';\n    });\n\n    jQuery(\"#my-bigimg\").load(function () {\n\n        document.getElementById(\"my-bigimg\").onmousedown = function (e) {\n            e.preventDefault();\n        };\n\n        var drag = document.getElementById(\"my-img-preview\");\n        var isDown = false;\n        var diffX = 0;\n        var diffY = 0;\n\n        drag.onmousedown = function (e) {\n            diffX = e.clientX - drag.offsetLeft;\n            diffY = e.clientY - drag.offsetTop;\n            isDown = true;\n        };\n\n        document.onmousemove = function (e) {\n            if (isDown === false) return;\n            var left = e.clientX - diffX;\n            var top = e.clientY - diffY;\n\n            if (left < 0) {\n                left = 0;\n            } else if (left > window.innerWidth - drag.offsetWidth) {\n                left = window.innerWidth - drag.offsetWidth;\n            }\n            if (top < 0) {\n                top = 0;\n            } else if (top > window.innerHeight - drag.offsetHeight) {\n                top = window.innerHeight - drag.offsetHeight;\n            }\n            drag.style.left = left + 'px';\n            drag.style.top = top + 'px';\n        };\n\n        document.onmouseup = function (e) {\n            isDown = false;\n        };\n    });\n}\nAddMyImg();\n"
            ;

        public static string JsAutoFillTitle =
                "function FocusBlurJQTag(jqTag){\n    var edbf = jqTag[0];\n    myEfocus={\n        target: edbf,\n        type: 'focus'\n    };\n    myEBlur={\n        target: edbf,\n        type: 'blur'\n    };\n    jqTag.trigger(myEfocus);\n    jqTag.trigger(myEfocus);\n    jqTag.trigger(myEBlur);\n    jqTag.trigger(myEBlur);\n}\n\nfunction GetInputTitle() {\n    var ip = document.getElementsByName(\"title\");\n    return ip[0].value;\n}\n\nfunction SetInputBrief(strBrief){\n    var bf = document.getElementById(\"editor-brief\");\n    bf.children[0].innerText = strBrief;\n    FocusBlurJQTag(jQuery(bf));\n}\n\nfunction AutoSetBrief(strFormat){\n    var mytitle = GetInputTitle();\n    var tsp = mytitle.split(/[:\uFF1A\\s]/);\n    var title1 = tsp[0];\n    var title2 = \"\";\n    if(tsp.length > 1)  title2 = tsp[1];\n\n    var tsp2 = mytitle.split(/(\u5982\u4F55|\u600E\u4E48)/);\n    var title3 = tsp2[0];\n    var title4 = \"\";\n    if(tsp2.length > 2)  title4 = tsp2[2];\n\n    var reg=/\\\\1/g;\n    var result = strFormat.replace(reg,title1);\n    reg = /\\\\2/g;\n    result = result.replace(reg,title2);\n    reg = /\\\\3/g;\n    result = result.replace(reg,title3);\n    reg = /\\\\4/g;\n    result = result.replace(reg,title4);\n    reg = /\\\\0/g;\n    result = result.replace(reg,mytitle);\n\n    SetInputBrief(result);\n}\n"
            ;

        public static string JsAutoFillTools =
                "function SetToolIndex(index, strTool){\n    var t0 = jQuery(\"#tools\");\n    l0 = t0.find(\"li\")[index];\n    i0 = t0.find(\"input\")[index];\n    if(!i0) return;\n    i0.value = strTool;\n\n    myEfocusin = {\n        target: i0,\n        type: 'focusin'\n    };\n    myEFocusout = {\n        target: i0,\n        type: 'focusout'\n    };\n    jQuery(l0).trigger(myEfocusin);\n    jQuery(l0).trigger(myEFocusout);\n}\n\nfunction AutoSetTools(strkt) {\n    var ip = document.getElementsByName(\"title\");\n    title = ip[0].value;\n    var kts = strkt.split(/[\\r\\n]+/);\n\n    var toolCount = 0;\n    for(i in kts)\n    {\n        kt = kts[i];\n        var ktgroup = kt.split('=');\n        if(ktgroup.length > 1)\n        {\n            if(title.toUpperCase().search(ktgroup[0].toUpperCase()) != -1)\n            {\n                SetToolIndex(toolCount, ktgroup[1]);\n                toolCount++;\n            }\n        }\n    }\n}\n"
            ;

        public static string JsAutoFillNotice =
                "function SetNoticeIndex(index, strNotice){\n    var t0 = jQuery(\"#notice\");\n    l0 = t0.find(\"li\")[index];\n    i0 = t0.find(\"input\")[index];\n    if(!i0) return;\n    i0.value = strNotice;\n\n    myEfocusin = {\n        target: i0,\n        type: 'focusin'\n    };\n    myEFocusout = {\n        target: i0,\n        type: 'focusout'\n    };\n    jQuery(l0).trigger(myEfocusin);\n    jQuery(l0).trigger(myEFocusout);\n}"
            ;

        public static string JsAutoSetCategory =
                "//AutoSetCategory(string)\nfunction SetCategory(ind1,ind2,ind3){\n    function SetCategoryLevel1(intIndex){\n        var st = document.getElementsByClassName(\"category-level-1\")[0];\n        if(!st) return;\n        var cont = document.getElementById(\"category\");\n        st.selectedIndex = intIndex;\n    }\n    function SetCategoryLevel2(intIndex){\n        var st = document.getElementsByClassName(\"category-level-2\")[0];\n        if(!st) return;\n        var cont = document.getElementById(\"category\");\n        st.selectedIndex = intIndex;\n    }\n    function SetCategoryLevel3(intIndex){\n        var st = document.getElementsByClassName(\"category-level-3\")[0];\n        if(!st) return;\n        var cont = document.getElementById(\"category\");\n        st.selectedIndex = intIndex;\n    }\n    myEChange1 = {\n        type: \"change\",\n        target: jQuery(\"#category .category-level-1\")[0]\n    };\n    myEChange2 = {\n        type: \"change\",\n        target: jQuery(\"#category .category-level-2\")[0]\n    };\n    myEChange3 = {\n        type: \"change\",\n        target: jQuery(\"#category .category-level-3\")[0]\n    };\n    SetCategoryLevel1(ind1);\n    jQuery(\"#category\").trigger(myEChange1);\n    SetCategoryLevel2(ind2);\n    jQuery(\"#category\").trigger(myEChange2);\n    SetCategoryLevel3(ind3);\n    jQuery(\"#category\").trigger(myEChange3);\n}\nfunction AutoSetCategory(strcg) {\n    var ip = document.getElementsByName(\"title\");\n    title = ip[0].value;\n    var kts = strcg.split(/[\\r\\n]+/);\n\n    var toolCount = 0;\n    for(i in kts)\n    {\n        kt = kts[i];\n        var ktgroup = kt.split('=');\n        if(ktgroup.length > 1)\n        {\n            if(title.toUpperCase().search(ktgroup[0].toUpperCase()) != -1)\n            {\n                var cs = new Array(0, 0, 0);\n\n                cints = ktgroup[1].trim().split(/\\s+/);\n                for(var i=0; i<cints.length; ++i)\n                {\n                    cs[i] = parseInt(cints[i]);\n                }\n                SetCategory(cs[0], cs[1], cs[2]);\n                break;\n            }\n        }\n    }\n}"
            ;

        public static string JsAutoCheckOrigin =
                "function CheckOrigin(ifDo)\n{\n    if(ifDo && !jQuery(\"#is-origin\")[0].checked) {\n        jQuery(\"#is-origin\").click();\n    }\n    jQuery(\"#submit .release-btn\").hover(function(){\n        if(!jQuery(\"#is-origin\")[0].checked){\n            alert(\"\u8BF7\u68C0\u67E5\u662F\u5426\u52FE\u9009\u539F\u521B\u3002\");\n        }\n    });\n}"
            ;

        public static string JsAutoAddStep =
                "function AddStep(n){\n    for(var i = 0; i<n; ++i)\n    {\n        jQuery(\"#steps-content .add-item-btn\").click();\n    }\n}"
            ;

        public static string JsAddCssScript =
                "function AddScriptUri(uri){\n    var se = document.createElement('script');\n    se.src = uri;\n    document.head.appendChild(se);\n}\n\nfunction AddCssUri(uri){\n    var ce = document.createElement('link');\n    ce.href = uri;\n    ce.rel = \"stylesheet\";\n    ce.type = \"text/css\";\n    document.head.appendChild(ce);\n}\n"
            ;

        public static string JsAutoFillSteps =
                "//Set Steps\n\nfunction htmlEscape(str) {\n    return String(str)\n        .replace(/&/g, '&amp;')\n        .replace(/\"/g, '&quot;')\n        .replace(/'/g, '&#39;')\n        .replace(/</g, '&lt;')\n        .replace(/>/g, '&gt;');\n}\n\nfunction FocusBlurEditor(jqDiv){\n    var edbf = jqDiv[0];\n    myEfocus={\n        target: edbf,\n        type: 'focus'\n    };\n    myEBlur={\n        target: edbf,\n        type: 'blur'\n    };\n    jqDiv.trigger(myEfocus);\n    jqDiv.trigger(myEfocus);\n    jqDiv.trigger(myEBlur);\n    jqDiv.trigger(myEBlur);\n}\n\nfunction SetStepIndex(index, stepContent)\n{\n    var edts = jQuery(\".edui-body-container\");\n    var stepbox =edts[index];\n    if(!stepbox) return;\n\n    var stepps = stepContent.split(/[\\n\\r]+/);\n    html = \"\";\n    for(i in stepps){\n        html += \"<p>\" + htmlEscape(stepps[i]) + \"</p>\\n\";\n    }\n    stepbox.innerHTML = html;\n    FocusBlurEditor(jQuery(stepbox));\n}\n\nfunction regexSetStep(arg0, indexStr, stepContent)\n{\n    console.log(\"index\uFF1A\"+indexStr + \" content:\" + stepContent);\n    var index = parseInt(indexStr);\n    if(index >= 0){\n        SetStepIndex(index, stepContent.trim());\n    }\n    return \"\";\n}\n\nfunction SetSteps(stepsStr)\n{\n    stepsStr.replace(/<\u6B65\u9AA4(\\d+)>([\\s\\S]+?)<\\/\u6B65\u9AA4\\1>/g, regexSetStep);\n}"
            ;

        public static async Task AddScriptUri(WebView webview, string uri)
        {
            await RunJs2(webview, JsAddCssScript, "AddScriptUri(\"" + uri + "\");");
        }

        public static async Task AddCssUri(WebView webview, string uri)
        {
            await RunJs2(webview, JsAddCssScript, "AddCssUri(\"" + uri + "\");");
        }

        public static async Task RunJs(WebView webview, string js)
        {
            await webview.InvokeScriptAsync("eval", new string[] {js});
        }

        public static async Task RunJs2(WebView webview, string js, string js2)
        {
            await webview.InvokeScriptAsync("eval", new string[] { js + js2 });
        }
    }
}
