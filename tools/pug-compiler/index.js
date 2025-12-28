const pug = require('pug');
const fs = require('fs');
const path = require('path');

const args = process.argv.slice(2);
if (args.length < 2) {
  console.error('Usage: node index.js <input.pug> <output.html> [localsJson]');
  process.exit(1);
}

const inputPath = path.resolve(args[0]);
const outputPath = path.resolve(args[1]);
const locals = args[2] ? JSON.parse(args[2]) : {};

try {
  const html = pug.renderFile(inputPath, locals);
  fs.writeFileSync(outputPath, html);
  console.log(`Successfully compiled ${inputPath} to ${outputPath}`);
} catch (err) {
  console.error('Compilation failed:', err);
  process.exit(1);
}
