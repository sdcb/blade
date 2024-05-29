const lobbyId = parseInt(location.href.split('/').pop());
const mapSize = 2000;

class State {
    players = Array<Player>();

    pickableBonus = Array<Bonus>();

    deadPlayers = Array<Player>();

    center = { x: 0, y: 0 };
    scale = 1;
    cursorPosition = { x: 0, y: 0 };

    onUpdated() {
        const userId = getUserId();
        this.me = this.players.find(p => p.userId === userId);
        if (!this.me) this.me = this.deadPlayers.find(p => p.userId === userId);

        if (this.me) {
            this.center = { x: this.me.position[0], y: this.me.position[1] };
            this.scale = getScale(this.me.getSize(), window.innerWidth, window.innerHeight);
        }
    }

    onPush(dto: PushStateDto) {
        this.players = dto.p.map(x => new Player(x));
        this.pickableBonus = dto.b.map(x => new Bonus(x));
        this.deadPlayers = dto.d.map(x => new Player(x));
        this.onUpdated();
    }

    constructor() {
        this.onPush(initState);
    }

    me: Player = null;
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

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub", {
            accessTokenFactory: ensureToken
        })
        .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
        .withAutomaticReconnect()
        .build();
    connection.on('update', (resp: PushStateDto) => state.onPush(resp));
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

    drawLeaderBoard(ctx);
    drawMiniMap(ctx, canvas);

    requestAnimationFrame(() => render(ctx, canvas));
}

function isSmallScreenDevice(canvas: HTMLCanvasElement) {
    return canvas.width < 800;
}

function drawMiniMap(ctx: CanvasRenderingContext2D, canvas: HTMLCanvasElement) {
    const scale = isSmallScreenDevice(canvas) ? canvas.width / 2.5 / mapSize : 0.15;
    const size = mapSize * scale;
    const margin = 5;
    // 画到鼠标左下角
    ctx.save();
    ctx.beginPath();
    ctx.rect(margin, canvas.height - size - margin, size, size);
    ctx.strokeStyle = 'yellow';
    ctx.stroke();
    ctx.restore();

    ctx.save();
    ctx.translate(size / 2 + margin, canvas.height - size / 2 - margin);
    ctx.scale(scale, scale);
    drawUnits(ctx, /* isMiniMap: */ true);
    ctx.restore();
}

function drawLeaderBoard(ctx: CanvasRenderingContext2D) {
    ctx.save();
    ctx.fillStyle = 'black';
    ctx.textAlign = 'left';
    ctx.textBaseline = 'top';
    ctx.font = '30px Monospace';
    ctx.fillStyle = 'white';
    let y = 10;
    ctx.fillText(`积分榜(${state.players.length + state.deadPlayers.length}人)`, 10, y);
    y += 30;
    ctx.font = '15px Monospace';
    for (const p of state.players.concat().sort((a, b) => b.score - a.score)) {
        drawPlayer(p, /* isDead */ false);
    }
    for (const p of state.deadPlayers) {
        drawPlayer(p, /* isDead */ true);
    }
    ctx.restore();
    return y;

    function drawPlayer(p: Player, isDead: boolean) {
        const isYou = p.userId === getUserId();
        ctx.fillStyle = isDead ? 'darkgray' : isYou ? 'yellow' : 'white';
        ctx.fillText(`${p.userName}: ${p.score}`, 10, y);
        y += 15;
    }
}

