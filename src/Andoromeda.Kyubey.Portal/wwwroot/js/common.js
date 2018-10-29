
//判断是PC还是移动端
function isPc() {
    //移动端PC端判断
    return /Android|webOS|iPhone|iPod|BlackBerry/i.test(navigator.userAgent) ? false : true;
} 

function setLang(lang) {
    $.get("/Lang/" + lang, function () {
        window.location.reload();
    });
}