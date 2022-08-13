console.log("========== BriefPic.js ==========");
/* ----------------------------------------------------  */
let RNState = {};

let RNStyles = {
  mainCanvas: {
    width: 600,
    height: 240,
    border: "wheat solid 1px"
  },
  bigButton: {
    fontSize: 14,
    padding: "5px 15px",
    marginTop: "6px"
  },
  bigSettingButton: {
    fontSize: 14,
    padding: "5px 15px",
    marginTop: "6px",
    marginRight: "12px"
  },
  uploadEnabled: {
    fontSize: 14,
    padding: "5px 15px",
    marginTop: "6px",
    color: "green",
    fontWeight: 900
  },
  uploadDisabled: {
    fontSize: 14,
    padding: "5px 15px",
    marginTop: "6px"
  },
  blockLabel: {
    display: "block"
  },
  noteA: {
    width: 424,
    marginTop: 5,
    display: "block",
    fontSize: 12,
    padding: 3
  },
  noteB: {
    width: 424,
    marginTop: 5,
    display: "block",
    fontSize: 12,
    padding: 3
  },
  urlsTextArea: {
    display: "block",
    width: 515,
    height: 130,
    marginTop: 5,
    fontSize: 12,
    padding: 11,
    overflowX: "scroll",
    whiteSpace: "nowrap"
  },
  saveConfigButton: {
    fontSize: 14,
    padding: "5px 10px",
    marginTop: 6
  },
  hiddenImg: {
    display: "none"
  },
  currentUrlStatus: {
    fontSize: 10
  },
  configGroup: {
    padding: 10,
    margin: 5,
    marginBottom: 15,
    border: "2px solid gray"
  },
  keyword: {
    color: "darkcyan",
    fontWeight: 900,
    padding: "0px 5px"
  },
  candidatesBar: {
    margin: 0,
    padding: 2,
    fontSize: 15
  },
  candidateItem: {
    display: "inline-block",
    borderRadius: 4,
    background: "black",
    color: "white",
    padding: "2px 4px",
    margin: "2px 4px",
    fontSize: 12
  },
  linkButton: {
    padding: "1px 3px",
    marginLeft: "5px",
    fontSize: 14,
    color: "darkblue"
  }
};

function closeBriefPic() {
  document.getElementById("brief-img-editor").style.display = "none";
}
function launchUrl(url) {
  try {
    window.external.notify("LAUNCH: " + url);
  }
  catch (e) { }
}

function saveCanvas(method) {
  var canvas = document.getElementById("brief-canvas");
  canvas.style.position = "fixed";
  canvas.style.left = 0;
  canvas.style.top = 0;
  canvas.style.zIndex = 200000;
  canvas.style.transform = "scale(" + document.body.clientWidth / 600 / 1.5 + ")";
  canvas.style.transformOrigin = "0% 0%";

  if (method == 0) {
    setTimeout(saveCanvasPhrase20, 200);
  }
  else if (method === 1) {
    setTimeout(saveCanvasPhrase21, 200);
  }
  else if (method === 2) {
    setTimeout(saveCanvasPhrase22, 200);
  }
  else {
    console.error("# saveCanvas unknown method: " + method);
  }

}
function saveCanvasPhrase20() {
  try { window.external.notify("SAVE-PIC: " + 600 / 240 + " | 600 | 240"); } catch (e) { };
  setTimeout(saveCanvasPhrase3, 200);
}
function saveCanvasPhrase21() {
  try { window.external.notify("SAVEAS-PIC: " + 600 / 240 + " | 600 | 240"); } catch (e) { };
  setTimeout(saveCanvasPhrase3, 200);
}
function saveCanvasPhrase22() {
  try {
    console.log("# saveCanvasPhrase22 called.");
    window.external.getUploadPic((dataUrl) => {
      console.log("# external.getUploadPic response with dataUrl.");
      let fakeFile = window.dataURLtoFile(dataUrl, "brief.png");
      window._onUploadFileChange({ type: "change", target: { "files": [fakeFile] } });
    });
  } catch (e) { };
  setTimeout(saveCanvasPhrase3, 200);
}
function saveCanvasPhrase3() {
  var canvas = document.getElementById("brief-canvas");
  canvas.style.position = "static";
  canvas.style.zIndex = "auto";
  canvas.style.transform = "initial";
  canvas.style.transformOrigin = "initial";
  try { window.external.notify("NOTIFY: 操作完成 | 界面恢复原来状态 | OK"); } catch (e) { };
}

