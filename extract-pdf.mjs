import { getDocument } from 'pdfjs-dist/legacy/build/pdf.mjs';
import fs from 'fs';
import { createRequire } from 'module';
const require = createRequire(import.meta.url);
const pdfjsWorker = require.resolve('pdfjs-dist/legacy/build/pdf.worker.mjs');

async function extractText(pdfPath, outPath) {
  const data = new Uint8Array(fs.readFileSync(pdfPath));
  const loadingTask = getDocument({ data, useSystemFonts: true });
  const pdf = await loadingTask.promise;
  let fullText = '';
  for (let i = 1; i <= pdf.numPages; i++) {
    const page = await pdf.getPage(i);
    const content = await page.getTextContent();
    const text = content.items.map(item => item.str).join('');
    fullText += `\n--- Page ${i} ---\n${text}`;
  }
  fs.writeFileSync(outPath, fullText);
  console.log(`Extracted ${pdfPath} -> ${outPath}`);
}

await extractText('手游立项方案.pdf', '手游立项方案-pdfjs.txt');
await extractText('系统1-核心解谜系统-策划文档.pdf', '系统1-pdfjs.txt');
