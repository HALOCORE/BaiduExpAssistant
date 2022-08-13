let babelCore = require("@babel/core");
let presetReact = require("@babel/preset-react");
let fs = require('fs');

let files = ['AllComps', 'BriefPic'];
for(let fName of files) {
    let inFileName = fName + ".jsx";
    let outFileName = fName + ".js";
    console.log("# input " + inFileName);
    let code = fs.readFileSync(inFileName, {"encoding": "utf8"});
    let result = babelCore.transform(code, {
        presets: [presetReact],
    });
    console.log("# output " + outFileName);
    fs.writeFileSync(outFileName, result.code, {"encoding": "utf8"});
}