function TryLoadSettings() {
  try {
    if (!CommonSetData) CommonSetData = {};
    if (!CommonSetData["bigpics"]) CommonSetData["bigpics"] = {};
    let settings = CommonSetData["bigpics"];
    if (settings['brief-background-first-checked'])
      RNState['useBigPic'][1](settings['brief-background-first-checked']);
    if (settings['brief-background-text'])
      RNState['backgroundUrls'][1](settings['brief-background-text']);
    if (settings['brief-icon-text'])
      RNState['iconUrls'][1](settings['brief-icon-text']);
  }
  catch (e) {
    try { window.external.notify("ERROR: 简介图设置恢复不成功."); } catch (e) { }
  }
}

function isSettingsDirty() {
  let settings = CommonSetData["bigpics"];
  if (settings['brief-background-first-checked'] !== RNState['useBigPic'][0]) return true;
  if (settings['brief-background-text'] !== RNState['backgroundUrls'][0]) return true;
  if (settings['brief-icon-text'] !== RNState['iconUrls'][0]) return true;
  return false;
}

function saveConfig() {
  var isChecked = RNState['useBigPic'][0];
  var backgroundText = RNState['backgroundUrls'][0];
  var iconText = RNState['iconUrls'][0];
  let settings = CommonSetData["bigpics"];
  settings['brief-background-first-checked'] = isChecked;
  settings['brief-background-text'] = backgroundText;
  settings['brief-icon-text'] = iconText;
  settings['group-id'] = "bigpics";
  var dataStr = JSON.stringify(settings);
  try {
    console.log("# saveConfig: " + dataStr);
    window.external.notify("SET-DATA: " + dataStr);
  } catch (e) {
    console.error(" saveConfig failed: " + dataStr);
  }
}

function compareCandidateList(list1, list2) {
  if (list1.length !== list2.length) return false;
  for (let i = 0; i < list1.length; i++) {
    if (list1[i][0] !== list2[i][0]) return false;
    if (list1[i][1] !== list2[i][1]) return false;
  }
  return true;
}

