const canvas = createCanvas()
const ctx = canvas.getContext("2d");

function createCanvas() {
    const existing = document.getElementById('effect-canvas')
    if (existing) {
        return existing;
    }

    const newCanvas = document.createElement("canvas");
    newCanvas.id = 'effect-canvas'
    document.body.appendChild(newCanvas)
    
    newCanvas.width = window.innerWidth;
    newCanvas.height = window.innerHeight;

    // Make sure canvas is always on top
    newCanvas.style.position = "fixed";
    newCanvas.style.top = 0;
    newCanvas.style.left = 0;
    newCanvas.style.pointerEvents = "none"; // so clicks pass through
    newCanvas.style.zIndex = 9999;
    newCanvas.style.background = "transparent";
    return newCanvas;
}

class Particle {
    constructor(x, y, color) {
        this.x = x;
        this.y = y;
        this.color = color;
        this.radius = Math.random() * 2 + 1;
        this.speed = Math.random() * 5 + 2;
        this.angle = Math.random() * Math.PI * 2;
        this.vx = Math.cos(this.angle) * this.speed;
        this.vy = Math.sin(this.angle) * this.speed;
        this.alpha = 1;
        this.gravity = 0.05;
    }
    update() {
        this.x += this.vx;
        this.y += this.vy;
        this.vy += this.gravity;
        this.alpha -= 0.015;
    }
    draw() {
        ctx.save();
        ctx.globalAlpha = this.alpha;
        ctx.fillStyle = this.color;
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();
    }
}

let particles = [];

function createFirework(x, y) {
    const colors = ["#ff0040", "#ff8000", "#ffff00", "#00ff00", "#00ffff", "#0040ff", "#ff00ff"];
    const color = colors[Math.floor(Math.random() * colors.length)];
    for (let i = 0; i < 100; i++) {
        particles.push(new Particle(x, y, color));
    }
}

function animate() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    particles.forEach((p, i) => {
        p.update();
        p.draw();
        if (p.alpha <= 0) particles.splice(i, 1);
    });

    requestAnimationFrame(animate);
}

window.addEventListener("resize", () => {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
});

animate();