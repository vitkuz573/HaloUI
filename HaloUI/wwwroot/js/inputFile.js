export function openFileInput(inputElement) {
    if (!(inputElement instanceof HTMLInputElement)) {
        return false;
    }

    if (inputElement.disabled) {
        return false;
    }

    inputElement.click();

    return true;
}
