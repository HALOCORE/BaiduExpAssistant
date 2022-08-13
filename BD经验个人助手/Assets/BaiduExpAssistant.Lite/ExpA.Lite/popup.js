let btn = document.getElementById("initializeButton");
let scriptDict = {
    "checkbox-basic-check": 'tools/Checker.js',
    "checkbox-pic-insert": 'tools/PicInsert.js',
    "checkbox-big-pic": 'tools/BigPic.js'
}

btn.addEventListener("click", function (element) {
    let scriptsToExecute = ['tools/AllPoly.js'];
    for(let key in scriptDict) {
        let checkBox = document.getElementById(key);
        if(checkBox.checked) scriptsToExecute.push(scriptDict[key]);
    }
    chrome.tabs.query(
        { active: true, currentWindow: true },
        function (tabs) {
            for(let script of scriptsToExecute) {
                if (script === null) {
                    console.log("Skip null script.");
                    continue;
                }
                console.log("EXECUTE:", script);
                chrome.tabs.executeScript(
                    tabs[0].id,
                    {file: script}
                );
            }
        }
    );
});