function drawUnits(ctx: CanvasRenderingContext2D, isMiniMap: boolean = false) {
    // 渲染顺序（从下到上）：尸体、奖励、玩家
    for (const player of state.deadPlayers) {
        drawPlayer(ctx, player, /* isDead: */ true, isMiniMap);
    }

    for (const bonus of state.pickableBonus) {
        const isUseable = bonus.isUseable(state.me);

        ctx.beginPath();
        ctx.arc(bonus.position[0], bonus.position[1], 20, 0, Math.PI * 2);
        ctx.fillStyle = bonusFillColor(isUseable);
        ctx.fill();
        ctx.strokeStyle = bonusStrokeColor(isUseable);
        ctx.stroke();

        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = bonusTextColor(isUseable);
        ctx.fillText(bonus.name, bonus.position[0], bonus.position[1]);

        function bonusFillColor(isUseable: boolean) {
            return isUseable ? 'green' : 'rgba(0, 128, 0, 0.5)';
        }

        function bonusStrokeColor(isUseable: boolean) {
            return isUseable ? 'white' : 'rgba(255, 255, 255, 0.5)';
        }

        function bonusTextColor(isUseable: boolean) {
            return isUseable ? 'black' : 'rgba(0, 0, 0, 0.5)';
        }
    }

    const me = state.me;
    if (me && me.destination[0] !== me.position[0] && me.destination[1] !== me.position[1] && !isMiniMap) {
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = 'blue';
        ctx.fillText('X', me.destination[0], me.destination[1]);

        // 不在小地图上画路径
        ctx.beginPath();
        ctx.moveTo(me.position[0], me.position[1]);
        ctx.lineTo(me.destination[0], me.destination[1]);
        ctx.strokeStyle = 'blue';
        ctx.setLineDash([5, 5]);
        ctx.stroke();
    }

    for (const player of state.players) {
        drawPlayer(ctx, player, /* isDead: */ false, isMiniMap);
    }

    if (state.me?.health <= 0 && !isMiniMap) {
        // draw dead player
        ctx.font = '100px Arial';
        ctx.fillText('你挂了', state.center.x, state.center.y);
    }
}

function drawPlayer(ctx: CanvasRenderingContext2D, player: Player, isDead: boolean, isMiniMap: boolean) {
    const currentUserId = getUserId();
    const miniMapRed = 'rgba(255, 0, 0, 0.6)';
    const red = 'red';
    const miniMapWhite = 'rgba(255, 255, 255, 0.6)';
    const white = 'white';

    ctx.beginPath();
    ctx.arc(player.position[0], player.position[1], player.getSize(), 0, Math.PI * 2);
    if (isDead) {
        ctx.fillStyle = 'gray';
    } else if (player.userId === currentUserId) {
        if (isMiniMap) {
            // 小地图显示更显眼的亮蓝色
            ctx.fillStyle = 'yellow';
        } else {
            ctx.fillStyle = 'dodgerblue';
        }
    } else {
        if (isMiniMap) {
            // 小地图显示更显眼的红色，但要有点透明度
            ctx.fillStyle = miniMapRed;
        } else {
            ctx.fillStyle = 'crimson';
        }
    }
    ctx.fill();
    ctx.strokeStyle = isMiniMap ? miniMapWhite : white;
    ctx.lineWidth = player.getSize() / Player.minSize;
    ctx.setLineDash([]);
    ctx.stroke();

    if (!isMiniMap) {
        // 小地图上不显示名字
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = 'black';
        ctx.fillText(player.userName, player.position[0], player.position[1]);
    }

    if (!isDead) {
        // player blades
        for (const blade of player.blades) {
            let angle = blade.angle * Math.PI / 180;
            const sin = Math.sin(angle);
            const cos = Math.cos(angle);

            ctx.beginPath();
            ctx.moveTo(player.position[0] + sin * player.getSize(), player.position[1] + -cos * player.getSize());
            const len = blade.length + player.getSize();
            ctx.lineWidth = blade.damage;
            ctx.lineTo(player.position[0] + sin * len, player.position[1] + -cos * len);
            ctx.strokeStyle = isMiniMap ? miniMapRed : red;
            
            if (player.isGoldBlade(blade)) {
                ctx.strokeStyle = 'gold';
            }
            ctx.stroke();
        }
    }
}

function drawGrid(ctx: CanvasRenderingContext2D) {
    // clear
    ctx.beginPath();
    ctx.rect(-mapSize / 2, -mapSize / 2, mapSize, mapSize);
    ctx.fillStyle = 'gray';
    ctx.fill();

    // draw horizontal lines
    for (let y = -mapSize / 2; y <= mapSize / 2; y += 20) {
        ctx.lineDashOffset = 0;
        ctx.lineWidth = y % 100 === 0 ? 1 : 0.1;
        ctx.beginPath();
        ctx.moveTo(-mapSize / 2, y);
        ctx.lineTo(mapSize / 2, y);
        ctx.stroke();
    }

    // draw vertical lines
    for (let x = -mapSize / 2; x <= mapSize / 2; x += 20) {
        ctx.lineDashOffset = 0;
        ctx.lineWidth = x % 100 === 0 ? 1 : 0.1;
        ctx.beginPath();
        ctx.moveTo(x, -mapSize / 2);
        ctx.lineTo(x, mapSize / 2);
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