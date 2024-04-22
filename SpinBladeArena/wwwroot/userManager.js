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
    token = await resp.text();

    localStorage.token = token;
    localStorage.tokenValidUntil = new Date(new Date().getTime() + 1000 * 60 * 60);
    return token;
}
