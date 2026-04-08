const VIEWPORT_PADDING_PX = 8;
const MIN_DROPDOWN_HEIGHT_PX = 112;
let activeOutsideCloseRegistration = null;

function toFiniteNumber(value, fallback) {
    return Number.isFinite(value) ? value : fallback;
}

function createsFixedPositionContainingBlock(element) {
    const style = window.getComputedStyle(element);
    if (!style) {
        return false;
    }

    if (style.transform && style.transform !== 'none') {
        return true;
    }

    if (style.perspective && style.perspective !== 'none') {
        return true;
    }

    if (style.filter && style.filter !== 'none') {
        return true;
    }

    const backdropFilter = style.backdropFilter || style.webkitBackdropFilter;
    if (backdropFilter && backdropFilter !== 'none') {
        return true;
    }

    const contain = (style.contain || '').toLowerCase();
    if (
        contain.includes('paint') ||
        contain.includes('layout') ||
        contain.includes('strict') ||
        contain.includes('content')
    ) {
        return true;
    }

    const willChange = (style.willChange || '').toLowerCase();
    return (
        willChange.includes('transform') ||
        willChange.includes('perspective') ||
        willChange.includes('filter')
    );
}

function findFixedPositionContainingBlock(element) {
    let current = element.parentElement;

    while (current instanceof Element) {
        if (createsFixedPositionContainingBlock(current)) {
            return current;
        }

        current = current.parentElement;
    }

    return null;
}

function getFixedPositionContainingBlockOffset(element) {
    const containingBlock = findFixedPositionContainingBlock(element);
    if (!(containingBlock instanceof Element)) {
        return { offsetLeftPx: 0, offsetTopPx: 0 };
    }

    const rect = containingBlock.getBoundingClientRect();

    if (!Number.isFinite(rect.left) || !Number.isFinite(rect.top)) {
        return { offsetLeftPx: 0, offsetTopPx: 0 };
    }

    return {
        offsetLeftPx: rect.left,
        offsetTopPx: rect.top
    };
}

function resolveRenderedDropdownHeightPx(dropdownElement, fallbackHeightPx) {
    if (!(dropdownElement instanceof HTMLElement)) {
        return fallbackHeightPx;
    }

    const measuredOffsetHeight = toFiniteNumber(dropdownElement.offsetHeight, NaN);
    if (Number.isFinite(measuredOffsetHeight) && measuredOffsetHeight > 0) {
        return measuredOffsetHeight;
    }

    const measuredRectHeight = toFiniteNumber(dropdownElement.getBoundingClientRect().height, NaN);
    if (Number.isFinite(measuredRectHeight) && measuredRectHeight > 0) {
        return measuredRectHeight;
    }

    const measuredScrollHeight = toFiniteNumber(dropdownElement.scrollHeight, NaN);
    if (Number.isFinite(measuredScrollHeight) && measuredScrollHeight > 0) {
        return Math.max(1, Math.min(fallbackHeightPx, measuredScrollHeight));
    }

    return fallbackHeightPx;
}

