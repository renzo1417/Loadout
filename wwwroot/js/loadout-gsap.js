// The landing owns every tween, observer, and listener created here.
// Its Blazor lifecycle calls init on entry and cleanup on navigation away.
window.loadoutEngine = {
    root: null,
    context: null,
    media: null,
    recommendationObserver: null,
    refreshFrame: null,
    handleLoad: null,
    handleKeyboard: null,
    handlePointer: null,
    keyboardInput: false,

    init() {
        this.cleanup();
        const root = document.querySelector("[data-loadout-page]");
        if (!root) return;
        if (!window.gsap || !window.ScrollTrigger) {
            root.dataset.motion = "unavailable";
            return; // The page is readable without animation or a JavaScript dependency.
        }
        this.root = root;
        gsap.registerPlugin(ScrollTrigger);
        root.classList.add("is-motion-ready");
        this.context = gsap.context(() => {
            this.media = gsap.matchMedia();
            this.media.add({
                motion: "(prefers-reduced-motion: no-preference)",
                desktop: "(min-width: 901px)",
                reduce: "(prefers-reduced-motion: reduce)"
            }, match => {
                root.dataset.motion = match.conditions.reduce ? "reduced" : "active";
                if (!match.conditions.motion) {
                    root.dataset.heroMotion = "static";
                    return;
                }
                this.initHero();
                this.initSections();
                this.initRecommendation();
                if (match.conditions.desktop) this.initStoryMotion();
                return () => this.stopRecommendation();
            });
        }, root);
        this.handleLoad = () => this.scheduleRefresh();
        root.addEventListener("load", this.handleLoad, true);
        document.fonts?.ready.then(() => {
            if (this.root === root) this.scheduleRefresh();
        });
        this.scheduleRefresh();
    },

    initHero() {
        const root = this.root;
        const hero = root.querySelector(".hero");
        if (hero.getBoundingClientRect().bottom <= 0) {
            root.dataset.heroMotion = "complete";
            return;
        }
        const timeline = gsap.timeline({
            defaults: { ease: "expo.out" },
            onStart: () => { root.dataset.heroMotion = "running"; },
            onComplete: () => { root.dataset.heroMotion = "complete"; }
        });
        timeline.fromTo(root.querySelector("[data-hero-word]"),
            { yPercent: 110 }, { yPercent: 0, duration: 1, clearProps: "transform" }, 0);
        timeline.fromTo(root.querySelector("[data-hero-title]"),
            { y: 24, opacity: 0 }, { y: 0, opacity: 1, duration: .7, clearProps: "transform,opacity" }, .08);
        timeline.fromTo(root.querySelectorAll("[data-hero-copy] > *, .hero__demo"),
            { y: 16, opacity: 0 }, { y: 0, opacity: 1, duration: .55, stagger: .06, clearProps: "transform,opacity" }, .18);
        timeline.fromTo(root.querySelector("[data-hero-ui]"),
            { y: 24, opacity: 0, scale: .985 }, { y: 0, opacity: 1, scale: 1, duration: .7, clearProps: "transform,opacity" }, .1);
    },

    reveal(targets, trigger) {
        const elements = Array.from(targets).filter(Boolean);
        if (!elements.length) return;
        gsap.fromTo(elements, { y: 20, opacity: 0 }, {
            y: 0, opacity: 1, duration: .65, stagger: .06, ease: "expo.out",
            immediateRender: false, clearProps: "opacity",
            scrollTrigger: { trigger, start: "top 88%", once: true }
        });
    },

    initSections() {
        this.root.querySelectorAll("[data-reveal-section]").forEach(section => {
            const intro = [section.querySelector("[data-reveal-title]"), section.querySelector("[data-reveal-copy]")];
            this.reveal(intro, section);
            // Story beats animate independently as they enter, below the sticky introduction.
            if (!section.classList.contains("story")) {
                const media = section.querySelector("[data-reveal-media]");
                if (media) this.reveal([media], media);
            }
        });
        this.root.querySelectorAll("[data-story-step]").forEach(step => {
            this.reveal([step.querySelector("[data-reveal-media]"), step.querySelector("[data-story-copy]")], step);
        });
    },

    initStoryMotion() {
        this.root.querySelectorAll("[data-story-step]").forEach((step, index) => {
            const visual = step.querySelector("[data-reveal-media]");
            if (!visual) return;
            gsap.fromTo(visual, { yPercent: index % 2 ? -2 : 2 }, {
                yPercent: index % 2 ? 2 : -2, ease: "none", immediateRender: false,
                scrollTrigger: { trigger: step, start: "top bottom", end: "bottom top", scrub: true }
            });
        });
        const statements = this.root.querySelectorAll(".mechanics__statement span");
        gsap.fromTo(statements, { xPercent: i => i === 1 ? 3 : -3, opacity: .6 }, {
            xPercent: 0, opacity: 1, ease: "none", immediateRender: false,
            scrollTrigger: { trigger: ".mechanics__statement", start: "top 90%", end: "bottom 65%", scrub: true }
        });
    },

    initRecommendation() {
        const panel = this.root.querySelector("[data-recommendation]");
        if (!panel) return;
        let previous = panel.querySelector("strong")?.textContent || "";
        this.handleKeyboard = () => { this.keyboardInput = true; };
        this.handlePointer = () => { this.keyboardInput = false; };
        this.root.addEventListener("keydown", this.handleKeyboard);
        this.root.addEventListener("pointerdown", this.handlePointer);
        this.recommendationObserver = new MutationObserver(() => {
            const current = panel.querySelector("strong")?.textContent || "";
            if (current === previous) return;
            previous = current;
            if (this.keyboardInput || !this.context) return;
            this.context.add(() => {
                gsap.fromTo(panel.querySelectorAll("h3, strong, p"), { y: 8, opacity: .6 }, {
                    y: 0, opacity: 1, duration: .18, ease: "power3.out", overwrite: "auto", clearProps: "transform,opacity"
                });
            });
        });
        this.recommendationObserver.observe(panel, { childList: true, subtree: true, characterData: true });
    },

    scheduleRefresh() {
        if (this.refreshFrame) cancelAnimationFrame(this.refreshFrame);
        this.refreshFrame = requestAnimationFrame(() => {
            this.refreshFrame = null;
            if (this.root) ScrollTrigger.refresh();
        });
    },

    stopRecommendation() {
        this.recommendationObserver?.disconnect();
        this.recommendationObserver = null;
        if (this.root && this.handleKeyboard) this.root.removeEventListener("keydown", this.handleKeyboard);
        if (this.root && this.handlePointer) this.root.removeEventListener("pointerdown", this.handlePointer);
        this.handleKeyboard = this.handlePointer = null;
        this.keyboardInput = false;
    },

    cleanup() {
        this.stopRecommendation();
        if (this.refreshFrame) cancelAnimationFrame(this.refreshFrame);
        if (this.root && this.handleLoad) this.root.removeEventListener("load", this.handleLoad, true);
        this.media?.revert();
        this.context?.revert();
        if (this.root) {
            this.root.classList.remove("is-motion-ready");
            delete this.root.dataset.motion;
            delete this.root.dataset.heroMotion;
        }
        this.root = this.context = this.media = this.handleLoad = this.refreshFrame = null;
    }
};
