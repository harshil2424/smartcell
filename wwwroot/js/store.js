/**
 * SmartCell — Global Store (Refactored for C# Backend)
 * Handles inventory, orders, and queue data management via API.
 */

class Store {
    constructor() {
        this.data = {
            inventory: [],
            orders: [],
            deliveryQueue: [],
            queueInProgress: [],
            queueDelivered: [],
            queueLog: [],
            recentActivity: []
        };
        this.initialized = false;
        this.initPromise = this._loadData();
    }

    async _loadData() {
        try {
            const response = await fetch('/api/store');
            if (response.ok) {
                this.data = await response.json();
                this.initialized = true;
                // Dispatch event so pages know data is ready
                window.dispatchEvent(new CustomEvent('storeReady'));
            }
        } catch (e) {
            console.error('Failed to load store data from server', e);
        }
    }

    async _sync(type, method, body = null, id = null) {
        let url = `/api/store/${type}`;
        if (id) url += `/${encodeURIComponent(id)}`;
        
        try {
            const options = {
                method: method,
                headers: { 'Content-Type': 'application/json' }
            };
            if (body) options.body = JSON.stringify(body);
            
            const response = await fetch(url, options);
            if (!response.ok) console.error(`Sync failed for ${url}`);
        } catch (e) {
            console.error(`Network error during sync for ${url}`, e);
        }
    }

    // --- Inventory Methods ---
    getInventory() { return this.data.inventory; }

    addInventoryItem(item) {
        // Optimistic Update
        const newItem = {
            id: Date.now(),
            date: new Date().toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' }),
            ...item
        };
        this.data.inventory.push(newItem);
        this.addActivity('Added', newItem.name, newItem.qty, newItem.status);
        
        // Background Sync
        this._sync('inventory', 'POST', item);
        return newItem;
    }

    updateInventoryItem(id, updates) {
        const idx = this.data.inventory.findIndex(item => item.id === id);
        if (idx > -1) {
            this.data.inventory[idx] = { ...this.data.inventory[idx], ...updates };
            this._sync('inventory', 'PUT', updates, id);
        }
    }

    deleteInventoryItem(id) {
        this.data.inventory = this.data.inventory.filter(item => item.id !== id);
        this._sync('inventory', 'DELETE', null, id);
    }

    // --- Order Methods ---
    getOrders() { return this.data.orders; }

    addOrder(order) {
        const newOrder = {
            id: '#SC-' + Math.floor(1000 + Math.random() * 9000),
            date: new Date().toISOString().split('T')[0],
            ...order
        };
        this.data.orders.push(newOrder);
        this.addActivity('Ordered', newOrder.item, 1, 'Pending');

        this._sync('orders', 'POST', newOrder);
        return newOrder;
    }

    deleteOrder(id) {
        this.data.orders = this.data.orders.filter(o => o.id !== id);
        if (this.data.deliveryQueue) this.data.deliveryQueue = this.data.deliveryQueue.filter(o => o.id !== id);
        if (this.data.queueInProgress) this.data.queueInProgress = this.data.queueInProgress.filter(o => o.id !== id);
        if (this.data.queueDelivered) this.data.queueDelivered = this.data.queueDelivered.filter(o => o.id !== id);

        this._sync('orders', 'DELETE', null, id);
    }

    updateOrderStatus(id, status) {
        const idx = this.data.orders.findIndex(o => o.id === id);
        if (idx > -1) {
            this.data.orders[idx].status = status;
            this._sync(`orders/${encodeURIComponent(id)}/status`, 'PUT', status);
        }
    }

    // --- Queue Methods ---
    getQueue() { return this.data.deliveryQueue || []; }
    getInProgress() { return this.data.queueInProgress || []; }
    getDelivered() { return this.data.queueDelivered || []; }
    getQueueLog() { return this.data.queueLog || []; }

    isEnqueued(orderId) {
        return this.data.deliveryQueue.some(o => o.id === orderId) ||
               this.data.queueInProgress.some(o => o.id === orderId) ||
               this.data.queueDelivered.some(o => o.id === orderId);
    }