export function calculateDropdownPlacement(triggerElement, dropdownElementOrRequest, requestOrUndefined) {
    let dropdownElement = dropdownElementOrRequest;
    let request = requestOrUndefined;

    if (requestOrUndefined === undefined && !(dropdownElementOrRequest instanceof Element)) {
        dropdownElement = null;
        request = dropdownElementOrRequest;
    }

    if (!(triggerElement instanceof Element)) {
        return null;
    }

    const rect = triggerElement.getBoundingClientRect();

    if (!Number.isFinite(rect.width) || rect.width <= 0) {
        return null;
    }

    const viewportWidth = toFiniteNumber(window.innerWidth, document.documentElement.clientWidth);
    const viewportHeight = toFiniteNumber(window.innerHeight, document.documentElement.clientHeight);

    if (viewportWidth <= 0 || viewportHeight <= 0) {
        return null;
    }

    const gapPx = Math.max(0, toFiniteNumber(request?.gapPx, 12));
    const requestedMaxHeightPx = Math.max(MIN_DROPDOWN_HEIGHT_PX, toFiniteNumber(request?.maxHeightPx, 256));
    const preferUpward = request?.preferUpward === true;

    const availableBelow = Math.max(
        0,
        viewportHeight - rect.bottom - gapPx - VIEWPORT_PADDING_PX);

    const availableAbove = Math.max(
        0,
        rect.top - gapPx - VIEWPORT_PADDING_PX);

    const minUsefulHeight = Math.min(MIN_DROPDOWN_HEIGHT_PX, requestedMaxHeightPx);

    let openUpward = preferUpward;
    const preferredSpace = preferUpward ? availableAbove : availableBelow;
    const fallbackSpace = preferUpward ? availableBelow : availableAbove;

    if (preferredSpace < minUsefulHeight && fallbackSpace > preferredSpace) {
        openUpward = !preferUpward;
    }

    const availableSpace = openUpward ? availableAbove : availableBelow;
    const maxHeightPx = Math.max(0, Math.min(requestedMaxHeightPx, availableSpace));

    if (maxHeightPx <= 0) {
        return null;
    }

    const widthPx = Math.max(0, rect.width);
    let leftPx = rect.left;

    if (leftPx + widthPx > viewportWidth - VIEWPORT_PADDING_PX) {
        leftPx = viewportWidth - widthPx - VIEWPORT_PADDING_PX;
    }

    leftPx = Math.max(VIEWPORT_PADDING_PX, leftPx);

    const renderedHeightPx = resolveRenderedDropdownHeightPx(dropdownElement, maxHeightPx);

    let topPx = openUpward
        ? Math.max(VIEWPORT_PADDING_PX, rect.top - gapPx - renderedHeightPx)
        : Math.min(
            viewportHeight - VIEWPORT_PADDING_PX - renderedHeightPx,
            rect.bottom + gapPx);

    const fixedOffset = getFixedPositionContainingBlockOffset(triggerElement);
    leftPx -= fixedOffset.offsetLeftPx;
    topPx -= fixedOffset.offsetTopPx;

    return {
        openUpward,
        topPx: Math.round(topPx),
        leftPx: Math.round(leftPx),
        widthPx: Math.round(widthPx),
        maxHeightPx: Math.floor(maxHeightPx)
    };
}

function clearActiveOutsideCloseRegistration() {
    if (!activeOutsideCloseRegistration) {
        return;
    }

    document.removeEventListener(
        'pointerdown',
        activeOutsideCloseRegistration.onPointerDownCapture,
        true);
    document.removeEventListener(
        'focusin',
        activeOutsideCloseRegistration.onFocusInCapture,
        true);
    activeOutsideCloseRegistration = null;
}

function isInside(element, target) {
    return element instanceof Element && element.contains(target);
}

function requestClose(dotNetReference) {
    if (!dotNetReference || typeof dotNetReference.invokeMethodAsync !== 'function') {
        return;
    }

    void dotNetReference.invokeMethodAsync('RequestClose').catch(() => { });
}

export function registerOutsideClose(selectId, triggerElement, dropdownElement, dotNetReference) {
    if (
        typeof selectId !== 'string' ||
        selectId.length === 0 ||
        !(triggerElement instanceof Element) ||
        !(dropdownElement instanceof Element) ||
        !dotNetReference) {
        return;
    }

    if (activeOutsideCloseRegistration) {
        const previous = activeOutsideCloseRegistration;
        clearActiveOutsideCloseRegistration();

        if (previous.selectId !== selectId) {
            requestClose(previous.dotNetReference);
        }
    }

    const onPointerDownCapture = (event) => {
        const target = event.target;
        if (!(target instanceof Node)) {
            return;
        }

        if (isInside(triggerElement, target) || isInside(dropdownElement, target)) {
            return;
        }

        clearActiveOutsideCloseRegistration();
        requestClose(dotNetReference);
    };

    const onFocusInCapture = (event) => {
        const target = event.target;
        if (!(target instanceof Node)) {
            return;
        }

        if (isInside(triggerElement, target) || isInside(dropdownElement, target)) {
            return;
        }

        clearActiveOutsideCloseRegistration();
        requestClose(dotNetReference);
    };

    activeOutsideCloseRegistration = {
        selectId,
        dotNetReference,
        onPointerDownCapture,
        onFocusInCapture
    };

    document.addEventListener('pointerdown', onPointerDownCapture, true);
    document.addEventListener('focusin', onFocusInCapture, true);
}

export function unregisterOutsideClose(selectId) {
    if (!activeOutsideCloseRegistration) {
        return;
    }

    if (activeOutsideCloseRegistration.selectId !== selectId) {
        return;
    }

    clearActiveOutsideCloseRegistration();
}
