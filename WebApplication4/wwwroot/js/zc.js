
function kong() {
    var name = document.getElementById("username").value;
    var pwd = document.getElementById("password").value;
    var pwd1 = document.getElementById("password1").value;

    if (name === "" || pwd === "" || pwd1 === "") {
        alert("用户名/密码不能为空！");
        return false;
    }
    if (name.indexOf(' ') !== -1) {
        alert("用户名不能包含空格！");
        return false;
    }
    if (pwd !== pwd1) {
        alert("两次输入的密码不一致！");
        return false;
    }
    return true;
}
