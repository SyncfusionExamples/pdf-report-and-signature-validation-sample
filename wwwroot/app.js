const $ = id => document.getElementById(id);
let lastBlob = null;
let lastObjectUrl = null;

// ---- Tabs ----
function showTab(name) {
  document.querySelectorAll('.tab').forEach(t => t.classList.toggle('active', t.dataset.tab === name));
  document.querySelectorAll('.panel').forEach(p => p.classList.toggle('active', p.id === name));
}
document.querySelectorAll('.tab').forEach(t => t.addEventListener('click', () => showTab(t.dataset.tab)));

// ---- Generate report ----
$('reportForm').addEventListener('submit', async e => {
  e.preventDefault();
  const btn = $('generateBtn');
  const err = $('generateError');
  err.hidden = true;
  btn.disabled = true;
  btn.textContent = 'Generating...';
  try {
    const policyId = $('policyId').value.trim();
    const res = await fetch('/api/pdf/createreport', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyId, name: $('name').value.trim(), sign: $('sign').checked })
    });
    if (!res.ok) throw new Error(await res.text() || res.statusText);

    lastBlob = await res.blob();
    if (lastObjectUrl) URL.revokeObjectURL(lastObjectUrl);
    lastObjectUrl = URL.createObjectURL(lastBlob);

    const fileName = filenameFrom(res) || `report-${policyId}.pdf`;
    $('preview').src = lastObjectUrl;
    $('download').href = lastObjectUrl;
    $('download').download = fileName;
    $('result').hidden = false;
  } catch (ex) {
    err.textContent = ex.message;
    err.hidden = false;
  } finally {
    btn.disabled = false;
    btn.textContent = 'Generate PDF';
  }
});

function filenameFrom(res) {
  const m = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(res.headers.get('Content-Disposition') || '');
  return m ? decodeURIComponent(m[1]) : null;
}

// ---- Validate signature ----
$('validateThis').addEventListener('click', () => {
  if (!lastBlob) return;
  showTab('validate');
  validate(lastBlob, $('download').download);
});

$('validateBtn').addEventListener('click', () => {
  const file = $('file').files[0];
  if (!file) { showError('Choose a PDF file first.'); return; }
  validate(file, file.name);
});

function showError(msg) {
  const el = $('validateError');
  el.textContent = msg || '';
  el.hidden = !msg;
}

async function validate(blob, fileName) {
  showError('');
  $('validation').hidden = true;
  const form = new FormData();
  form.append('file', blob, fileName);
  try {
    const res = await fetch('/api/pdf/validatesignature', { method: 'POST', body: form });
    if (!res.ok) throw new Error(await res.text() || res.statusText);
    renderValidation(await res.json());
  } catch (ex) {
    showError(ex.message);
  }
}

const esc = s => String(s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
const yn = (v, goodWhenTrue = true) =>
  `<span class="badge ${v === goodWhenTrue ? 'ok' : 'bad'}">${v ? 'Yes' : 'No'}</span>`;

function renderValidation(r) {
  const box = $('validation');
  let html = `<h2>${esc(r.fileName)} &mdash; ${r.signatureCount} signature(s)</h2>`;
  if (r.message) html += `<p>${esc(r.message)}</p>`;
  for (const s of r.signatures) {
    const ok = s.isSignatureValid && !s.isDocumentModified;
    html += `
      <div class="sig">
        <h3>${esc(s.fieldName)}
          <span class="badge ${ok ? 'ok' : 'bad'}">${ok ? 'Signature valid' : 'Signature invalid / modified'}</span></h3>
        <table>
          <tr><td>Signer</td><td>${esc(s.signerName)}</td></tr>
          <tr><td>Certificate subject</td><td>${esc(s.certificateSubject)}</td></tr>
          <tr><td>Certificate issuer</td><td>${esc(s.certificateIssuer)}</td></tr>
          <tr><td>Signed at</td><td>${esc(s.signedAt)}</td></tr>
          <tr><td>Status</td><td>${esc(s.status)}</td></tr>
          <tr><td>Signature cryptographically valid</td><td>${yn(s.isSignatureValid)}</td></tr>
          <tr><td>Document modified after signing</td><td>${yn(s.isDocumentModified, false)}</td></tr>
          <tr><td>Digest algorithm</td><td>${esc(s.digestAlgorithm)}</td></tr>
          <tr><td>Cryptographic standard</td><td>${esc(s.cryptographicStandard)}</td></tr>
          <tr><td>Timestamp</td><td>${yn(s.hasTimeStamp)}</td></tr>
          <tr><td>LTV enabled</td><td>${yn(s.isLtvEnabled)}</td></tr>
          <tr><td>Revocation</td><td>${esc(s.revocationStatus)}</td></tr>          
        </table>
      </div>`;
  }
  box.innerHTML = html;
  box.hidden = false;
}
