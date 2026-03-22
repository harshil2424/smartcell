import { store } from '../store.js';

// ── PRIORITY CLASSES ─────────────────────────────────────
const priClass = { High:'pri-high', Medium:'pri-medium', Low:'pri-low' };
const priDot   = { High:'bg-red-500', Medium:'bg-accent-yellow', Low:'bg-accent-green' };

// ── RENDER CARD ──────────────────────────────────────────
function cardHTML(order, colType) {
const isQueue    = colType === 'queue';
const isProgress = colType === 'progress';

const actionBtn = isQueue
    ? `<button onclick="processOrder('${order.id}')" title="Start processing" class="p-1.5 rounded-lg hover:bg-accent-purple/10 text-gray-600 hover:text-accent-purple transition-colors flex-shrink-0"><i data-lucide="play" class="w-3.5 h-3.5"></i></button>`
    : isProgress
    ? `<button onclick="completeOrder('${order.id}')" title="Mark delivered" class="p-1.5 rounded-lg hover:bg-accent-green/10 text-gray-600 hover:text-accent-green transition-colors flex-shrink-0"><i data-lucide="check" class="w-3.5 h-3.5"></i></button>`
    : '';

return `
    <div class="delivery-card card-enter" id="card-${order.id}">
    <div class="flex items-start justify-between gap-2 mb-3">
        <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2 mb-1">
            <span class="text-[10px] font-mono text-gray-600">${order.id}</span>
            <span class="text-[10px] font-bold ${priClass[order.priority] || 'pri-medium'} px-2 py-0.5 rounded-full">${order.priority}</span>
        </div>
        <h4 class="text-sm font-semibold text-white leading-tight truncate">${order.item}</h4>
        </div>
        ${actionBtn}
    </div>
    <div class="space-y-1.5">
        <div class="flex items-center gap-2 text-[11px] text-gray-500">
        <i data-lucide="user" class="w-3 h-3 flex-shrink-0"></i><span>${order.customer}</span>
        </div>
        <div class="flex items-center gap-2 text-[11px] text-gray-500">
        <i data-lucide="map-pin" class="w-3 h-3 flex-shrink-0"></i><span class="truncate">${order.address}</span>
        </div>
        <div class="flex items-center justify-between pt-2 border-t border-gray-800/60">
        <div class="flex items-center gap-1.5 text-[10px] text-gray-600">
            <i data-lucide="clock" class="w-3 h-3"></i>${order.time} · ${order.date}
        </div>
        <div class="flex items-center gap-1 text-[10px] text-gray-500">
            Qty: <span class="font-bold text-white">${order.qty}</span>
        </div>
        </div>
    </div>
    </div>`;
}

// ── RENDER QUEUE BAR ─────────────────────────────────────
function renderQueueBar() {
const queue = store.getQueue();
const bar = document.getElementById('queueBar');
if (!queue.length) {
    bar.innerHTML = `<div class="flex-1 flex items-center justify-center text-xs text-gray-700 italic">Queue is empty</div>`;
    document.getElementById('queueInfo').textContent = 'size: 0';
    return;
}
bar.innerHTML = queue.map((o, i) => `
    <div class="flex-shrink-0 flex flex-col items-center gap-1" title="${o.id}: ${o.item}">
    <div class="h-10 px-3 rounded-lg border ${i===0?'border-accent-green/40 bg-accent-green/10':i===queue.length-1?'border-accent-lime/40 bg-accent-lime/10':'border-gray-800 bg-[#1a1a1a]'} flex items-center gap-1.5 text-[11px] font-medium ${i===0?'text-accent-green':i===queue.length-1?'text-accent-lime':'text-gray-400'}">
        <span class="w-1.5 h-1.5 rounded-full ${priDot[o.priority] || 'bg-accent-yellow'} flex-shrink-0"></span>
        <span class="max-w-[80px] truncate">${o.id}</span>
    </div>
    <span class="text-[8px] text-gray-700">${i===0?'FRONT':i===queue.length-1?'REAR':i+1}</span>
    </div>
`).join(`<div class="flex items-center self-start mt-3 text-gray-800"><i data-lucide="chevron-right" class="w-3 h-3"></i></div>`);
document.getElementById('queueInfo').textContent = `size: ${queue.length}`;
if(window.lucide) window.lucide.createIcons();
}

// ── RENDER ALL COLUMNS ───────────────────────────────────
function renderAll() {
const queue = store.getQueue();
const inProgress = store.getInProgress();
const delivered = store.getDelivered();

const s = (document.getElementById('searchInput')?.value || '').toLowerCase();
const filterQ = arr => arr.filter(o =>
    !s || o.id.toLowerCase().includes(s) || o.item.toLowerCase().includes(s) || o.customer.toLowerCase().includes(s)
);

document.getElementById('col-queue').innerHTML     = filterQ(queue).map(o => cardHTML(o,'queue')).join('') || emptyCol();
document.getElementById('col-progress').innerHTML  = filterQ(inProgress).map(o => cardHTML(o,'progress')).join('') || emptyCol();
document.getElementById('col-delivered').innerHTML = filterQ(delivered).map(o => cardHTML(o,'delivered')).join('') || emptyCol();

// badges
document.getElementById('badge-queue').textContent     = queue.length;
document.getElementById('badge-progress').textContent  = inProgress.length;
document.getElementById('badge-delivered').textContent = delivered.length;

// stats
document.getElementById('stat-queue').textContent     = queue.length;
document.getElementById('stat-inprogress').textContent = inProgress.length;
document.getElementById('stat-delivered').textContent  = delivered.length;
document.getElementById('stat-total').textContent      = queue.length + inProgress.length + delivered.length;

renderQueueBar();
renderLog();
if(window.lucide) window.lucide.createIcons();
}

