import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';

const configuration = process.env.HALOUI_BUILD_CONFIGURATION ?? 'Release';
const repoRoot = path.resolve(process.cwd(), '..', '..');
const budgetConfigPath = path.resolve(process.cwd(), 'perf-budgets.json');

if (!fs.existsSync(budgetConfigPath)) {
  console.error(`Budget config was not found: ${budgetConfigPath}`);
  process.exit(1);
}

const config = JSON.parse(fs.readFileSync(budgetConfigPath, 'utf8'));
const budgets = Array.isArray(config.budgets) ? config.budgets : [];

if (budgets.length === 0) {
  console.error('No budgets defined in perf-budgets.json');
  process.exit(1);
}

const failures = [];
const rows = [];

for (const budget of budgets) {
  const declaredPath = String(budget.path ?? '');
  let resolvedRelative = declaredPath.replaceAll('${configuration}', configuration);
  let absolutePath = path.resolve(repoRoot, resolvedRelative);

  if (!fs.existsSync(absolutePath) && declaredPath.includes('${configuration}') && configuration !== 'Debug') {
    resolvedRelative = declaredPath.replaceAll('${configuration}', 'Debug');
    absolutePath = path.resolve(repoRoot, resolvedRelative);
  }

  if (!fs.existsSync(absolutePath)) {
    failures.push(`${budget.name}: file not found (${resolvedRelative})`);
    continue;
  }

  const content = fs.readFileSync(absolutePath);
  const rawBytes = content.byteLength;
  const gzipBytes = zlib.gzipSync(content, { level: 9 }).byteLength;
  const maxBytes = Number(budget.maxBytes ?? 0);
  const maxGzipBytes = Number(budget.maxGzipBytes ?? 0);

  rows.push({
    name: budget.name,
    file: resolvedRelative,
    rawBytes,
    maxBytes,
    gzipBytes,
    maxGzipBytes,
  });

  if (maxBytes > 0 && rawBytes > maxBytes) {
    failures.push(`${budget.name}: raw size ${rawBytes} > ${maxBytes}`);
  }

  if (maxGzipBytes > 0 && gzipBytes > maxGzipBytes) {
    failures.push(`${budget.name}: gzip size ${gzipBytes} > ${maxGzipBytes}`);
  }
}

for (const row of rows) {
  console.log(
    `${row.name}\n` +
      `  file: ${row.file}\n` +
      `  raw: ${row.rawBytes}/${row.maxBytes} bytes\n` +
      `  gzip: ${row.gzipBytes}/${row.maxGzipBytes} bytes`,
  );
}

if (failures.length > 0) {
  console.error('\nPerformance budget violations:');
  for (const failure of failures) {
    console.error(`- ${failure}`);
  }
  process.exit(1);
}

console.log('\nAll HaloUI performance budgets are within limits.');