    enqueue(order, sync = true) {
        if (order.id && this.isEnqueued(order.id)) return null;

        const orderId = order.id || '#SC-' + Math.floor(1000 + Math.random() * 9000);
        const newOrder = {
            id: orderId,
            time: new Date().toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' }),
            date: new Date().toLocaleDateString('en-IN', { day: 'numeric', month: 'short' }),
            ...order
        };

        const existingOrder = this.data.orders.find(o => o.id === orderId);
        if (!existingOrder) {
            this.data.orders.push({
                id: orderId,
                customer: order.customer || 'Unknown',
                email: (order.customer || 'user').toLowerCase().replace(/ /g,'.') + '@example.com',
                item: order.item || 'Unnamed Item',
                category: order.category || 'General',
                date: new Date().toISOString().split('T')[0],
                amount: order.amount || (order.qty || 1) * 1500,
                status: 'pending'
            });
            this.addActivity('Ordered', order.item, order.qty || 1, 'Pending');
        } else {
            existingOrder.status = 'pending';
        }

        if (!this.data.deliveryQueue) this.data.deliveryQueue = [];
        this.data.deliveryQueue.push(newOrder);
        this.addQueueLog('ENQUEUE', newOrder.id, newOrder.item, 'Pending');
        
        if (sync) this._sync('queue/enqueue', 'POST', order);
        return newOrder;
    }

    moveToInProgress(orderId) {
        this.updateOrderStatus(orderId, 'processing');
        const idx = this.data.deliveryQueue.findIndex(o => o.id === orderId);
        if (idx > -1) {
            const order = this.data.deliveryQueue.splice(idx, 1)[0];
            this.data.queueInProgress.push(order);
            this.addQueueLog('PROCESS', order.id, order.item, 'In Progress');
            this._sync(`queue/process/${encodeURIComponent(orderId)}`, 'POST');
            return order;
        }
        return null;
    }

    async backendDequeue() {
        try {
            const response = await fetch('/api/store/queue/dequeue', { method: 'POST' });
            if (response.ok) {
                const order = await response.json();
                const idx = this.data.deliveryQueue.findIndex(o => o.id === order.id);
                if (idx > -1) this.data.deliveryQueue.splice(idx, 1);
                this.data.queueInProgress.push(order);
                this.updateOrderStatus(order.id, 'processing');
                this.addQueueLog('PROCESS', order.id, order.item, 'In Progress');
                return order;
            }
            return null;
        } catch (e) {
            console.error('Dequeue failed', e);
            return null;
        }
    }

    moveToDelivered(orderId) {
        this.updateOrderStatus(orderId, 'delivered');
        const idx = this.data.queueInProgress.findIndex(o => o.id === orderId);
        if (idx > -1) {
            const order = this.data.queueInProgress.splice(idx, 1)[0];
            this.data.queueDelivered.push(order);
            this.addQueueLog('DELIVER', order.id, order.item, 'Delivered');
            this.addActivity('Delivered', order.item, order.qty || 1, 'Completed');
            this._sync(`queue/deliver/${encodeURIComponent(orderId)}`, 'POST');
            return order;
        }
        return null;
    }

    addQueueLog(event, orderId, item, status) {
        if (!this.data.queueLog) this.data.queueLog = [];
        this.data.queueLog.unshift({
            id: this.data.queueLog.length + 1,
            event,
            orderId,
            item,
            status,
            time: new Date().toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' })
        });
        if (this.data.queueLog.length > 50) this.data.queueLog.pop();
    }

    clearQueueLog() {
        this.data.queueLog = [];
        this._sync('queue/log', 'DELETE');
    }

    // --- Activity ---
    getRecentActivity() { return this.data.recentActivity; }

    addActivity(action, item, qty, status) {
        this.data.recentActivity.unshift({
            id: Date.now(),
            action,
            item,
            qty,
            status,
            time: 'Just now'
        });
        if (this.data.recentActivity.length > 10) this.data.recentActivity.pop();
    }

    async updateHashingUnit(index, itemId) {
        try {
            await fetch(`/api/store/hashing/${index}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(itemId)
            });
            if (this.data.hashingTable) {
                this.data.hashingTable[index] = itemId;
            }
        } catch (e) {
            console.error('Error updating hash unit', e);
        }
    }

    async clearHashingTable() {
        try {
            await fetch(`/api/store/hashing`, { method: 'DELETE' });
            if (this.data.hashingTable) {
                this.data.hashingTable = new Array(13).fill(null);
            }
        } catch (e) {
            console.error('Error clearing hash table', e);
        }
    }

    async calculateHash(itemId) {
        try {
            const response = await fetch('/api/store/hashing/calculate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(itemId)
            });
            if (response.ok) {
                const result = await response.json();
                if (result.success && this.data.hashingTable) {
                    this.data.hashingTable[result.finalIndex] = itemId;
                }
                return result;
            }
        } catch (e) {
            console.error('Error calculating hash via backend', e);
        }
        return null;
    }
}

export const store = new Store();
