/**
    @returns {Promise<string>} token
*/
async function ensureToken() {
    let token = localStorage.token;
    const tokenValidUntil = localStorage.tokenValidUntil;
    if (token && tokenValidUntil && new Date(tokenValidUntil) > new Date()) {
        return token;
    }

    let userName = localStorage.userName;
    if (!userName) {
        userName = prompt("请输入用户名");
        localStorage.userName = userName;
    }
    
    const resp = await fetch('/token?userName=' + encodeURIComponent(userName));
    const tokenObj = await resp.json();

    localStorage.token = tokenObj.token;
    localStorage.userId = tokenObj.userId;
    localStorage.tokenValidUntil = new Date(new Date().getTime() + 1000 * 60 * 60);
    return localStorage.token;
}

function getUserId() {
    return parseInt(localStorage.userId);
}

function getUserName() {
    return localStorage.userName;
}
