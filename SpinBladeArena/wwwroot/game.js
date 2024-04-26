/// <reference path="defs.d.ts" />

const userId = getUserId();;

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
    drawPlayers(ctx);
    ctx.restore();

    requestAnimationFrame(() => render(ctx, canvas));
}

/**
 * 
 * @param {CanvasRenderingContext2D} ctx
 */
function drawPlayers(ctx) {
    for (const player of state.players) {
        ctx.beginPath();
        ctx.arc(player.position[0], player.position[1], player.size, 0, Math.PI * 2);
        ctx.fillStyle = player.userId === userId ? 'crimson' : 'dodgerblue';
        ctx.fill();
        ctx.strokeStyle = 'white';
        ctx.stroke();

        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = 'black';
        ctx.fillText(player.userName, player.position[0], player.position[1]);
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
    for (let y = -500; y <= 500; y += 10) {
        ctx.lineDashOffset = 0;
        ctx.lineWidth = y % 50 === 0 ? 1 : 0.1;
        ctx.beginPath();
        ctx.moveTo(-500, y);
        ctx.lineTo(500, y);
        ctx.stroke();
    }

    // draw vertical lines
    for (let x = -500; x <= 500; x += 10) {
        ctx.lineDashOffset = 0;
        ctx.lineWidth = x % 50 === 0 ? 1 : 0.1;
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