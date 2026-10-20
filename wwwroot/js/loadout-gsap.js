window.loadoutEngine = {
    context: null,
    media: null,
    recommendationObserver: null,
    refreshTimer: null,
    root: null,

    init: function () {
        this.cleanup();

        const root = document.querySelector("[data-loadout-page]");
        if (!root || !window.gsap || !window.ScrollTrigger) {
            return;
        }

        this.root = root;
        gsap.registerPlugin(ScrollTrigger);
        root.classList.add("is-motion-ready");

        const engine = this;

        this.context = gsap.context(function () {
            engine.media = gsap.matchMedia();

            engine.media.add("(prefers-reduced-motion: no-preference)", function () {
                engine.initHero();
                engine.initSectionReveals();
                engine.initRecommendationFeedback();
            });

            engine.media.add("(min-width: 901px) and (prefers-reduced-motion: no-preference)", function () {
                engine.initDesktopStoryMotion();
            });

            engine.media.add("(prefers-reduced-motion: reduce)", function () {
                gsap.set(root.querySelectorAll("[data-hero-word], [data-hero-title], [data-hero-copy] > *, [data-hero-ui], [data-reveal-title], [data-reveal-copy], [data-reveal-media]"), {
                    clearProps: "all"
                });
            });
        }, root);

        this.refreshTimer = window.setTimeout(function () {
            ScrollTrigger.refresh();
        }, 180);
    },

    initHero: function () {
        const word = this.root.querySelector("[data-hero-word]");
        const title = this.root.querySelector("[data-hero-title]");
        const copy = this.root.querySelectorAll("[data-hero-copy] > *");
        const product = this.root.querySelector("[data-hero-ui]");

        const timeline = gsap.timeline({
            defaults: { ease: "expo.out" }
        });

        if (word) {
            timeline.from(word, {
                yPercent: 112,
                duration: 1.05,
                immediateRender: false,
                clearProps: "transform"
            }, 0);
        }

        if (title) {
            timeline.from(title, {
                yPercent: 110,
                duration: 0.95,
                immediateRender: false,
                clearProps: "transform"
            }, 0.09);
        }

        if (copy.length) {
            timeline.from(copy, {
                y: 20,
                opacity: 0,
                duration: 0.72,
                stagger: 0.08,
                immediateRender: false,
                clearProps: "transform,opacity"
            }, 0.28);
        }

        if (product) {
            timeline.from(product, {
                y: 28,
                opacity: 0,
                scale: 0.985,
                duration: 0.82,
                ease: "expo.out",
                immediateRender: false,
                clearProps: "transform,opacity"
            }, 0.08);

            gsap.to(product, {
                yPercent: -5,
                ease: "none",
                scrollTrigger: {
                    trigger: "#hero-tactical",
                    start: "top top",
                    end: "bottom top",
                    scrub: true
                }
            });
        }
    },

    initSectionReveals: function () {
        const sections = this.root.querySelectorAll("[data-reveal-section]");

        sections.forEach(function (section) {
            const title = section.querySelector("[data-reveal-title]");
            const copy = section.querySelector("[data-reveal-copy]");
            const media = section.querySelector("[data-reveal-media]");

            const timeline = gsap.timeline({ paused: true });

            if (title) {
                timeline.from(title, {
                    y: 28,
                    opacity: 0,
                    duration: 0.82,
                    ease: "expo.out",
                    immediateRender: false,
                    clearProps: "transform,opacity"
                }, 0);
            }

            if (copy) {
                timeline.from(copy, {
                    y: 18,
                    opacity: 0,
                    duration: 0.72,
                    ease: "power3.out",
                    immediateRender: false,
                    clearProps: "transform,opacity"
                }, 0.08);
            }

            if (media) {
                timeline.from(media, {
                    y: 24,
                    opacity: 0,
                    duration: 0.88,
                    ease: "expo.out",
                    immediateRender: false,
                    clearProps: "transform,opacity"
                }, 0.16);
            }

            ScrollTrigger.create({
                trigger: section,
                start: "top 84%",
                once: true,
                onEnter: function () {
                    timeline.play();
                }
            });
        });

        const mechanicStatements = this.root.querySelectorAll(".mechanics__statement span");
        if (mechanicStatements.length) {
            gsap.from(mechanicStatements, {
                xPercent: function (index) { return index === 1 ? 8 : -8; },
                opacity: 0.42,
                ease: "none",
                stagger: 0.08,
                scrollTrigger: {
                    trigger: ".mechanics__statement",
                    start: "top 88%",
                    end: "bottom 55%",
                    scrub: true
                }
            });
        }

        const ruleRows = this.root.querySelectorAll(".rules__list details");
        if (ruleRows.length) {
            gsap.from(ruleRows, {
                x: 24,
                opacity: 0,
                duration: 0.72,
                stagger: 0.07,
                ease: "power3.out",
                immediateRender: false,
                scrollTrigger: {
                    trigger: ".rules__list",
                    start: "top 82%",
                    once: true
                }
            });
        }
    },

    initDesktopStoryMotion: function () {
        const steps = this.root.querySelectorAll("[data-story-step]");

        steps.forEach(function (step, index) {
            const visual = step.querySelector("[data-reveal-media]");
            if (!visual) return;

            gsap.fromTo(visual,
                {
                    yPercent: index % 2 === 0 ? 4 : -4,
                    scale: 0.97
                },
                {
                    yPercent: index % 2 === 0 ? -4 : 4,
                    scale: 1,
                    ease: "none",
                    scrollTrigger: {
                        trigger: step,
                        start: "top bottom",
                        end: "bottom top",
                        scrub: true
                    }
                }
            );
        });
    },

    initRecommendationFeedback: function () {
        const recommendation = this.root.querySelector("[data-recommendation]");
        if (!recommendation || !window.MutationObserver) return;

        const animate = function () {
            const parts = recommendation.querySelectorAll("h3, strong, p, .roulette__result-foot");
            gsap.fromTo(parts,
                { y: 14, opacity: 0.45 },
                {
                    y: 0,
                    opacity: 1,
                    duration: 0.42,
                    stagger: 0.045,
                    ease: "power3.out",
                    overwrite: "auto",
                    clearProps: "transform,opacity"
                }
            );
        };

        this.recommendationObserver = new MutationObserver(animate);
        this.recommendationObserver.observe(recommendation, {
            subtree: true,
            childList: true,
            characterData: true
        });
    },

    cleanup: function () {
        if (this.refreshTimer) {
            window.clearTimeout(this.refreshTimer);
            this.refreshTimer = null;
        }

        if (this.recommendationObserver) {
            this.recommendationObserver.disconnect();
            this.recommendationObserver = null;
        }

        if (this.media) {
            this.media.revert();
            this.media = null;
        }

        if (this.context) {
            this.context.revert();
            this.context = null;
        }

        if (this.root) {
            this.root.classList.remove("is-motion-ready");
            this.root = null;
        }
    }
};
