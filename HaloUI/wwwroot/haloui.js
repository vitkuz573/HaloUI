export function measureElementHeight(elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
        return 0;
    }

    const baseWidth = element.getBoundingClientRect().width || element.offsetWidth || 0;

    const clone = element.cloneNode(true);
    clone.style.visibility = "visible";
    clone.style.position = "relative";
    clone.style.height = "auto";
    clone.style.maxHeight = "none";
    clone.style.opacity = "0";
    clone.style.pointerEvents = "none";
    clone.style.display = "block";

    const wrapper = document.createElement("div");
    wrapper.style.position = "absolute";
    wrapper.style.visibility = "hidden";
    wrapper.style.left = "-9999px";
    wrapper.style.top = "0";
    wrapper.style.height = "auto";
    wrapper.style.maxHeight = "none";
    wrapper.style.width = baseWidth > 0 ? `${baseWidth}px` : "auto";
    wrapper.appendChild(clone);

    document.body.appendChild(wrapper);
    const height = clone.scrollHeight;
    document.body.removeChild(wrapper);

    return height;
}
