async function ensureToken(): Promise<string> {
    const expires = localStorage.tokenExpire ? new Date(localStorage.tokenExpire) : new Date(0);
    if (expires > new Date()) {
        return localStorage.token;
    }

    if (!localStorage.uid) {
        localStorage.uid = randomPassword();
    }

    let userName = localStorage.userName;
    while (!userName) {
        userName = prompt("请输入用户名", randomName());
    }
    
    const resp = await fetch(`/token?userName=${encodeURIComponent(userName)}&uid=${encodeURIComponent(localStorage.uid)}`);
    if (resp.status !== 200 && await resp.text() === 'SSO is enabled') {
        location.href = '/sso-login';
    } else if (resp.status === 200) {
        const tokenObj = await resp.json();
        setToken(tokenObj);
    }
}

type TokenDto = {
    userId: number;
    userName: string;
    token: string;
    expires: string;
};

function setToken(token: TokenDto) {
    localStorage.token = token.token;
    localStorage.userId = token.userId.toString();
    localStorage.userName = token.userName;
    localStorage.tokenExpire = new Date(token.expires);
}

function randomPassword(length = 20) {
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_+=';
    let password = '';

    for (let i = 0; i < length; i++) {
        const randomIndex = Math.floor(Math.random() * characters.length);
        password += characters.charAt(randomIndex);
    }

    return password;
}

function randomName() {
    const names = [
        "牛三刀",
        "朱铁志",
        "苏山河",
        "萧风月",
        "何铁柱",
        "柳长空",
        "程百川",
        "温浩然",
        "袁天锋",
        "蓝玉强",
        "邵飞云",
        "庞雷音",
        "秦无痕",
        "姚星海",
        "傅剑波",
        "宋霜叶",
        "白浪客",
        "范翔羽",
        "武羲尘",
        "尹风怒",
        "聂远山",
        "丁铁君",
        "桂三峰",
        "夏侯飞絮",
        "华铭晨",
        "戚绝尘",
        "樊翼虎",
        "申屠霜",
        "江流石",
        "纪飞虹",
        "顾盘山",
        "荆紫电",
        "卓不群",
        "罗天朗",
        "宗遁空",
        "辛万里",
        "周银龙",
        "泰山客",
        "杜云鹏",
        "尤风魔",
        "蒋泽天",
        "费冷月",
        "阎一翔",
        "韩无声",
        "赵破虚",
        "赖明刚",
        "翟天赋",
        "龚云起",
        "薛山河",
        "范明刚",
        "司空影青",
    ];

    return names[Math.floor(Math.random() * names.length)];
}

function getUserId() {
    return parseInt(localStorage.userId);
}

function getUserName() {
    return localStorage.userName;
}
