// 确保在 DOM 加载完成后执行脚本
document.addEventListener('DOMContentLoaded', () => {
    /** @type {HTMLCanvasElement} */
    const canvas = document.getElementById('canvas');
    const ctx = canvas.getContext('2d');

    // 动态设置画布大小匹配屏幕分辨率，并响应窗口尺寸变化
    const resizeCanvas = () => {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    }

    window.addEventListener('resize', resizeCanvas, false);

    // 首次调用，初始化画布尺寸
    resizeCanvas();

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