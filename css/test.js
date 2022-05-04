var s = '{"name":"Lập trình mạng","local":"Nguyễn Quang Huy"}';

var obj = JSON.parse(s);
document.getElementById("name").innerHTML = obj.name;
document.getElementById("location").innerHTML = obj.local;