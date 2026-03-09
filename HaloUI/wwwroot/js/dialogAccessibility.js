const focusableSelectors = [
    "a[href]",
    "area[href]",
    'input:not([disabled]):not([type="hidden"])',
    "select:not([disabled])",
    "textarea:not([disabled])",
    "button:not([disabled])",
    "iframe",
    "object",
    "embed",
    "[contenteditable]",
    '[tabindex]:not([tabindex="-1"])'
].join(",");

const traps = new Map();
let scrollLockCount = 0;
let previousOverflow = "";
let previousPaddingRight = "";
let previousScrollTop = 0;
let previousScrollLeft = 0;

function getFocusableElements(container) {
    if (!container) {
        return [];
    }

    return Array.from(container.querySelectorAll(focusableSelectors))
        .filter(element => element.offsetParent !== null && !element.hasAttribute("aria-hidden"));
}

export function trapFocus(container) {
    if (!container) {
        return null;
    }

    releaseFocusTrap(container);

    const previouslyFocused = document.activeElement instanceof HTMLElement ? document.activeElement : null;

    const handleKeyDown = event => {
        if (event.key !== "Tab") {
            return;
        }

        const focusable = getFocusableElements(container);

        if (focusable.length === 0) {
            event.preventDefault();
            container.focus();
            return;
        }

        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        const active = document.activeElement;

        if (!event.shiftKey && active === last) {
            event.preventDefault();
            first.focus();
        }
        else if (event.shiftKey && active === first) {
            event.preventDefault();
            last.focus();
        }
    };

    container.addEventListener("keydown", handleKeyDown);

    traps.set(container, {
        handleKeyDown,
        previouslyFocused
    });

    return previouslyFocused?.id ?? null;
}

export function releaseFocusTrap(container, fallbackElementId) {
    const trap = traps.get(container);

    if (!trap) {
        return;
    }

    container?.removeEventListener("keydown", trap.handleKeyDown);
    traps.delete(container);

    const { previouslyFocused } = trap;
    const target = resolveTarget(fallbackElementId, previouslyFocused);

    if (target && typeof target.focus === "function") {
        target.focus();
    }
}

export function lockBodyScroll() {
    const body = document.body;
    const root = document.documentElement;

    if (!body || !root) {
        return;
    }

    if (scrollLockCount === 0) {
        previousOverflow = body.style.overflow || "";
        previousPaddingRight = body.style.paddingRight || "";
        previousScrollTop = window.scrollY || root.scrollTop || 0;
        previousScrollLeft = window.scrollX || root.scrollLeft || 0;

        const scrollBarWidth = window.innerWidth - root.clientWidth;
        body.style.overflow = "hidden";

        if (scrollBarWidth > 0) {
            body.style.paddingRight = `${scrollBarWidth}px`;
        }
    }

    scrollLockCount += 1;
}

export function unlockBodyScroll() {
    const body = document.body;
    const root = document.documentElement;

    if (!body || !root || scrollLockCount === 0) {
        return;
    }

    scrollLockCount -= 1;

    if (scrollLockCount === 0) {
        body.style.overflow = previousOverflow;
        body.style.paddingRight = previousPaddingRight;

        window.scrollTo(previousScrollLeft, previousScrollTop);
    }
}

export function focusElementById(id) {
    if (!id) {
        return false;
    }

    const element = document.getElementById(id);

    if (!element || typeof element.focus !== "function") {
        return false;
    }

    element.focus();

    return document.activeElement === element;
}

function resolveTarget(fallbackElementId, previouslyFocused) {
    if (previouslyFocused && typeof previouslyFocused.focus === "function") {
        return previouslyFocused;
    }

    if (!fallbackElementId) {
        return null;
    }

    const element = document.getElementById(fallbackElementId);
    return element instanceof HTMLElement ? element : null;
}