function RN_BriefPicSettingZone() {
  console.log("# RN_BriefPicSettingZone render.");
  RNState['iconNote'] = React.useState(["可选图标为从上往下找到的前n个关键词匹配。", "darkcyan"]);
  let iconNote = RNState['iconNote'][0];

  RNState['backgroundNote'] = React.useState(["可选背景图为从上往下找到的前n个关键词匹配。", "darkcyan"])
  let backgroundNote = RNState['backgroundNote'][0];

  RNState['useBigPic'] = React.useState(false);
  let [useBigPic, setUseBigPic] = RNState['useBigPic'];

  RNState['backgroundUrls'] = React.useState(`手机 = https://ss0.bdstatic.com/70cFuHSh_Q1YnxGkpoWK1HF6hhy/it/u=1797044092,2303584770&fm=26&gp=0.jpg
win = https://ss0.bdstatic.com/70cFvHSh_Q1YnxGkpoWK1HF6hhy/it/u=3511096425,263250315&fm=26&gp=0.jpg
word = https://cn.bing.com/th?id=OIP.2l4mI6F0_MiyyGcPB-aoYAHaEK&pid=Api&rs=1
chrome = http://img.mp.sohu.com/upload/20170526/ee6776da5af84cec81f68e3fce9274aa_th.png

= https://ss2.bdstatic.com/70cFvnSh_Q1YnxGkpoWK1HF6hhy/it/u=342786909,3873323405&fm=11&gp=0.jpg
= http://bpic.588ku.com/back_pic/00/01/71/385608b3bfcbbd0.jpg`);
  let [backgroundUrls, setBackgroundUrls] = RNState['backgroundUrls'];

  RNState['iconUrls'] = React.useState(`word = https://www.easyicon.net/api/resizeApi.php?id=1212930&size=128
谷歌浏览器 = https://www.easyicon.net/api/resizeApi.php?id=1212918&size=128
android = https://www.easyicon.net/api/resizeApi.php?id=1229034&size=128
win = https://www.easyicon.net/api/resizeApi.php?id=1229085&size=128

= https://www.easyicon.net/api/resizeApi.php?id=1236657&size=128`);
  let [iconUrls, setIconUrls] = RNState['iconUrls'];

  return (
    <div>
      <div>
        <p><span style={{ paddingRight: 20 }}>每一行输入格式:</span> <b>关键词=图片链接</b></p>
        <p><span style={{ paddingRight: 20 }}>匹配任意标题输入格式:</span> <b>=图片链接</b></p>
        <p><span style={{ paddingRight: 20 }}>关键词可重复, 多个选项都会出现. </span></p>
        <p>更多详细说明:
          <button style={RNStyles.linkButton} onClick={() => launchUrl('https://jingyan.baidu.com/article/08b6a591e624ca55a80922ec.html')}>基本设置说明</button>
          <button style={RNStyles.linkButton} onClick={() => launchUrl('https://jingyan.baidu.com/article/066074d68e776182c21cb0ec.html')}>如何使用本地图片</button>
          <button style={RNStyles.linkButton} onClick={() => launchUrl('https://jingyan.baidu.com/article/22a299b50705efdf19376aed.html')}>如何禁止图片缓存</button>
        </p>
      </div>
      <div style={RNStyles.configGroup}>
        <div>
          <span>背景图：</span>
          <button style={RNStyles.linkButton} onClick={() => launchUrl('https://image.baidu.com/')}>百度图片</button>
          <button style={RNStyles.linkButton} onClick={() => launchUrl('https://cn.bing.com/images/')}>Bing图片</button>
          <button style={RNStyles.linkButton} onClick={() => launchUrl('https://pic.sogou.com/')}>搜狗图片</button>
        </div>
        <div>
          <input checked={useBigPic} onChange={() => setUseBigPic(!useBigPic)}
            type="checkbox" style={{ marginLeft: 15 }} /> 使用大图片框中的图（不使用这里的关键词匹配）
        </div>
        <div style={RNStyles.noteB}>
          <span id="background-urls-note" style={{ color: backgroundNote[1] }}>{backgroundNote[0]}</span>
        </div>
        <textarea id="background-urls-textarea"
          style={RNStyles.urlsTextArea} value={backgroundUrls} onChange={(e) => setBackgroundUrls(e.target.value)}>
        </textarea>
      </div>
      <div style={RNStyles.configGroup}>
        <div>
          <span>图标：</span>
          <button style={RNStyles.linkButton} onClick={() => launchUrl('https://www.easyicon.net/')}>图标网站</button>
        </div>
        <div style={RNStyles.noteA}>
          <span style={{ color: iconNote[1] }}>{iconNote[0]}</span>
        </div>
        <textarea id="icon-urls-textarea"
          style={RNStyles.urlsTextArea} value={iconUrls} onChange={(e) => setIconUrls(e.target.value)}>
        </textarea>
      </div>
      <p>注意事项：图片图标在浏览器查找，浏览器中右键图片，可以复制图片地址。<br />
        修改配置后，“收起设置”按钮左侧可以点击保存。<br />
        <b>生成简介图后，可以一键上传，也可以另存为图片。</b>
      </p>
      <img className="brief-img" style={RNStyles.hiddenImg} id="brief-backgroundImage"></img>
      <img className="brief-img" style={RNStyles.hiddenImg} id="brief-iconImage"></img>
    </div>
  );
}