function emptyCol() {
return `<div class="flex flex-col items-center justify-center py-10 text-gray-700 text-xs italic gap-2">
    <i data-lucide="inbox" class="w-8 h-8 opacity-30"></i>Empty
</div>`;
}

// ── ENQUEUE ──────────────────────────────────────────────
function enqueueOrder() {
const item     = document.getElementById('f-item').value.trim();
const customer = document.getElementById('f-customer').value.trim();
const address  = document.getElementById('f-address').value.trim() || 'Not specified';
const priority = document.getElementById('f-priority').value;
const qty      = parseInt(document.getElementById('f-qty').value) || 1;
if (!item || !customer) {
    showToast('⚠️ Item and Customer are required!', '#FFD700');
    return;
}
const order = store.enqueue({ item, customer, address, priority, qty });
closeModal();
renderAll();
showToast(`✓ ${order.id} added to REAR of queue`);
}

// ── DEQUEUE (front → In Progress) ────────────────────────
async function dequeueOrder() {
const queue = store.getQueue();
if (!queue.length) { showToast('⚠️ Queue is empty!', '#ef4444'); return; }
const order = await store.backendDequeue();
if (order) {
    renderAll();
    showToast(`▶ ${order.id} dequeued → In Progress`);
} else {
    showToast('⚠️ Dequeue failed on backend', '#ef4444');
}
}

// ── PROCESS (individual card Play button) ────────────────
function processOrder(id) {
const queue = store.getQueue();
if (!queue.length) return;
if (queue[0].id !== id) { showToast('⚠️ FIFO Rule: Only the FRONT order can be processed!', '#FFD700'); return; }
dequeueOrder();
}

// ── COMPLETE (In Progress → Delivered) ───────────────────
function completeOrder(id) {
store.moveToDelivered(id);
renderAll();
showToast(`✅ ${id} delivered successfully!`);
}

// ── LOG ──────────────────────────────────────────────────
const logEventColors = {
'ENQUEUE': 'bg-accent-green/10 text-accent-green',
'DEQUEUE': 'bg-accent-purple/10 text-accent-purple',
'PROCESS': 'bg-accent-blue/10 text-accent-blue',
'DELIVER': 'bg-accent-lime/10 text-accent-lime',
};

function renderLog() {
const log = store.getQueueLog();
const body = document.getElementById('logBody');
if (!log.length) { body.innerHTML = `<tr><td colspan="6" class="px-6 py-8 text-center text-xs text-gray-700 italic">No events yet</td></tr>`; return; }
body.innerHTML = log.map((l, i) => `
    <tr class="hover:bg-accent-green/5 transition-colors">
    <td class="px-6 py-3 text-xs text-gray-600">${log.length - i}</td>
    <td class="px-6 py-3"><span class="inline-flex px-2.5 py-0.5 rounded-full text-[10px] font-bold uppercase ${logEventColors[l.event] || 'bg-gray-800 text-gray-400'}">${l.event}</span></td>
    <td class="px-6 py-3 text-xs font-mono text-gray-400">${l.orderId}</td>
    <td class="px-6 py-3 text-xs text-white font-medium max-w-[160px] truncate">${l.item}</td>
    <td class="px-6 py-3 text-xs text-gray-500">${l.status}</td>
    <td class="px-6 py-3 text-xs text-gray-600">${l.time}</td>
    </tr>`).join('');
}

function clearLog() { store.clearQueueLog(); renderAll(); showToast('Log cleared'); }

// ── MODAL ────────────────────────────────────────────────
function openModal() {
['f-item','f-customer','f-address'].forEach(id => document.getElementById(id).value='');
document.getElementById('f-qty').value = 1;
document.getElementById('f-priority').value = 'Medium';
document.getElementById('addModal').classList.remove('hidden');
}
function closeModal() { document.getElementById('addModal').classList.add('hidden'); }
function closeModalBackdrop(e) { if (e.target.id==='addModal') closeModal(); }

// ── TOAST ────────────────────────────────────────────────
let toastTimer;
function showToast(msg, dotColor) {
const t = document.getElementById('toast');
if (!t) return;
const dot = t.querySelector('.toast-dot');
const msgEl = document.getElementById('toastMsg');
if (msgEl) msgEl.textContent = msg;
if (dot) {
    if (dotColor) dot.style.background = dotColor;
    else dot.style.background = '#32D583';
}
t.classList.add('show');
clearTimeout(toastTimer);
toastTimer = setTimeout(() => t.classList.remove('show'), 3000);
}

// ── BIND GLOBALS FOR INLINE HTML HANDLERS ────────────────
window.openModal = openModal;
window.closeModal = closeModal;
window.closeModalBackdrop = closeModalBackdrop;
window.enqueueOrder = enqueueOrder;
window.dequeueOrder = dequeueOrder;
window.processOrder = processOrder;
window.completeOrder = completeOrder;
window.clearLog = clearLog;
window.renderAll = renderAll;

// ── INIT ─────────────────────────────────────────────────
window.addEventListener('DOMContentLoaded', () => {
store.initPromise.then(() => {
    renderAll();
});
if(window.lucide) window.lucide.createIcons();
});
