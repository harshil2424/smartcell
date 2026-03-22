import { store } from '../store.js';

  /* ──────────────────────────────────────────────────────
     STATE
  ────────────────────────────────────────────────────── */
  let algo = 'sjf';
  let processes = [];    // { id, name, arrival, burst, priority, wt, tat, ct, colorIdx, originalDate }
  let pidCount = 0;

  function syncWithOrders() {
      const orders = store.getOrders().filter(o => o.status === 'pending' || o.status === 'processing');
      if (!orders.length) {
          processes = [];
          renderTable();
          return;
      }

      // Find earliest date to calculate relative arrival times
      const dates = orders.map(o => o.date).filter(d => d).map(d => new Date(d).getTime());
      const minDate = dates.length ? Math.min(...dates) : new Date().getTime();
      const oneDay = 24 * 60 * 60 * 1000;

      processes = orders.map((o, i) => {
          const arrivalTime = o.date ? Math.max(0, Math.floor((new Date(o.date).getTime() - minDate) / oneDay)) : 0;
          return {
              id: o.id || `ORD-${i}`,
              name: o.item,
              arrival: arrivalTime,
              burst: Math.ceil(o.distance) || 1, // use distance as burst
              priority: (o.category === 'Smartphones' ? 1 : (o.category === 'Laptops' ? 2 : 3)),
              colorIdx: i,
              originalDate: o.date
          };
      });
      pidCount = processes.length;
      renderTable();
  }

  // Initialize and sync
  store.initPromise.then(() => {
      syncWithOrders();
  });

  const COLORS = ['pc-0','pc-1','pc-2','pc-3','pc-4','pc-5','pc-6','pc-7','pc-8','pc-9'];

  /* ──────────────────────────────────────────────────────
     ALGORITHM SELECTOR
  ────────────────────────────────────────────────────── */
  const algoInfo = {
    sjf: {
      title: 'SJF (Shortest Job First)',
      sub: 'Non-preemptive',
      desc: 'Processes are scheduled in order of <span class="text-accent-green font-semibold">ascending burst time</span>. Among processes arriving at the same time, the one with the smallest burst is picked first. SJF minimises average waiting time.',
      steps: [
        'Sort by arrival time, break ties by burst time',
        'Pick the available process with smallest burst',
        'Execute fully before scheduling next'
      ]
    },
    priority: {
      title: 'Priority Scheduling',
      sub: 'Non-preemptive (1 = highest)',
      desc: 'Processes are scheduled based on <span class="text-accent-purple font-semibold">priority number</span> — lower number = higher priority. Ties in priority are broken by arrival time. Useful for urgent delivery orders.',
      steps: [
        'Assign each order a priority number (1 = most urgent)',
        'Among available processes, pick the lowest priority number',
        'Execute fully, then pick next available by priority'
      ]
    }
  };

  function setAlgo(a) {
    algo = a;
    const tabSjf = document.getElementById('tab-sjf');
    const tabPri = document.getElementById('tab-priority');
    if (tabSjf) tabSjf.classList.toggle('active', a === 'sjf');
    if (tabPri) tabPri.classList.toggle('active', a === 'priority');
    
    const priField = document.getElementById('priority-field');
    if (priField) {
        priField.style.opacity = a === 'priority' ? '1' : '0.35';
        priField.style.pointerEvents = a === 'priority' ? 'auto' : 'none';
    }
    
    const priColHdr = document.getElementById('pri-col-hdr');
    if (priColHdr) priColHdr.style.opacity = a === 'priority' ? '1' : '0.3';
    
    const info = algoInfo[a];
    const infoTitle = document.getElementById('info-title');
    const infoDesc = document.getElementById('info-desc');
    const infoSteps = document.getElementById('info-steps');
    
    if (infoTitle) infoTitle.textContent = info.title;
    if (infoDesc) infoDesc.innerHTML = info.desc;
    if (infoSteps) infoSteps.innerHTML = info.steps.map((s,i) => `
      <div class="flex items-start gap-2 text-xs text-gray-500">
        <span class="w-4 h-4 mt-0.5 rounded-full bg-accent-green/10 text-accent-green text-[9px] font-bold flex items-center justify-center flex-shrink-0">${i+1}</span>
        ${s}
      </div>`).join('');
    
    const statAlgo = document.getElementById('stat-algo');
    const algoBadge = document.getElementById('algo-badge');
    if (statAlgo) statAlgo.textContent = a === 'sjf' ? 'SJF' : 'Priority';
    if (algoBadge) algoBadge.textContent = a === 'sjf' ? 'SJF' : 'Priority';
    
    const modalHint = document.getElementById('modal-hint');
    if (modalHint) {
        modalHint.innerHTML = a === 'sjf'
          ? 'SJF: Processes with <span class="text-accent-green font-semibold">smaller burst time</span> are scheduled first.'
          : 'Priority: Processes with <span class="text-accent-purple font-semibold">lower priority number (1=high)</span> are scheduled first.';
    }
    resetResults();
  }

  /* ──────────────────────────────────────────────────────
     PROCESS TABLE
  ────────────────────────────────────────────────────── */
  function renderTable() {
    const body = document.getElementById('procBody');
    const statTotal = document.getElementById('stat-total');
    if (statTotal) statTotal.textContent = processes.length;
    if (!processes.length) {
      body.innerHTML = `<tr><td colspan="8" class="px-5 py-10 text-center text-xs text-gray-700 italic">No processes — click "Add Process" or "Load Sample"</td></tr>`;
      return;
    }
    body.innerHTML = processes.map((p, idx) => `
      <tr class="proc-row">
        <td class="px-5 py-3">
          <div class="flex items-center gap-2.5">
            <span class="w-7 h-7 rounded-lg ${COLORS[p.colorIdx % 10]} flex items-center justify-center text-[10px] font-bold flex-shrink-0">P${idx+1}</span>
            <div>
              <p class="text-sm font-semibold text-white leading-tight">${p.name}</p>
              <p class="text-[10px] text-gray-600">${p.id}</p>
            </div>
          </div>
        </td>
        <td class="px-5 py-3 text-center font-mono text-sm text-gray-300">${p.originalDate || p.arrival}</td>
        <td class="px-5 py-3 text-center font-mono text-sm text-white font-bold">${p.burst}</td>
        <td class="px-5 py-3 text-center font-mono text-sm ${algo === 'priority' ? 'text-accent-purple font-bold' : 'text-gray-600'}">${p.priority}</td>
        <td class="px-5 py-3 text-center font-mono text-sm ${p.wt !== undefined ? 'text-accent-yellow font-bold' : 'text-gray-700'}">${p.wt !== undefined ? p.wt : '—'}</td>
        <td class="px-5 py-3 text-center font-mono text-sm ${p.tat !== undefined ? 'text-accent-purple font-bold' : 'text-gray-700'}">${p.tat !== undefined ? p.tat : '—'}</td>
        <td class="px-5 py-3 text-center font-mono text-sm ${p.ct !== undefined ? 'text-accent-green font-bold' : 'text-gray-700'}">${p.ct !== undefined ? p.ct : '—'}</td>
        <td class="px-5 py-3 text-right">
          <button onclick="removeProcess('${p.id}')" class="p-1.5 rounded-lg hover:bg-red-500/10 text-gray-600 hover:text-red-500 transition-colors">
            <i data-lucide="trash-2" class="w-3.5 h-3.5"></i>
          </button>
        </td>
      </tr>`).join('');
    if(window.lucide) window.lucide.createIcons();
  }

  /* ──────────────────────────────────────────────────────
     SCHEDULING ALGORITHMS
  ────────────────────────────────────────────────────── */
  function runScheduler() {
    if (!processes.length) { showToast('⚠️ Add at least one process first!', '#FFD700'); return; }

    // Reset results
    processes.forEach(p => { p.wt = undefined; p.tat = undefined; p.ct = undefined; });

    const ganttBlocks = [];
    const logs = [];
    let time = 0;
    const remaining = processes.map(p => ({ ...p }));

    if (algo === 'sjf') {
      const done = new Set();
      while (done.size < remaining.length) {
        const available = remaining.filter(p => p.arrival <= time && !done.has(p.id));
        if (!available.length) {
          const nextArrival = Math.min(...remaining.filter(p => !done.has(p.id)).map(p => p.arrival));
          ganttBlocks.push({ label: 'IDLE', start: time, end: nextArrival, colorIdx: -1 });
          logs.push(`⏸ [t=${time}] CPU idle — waiting for next process (arrives at t=${nextArrival})`);
          time = nextArrival;
          continue;
        }
        available.sort((a, b) => a.burst - b.burst || a.arrival - b.arrival);
        const p = available[0];
        const start = time;
        const end = time + p.burst;
        ganttBlocks.push({ label: `P${processes.findIndex(x => x.id === p.id) + 1}`, start, end, colorIdx: p.colorIdx, id: p.id });
        logs.push(`▶ [t=${start}→${end}] Running ${p.name} | Burst=${p.burst}`);
        time = end;
        const orig = processes.find(x => x.id === p.id);
        orig.ct = end;
        orig.tat = end - p.arrival;
        orig.wt = orig.tat - p.burst;
        done.add(p.id);
      }
    } else {
      const done = new Set();
      while (done.size < remaining.length) {
        const available = remaining.filter(p => p.arrival <= time && !done.has(p.id));
        if (!available.length) {
          const nextArrival = Math.min(...remaining.filter(p => !done.has(p.id)).map(p => p.arrival));
          ganttBlocks.push({ label: 'IDLE', start: time, end: nextArrival, colorIdx: -1 });
          logs.push(`⏸ [t=${time}] CPU idle — next process arrives at t=${nextArrival}`);
          time = nextArrival;
          continue;
        }
        available.sort((a, b) => a.priority - b.priority || a.arrival - b.arrival);
        const p = available[0];
        const start = time;
        const end = time + p.burst;
        ganttBlocks.push({ label: `P${processes.findIndex(x => x.id === p.id) + 1}`, start, end, colorIdx: p.colorIdx, id: p.id });
        logs.push(`▶ [t=${start}→${end}] Running ${p.name} | Priority=${p.priority} | Burst=${p.burst}`);
        time = end;
        const orig = processes.find(x => x.id === p.id);
        orig.ct = end;
        orig.tat = end - p.arrival;
        orig.wt = orig.tat - p.burst;
        done.add(p.id);
      }
    }

    const totalWT  = processes.reduce((s, p) => s + (p.wt || 0), 0);
    const totalTAT = processes.reduce((s, p) => s + (p.tat || 0), 0);
    const awt = (totalWT / processes.length).toFixed(2);
    const att = (totalTAT / processes.length).toFixed(2);
    const statAwt = document.getElementById('stat-awt');
    const statAtt = document.getElementById('stat-att');
    if (statAwt) statAwt.textContent = awt;
    if (statAtt) statAtt.textContent = att;

    renderTable();
    renderGantt(ganttBlocks);
    renderLog(logs, awt, att);
    showToast(`✓ Schedule complete — AWT=${awt}, ATT=${att}`);
  }

  /* ──────────────────────────────────────────────────────
     GANTT CHART RENDERER
  ────────────────────────────────────────────────────── */
  function renderGantt(blocks) {
    const container = document.getElementById('gantt-container');
    const empty = document.getElementById('gantt-empty');
    const rowsEl = document.getElementById('gantt-rows');
    const rulerEl = document.getElementById('gantt-ruler');
    const statsEl = document.getElementById('gantt-stats');
    const legendEl = document.getElementById('gantt-legend');

    if (container) container.classList.remove('hidden');
    if (empty) empty.classList.add('hidden');

    const totalTime = blocks[blocks.length - 1].end;
    const CELL_W = 28;

    if (rowsEl) {
        rowsEl.innerHTML = `
          <div class="flex items-center gap-1 mb-1">
            <span class="text-[9px] text-gray-600 w-16 flex-shrink-0 text-right pr-2">CPU</span>
            <div class="flex items-stretch gap-px">
              ${blocks.map((b, i) => {
                const w = (b.end - b.start) * CELL_W;
                const delay = i * 0.04;
                if (b.colorIdx === -1) {
                  return `<div class="gantt-cell bg-[#1a1a1a] border border-gray-800/60 text-gray-600" style="width:${w}px;animation-delay:${delay}s" title="IDLE [${b.start}→${b.end}]">
                    <span class="truncate px-1" style="font-size:9px">IDLE</span>
                  </div>`;
                }
                return `<div class="gantt-cell ${COLORS[b.colorIdx % 10]}" style="width:${w}px;animation-delay:${delay}s" title="${b.label} [${b.start}→${b.end}]">
                  <span class="truncate px-1">${b.label}</span>
                </div>`;
              }).join('')}
            </div>
          </div>`;
    }

    if (rulerEl) {
        rulerEl.innerHTML = `<div class="w-16 flex-shrink-0"></div>` +
          Array.from({ length: totalTime + 1 }, (_, t) =>
            `<div class="timeline-tick" style="flex:0 0 ${CELL_W}px">${t}</div>`
          ).join('');
    }

    if (statsEl) {
        statsEl.innerHTML = processes.map((p, i) => `
          <div class="bg-[#0d0d0d] border border-gray-800/40 rounded-xl p-3 flex items-center gap-3">
            <span class="w-7 h-7 rounded-lg ${COLORS[p.colorIdx % 10]} flex items-center justify-center text-[10px] font-bold flex-shrink-0">P${i+1}</span>
            <div class="min-w-0">
              <p class="text-[10px] text-white font-semibold truncate">${p.name}</p>
              <p class="text-[9px] text-gray-500 mt-0.5">WT=<b class="text-accent-yellow">${p.wt ?? '—'}</b> · TAT=<b class="text-accent-purple">${p.tat ?? '—'}</b> · CT=<b class="text-accent-green">${p.ct ?? '—'}</b></p>
            </div>
          </div>`).join('');
    }

    if (legendEl) {
        legendEl.innerHTML = processes.map((p, i) => `
          <span class="flex items-center gap-1.5 text-[10px] text-gray-400">
            <span class="w-2.5 h-2.5 rounded-sm ${COLORS[p.colorIdx % 10].split(' ')[0]}"></span>P${i+1}
          </span>`).join('');
    }

    if(window.lucide) window.lucide.createIcons();
  }

  function renderLog(logs, awt, att) {
    const el = document.getElementById('stepLog');
    if (el) {
        el.innerHTML = `
          <div class="not-italic space-y-1.5">
            ${logs.map((l, i) => `
              <div class="flex items-start gap-2">
                <span class="text-[9px] text-gray-700 font-mono w-5 flex-shrink-0 mt-0.5">${String(i+1).padStart(2,'0')}</span>
                <span class="text-xs ${l.startsWith('▶') ? 'text-accent-green' : 'text-gray-500'}">${l}</span>
              </div>`).join('')}
            <div class="mt-3 pt-3 border-t border-gray-800/40 flex gap-6">
              <span class="text-xs text-gray-400">Avg. Wait: <b class="text-accent-yellow">${awt}</b> units</span>
              <span class="text-xs text-gray-400">Avg. TAT: <b class="text-accent-purple">${att}</b> units</span>
            </div>
          </div>`;
    }
  }

  /* ──────────────────────────────────────────────────────
     MODAL
  ────────────────────────────────────────────────────── */
  function openAddModal() {
    ['f-pname','f-arrival','f-burst','f-priority'].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = (id==='f-arrival'?0:(id==='f-pname'?'':1));
    });
    const modal = document.getElementById('addModal');
    if (modal) modal.classList.remove('hidden');
    setTimeout(() => {
        const pname = document.getElementById('f-pname');
        if (pname) pname.focus();
    }, 50);
  }
  function closeAddModal() { const modal = document.getElementById('addModal'); if (modal) modal.classList.add('hidden'); }
  function closeAddBackdrop(e) { if (e.target.id === 'addModal') closeAddModal(); }

  function addProcess() {
    const name    = document.getElementById('f-pname').value.trim();
    const arrival = parseInt(document.getElementById('f-arrival').value) || 0;
    const burst   = parseInt(document.getElementById('f-burst').value) || 1;
    const priority = parseInt(document.getElementById('f-priority').value) || 1;
    if (!name) { showToast('⚠️ Process name is required!', '#FFD700'); return; }
    if (burst < 1) { showToast('⚠️ Burst time must be ≥ 1', '#FFD700'); return; }
    const p = { id: 'PID-' + (++pidCount), name, arrival, burst, priority, colorIdx: pidCount - 1 };
    processes.push(p);
    closeAddModal();
    renderTable();
    showToast(`✓ ${name} added (Burst=${burst})`);
  }

  function removeProcess(id) {
    processes = processes.filter(p => p.id !== id);
    renderTable();
    resetResults();
  }

  function clearAll() {
    processes = [];
    pidCount = 0;
    renderTable();
    resetResults();
    showToast('All processes cleared');
  }

  /* ──────────────────────────────────────────────────────
     STORE / SAMPLE DATA
  ────────────────────────────────────────────────────── */
  function importFromStore() {
    const orders = store.getQueue(); // Pull from the active delivery queue
    if (!orders.length) {
        showToast('⚠️ Delivery queue is empty! No orders to import.', '#ef4444');
        return;
    }
    clearAll();
    orders.forEach((o, i) => {
        // Map order fields to process fields
        // Burst time = quantity + 2 (simulated)
        // Priority = (High:1, Medium:2, Low:3)
        let pri = 2;
        if (o.priority === 'High') pri = 1;
        if (o.priority === 'Low') pri = 3;

        processes.push({
            id: o.id,
            name: `${o.id} · ${o.customer}`,
            arrival: i, // Simulated arrival in order of queue
            burst: (o.qty || 1) + 2,
            priority: pri,
            colorIdx: pidCount++
        });
    });
    renderTable();
    showToast(`✓ Imported ${orders.length} orders from queue!`);
  }

  function loadSample() {
    clearAll();
    const samples = [
      { name: 'ORD-101 · Rahul (TV)',      arrival: 0, burst: 6, priority: 2 },
      { name: 'ORD-102 · Priya (Shoes)',    arrival: 1, burst: 3, priority: 1 },
      { name: 'ORD-103 · Ankit (Noodles)', arrival: 2, burst: 8, priority: 4 },
      { name: 'ORD-104 · Sneha (Shelf)',   arrival: 3, burst: 2, priority: 3 },
      { name: 'ORD-105 · Arun (Earbuds)',  arrival: 4, burst: 4, priority: 2 },
    ];
    samples.forEach(s => {
      processes.push({ id: 'PID-' + (++pidCount), ...s, colorIdx: pidCount - 1 });
    });
    renderTable();
    showToast('✓ Sample data loaded — click Run Schedule!');
  }

  function resetResults() {
    processes.forEach(p => { delete p.wt; delete p.tat; delete p.ct; });
    
    const statAwt = document.getElementById('stat-awt');
    const statAtt = document.getElementById('stat-att');
    if (statAwt) statAwt.textContent = '—';
    if (statAtt) statAtt.textContent = '—';
    
    const ganttCont = document.getElementById('gantt-container');
    const ganttEmpty = document.getElementById('gantt-empty');
    if (ganttCont) ganttCont.classList.add('hidden');
    if (ganttEmpty) ganttEmpty.classList.remove('hidden');
    
    const stepLog = document.getElementById('stepLog');
    if (stepLog) stepLog.innerHTML = '<span class="italic">Run the scheduler to see the execution trace here.</span>';
    
    const ganttLegend = document.getElementById('gantt-legend');
    if (ganttLegend) ganttLegend.innerHTML = '';
  }

  let toastTimer;
  function showToast(msg, dotColor) {
    const t = document.getElementById('toast');
    if (!t) return;
    const dot = t.querySelector('.tdot');
    const tMsg = document.getElementById('tMsg');
    if (tMsg) tMsg.textContent = msg;
    if (dot) dot.style.background = dotColor || '#32D583';
    t.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => t.classList.remove('show'), 3200);
  }

  // Bind to window
  window.setAlgo = setAlgo;
  window.runScheduler = runScheduler;
  window.openAddModal = openAddModal;
  window.closeAddModal = closeAddModal;
  window.closeAddBackdrop = closeAddBackdrop;
  window.addProcess = addProcess;
  window.removeProcess = removeProcess;
  window.clearAll = clearAll;
  window.loadSample = loadSample;
  window.importFromStore = importFromStore;

  /* ──────────────────────────────────────────────────────
     INIT
  ────────────────────────────────────────────────────── */
  window.addEventListener('DOMContentLoaded', () => {
    store.initPromise.then(() => {
        renderProcessTable();
    });
    if(window.lucide) window.lucide.createIcons();
  });
