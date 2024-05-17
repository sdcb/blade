const lobbyId = parseInt(location.href.split('/').pop());

class State {
    players = Array<PlayerDto>();

    pickableBonus = Array<PickableBonusDto>();

    deadPlayers = Array<PlayerDto>();

    center = { x: 0, y: 0 };
    scale = 1;
    cursorPosition = { x: 0, y: 0 };

    onUpdated() {
        const userId = getUserId();
        this.me = this.players.find(p => p.userId === userId);
        if (!this.me) this.me = this.deadPlayers.find(p => p.userId === userId);

        if (this.me) {
            this.center = { x: this.me.position[0], y: this.me.position[1] };
        }
        this.scale = getScale(this.me.size, window.innerWidth, window.innerHeight);
    }

    me: PlayerDto = null;
}
const state = new State();

// 确保在 DOM 加载完成后执行脚本
document.addEventListener('DOMContentLoaded', async () => {
    const canvas = <HTMLCanvasElement>document.getElementById('canvas');
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

function render(ctx: CanvasRenderingContext2D, canvas: HTMLCanvasElement) {
    // clear
    ctx.beginPath();
    ctx.rect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = 'cornflowerblue';
    ctx.fill();

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
    ctx.fillText(`积分榜（${state.players.length} 人）`, 10, y);
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

function drawUnits(ctx: CanvasRenderingContext2D) {
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

    const me = state.me;
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
        for (const blade of player.blades) {
            let angle = blade.angle * Math.PI / 180;
            const sin = Math.sin(angle);
            const cos = Math.cos(angle);

            ctx.beginPath();
            ctx.moveTo(player.position[0] + sin * player.size, player.position[1] + -cos * player.size);
            const len = player.blades.length + player.size;
            ctx.lineWidth = blade.damage;
            ctx.lineTo(player.position[0] + sin * len, player.position[1] + -cos * len);
            ctx.stroke();
        }
    }

    if (state.me?.health <= 0) {
        // draw dead player
        ctx.font = '100px Arial';
        ctx.fillText('你挂了', state.center.x, state.center.y);
    }
}

function drawGrid(ctx: CanvasRenderingContext2D) {
    const size = 2000;
    // clear
    ctx.beginPath();
    ctx.rect(-size / 2, -size / 2, size, size);
    ctx.fillStyle = 'gray';
    ctx.fill();

    // draw horizontal lines
    for (let y = -size / 2; y <= size / 2; y += 20) {
        ctx.lineDashOffset = 0;
        ctx.lineWidth = y % 100 === 0 ? 1 : 0.1;
        ctx.beginPath();
        ctx.moveTo(-size / 2, y);
        ctx.lineTo(size / 2, y);
        ctx.stroke();
    }

    // draw vertical lines
    for (let x = -size / 2; x <= size / 2; x += 20) {
        ctx.lineDashOffset = 0;
        ctx.lineWidth = x % 100 === 0 ? 1 : 0.1;
        ctx.beginPath();
        ctx.moveTo(x, -size / 2);
        ctx.lineTo(x, size / 2);
        ctx.stroke();
    }
}

function getScale(playerSize: number, resolutionX: number, resolutionY: number) {
    // 确定屏幕上角色占据的比例
    const targetScreenRatio = 1 / 20;

    const scaleX = (resolutionX * targetScreenRatio) / playerSize;
    const scaleY = (resolutionY * targetScreenRatio) / playerSize;

    // 选择两者中更小的缩放比例来确保角色既不会太宽也不会太高
    let scale = Math.min(scaleX, scaleY);
    scale = Math.max(1, scale);

    return scale;
}