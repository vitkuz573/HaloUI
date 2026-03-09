# Brand Preview Gallery

Prototype static gallery that consumes CLI output (`themes.json`).

## Usage
1. Run the CLI to export `themes.json`:
   ```bash
   dotnet run --project tools/BrandPreviewCli/BrandPreviewCli.csproj -- \
     --manifest HaloUI/Theme/Tokens/design-tokens.json \
     --themes Light,DarkGlass \
     --brands HaloUI \
     --output artifacts/brand-previews
   ```
2. Copy/serve `tools/BrandPreviewCli/gallery/` alongside the generated JSON:
   ```bash
   cp artifacts/brand-previews/*.json tools/BrandPreviewCli/gallery/
   cd tools/BrandPreviewCli/gallery && python3 -m http.server 4173
   ```
3. Open `http://localhost:4173` to inspect themes.

Future work: integrate this gallery into CI artifacts and expand with component mock-ups.
