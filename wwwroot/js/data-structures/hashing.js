import { store } from '../store.js';

const TABLE_SIZE = 13;
let hashTable = new Array(TABLE_SIZE).fill(null);
const unitGrid = document.getElementById('unitGrid');
const logEl = document.getElementById('hashLog');

function init() {
// Load persisted table
if (store.data.hashingTable && store.data.hashingTable.length === TABLE_SIZE) {
    const inventory = store.getInventory();
    hashTable = store.data.hashingTable.map(id => {
        if (id === null) return null;
        return inventory.find(it => it.id == id) || null;
    });
}
renderUnits();
loadItems();
if(window.lucide) window.lucide.createIcons();
}

function renderUnits() {
unitGrid.innerHTML = hashTable.map((item, i) => `
    <div class="unit group" id="unit-${i}">
    <span class="absolute top-2 left-2 text-[9px] font-mono text-gray-700">${i}</span>
    ${item ? `
        <button onclick="emptyUnit(${i})" class="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity p-1 bg-red-500/10 hover:bg-red-500/20 text-red-500 rounded-md">
        <i data-lucide="x" class="w-3 h-3"></i>
        </button>
        <div class="hash-item text-center px-2">
        <p class="text-[10px] font-bold text-accent-green truncate w-full">${item.name}</p>
        <p class="text-[8px] text-gray-600 mt-0.5">${item.sku}</p>
        </div>
    ` : `
        <i data-lucide="box-select" class="w-4 h-4 text-gray-800 opacity-20"></i>
    `}
    </div>
`).join('');
if(window.lucide) window.lucide.createIcons();
}

function loadItems() {
store.initPromise.then(() => {
    const items = store.getInventory();
    const select = document.getElementById('itemSelect');
    select.innerHTML = items.map(it => `<option value="${it.id}">${it.name} (${it.sku})</option>`).join('');
});
}

window.performHash = async function() {
const id = document.getElementById('itemSelect').value;
const items = store.getInventory();
const item = items.find(it => it.id == id);
if (!item) return;

// Backend C# calculation
const result = await store.calculateHash(item.id);
if (!result) return;

result.log.forEach(msg => {
    if (msg.includes('Collision') || msg.includes('Error')) {
        addLog(`<span class="text-red-400">${msg}</span>`);
    } else if (msg.includes('✓')) {
        addLog(`<span class="text-accent-green">${msg}</span>`);
    } else {
        addLog(msg);
    }
});

if (!result.success) return;

// Success
hashTable[result.finalIndex] = item;
renderUnits();

// Animate active unit
const unitEl = document.getElementById(`unit-${result.finalIndex}`);
if (unitEl) {
    unitEl.classList.add('active');
    if (result.collisionDetected) unitEl.classList.add('collision');
    setTimeout(() => {
        unitEl.classList.remove('active', 'collision');
    }, 2000);
}
};

window.emptyUnit = async function(index) {
    if (hashTable[index]) {
        addLog(`Emptying unit ${index}...`);
        hashTable[index] = null;
        await store.updateHashingUnit(index, null);
        renderUnits();
    }
};

window.clearTable = async function() {
addLog(`Clearing entire storage table...`);
hashTable = new Array(TABLE_SIZE).fill(null);
await store.clearHashingTable();
renderUnits();
logEl.innerHTML = '<p>> Table cleared.</p>';
addLog('System ready.');
}

function addLog(msg) {
const p = document.createElement('p');
p.innerHTML = `> ${msg}`;
logEl.prepend(p);
}

store.initPromise.then(init);
