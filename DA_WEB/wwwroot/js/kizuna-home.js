// Enhanced cursor functionality
const cursor = document.getElementById('cursor');
const ring = document.getElementById('cursorRing');
let mx = 0, my = 0, rx = 0, ry = 0;

// Enhanced mouse tracking with smoother movement
document.addEventListener('mousemove', e => {
    mx = e.clientX;
    my = e.clientY;
});

(function animateCursor() {
    cursor.style.left = mx + 'px';
    cursor.style.top = my + 'px';

    rx += (mx - rx) * 0.12;
    ry += (my - ry) * 0.12;
    ring.style.left = rx + 'px';
    ring.style.top = ry + 'px';

    requestAnimationFrame(animateCursor);
})();

// Enhanced cursor effects with different states
const hoverElements = document.querySelectorAll('a, button, .product-card, .featured-card, .btn-primary, .btn-ghost');

hoverElements.forEach(el => {
    el.addEventListener('mouseenter', () => {
        cursor.style.transform = 'translate(-50%,-50%) scale(2.5)';
        ring.style.transform = 'translate(-50%,-50%) scale(1.5)';
        ring.style.opacity = '0.8';
        cursor.style.background = getComputedStyle(el).color || 'var(--primary)';
    });

    el.addEventListener('mouseleave', () => {
        cursor.style.transform = 'translate(-50%,-50%) scale(1)';
        ring.style.transform = 'translate(-50%,-50%) scale(1)';
        ring.style.opacity = '0.5';
        cursor.style.background = 'var(--primary)';
    });
});

// Enhanced header scroll effect
const header = document.getElementById('mainHeader');

window.addEventListener('scroll', () => {
    const scrolled = window.scrollY > 60;
    header.classList.toggle('scrolled', scrolled);

    // Update logo color based on scroll
    if (header.classList.contains('scrolled')) {
        document.querySelector('.logo-text').style.color = 'var(--ink)';
    } else {
        document.querySelector('.logo-text').style.color = 'var(--white)';
    }
});

// Enhanced scroll reveal with stagger animation
const revealObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('visible');

            // Add stagger effect for collection items
            if (entry.target.classList.contains('product-card')) {
                const index = Array.from(entry.target.parentNode.children).indexOf(entry.target);
                setTimeout(() => {
                    entry.target.style.transitionDelay = `${index * 0.1}s`;
                }, 10);
            }
        }
    });
}, {
    threshold: 0.15,
    rootMargin: '0px 0px -50px 0px'
});

document.querySelectorAll('.reveal').forEach(el => revealObserver.observe(el));

// Enhanced marquee effect
const marqueeTrack = document.querySelector('.marquee-track');
if (marqueeTrack) {
    marqueeTrack.addEventListener('mouseenter', () => {
        marqueeTrack.style.animationPlayState = 'paused';
    });

    marqueeTrack.addEventListener('mouseleave', () => {
        marqueeTrack.style.animationPlayState = 'running';
    });
}

// Add scroll progress indicator
const scrollProgress = document.createElement('div');
scrollProgress.className = 'scroll-progress';
document.body.appendChild(scrollProgress);

window.addEventListener('scroll', () => {
    const scrollPercent = (window.scrollY / (document.documentElement.scrollHeight - window.innerHeight)) * 100;
    scrollProgress.style.transform = `scaleX(${scrollPercent / 100})`;
});

// Enhanced image lazy loading
const imageObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            const img = entry.target;
            img.src = img.dataset.src;
            img.classList.remove('lazy');
            imageObserver.unobserve(img);
        }
    });
}, {
    rootMargin: '50px 0px 50px 0px',
    threshold: 0.1
});

document.querySelectorAll('img[data-src]').forEach(img => imageObserver.observe(img));

// Enhanced performance optimization
let ticking = false;

function updateScrollEffects() {
    // Header fade effect
    const headerOpacity = Math.max(0, 1 - (window.scrollY / 200));
    header.style.opacity = headerOpacity;

    // Parallax effect for hero images
    const heroRight = document.querySelector('.hero-right-img');
    if (heroRight) {
        const scrollOffset = window.scrollY * 0.5;
        heroRight.style.transform = `translateY(${scrollOffset}px) scale(1.04)`;
    }

    ticking = false;
}

window.addEventListener('scroll', () => {
    if (!ticking) {
        requestAnimationFrame(updateScrollEffects);
        ticking = true;
    }
});

// Enhanced error handling
window.addEventListener('error', (e) => {
    console.warn('Error caught:', e.error);
});