function RN_BriefCurrentUrlsZone() {
  console.log("# RN_BriefCurrentUrlsZone render.");
  RNState['backgroundCandidates'] = React.useState([]);
  let backgroundCandidates = RNState['backgroundCandidates'][0];

  RNState['iconCandidates'] = React.useState([]);
  let iconCandidates = RNState['iconCandidates'][0];

  RNState['backgroundKey'] = React.useState("<无匹配>");
  let backgroundKey = RNState['backgroundKey'][0];

  RNState['iconKey'] = React.useState("<无匹配>");
  let iconKey = RNState['iconKey'][0];

  RNState['backgroundSrc'] = React.useState("https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=4061924083,1937802988&fm=26&gp=0.jpg");
  let backgroundSrc = RNState['backgroundSrc'][0];

  RNState['iconSrc'] = React.useState("https://www.easyicon.net/api/resizeApi.php?id=1236657&size=128");
  let iconSrc = RNState['iconSrc'][0];

  RNState['isBackgroundUserSelected'] = React.useState(false);
  RNState['isIconUserSelected'] = React.useState(false);
  RNState['iconStatus'] = React.useState("loading");
  RNState['backgroundStatus'] = React.useState("loading");

  function setBackgroundCandidate(candidate) {
    RNState['backgroundKey'][1](candidate[0]);
    RNState['backgroundSrc'][1](candidate[1]);
    RNState['isBackgroundUserSelected'][1](true);
  }
  function setIconCandidate(candidate) {
    RNState['iconKey'][1](candidate[0]);
    RNState['iconSrc'][1](candidate[1]);
    RNState['isIconUserSelected'][1](true);
  }
  function statusToColor(status) {
    if (status === "failed") return "red";
    if (status === "succeed") return "green";
    if (status === "loading") return "orange";
    return "black";
  }
  return (
    <div>
      {
        backgroundCandidates.length > 0 ?
          <div style={RNStyles.candidatesBar}>
            <span style={{ paddingRight: 5 }}>可选背景图: </span>
            {

              backgroundCandidates.map((candidate) => (
                <div style={RNStyles.candidateItem} key={candidate[1]}>
                  <label>
                    <input type="radio" name="background-candidate" style={{ paddingRight: 2 }}
                      onClick={() => setBackgroundCandidate(candidate)} readOnly checked={backgroundKey === candidate[0] && backgroundSrc === candidate[1]} />
                    {candidate[0] === "" ? "<无关键词>" : candidate[0]}
                  </label>
                </div>
              ))
            }
            {backgroundCandidates.length === 1 ? <span style={{ color: "gray" }}>增加更多选项请“展开设置”</span> : <></>}
          </div> : <></>
      }
      {
        iconCandidates.length > 0 ?
          <div style={RNStyles.candidatesBar}>
            <span style={{ paddingRight: 5 }}>可选图标: </span>
            {
              iconCandidates.map((candidate) => (
                <div style={RNStyles.candidateItem} key={candidate[1]}>
                  <label>
                    <input type="radio" name="icon-candidate" style={{ paddingRight: 2 }}
                      onClick={() => setIconCandidate(candidate)} readOnly checked={iconKey === candidate[0] && iconSrc === candidate[1]} />
                    {candidate[0] === "" ? "<无关键词>" : candidate[0]}
                  </label>
                </div>
              ))
            }
            {iconCandidates.length === 1 ? <span style={{ color: "gray" }}>增加更多选项请“展开设置”</span> : <></>}
          </div> : <></>
      }
      <p style={RNStyles.currentUrlStatus}>选中背景图
        <span style={{ fontWeight: 900, color: statusToColor(RNState['backgroundStatus'][0]) }}>({RNState['backgroundStatus'][0]})</span>:
        <span style={RNStyles.keyword}>{backgroundKey === "" ? "<无关键词>" : backgroundKey}</span> {backgroundSrc}
      </p>
      <p style={RNStyles.currentUrlStatus}>选中图标
        <span style={{ fontWeight: 900, color: statusToColor(RNState['iconStatus'][0]) }}>({RNState['iconStatus'][0]})</span>:
        <span style={RNStyles.keyword}>{iconKey === "" ? "<无关键词>" : iconKey}</span> {iconSrc}
      </p>
    </div>
  )
}

