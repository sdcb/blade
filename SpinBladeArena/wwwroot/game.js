// 确保在 DOM 加载完成后执行脚本
document.addEventListener('DOMContentLoaded', async () => {
    /** @type {HTMLCanvasElement} */
    const canvas = document.getElementById('canvas');
    const ctx = canvas.getContext('2d');

    const resizeCanvas = () => {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    }

    window.addEventListener('resize', resizeCanvas, false);
    resizeCanvas();

    const connection = new signalR.HubConnectionBuilder().withUrl("/gameHub", {
        accessTokenFactory: ensureToken
    }).build();
    connection.on('update', (a, b, c) => console.log(a, b, c));
    await connection.start();
    await connection.invoke('JoinLobby', parseInt(location.href.split('/').pop()))

    render(ctx, canvas);
});

let x = 0;

/**
 * 渲染函数
 * @param {CanvasRenderingContext2D} ctx
 * @param {HTMLCanvasElement} canvas
  */
function render(ctx, canvas) {
    // clear
    ctx.beginPath();
    ctx.rect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = 'cornflowerblue';
    ctx.fill();

    ctx.beginPath();
    ctx.rect(x++, 100, 100, 100);
    ctx.fillStyle = 'red';
    ctx.fill();

    requestAnimationFrame(() => render(ctx, canvas));
}