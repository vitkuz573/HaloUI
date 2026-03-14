const VIEWPORT_PADDING_PX = 8;
const MIN_DROPDOWN_HEIGHT_PX = 112;

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

export function calculateDropdownPlacement(triggerElement, request) {
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

    let topPx = openUpward
        ? Math.max(VIEWPORT_PADDING_PX, rect.top - gapPx - maxHeightPx)
        : Math.min(
            viewportHeight - VIEWPORT_PADDING_PX - maxHeightPx,
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
