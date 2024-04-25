/// <reference path="defs.d.ts" />

const state = {
    /**
     * @type {PlayerDto[]}
     */
    players: [], 
    /**
     * @type {PickableBonusDto[]}
     */
    pickableBonus: [], 
    /**
     * @type {PlayerDto[]}
     */
    deadPlayers: [],

    center: { x: 0, y: 0 }

    onUpdated() {

    }
}

// 确保在 DOM 加载完成后执行脚本
document.addEventListener('DOMContentLoaded', async () => {
    /** @type {HTMLCanvasElement} */
    const canvas = document.getElementById('canvas');
    const ctx = canvas.getContext('2d');

    const resizeCanvas = () => {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        console.log('resize', canvas.width, canvas.height);
    }

    window.addEventListener('resize', resizeCanvas, false);
    resizeCanvas();

    const connection = new signalR.HubConnectionBuilder().withUrl("/gameHub", {
        accessTokenFactory: ensureToken
    }).build();
    connection.on('update', (players, pickableBonus, deadPlayers) => {
        state.players = players;
        state.pickableBonus = pickableBonus;
        state.deadPlayers = deadPlayers;
    });
    await connection.start();
    await connection.invoke('JoinLobby', parseInt(location.href.split('/').pop()))

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

    drawGrid(ctx, canvas);

    requestAnimationFrame(() => render(ctx, canvas));
}

function drawGrid(ctx, canvas) {
    
}