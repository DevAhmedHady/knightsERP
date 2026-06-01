import pkg from 'jsdom';
const { JSDOM, VirtualConsole } = pkg;

const errors = [];
const vc = new VirtualConsole();
vc.on('jsdomError', e => errors.push('jsdomError: ' + (e.detail?.message || e.message || e)));
vc.on('error', (...a) => errors.push('console.error: ' + a.join(' ')));

const dom = await JSDOM.fromURL('http://localhost:4399/', {
  runScripts: 'dangerously',
  resources: 'usable',
  pretendToBeVisual: true,
  virtualConsole: vc,
});

await new Promise(r => setTimeout(r, 7000));

const root = dom.window.document.querySelector('app-root');
const childCount = root ? root.childNodes.length : -1;
const innerLen = root ? root.innerHTML.trim().length : -1;
console.log('app-root childNodes:', childCount);
console.log('app-root innerHTML length:', innerLen);
console.log('snippet:', (root?.innerHTML || '').replace(/\s+/g, ' ').slice(0, 240));
console.log('ERRORS:', errors.length ? errors.join(' || ') : 'none');
dom.window.close();
