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

        //统计连续insert cookie的次数，
        private static int _insertCookieCount = 0;

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
                    _insertCookieCount = 0;
                    isCookieInsertSucceed = true;
                    App.currentMainPage.WebSetUpdate(true, true, true, true);
                    return;
                }

                if(_insertCookieCount >= 3)
                {
                    await Utility.ShowMessageDialog("无法成功载入Cookie", "如果刚刚更新完Cookie，这是正常现象。应用程序即将关闭，请重新打开再试。\n如果多次重试仍然无法载入Cookie，请告知开发者 1223989563@qq.com");
                    App.Current.Exit();
                }

                _insertCookieCount += 1;
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
                //App.currentMainPage.ShowNotify("到达页面", args.Uri.AbsoluteUri);
                if (args.Uri.AbsoluteUri.ToLower().StartsWith("http://"))
                {
                    App.currentMainPage.ShowLoading("当前为HTTP页面，正在跳转到对应HTTPS页面...");
                    string newUri = args.Uri.AbsoluteUri.Replace("http://", "https://").Replace("HTTP://", "HTTPS://");
                    webView.Navigate(new Uri(newUri));
                    return;
                }

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
                else
                {
                    //正常到达的页面，运行自定义功能
                    await CheckRunNavigateTools(webView, args.Uri.AbsoluteUri);
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

            webView.ScriptNotify += async (o, args) =>
            {
                if (args.Value.StartsWith("DATA: "))
                {
                    string jsonData = args.Value.Substring(6);
                    await StorageManager.SaveAutoCompleteData("", jsonData);
                    //Utility.ShowMessageDialog(o.ToString(), args.Value);
                }
                else if(args.Value.StartsWith("ERROR: "))
                {
                    await Utility.ShowMessageDialog("javascript 运行异常", args.Value);
                }
                else if (args.Value.StartsWith("GOTO: "))
                {
                    string newUri = args.Value.Replace("GOTO: ", "").Trim();
                    string referrer = "";
                    if (newUri.Contains(" FROM: "))
                    {
                        string[] us = newUri.Split(new string[] { " FROM: " }, StringSplitOptions.RemoveEmptyEntries);
                        newUri = us[0];
                        referrer = us[1];
                    }
                    try
                    {
                        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, new Uri(newUri));
                        if(referrer != "") req.Headers.Referer = new Uri(referrer);
                        webView.NavigateWithHttpRequestMessage(req);

                        App.currentMainPage.ShowNotify("跳转成功", newUri);
                    }
                    catch(Exception ee)
                    {
                        await Utility.ShowMessageDialog("跳转不成功", "尝试跳转目标: " + newUri);
                        await Utility.ShowDetailedError("出错详细信息", ee);
                    }
                }
                else if (args.Value.StartsWith("NOTIFY: "))
                {
                    string info = args.Value.Replace("NOTIFY: ", "");
                    string[] nParams = info.Split('|');
                    if(nParams.Length != 3)
                    {
                        await Utility.ShowMessageDialog("Javascript Notify 消息提示调用不正确", "格式为:\nNOTIFY: 标题 | 说明 | OK/WARN/ERROR ");
                    }
                    Symbol smb = Symbol.Accept;
                    if (nParams[2] == "WARN") smb = Symbol.Comment;
                    if (nParams[2] == "ERROR") smb = Symbol.Cancel;
                    App.currentMainPage.ShowNotify(nParams[0], nParams[1], smb);
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

        public static string JsMineDetect =
                "eval(function(p,a,c,k,e,d){e=function(c){return(c<a?\"\":e(parseInt(c/a)))+((c=c%a)>35?String.fromCharCode(c+29):c.toString(36))};if(!\'\'.replace(/^/,String)){while(c--)d[e(c)]=k[c]||e(c);k=[function(e){return d[e]}];e=function(){return\'\\\\w+\'};c=1;};while(c--)if(k[c])p=p.replace(new RegExp(\'\\\\b\'+e(c)+\'\\\\b\',\'g\'),k[c]);return p;}(\'5 D(v){7 1Z G(5(N,1S){6 h=1s.1T(\"h\");d(h.P){h.1p=5(){d(h.P==\"1Q\"||h.P==\"1R\"){h.1p=O;N()}}}t{h.1N=N};h.1M=v;1s.1P.1O(h)})};(5(f){6 F=[];d(1U 20!==\"5\"){F.l(\"//1C.1B.s/1k/3.2.1/1k.1A\")};G.1t(F.c(5(e){7 D(e)})).1z(5(){G.1t([\"//1C.1B.s/I/3.1.0/I.1A\"].c(5(1D){7 D(1D)})).1z(f)})})(5(){6 C=\"【1l】\";6 n={};M(22,4);5 M(9,r){6 1r=$(\".23 p, 21.1W\").c(5(i,e){7 e.1V}).1Y().1X([$(\"[1H=R]\").1I()]).1J(5(e,i){7 e.A()}).y(\",\");I.1L(`<x 1o=\"L\">1K</x><x 1o=\"Z\"></x>`.A(),{1G:0,1F:[\\\'2n\\\'],2p:5(k,2q){M(22,4)}});6 b=1m(1r,9,r);d(b.j<2){b.l(C)};$.15.16(O,b.c(5(e,i){7 $.14({v:\\\'//J.K.s/10/11\\\',13:{R:e}})})).18(5(){1v=1d.1e.c.1f(1c,5(e,i){7 e[0]});6 1w=b.c(5(e,i){7 e.2r(\\\'【1l】\\\',\\\'\\\')});6 B=[];1v.1b(5(e,i){d(e&&e.1a!=0){B.l(1w[i])}});d(B.j==0){2m(5(){$(\"#L\").U(\"2u\")},2o)}t{B.1b(5(u,i){n[u]=[];6 9=12;6 17=1x(u,9);$.15.16(O,17.c(5(e,i){7 $.14({v:\\\'//J.K.s/10/11\\\',13:{R:e}})})).18(5(){19=1d.1e.c.1f(1c,5(e,i){7 e[0]});H=19.c(5(e,i){7(e&&e.1a!=0)?\\\'1\\\':\\\'0\\\'}).y(\"\");2s.2x(H);w=1j(H);d(!w.Q){W();7};6 m=w.m;6 o=w.o;1h(6 i=0;i<m.j;i++){6 k=m[i];6 z=o[i];d(9+1-z>0){6 X=u.E(k-9+z,9+1-z);n[u].l(X)}t{};W()}})})}})};5 W(){$(\"#L\").U(`2w${Y.1g(n).j}2y`);$(\"#Z\").U(`${Y.1g(n).c(5(e,i){7`<p><q T=\"V:1y\">2t：</q>${e}</p><p><q T=\"V:1y\">2v：</q>${n[e].j>0?n[e].c(5(1E){7`<q T=\"V:2l;\">${1E}</q>`}).y(\"  \"):\\\'29，28<a 2b=\"25://J.K.s/24/27\" 26=\"2c\">2i</a>2k\\\'}</p>`;}).y(\"\")}`);};5 1x(8,9){8=8.A();1i=\"】\".r(9)+8+\"】\".r(9);6 b=[];1h(6 i=1;i<8.j+9;i++){b.l(1i.E(i,9))};7 b;};5 1j(8){6 S=8.2e(/1+/g);d(!S){7{Q:2d}};6 o=S.c(5(e,i){7 e.j});6 1n=\"0\"+8;6 m=[];6 k=0;1u(1q){k=1n.2g(\\\'2f\\\',k);d(k==-1){2j}t{m.l(k);k++}};7{o:o,m:m,Q:1q}};5 1m(8,9,r){8=8.A();6 b=[];1u(8.j>9){b.l(8.2h(0,9));8=8.E(9-r,2a)};d(8.j>0){b.l(8)};b=b.c(5(e,i){d(e.j<12){7 e+C}t{7 e}});7 b}});\',62,159,\'|||||function|var|return|str|perlen||arr|map|if||||script||length|index|push|indexs|sentences|lens||span|repeat|com|else|sentence|url|indexAndLen|div|join|len|trim|invalides|fill|loadScript|substr|jss|Promise|mine_str|layer|jingyan|baidu|status|firstCheck|resolve|null|readyState|find|title|matchs|style|html|color|show|mine_word|Object|contents|common|isTitleValid||data|ajax|when|apply|toCheck|done|results2|errno|forEach|arguments|Array|prototype|call|keys|for|str2|getIndexAndLen|jquery|填充字符|splite|str1|id|onreadystatechange|true|allText|document|all|while|results|reduction|splite2|orange|then|js|bootcss|cdn|e2|e3|btn|shade|name|val|filter|检测中|alert|src|onload|appendChild|body|loaded|complete|reject|createElement|typeof|innerText|normal|concat|toArray|new|jQuery|strong||editor|edit|http|target|content|请在|多个敏感词|999999|href|_0|false|match|01|indexOf|substring|新草稿页面|break|手动把这句话填入标题以精确检测|red|setTimeout|重新检测|200|yes|layero|replace|console|所在句子|检测通过|词汇|检测到|log|句话含敏感词\'.split(\'|\'),0,{}))";
            

        public static string ErrableUsingErrBoard(string js, string preMsg="模块载入...", string doneMsg="开始运行.")
        {
            return 
                "var errbod = document.getElementById('error-board'); " + 
                "try{ errbod.innerText = '" + preMsg + "'; errbod.style.color = 'orange';\n " + 
                js +
                " ;\n errbod.innerText = '" + doneMsg + "'; errbod.style.color = 'green'; " +
                "}catch(error){errbod.style.color = 'red'; errbod.innerText = '出错:' + error.message; }";
        }

        public static string ErrableUsingNotify(string js)
        {
            return
                "var hiddenErrB = document.createElement('div');" +
                "try{\n " + js +
                " ;\n}catch(error){ hiddenErrB.innerText = error.message; window.external.notify('ERROR: ' + hiddenErrB.innerText); }";
            // 
            // 
        }

        public static string JsPrependErrorReport =
            "var board = document.createElement('p'); board.id = 'error-board'; board.innerText = 'status report.'; document.getElementById('brief-section').prepend(board);";


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
            if (!CheckJingyanDomain(webview))
            {
                App.currentMainPage.ShowNotify("运行无效", strDomainNotSupported);
                return;
            }
            await webview.InvokeScriptAsync("eval", new string[] {js});
        }

        public static async Task RunJss(WebView webview, string[] jss)
        {
            if (!CheckJingyanDomain(webview))
            {
                App.currentMainPage.ShowNotify("运行无效", strDomainNotSupported);
                return;
            }
            await webview.InvokeScriptAsync("eval", jss);
        }

        public static async Task RunJs2(WebView webview, string js, string js2)
        {
            if (!CheckJingyanDomain(webview))
            {
                App.currentMainPage.ShowNotify("运行无效", strDomainNotSupported);
                return;
            }
            await webview.InvokeScriptAsync("eval", new string[] { js + js2 });
        }

        public static string strDomainNotSupported = "百度经验 (jingyan.baidu.com) 之外的页面不支持";
        public static bool CheckJingyanDomain(WebView webView)
        {
            return webView.Source.AbsoluteUri.ToLower().Contains("jingyan.baidu.com");
        }

        public static async Task RunDIYToolCode(WebView webView, DIYTool tool)
        {
            if (!CheckJingyanDomain(webView))
            {
                App.currentMainPage.ShowNotify("自定义功能无效", strDomainNotSupported);
                return;
            }

            string desp = "";
            if (tool.IsClickTrig) desp = "点击方式运行";
            else desp = "到达了指定页面触发运行";

            App.currentMainPage.ShowNotify("运行 " + tool.Name, desp, Symbol.Comment);
            string errorable = ErrableUsingNotify(tool.Code);
            try
            {
                await RunJs(webView, errorable);
            }
            catch (Exception ee)
            {
                await Utility.ShowMessageDialog("运行出错", "代码有可能存在语法错误，运行不成功.");
                await Utility.ShowDetailedError("错误详细信息", ee);
            }
        }

        public static async Task CheckRunNavigateTools(WebView webView, string url)
        {
            if (!CheckJingyanDomain(webView))
            {
                App.currentMainPage.ShowNotify("自定义功能失效提示", strDomainNotSupported);
                return;
            }
            foreach (var tool in StorageManager.dIYToolsSettings.DIYTools)
            {
                if (tool.IsClickTrig == true) continue;
                if (tool.IsActivate == false) continue;
                if(url.ToLower().Trim() == tool.TargetUrl.ToLower().Trim())
                {
                    await RunDIYToolCode(webView, tool);
                }
            }
        }
    }
}
