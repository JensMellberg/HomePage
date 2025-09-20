const DPR = Math.max(1, Math.min(window.devicePixelRatio || 1, 2));

    /** Utilities **/
    const clamp = (v, a=0, b=1) => Math.max(a, Math.min(b, v));
    const lerp = (a,b,t) => a + (b-a)*t;
    const easeOutCubic = t => 1 - Math.pow(1 - t, 3);
    const easeOutBack = (t, s = 1.70158) => 1 + (s + 1) * Math.pow(t - 1, 3) + s * Math.pow(t - 1, 2);
    const now = () => performance.now();

    /** Confetti particle **/
    class StreakParticle {
      constructor(x, y, palette) {
        const angle = Math.random() * Math.PI * 2;
        const speed = 1.5 + Math.random() * 3.5;
        this.vx = Math.cos(angle) * speed;
        this.vy = Math.sin(angle) * speed - (0.5 + Math.random());
        this.x = x; this.y = y;
        this.size = 2 + Math.random() * 4;
        this.life = 0; this.maxLife = 60 + Math.random() * 30; // frames
        this.rot = Math.random() * Math.PI;
        this.rotSpeed = (Math.random() - 0.5) * 0.2;
        this.color = palette[Math.floor(Math.random() * palette.length)];
      }
      step(gravity = 0.05) {
        this.vy += gravity;
        this.x += this.vx;
        this.y += this.vy;
        this.rot += this.rotSpeed;
        this.life++;
      }
      get dead() { return this.life >= this.maxLife; }
    }

    /** Popup controller **/
    class StreakPopup {
      constructor(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.active = false;
        this.particles = [];
        this.lastTime = now();
        this.items = []; // queue of animations

        const resize = () => {
          const { clientWidth:w, clientHeight:h } = this.canvas;
          this.canvas.width = Math.floor(w * DPR);
          this.canvas.height = Math.floor(h * DPR);
        };
        const ro = new ResizeObserver(resize);
        ro.observe(canvas);
        resize();

        this.loop = this.loop.bind(this);
        requestAnimationFrame(this.loop);
      }

      show(count, clientX, clientY, opts={}) {
        const prefersReduced = getComputedStyle(document.documentElement).getPropertyValue('--reduced-motion').trim() === '1';
        const defaults = {
          duration: prefersReduced ? 1200 : 2200,
          hold: 350,
          text: `Streak ${count} \u{1F525}`,
          fontFamily: "system-ui, -apple-system, Segoe UI, Roboto, Inter, Helvetica, Arial, sans-serif",
          // colors adapt to theme
          light: {
            fg: '#111827',
            glow: 'rgba(255, 90, 31, 0.35)',
            gradient: ['#ff9a3d', '#ff4d4d'],
            confetti: ['#ff7a59','#ffd166','#06d6a0','#4cc9f0','#f72585']
          },
          dark: {
            fg: '#f3f4f6',
            glow: 'rgba(255, 130, 64, 0.35)',
            gradient: ['#ffd166', '#ff6b6b'],
            confetti: ['#ffd166','#7bdff2','#b2f7ef','#cdb4db','#ffadad']
          },
          scale: 1.0
        };
        const cfg = Object.assign({}, defaults, opts);
        cfg.theme = cfg.light

        const item = {
          start: now(),
          count,
          cfg,
          particles: [],
          done: false
        };

        // seed particles around center
        const center = { x: clientX, y: clientY }
        this.center = () => center
        const palette = cfg.theme.confetti;
        const particleCount = prefersReduced ? 30 : 90;
        for (let i = 0; i < particleCount; i++) {
          item.particles.push(new StreakParticle(center.x, center.y, palette));
        }

        this.items.push(item);
        this.active = true;
      }

      loop() {
        const t = now();
        const dt = t - this.lastTime;
        this.lastTime = t;

        const ctx = this.ctx;
        const { width:W, height:H } = this.canvas;

        // clear frame
        ctx.clearRect(0, 0, W, H);

        // draw each active item
        this.items = this.items.filter(item => !item.done);
        for (const item of this.items) {
          const { cfg } = item;
          const elapsed = t - item.start;
          const pEnter = clamp(elapsed / 500);
          const pMain = clamp((elapsed - 150) / cfg.duration);
          const pExit = clamp((elapsed - (cfg.duration - cfg.hold)) / cfg.hold);

          const c = this.center();

          // animate particles
          for (const pt of item.particles) {
            pt.step(0.05);
          }
          item.particles = item.particles.filter(p => !p.dead);

          // draw particles
          ctx.save();
          ctx.scale(DPR, DPR); // draw particles in CSS pixels for softer look
          ctx.globalCompositeOperation = 'lighter';
          for (const pt of item.particles) {
            const lifeT = pt.life / pt.maxLife;
            const alpha = 1 - lifeT;
            ctx.globalAlpha = alpha;
            ctx.translate(0.5, 0.5); // subpixel smoothness
            ctx.beginPath();
            ctx.arc(pt.x / DPR, pt.y / DPR, pt.size, 0, Math.PI * 2);
            ctx.fillStyle = pt.color;
            ctx.fill();
          }
          ctx.restore();

          // text pop + glow
          const scale = lerp(0.6, 1.0 * cfg.scale, easeOutBack(pEnter));
          const exit = 1 - pExit; // ease out opacity
          const opacity = clamp(easeOutCubic(pMain) * exit);

          ctx.save();
          ctx.translate(c.x, c.y);
          ctx.scale(scale, scale);
          ctx.globalAlpha = opacity;

          const text = cfg.text;
          ctx.font = `${Math.round(64 * DPR)}px ${cfg.fontFamily}`;
          ctx.textAlign = 'center';
          ctx.textBaseline = 'middle';

          // glow
          ctx.shadowColor = 'black';
            ctx.shadowBlur = 6 * DPR;
            ctx.shadowOffsetX = 3 * DPR;
            ctx.shadowOffsetY = 3 * DPR;

          // gradient fill
          const grad = ctx.createLinearGradient(-200 * DPR, 0, 200 * DPR, 0);
          const [g0, g1] = cfg.theme.gradient;
          grad.addColorStop(0, g0);
          grad.addColorStop(1, g1);
          ctx.fillStyle = grad;
          ctx.fillText(text, 0, 0);

          // crisp outline for readability
          ctx.shadowBlur = 0;
          ctx.lineWidth = 2 * DPR;
          ctx.strokeStyle = 'rgba(0,0,0,0.15)';
          ctx.strokeText(text, 0, 0);

          // foreground tint
          ctx.fillStyle = cfg.theme.fg;
          ctx.globalAlpha = opacity * 0.08;
          ctx.fillText(text, 0, 0);

          ctx.restore();

          // finish
          if (pMain >= 1 && item.particles.length === 0) {
            item.done = true;
          }
        }

        // schedule next frame
        requestAnimationFrame(this.loop);
      }
    }

    // bootstrap
    //const theCanvas = createCanvas()
    //const popup = new StreakPopup(theCanvas);

    // public API
    //window.showStreak = function(count, clientX, clientY, options) {
      //  popup.show(count, clientX, clientY, options);
    //};