function RN_BriefControlZone(props) {
  console.log("# RN_BriefControlZone render.");
  let { settingsVisible, setSettingsVisible } = props;
  const [isAutoWrap, setIsAutoWrap] = RNState['isAutoWrap'] = React.useState(true);
  const [isUploaderHacked] = RNState['isUploaderHacked'] = React.useState(false);
  const [isConfigDirty] = RNState['isConfigDirty'] = React.useState(false);
  return (
    <div>
      <button onClick={saveConfig}
        style={RNStyles.saveConfigButton} disabled={!isConfigDirty}>
        {isConfigDirty ? '💾' : '✅'}
      </button>
      <button onClick={() => setSettingsVisible(!settingsVisible)}
        style={RNStyles.bigSettingButton}>
        {settingsVisible ? "↑ 收起设置 ↑" : "↓ 展开设置 ↓"} </button>
      <button onClick={() => saveCanvas(0)}
        style={RNStyles.bigButton}>
        保存简介图</button>
      <button onClick={() => saveCanvas(1)}
        style={RNStyles.bigButton}>
        简介图另存为</button>
      <button disabled={!isUploaderHacked}
        style={isUploaderHacked ? RNStyles.uploadEnabled : RNStyles.uploadDisabled}
        onClick={() => saveCanvas(2)}>
        {isUploaderHacked ? '>>> 一键上传 >>>' : '等待执行入口...'} </button>
      <input checked={isAutoWrap} onChange={() => setIsAutoWrap(!isAutoWrap)}
        type="checkbox" style={{ marginLeft: 15 }} />
        标题自动折行
    </div>
  );
}

function RN_BriefPicEditor() {
  console.log("# RN_BriefPicEditor render.");
  const [settingsVisible, setSettingsVisible] = RNState['settingsVisible'] = React.useState(false);
  return (
    <div>
      <RN_BriefCurrentUrlsZone />
      <canvas width="600" height="240" style={RNStyles.mainCanvas} id="brief-canvas"></canvas>
      <RN_BriefControlZone settingsVisible={settingsVisible} setSettingsVisible={setSettingsVisible} />
      <div style={{
        display: settingsVisible ? "block" : "none",
        padding: 15,
        bordeRadius: 10,
        background: "white",
        marginRight: 85
      }}>
        <RN_BriefPicSettingZone />
      </div>
    </div>
  );
}

