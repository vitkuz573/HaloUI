const outsideClickListeners = new Map();
const viewportObservers = new Map();

export function registerOutsideClick(rootId, dotNetRef) {
    unregisterOutsideClick(rootId);

    const root = document.getElementById(rootId);
    if (!root) {
        return;
    }

    const handler = (event) => {
        if (!document.contains(root)) {
            unregisterOutsideClick(rootId);
            return;
        }

        if (!root.contains(event.target)) {
            invokeDotNetMethodAsync(dotNetRef, "HandleOutsideClick", undefined, () => unregisterOutsideClick(rootId));
        }
    };

    document.addEventListener('mousedown', handler, true);
    document.addEventListener('touchstart', handler, true);
    outsideClickListeners.set(rootId, handler);
}

export function unregisterOutsideClick(rootId) {
    const handler = outsideClickListeners.get(rootId);
    if (!handler) {
        return;
    }

    document.removeEventListener('mousedown', handler, true);
    document.removeEventListener('touchstart', handler, true);
    outsideClickListeners.delete(rootId);
}

export function registerViewportObserver(rootId, maxWidth = 640, dotNetRef) {
    unregisterViewportObserver(rootId);

    if (!dotNetRef) {
        return;
    }

    const mediaQuery = window.matchMedia(`(max-width: ${maxWidth}px), (hover: none) and (pointer: coarse)`);
    const observer = {
        mediaQuery,
        frameId: 0,
        onMediaQueryChanged: null,
        onResize: null,
        notify: null
    };

    observer.notify = () => {
        if (observer.frameId !== 0) {
            return;
        }

        observer.frameId = window.requestAnimationFrame(() => {
            observer.frameId = 0;

            if (!document.getElementById(rootId)) {
                unregisterViewportObserver(rootId);
                return;
            }

            invokeDotNetMethodAsync(dotNetRef, "HandleViewportModeChanged", shouldUseNativeSelect(maxWidth), () => unregisterViewportObserver(rootId));
        });
    };

    observer.onMediaQueryChanged = () => observer.notify();
    observer.onResize = () => observer.notify();

    addMediaQueryListener(mediaQuery, observer.onMediaQueryChanged);
    window.addEventListener('resize', observer.onResize, { passive: true });
    viewportObservers.set(rootId, observer);

    observer.notify();
}

export function unregisterViewportObserver(rootId) {
    const observer = viewportObservers.get(rootId);
    if (!observer) {
        return;
    }

    if (observer.frameId !== 0) {
        window.cancelAnimationFrame(observer.frameId);
    }

    removeMediaQueryListener(observer.mediaQuery, observer.onMediaQueryChanged);
    window.removeEventListener('resize', observer.onResize);
    viewportObservers.delete(rootId);
}

export function measureTrigger(triggerElement) {
    if (!triggerElement) {
        return null;
    }

    const rect = triggerElement.getBoundingClientRect();
    const viewportWidth = window.innerWidth || document.documentElement.clientWidth;
    const viewportHeight = window.innerHeight || document.documentElement.clientHeight;
    const container = findFixedContainingBlock(triggerElement);
    const isInDialog = !!triggerElement.closest('.halo-dialog__modal, .halo-dialog__drawer');

    if (container) {
        const containerRect = container.getBoundingClientRect();

        return {
            width: rect.width,
            height: rect.height,
            top: rect.top - containerRect.top,
            bottom: rect.bottom - containerRect.top,
            left: rect.left - containerRect.left,
            right: rect.right - containerRect.left,
            viewportWidth: containerRect.width,
            viewportHeight: containerRect.height,
            isInDialog
        };
    }

    return {
        width: rect.width,
        height: rect.height,
        top: rect.top,
        bottom: rect.bottom,
        left: rect.left,
        right: rect.right,
        viewportWidth,
        viewportHeight,
        isInDialog
    };
}

export function shouldUseNativeSelect(maxWidth = 640) {
    const viewportWidth = window.innerWidth || document.documentElement.clientWidth || 0;
    const hasCoarsePointer = window.matchMedia
        ? window.matchMedia('(hover: none) and (pointer: coarse)').matches
        : false;

    return hasCoarsePointer || viewportWidth <= maxWidth;
}

function findFixedContainingBlock(element) {
    let current = element.parentElement;

    while (current && current !== document.body && current !== document.documentElement) {
        const style = getComputedStyle(current);
        const contain = style.contain || "";
        const willChange = style.willChange || "";
        const backdropFilter = style.backdropFilter || style.webkitBackdropFilter || "";

        if (style.transform !== "none"
            || style.perspective !== "none"
            || style.filter !== "none"
            || backdropFilter !== "none"
            || contain.includes("paint")
            || contain.includes("layout")
            || willChange.includes("transform")) {
            return current;
        }

        current = current.parentElement;
    }

    return null;
}

function addMediaQueryListener(mediaQuery, listener) {
    if (!mediaQuery || !listener) {
        return;
    }

    if (typeof mediaQuery.addEventListener === 'function') {
        mediaQuery.addEventListener('change', listener);
        return;
    }

    if (typeof mediaQuery.addListener === 'function') {
        mediaQuery.addListener(listener);
    }
}

function removeMediaQueryListener(mediaQuery, listener) {
    if (!mediaQuery || !listener) {
        return;
    }

    if (typeof mediaQuery.removeEventListener === 'function') {
        mediaQuery.removeEventListener('change', listener);
        return;
    }

    if (typeof mediaQuery.removeListener === 'function') {
        mediaQuery.removeListener(listener);
    }
}

function invokeDotNetMethodAsync(dotNetRef, methodName, argument, onDisposed) {
    if (!dotNetRef || typeof dotNetRef.invokeMethodAsync !== "function") {
        return;
    }

    const invocation = argument === undefined
        ? dotNetRef.invokeMethodAsync(methodName)
        : dotNetRef.invokeMethodAsync(methodName, argument);

    Promise.resolve(invocation).catch((error) => {
        if (isDisposedDotNetReferenceError(error)) {
            if (typeof onDisposed === "function") {
                onDisposed();
            }

            return;
        }

        console.error(`uiSelect.js: invokeMethodAsync('${methodName}') failed.`, error);
    });
}

function isDisposedDotNetReferenceError(error) {
    const message = typeof error === "string"
        ? error
        : (error?.message || "");

    if (!message) {
        return false;
    }

    return message.includes("There is no tracked object with id")
        || message.includes("DotNetObjectReference instance was already disposed");
}
