/// <reference path="defs.d.ts" />

const lobbyId = parseInt(location.href.split('/').pop());

class State {
    /**
     * @type {PlayerDto[]}
     */
    players = [];

    /**
     * @type {PickableBonusDto[]}
     */
    pickableBonus = [];

    /**
     * @type {PlayerDto[]}
     */
    deadPlayers = [];

    center = { x: 0, y: 0 };
    scale = 1;
    cursorPosition = { x: 0, y: 0 };

    onUpdated() {
        const userId = getUserId();
        const user = this.players.find(p => p.userId === userId);
        if (!user) user = this.deadPlayers.find(p => p.userId === userId);

        if (user) {
            this.center = { x: user.position[0], y: user.position[1] };
        }
    }
}
const state = new State();

// 确保在 DOM 加载完成后执行脚本
document.addEventListener('DOMContentLoaded', async () => {
    /** @type {HTMLCanvasElement} */
    const canvas = document.getElementById('canvas');
    const ctx = canvas.getContext('2d');

    const resizeCanvas = () => {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        //console.log('resize', canvas.width, canvas.height);
    }

    window.addEventListener('resize', resizeCanvas, false);
    resizeCanvas();

    document.addEventListener('mousemove', e => {
        const rect = canvas.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        const offsetX = x - canvas.width / 2;
        const offsetY = y - canvas.height / 2;

        state.cursorPosition = { x: state.center.x + offsetX / state.scale, y: state.center.y + offsetY / state.scale };
    });

    const connection = new signalR.HubConnectionBuilder().withUrl("/gameHub", {
        accessTokenFactory: ensureToken
    }).build();
    connection.on('update', (players, pickableBonus, deadPlayers) => {
        state.players = players;
        state.pickableBonus = pickableBonus;
        state.deadPlayers = deadPlayers;
        state.onUpdated();
    });
    await connection.start();

    document.addEventListener('click', e => {
        connection.invoke('SetDestination', lobbyId, state.cursorPosition.x, state.cursorPosition.y);
    });
    await connection.invoke('JoinLobby', lobbyId)

    render(ctx, canvas);
});

/**
 * 
 * @param {CanvasRenderingContext2D} ctx
 * @param {HTMLCanvasElement} canvas
 */
function render(ctx, canvas) {
    // clear
    ctx.beginPath();
    ctx.rect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = 'cornflowerblue';
    ctx.fill();

    state.scale = getScale(canvas.width, canvas.height);
    ctx.save();
    ctx.translate(-state.center.x * state.scale, -state.center.y * state.scale);
    ctx.scale(state.scale, state.scale);
    ctx.translate(canvas.width / 2 / state.scale, canvas.height / 2 / state.scale);
    drawGrid(ctx);
    drawUnits(ctx);
    ctx.restore();

    ctx.save();
    ctx.fillStyle = 'black';
    ctx.textAlign = 'left';
    ctx.textBaseline = 'top';
    ctx.font = '40px Arial';
    ctx.fillStyle = 'white';
    let y = 10;
    ctx.fillText('积分榜', 10, y);
    y += 40;
    ctx.font = '20px Arial';
    for (const p of state.players.concat().sort((a, b) => b.score - a.score)) {
        const isYou = p.userId === getUserId();
        ctx.fillStyle = isYou ? 'yellow' : 'white';
        ctx.fillText(`${p.userName}: ${p.score}`, 10, y);
        y += 20;
    }
    ctx.restore();
    

    requestAnimationFrame(() => render(ctx, canvas));
}

/**
 * 
 * @param {CanvasRenderingContext2D} ctx
 */