function AddBriefImgEditor() {
  let existEditor = document.getElementById("brief-img-editor");
  if (existEditor) {
    console.log("# AddBriefImgEditor 简介图已经添加, 修改display.");
    existEditor.style.display = "block";
    return;
  }

  let title = document.getElementById("title");
  let titleInput = title.getElementsByClassName("title pr")[0].getElementsByTagName("input")[0];
  let oldCompInputVal = "<<init>>";

  let editorBox = document.createElement("div");
  editorBox.id = "brief-img-editor";
  editorBox.style.marginTop = "20px";
  editorBox.style.marginBottom = "20px";
  editorBox.style.paddingRight = "30px";
  title.appendChild(editorBox);
  console.log("# start ReactDOM.render");
  ReactDOM.render(<RN_BriefPicEditor />, editorBox);

  function backgroundShowNote(note, color) {
    if (!note) {
      note = "可选背景图为从上往下找到的前n个关键词匹配。";
      color = "darkcyan";
    }
    if (note !== RNState['backgroundNote'][0][0] || color !== RNState['backgroundNote'][0][1]) {
      RNState['backgroundNote'][1]([note, color]);
    }
  }

  function iconShowNote(note, color) {
    if (!note) {
      note = "可选图标为从上往下找到的前n个关键词匹配。";
      color = "darkcyan";
    }
    if (note !== RNState['iconNote'][0][0] || color !== RNState['iconNote'][0][1]) {
      RNState['iconNote'][1]([note, color]);
    }
  }

  setInterval(updateBriefImage, 800);

  let oldBackgroundSrc = "";
  let oldIconSrc = "";
  let briefImgEditorElement = document.getElementById("brief-img-editor");

  function updateBriefImage() {
    if (briefImgEditorElement.style.display == "none") return;
    let [isUploaderHacked, setIsUploaderHacked] = RNState['isUploaderHacked'];
    if (window._onUploadFileChange && !isUploaderHacked) {
      console.log("# updateBriefImage found isUploaderHacked.");
      setIsUploaderHacked(true);
    }

    //update brief icon
    let pairs = [];
    try {
      let urltxt = document.getElementById("icon-urls-textarea");
      let lines = urltxt.value.split("\n").map(x => x.trim()).filter(x => x != "");
      pairs = lines.map(x => x.split("=")).map(y => [y[0], y.slice(1).join("=")].map(z => z.trim()));
      for (let elem of pairs) {
        if (elem.length != 2) throw "每一行的格式是: 关键词=链接";;
        if ((!elem[1].startsWith("http://"))
          && (!elem[1].startsWith("https://"))
          && (!elem[1].startsWith("nocache-http://"))
          && (!elem[1].startsWith("nocache-https://"))
          && (!elem[1].startsWith("local://"))) throw elem[1];
      }
      iconShowNote();
    }
    catch (e) {
      iconShowNote("输入格式不正确. " + e.toString(), "red");
    }

    //update ICON candidates and key src.
    let iconCandidates = [];
    let isIconCandidateValid = false;
    for (let p of pairs) {
      if (titleInput.value.toLowerCase().indexOf(p[0].toLowerCase()) >= 0) {
        iconCandidates.push(p);
        if (RNState['iconKey'][0] === p[0]) isIconCandidateValid = true;
      }
    }
    if (!compareCandidateList(RNState['iconCandidates'][0], iconCandidates)) {
      RNState['iconCandidates'][1](iconCandidates);
      if ((!isIconCandidateValid || !RNState['isIconUserSelected'][0]) && iconCandidates.length > 0) {
        if (RNState['iconKey'][0] !== iconCandidates[0][0] || RNState['iconSrc'][0] !== iconCandidates[0][1]) {
          RNState['iconKey'][1](iconCandidates[0][0]);
          RNState['iconSrc'][1](iconCandidates[0][1]);
          RNState['iconStatus'][1]("loading");
          RNState['isIconUserSelected'][1](false);
        }
      }
    }

    //update brief background
    pairs = [];
    try {
      let urltxt = document.getElementById("background-urls-textarea");
      let lines = urltxt.value.split("\n").map(x => x.trim()).filter(x => x != "");
      pairs = lines.map(x => x.split("=")).map(y => [y[0], y.slice(1).join("=")].map(z => z.trim()));
      for (let elem of pairs) {
        if (elem.length != 2) throw "每一行的格式是: 关键词=链接";;
        if ((!elem[1].startsWith("http://"))
          && (!elem[1].startsWith("https://"))
          && (!elem[1].startsWith("nocache-http://"))
          && (!elem[1].startsWith("nocache-https://"))
          && (!elem[1].startsWith("local://"))) throw elem[1];
      }
      backgroundShowNote();
    }
    catch (e) {
      backgroundShowNote("输入格式不正确. " + e.toString(), "red");
    }

    //update BACKGROUND candidates and key src
    if (RNState['useBigPic'][0]) {
      let myimg = document.getElementById("my-bigimg");
      RNState['backgroundCandidates'][1]([]);
      RNState['backgroundKey'][1]("<大图片框>");
      if (myimg && myimg.src) {
        RNState['backgroundSrc'][1](myimg.src);
      } else {
        RNState['backgroundSrc'][1]("大图片框图片不存在");
      }
    } else {
      let backgroundCandidates = [];
      let isBackgroundValid = false;
      for (let p of pairs) {
        if (titleInput.value.toLowerCase().indexOf(p[0].toLowerCase()) >= 0) {
          backgroundCandidates.push(p);
          if (p[0] === RNState['backgroundKey'][0]) isBackgroundValid = true;
        }
      }
      if (!compareCandidateList(RNState['backgroundCandidates'][0], backgroundCandidates)) {
        RNState['backgroundCandidates'][1](backgroundCandidates);
        if ((!isBackgroundValid || !RNState['isBackgroundUserSelected'][0]) && backgroundCandidates.length > 0) {
          if (RNState['backgroundKey'][0] !== backgroundCandidates[0][0] || RNState['backgroundSrc'][0] !== backgroundCandidates[0][1]) {
            RNState['backgroundKey'][1](backgroundCandidates[0][0]);
            RNState['backgroundSrc'][1](backgroundCandidates[0][1]);
            RNState['backgroundStatus'][1]("loading");
            RNState['isBackgroundUserSelected'][1](false);
          }
        }
      }
    }

    //update is settings dirty
    RNState['isConfigDirty'][1](isSettingsDirty());

    //check canvas update.
    let isBksrc1Checked = RNState['useBigPic'][0];
    let wrapLineEnabled = RNState['isAutoWrap'][0];
    let backgroundSrc = RNState['backgroundSrc'][0];
    let iconSrc = RNState['iconSrc'][0];
    var newCompInputVal = titleInput.value + " | "
      + backgroundSrc + " | " + iconSrc + " | "
      + wrapLineEnabled + " | " + isBksrc1Checked;
    if (oldCompInputVal == newCompInputVal) return;

    console.log("# updateBriefImage 比较值变化: " + newCompInputVal);
    oldCompInputVal = newCompInputVal;

    if (iconSrc != oldIconSrc) { //there's no ?t in new/old src var.
      RNState['iconStatus'][1]("loading");
      window.external.getImage(iconSrc, (dataUrl) => {
        console.log("# dataUrl for Icon recieved.");
        let currentSrc = RNState['iconSrc'][0];
        if (currentSrc === iconSrc) {
          console.log("# dataUrl src for Icon matched.");
          RNState['iconStatus'][1]("succeed");
          document.getElementById("brief-iconImage").src = dataUrl;
          setTimeout(() => drawCanvas(titleInput), 200);
          setTimeout(() => drawCanvas(titleInput), 600);
        }
      }, (msg) => {
        console.error("# getImage icon failed: " + msg);
        let currentSrc = RNState['iconSrc'][0];
        if (currentSrc == iconSrc) {
          RNState['iconStatus'][1]("failed");
        }
      });
      oldIconSrc = iconSrc;
    }
    if (backgroundSrc != oldBackgroundSrc) {
      RNState['backgroundStatus'][1]("loading");
      window.external.getImage(backgroundSrc, (dataUrl) => {
        console.log("# dataUrl for Background recieved.");
        let currentSrc = RNState['backgroundSrc'][0];
        if (currentSrc === backgroundSrc) {
          console.log("# dataUrl src for Background matched.");
          RNState['backgroundStatus'][1]("succeed");
          document.getElementById("brief-backgroundImage").src = dataUrl;
          setTimeout(() => drawCanvas(titleInput), 200);
          setTimeout(() => drawCanvas(titleInput), 600);
        }
      }, (msg) => {
        console.error("# getImage background failed: " + msg);
        let currentSrc = RNState['backgroundSrc'][0];
        if (currentSrc == backgroundSrc) {
          RNState['backgroundStatus'][1]("failed");
        }
      });
      oldBackgroundSrc = backgroundSrc;
    }
    drawCanvas(titleInput);
  }
}

