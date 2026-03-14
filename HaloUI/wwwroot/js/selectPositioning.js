const VIEWPORT_PADDING_PX = 8;
const MIN_DROPDOWN_HEIGHT_PX = 112;

function toFiniteNumber(value, fallback) {
    return Number.isFinite(value) ? value : fallback;
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

    const topPx = openUpward
        ? Math.max(VIEWPORT_PADDING_PX, rect.top - gapPx - maxHeightPx)
        : Math.min(
            viewportHeight - VIEWPORT_PADDING_PX - maxHeightPx,
            rect.bottom + gapPx);

    return {
        openUpward,
        topPx: Math.round(topPx),
        leftPx: Math.round(leftPx),
        widthPx: Math.round(widthPx),
        maxHeightPx: Math.floor(maxHeightPx)
    };
}