function drawUnits(ctx) {
    const userId = getUserId();

    for (const bonus of state.pickableBonus) {
        ctx.beginPath();
        ctx.arc(bonus.position[0], bonus.position[1], 25, 0, Math.PI * 2);
        ctx.fillStyle = 'green';
        ctx.fill();
        ctx.strokeStyle = 'white';
        ctx.stroke();

        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = 'black';
        ctx.fillText(bonus.name, bonus.position[0], bonus.position[1]);
    }

    const me = state.players.find(p => p.userId === userId);
    if (me && me.destination[0] !== me.position[0] && me.destination[1] !== me.position[1]) {
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = 'blue';
        ctx.fillText('X', me.destination[0], me.destination[1]);

        ctx.beginPath();
        ctx.moveTo(me.position[0], me.position[1]);
        ctx.lineTo(me.destination[0], me.destination[1]);
        ctx.strokeStyle = 'blue';
        ctx.setLineDash([5, 5]);
        ctx.stroke();
    }

    for (const player of state.players) {
        ctx.beginPath();
        ctx.arc(player.position[0], player.position[1], player.size, 0, Math.PI * 2);
        ctx.fillStyle = player.userId === userId ? 'dodgerblue' : 'crimson';
        ctx.fill();

        for (let hp = 0; hp < player.health; ++hp) {
            ctx.beginPath();
            ctx.arc(player.position[0], player.position[1], player.size + hp, 0, Math.PI * 2);
            ctx.strokeStyle = 'white';
            ctx.setLineDash([1, 0]);
            ctx.stroke();
        }

        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = 'black';
        ctx.fillText(player.userName, player.position[0], player.position[1]);

        // player blades
        ctx.strokeStyle = 'red';
        for (const degree of player.blades.angles) {
            let angle = degree * Math.PI / 180;
            const sin = Math.sin(angle);
            const cos = Math.cos(angle);

            ctx.beginPath();
            ctx.moveTo(player.position[0] + sin * player.size, player.position[1] + -cos * player.size);
            const len = player.blades.length + player.size;
            ctx.lineWidth = player.blades.damage;
            ctx.lineTo(player.position[0] + sin * len, player.position[1] + -cos * len);
            ctx.stroke();
        }
    }

    const deadMe = state.deadPlayers.find(p => p.userId === userId);
    if (deadMe) {
        // draw dead player
        ctx.font = '100px Arial';
        ctx.fillText('你挂了', state.center.x, state.center.y);
    }
}

/**
 * 
 * @param {CanvasRenderingContext2D} ctx
 */
function drawGrid(ctx) {
    // clear
    ctx.beginPath();
    ctx.rect(-500, -500, 1000, 1000);
    ctx.fillStyle = 'gray';
    ctx.fill();

    // draw horizontal lines
    for (let y = -500; y <= 500; y += 20) {
        ctx.lineDashOffset = 0;
        ctx.lineWidth = y % 100 === 0 ? 1 : 0.1;
        ctx.beginPath();
        ctx.moveTo(-500, y);
        ctx.lineTo(500, y);
        ctx.stroke();
    }

    // draw vertical lines
    for (let x = -500; x <= 500; x += 20) {
        ctx.lineDashOffset = 0;
        ctx.lineWidth = x % 100 === 0 ? 1 : 0.1;
        ctx.beginPath();
        ctx.moveTo(x, -500);
        ctx.lineTo(x, 500);
        ctx.stroke();
    }
}

function getScale(resolutionX, resolutionY) {
    // 角色大小
    const characterWidth = 50;
    const characterHeight = 50;

    // 确定屏幕上角色占据的比例
    const targetScreenWidthRatio = 1 / 10;
    const targetScreenHeightRatio = 1 / 10;

    let scaleX = (resolutionX * targetScreenWidthRatio) / characterWidth;
    let scaleY = (resolutionY * targetScreenHeightRatio) / characterHeight;

    // 选择两者中更小的缩放比例来确保角色既不会太宽也不会太高
    let scale = Math.min(scaleX, scaleY);

    // 限制最小缩放比例为1，避免放大地图，因为地图只有1000x1000
    scale = Math.max(1, scale);

    return scale;
}