////////////////////////////////////////////////////////////////////////
//////////////////////////// Canvas Graphic 

function drawCanvas(titleInput) {
  //获得画布元素
  var briefCanvas1 = document.getElementById("brief-canvas");
  //获得2维绘图的上下文
  var ctx = briefCanvas1.getContext("2d");
  //清除
  ctx.clearRect(0, 0, 600, 240);
  //图片
  let apple = document.getElementById("brief-backgroundImage");//TODO
  //将图像绘制到画布的，图片的左上角
  try {
    ctx.drawImage(apple, 0, 0, apple.width - 30, apple.height - 30, 0, 0, 600, 240);
  } catch (e) {
    console.error("# drawCanvas background not succeed: " + e.message);
  }

  //画一个实心矩形
  fillRoundRect(ctx, 66, 66, 468, 108, 12, "rgba(255,255,255,0.9)"); //TODO

  apple = document.getElementById("brief-iconImage");
  try {
    //将图像绘制到画布的，图片的左上角
    ctx.drawImage(apple, 96, 79.2, 81.6, 81.6);
  } catch (e) {
    console.error("# drawCanvas icon not succeed: " + e.message);
  }
  // 设置颜色
  ctx.fillStyle = "black";
  // 设置水平对齐方式
  ctx.textAlign = "left";
  // 设置垂直对齐方式
  ctx.textBaseline = "middle";
  // 设置字体
  ctx.font = "32px bold 黑体";
  // 测量单行能否放下
  var words = [].concat.apply([],
    (titleInput.value.split(/([a-zA-Z0-9]+)/g)
      .map(x =>
        (x.trim() != "" && (!x.match(/[a-zA-Z0-9]+/g))) ? x.split("") : [x]
      )
    ));
  var firstLine = "";
  var secondLine = "";
  var oneLineOK = true;
  for (var word of words) {
    if (!oneLineOK) {
      secondLine += word;
    } else {
      firstLine += word;
      var metrics = ctx.measureText(firstLine);
      if (metrics.width > 297.6 + 30) {
        oneLineOK = false;
      }
    }
  }
  if (secondLine.trim() == "") oneLineOK = true;
  if (oneLineOK || (!RNState['isAutoWrap'][0])) {
    // 绘制文字（参数：要写的字，x坐标，y坐标）
    ctx.fillText(firstLine + secondLine, 201.6, 122.4, 297.6);
  } else {
    ctx.font = "28px bold 黑体";
    // 绘制文字（参数：要写的字，x坐标，y坐标）
    ctx.fillText(firstLine, 201.6, 104.4, 297.6);
    ctx.fillText(secondLine, 201.6, 141.6, 297.6);
  }
};

