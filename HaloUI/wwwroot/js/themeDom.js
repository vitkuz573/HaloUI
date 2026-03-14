export function setBodyThemeAttribute(themeValue) {
  if (!themeValue) {
    return;
  }

  document.body.setAttribute('data-theme', themeValue);
}
