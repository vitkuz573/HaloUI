async function loadThemes() {
  const response = await fetch('themes.json');
  const themes = await response.json();
  const selector = document.getElementById('theme-selector');
  Object.keys(themes).forEach((key) => {
    const option = document.createElement('option');
    option.value = key;
    option.textContent = key;
    selector.appendChild(option);
  });
  selector.addEventListener('change', () => renderTheme(themes[selector.value]));
  const firstKey = Object.keys(themes)[0];
  if (firstKey) {
    selector.value = firstKey;
    renderTheme(themes[firstKey]);
  }
}

function renderTheme(theme) {
  const semanticBody = document.querySelector('#semantic-table tbody');
  semanticBody.innerHTML = '';
  const colors = theme.semantic?.color ?? {};
  Object.entries(colors).forEach(([token, value]) => {
    const row = document.createElement('tr');
    row.innerHTML = `<td>${token}</td><td><span style="background:${value};display:inline-block;width:1.5rem;height:1.5rem;border-radius:0.25rem;margin-right:0.5rem;border:1px solid rgba(15,23,42,0.3)"></span><code>${value}</code></td>`;
    semanticBody.appendChild(row);
  });

  renderButtons(theme.component?.Button);
  renderDialog(theme.component?.Dialog, theme.component?.Button);
  renderSelect(theme.component?.Select);
  renderTabs(theme.component?.Tab);
}

function renderButtons(buttonNode) {
  const grid = document.getElementById('button-grid');
  grid.innerHTML = '';
  if (!buttonNode) return;
  ['Primary', 'Secondary', 'Tertiary', 'Warning', 'Danger', 'Ghost'].forEach((variant) => {
    const node = buttonNode[variant];
    if (!node) return;
    const card = document.createElement('div');
    card.className = 'variant-card';
    card.innerHTML = `<h3>${variant}</h3>
      <div class="variant-swatch" style="background:${node.Background};color:${node.Text}">Aa</div>
      <p><strong>Background:</strong> ${node.Background}</p>
      <p><strong>Text:</strong> ${node.Text}</p>`;
    grid.appendChild(card);
  });
}

function renderDialog(dialogNode, buttonNode) {
  const panel = document.getElementById('dialog-preview');
  panel.innerHTML = '';
  panel.style.background = dialogNode?.OverlayBackground ?? 'rgba(15,23,42,0.6)';
  panel.style.backdropFilter = dialogNode?.OverlayBackdropFilter ?? 'none';

  if (!dialogNode) {
    panel.textContent = 'Dialog tokens not available for this theme.';
    return;
  }

  const dialog = document.createElement('div');
  dialog.className = 'dialog-sample';
  dialog.style.background = dialogNode.Background ?? '#ffffff';
  dialog.style.borderColor = dialogNode.BorderColor ?? 'transparent';
  dialog.style.boxShadow = dialogNode.Shadow ?? 'none';
  dialog.style.color = dialogNode.BodyTextColor ?? '#111827';
  dialog.style.borderWidth = dialogNode.BorderWidth ?? '1px';
  dialog.style.borderStyle = 'solid';

  const header = document.createElement('div');
  header.className = 'dialog-sample__header';
  header.style.padding = `${dialogNode.Header?.PaddingY ?? '1rem'} ${dialogNode.Header?.PaddingX ?? '1.5rem'}`;
  header.style.background = dialogNode.Header?.Background ?? 'transparent';
  header.innerHTML = `<span>Dialog Preview</span>`;

  const closeGlyph = document.createElement('span');
  closeGlyph.className = 'dialog-sample__close';
  closeGlyph.textContent = '×';
  const ghost = buttonNode?.Ghost;
  closeGlyph.style.color = ghost?.Text ?? '#fff';
  closeGlyph.style.background = ghost?.Background ?? 'transparent';
  closeGlyph.style.borderRadius = '999px';
  closeGlyph.style.padding = '0.15rem 0.5rem';
  closeGlyph.title = 'Close dialog preview';
  header.appendChild(closeGlyph);

  const body = document.createElement('div');
  body.className = 'dialog-sample__body';
  body.style.padding = `${dialogNode.BodyPaddingY ?? '1rem'} ${dialogNode.BodyPaddingX ?? '1.5rem'}`;
  body.innerHTML = `<p>This mock dialog pulls background, border, and typography values straight from <code>component.Dialog</code> tokens so reviewers can inspect overlay + button pairing without running the UI.</p>`;

  const footer = document.createElement('div');
  footer.className = 'dialog-sample__footer';
  footer.style.padding = `${dialogNode.Footer?.PaddingY ?? '1rem'} ${dialogNode.Footer?.PaddingX ?? '1.5rem'}`;
  footer.appendChild(createDialogButton('Cancel', buttonNode?.Secondary));
  footer.appendChild(createDialogButton('Confirm', buttonNode?.Primary, true));

  dialog.appendChild(header);
  dialog.appendChild(body);
  dialog.appendChild(footer);
  panel.appendChild(dialog);
}

function createDialogButton(label, token, emphasize = false) {
  const button = document.createElement('button');
  button.type = 'button';
  button.className = 'dialog-sample__button';
  button.textContent = label;

  if (token) {
    button.style.background = token.Background ?? 'transparent';
    button.style.color = token.Text ?? '#0f172a';
    button.style.borderColor = token.Border ?? 'transparent';
    button.style.boxShadow = token.Shadow ?? 'none';
    button.style.setProperty('--dialog-button-hover', token.BackgroundHover ?? token.Background);
  }

  if (emphasize) {
    button.classList.add('dialog-sample__button--primary');
  }

  return button;
}

function renderSelect(selectNode) {
  const panel = document.getElementById('select-info');
  panel.innerHTML = '';
  if (!selectNode) return;
  const optionDefault = selectNode.Option?.Default;
  if (!optionDefault) return;
  const card = document.createElement('div');
  card.className = 'variant-card';
  card.innerHTML = `<h3>Default Option</h3>
    <div class="variant-swatch" style="background:${optionDefault.Background};"></div>
    <p><strong>Background:</strong> ${optionDefault.Background}</p>
    <p><strong>Text:</strong> ${optionDefault.Text}</p>`;
  panel.appendChild(card);
}

function renderTabs(tabNode) {
  const panel = document.getElementById('tab-info');
  panel.innerHTML = '';
  if (!tabNode) return;
  const inactive = tabNode.Inactive;
  if (!inactive) return;
  const card = document.createElement('div');
  card.className = 'variant-card';
  card.innerHTML = `<h3>Inactive</h3>
    <div class="variant-swatch" style="background:${inactive.Background};"></div>
    <p><strong>Background:</strong> ${inactive.Background}</p>
    <p><strong>Text:</strong> ${inactive.TextColor}</p>`;
  panel.appendChild(card);
}

loadThemes();