function fillRoundRect(cxt, x, y, width, height, radius, /*optional*/ fillColor) {
  //圆的直径必然要小于矩形的宽高
  if (2 * radius > width || 2 * radius > height) {
    return false;
  }
  cxt.save();
  cxt.translate(x, y);
  //绘制圆角矩形的各个边
  drawRoundRectPath(cxt, width, height, radius);
  cxt.fillStyle = fillColor || "#000"; //若是给定了值就用给定的值否则给予默认值
  cxt.fill();
  cxt.restore();
}

function drawRoundRectPath(cxt, width, height, radius) {
  cxt.beginPath(0);
  //从右下角顺时针绘制，弧度从0到1/2PI
  cxt.arc(width - radius, height - radius, radius, 0, Math.PI / 2);
  //矩形下边线
  cxt.lineTo(radius, height);
  //左下角圆弧，弧度从1/2PI到PI
  cxt.arc(radius, height - radius, radius, Math.PI / 2, Math.PI);
  //矩形左边线
  cxt.lineTo(0, radius);
  //左上角圆弧，弧度从PI到3/2PI
  cxt.arc(radius, radius, radius, Math.PI, Math.PI * 3 / 2);
  //上边线
  cxt.lineTo(width - radius, 0);
  //右上角圆弧
  cxt.arc(width - radius, radius, radius, Math.PI * 3 / 2, Math.PI * 2);
  //右边线
  cxt.lineTo(width, height - radius);
  cxt.closePath();
}
//////////////////////////// Canvas Graphic 
////////////////////////////////////////////////////////////////////////


(function () {
  try {
    // AddMyImg();
    AddBriefImgEditor();
    //window.external.notify("2ND-GOTO: https://www.easyicon.net/");
    setTimeout(TryLoadSettings, 100); //no reason yet.
    //window.external.notify("NOTIFY: 添加成功 | 简介图已载入 | OK");
    let isSaveConfigCanceled = false;
    document.getElementById("submit").addEventListener("mouseenter", () => {
      let isDirty = isSettingsDirty();
      if (isDirty === true && !isSaveConfigCanceled) {
        window.external.cofirmDialog("保存提醒: 是否保存简介图设置? ", "简介图设置本次已修改，但是还没有保存。", () => {
          console.log("# confirmDialog callback: prepare to saveConfig.");
          saveConfig();
        },
          () => {
            isSaveConfigCanceled = true;
          });
      }
    });
  } catch (e) {
    var emm = document.createElement("div");
    emm.innerText = e.message;
    try { window.external.notify("SHOW-DIALOG: 简介图初始化失败 | 可联系开发者1223989563@qq.com. 代码运行出错" + emm.innerText.replace("|", " I ")); } catch (e2) { };
  }
})();

console.log("^^^^^^^^^^ BriefPic.js ^^^^^^^^^^");