import { chromium } from 'playwright';

const consoleMsgs = [];
const pageErrors = [];

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();
page.on('console', m => { if (m.type() === 'error') consoleMsgs.push(m.text()); });
page.on('pageerror', e => pageErrors.push(e.message));

const url = process.argv[2] || 'http://localhost:4399/';
await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });
await page.waitForTimeout(2500);

const rootHtmlLen = await page.$eval('app-root', el => el.innerHTML.trim().length).catch(() => -1);
const sidebarPresent = await page.$('app-sidebar') !== null;
const topbarPresent = await page.$('app-topbar') !== null;
const navLinks = await page.$$eval('app-sidebar a', els => els.map(e => e.textContent.trim()).filter(Boolean)).catch(() => []);
const heading = await page.$eval('h1, h2', el => el.textContent.trim()).catch(() => '(none)');
const currentUrl = page.url();

console.log('URL after load:', currentUrl);
console.log('app-root innerHTML length:', rootHtmlLen);
console.log('app-sidebar present:', sidebarPresent);
console.log('app-topbar present:', topbarPresent);
console.log('sidebar nav links:', JSON.stringify(navLinks));
console.log('first heading:', heading);
console.log('console.error msgs:', consoleMsgs.length ? JSON.stringify(consoleMsgs) : 'none');
console.log('pageerror msgs:', pageErrors.length ? JSON.stringify(pageErrors) : 'none');

await browser.close();
