/**
 * SmartCell — Shared UI & Global Router
 * Handles sidebar navigation, tooltips, and SPA-like routing.
 */

class SharedUI {
    constructor() {
        this.navItems = [
            { icon: 'layout-grid', label: 'Dashboard', id: 'dashboard', href: 'index.html' },
            { type: 'spacer', label: 'Algorithms' },
            { icon: 'database', label: 'Hash Storage', id: 'hashing', href: 'hashing.html' },
            { icon: 'list-ordered', label: 'Delivery Queue', id: 'queue', href: 'queue.html' },
            { icon: 'bar-chart-2', label: 'Scheduler', id: 'scheduler', href: 'scheduler.html' },
            { type: 'spacer', label: 'Inventory' },
            { icon: 'package', label: 'Products', id: 'inventory', href: 'inventory.html' },
            { icon: 'package-plus', label: 'Add Item', id: 'add-item', href: 'add-item.html' },
            { icon: 'receipt-text', label: 'Orders', id: 'orders', href: 'orders.html' },
            { icon: 'triangle-alert', label: 'Low Stock Alerts', id: 'alerts', href: 'alerts.html' },
        ];
    }

    renderSidebar(activeId) {
        const sidebar = document.createElement('aside');
        sidebar.className = 'sidebar';
        
        // Logo
        const logo = `
            <div class="logo-container">
                <a href="index.html">
                    <div class="logo-box">
                        <i data-lucide="box"></i>
                    </div>
                </a>
            </div>
        `;
        
        // Nav List
        const nav = `
            <nav class="nav-list">
                ${this.navItems.map(item => {
                    if (item.type === 'spacer') {
                        return `<span class="nav-section-label">${item.label}</span>`;
                    }
                    const activeClass = item.id === activeId ? 'active' : '';
                    return `
                        <a href="${item.href}" class="nav-item ${activeClass}" id="nav-${item.id}">
                            <i data-lucide="${item.icon}"></i>
                            <span class="nav-label">${item.label}</span>
                            <span class="tooltip">${item.label}</span>
                        </a>
                    `;
                }).join('')}
            </nav>
        `;

        // Profile Avatar at bottom
        const bottom = `
            <div class="sidebar-bottom" style="margin-top: 20px; text-align: center;">
                <div class="nav-item">
                    <i data-lucide="settings"></i>
                    <span class="tooltip">Settings</span>
                </div>
                <div style="margin-top: 15px; cursor: pointer;">
                    <img src="https://api.dicebear.com/9.x/notionists/svg?seed=SmartCell" 
                         style="width: 36px; height: 36px; border-radius: 12px; border: 2px solid var(--border);" alt="User" />
                </div>
            </div>
        `;

        sidebar.innerHTML = logo + nav + bottom;
        document.querySelector('.app-container')?.prepend(sidebar);
    }

    renderHeader(title, breadcrumb = 'Dashboard') {
        const header = document.createElement('header');
        header.className = 'header';
        
        header.innerHTML = `
            <div style="display: flex; align-items: center; gap: 24px;">
                <button style="color: var(--text-muted);"><i data-lucide="panel-left"></i></button>
                <div style="position: relative;">
                    <i data-lucide="search" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); font-size: 16px; color: var(--text-muted);"></i>
                    <input type="text" placeholder="Search..." style="background: #1a1a1a; border: none; padding: 8px 16px 8px 40px; border-radius: 20px; font-size: 14px; width: 240px; color: #fff;" />
                </div>
            </div>
            
            <div style="display: flex; align-items: center; gap: 20px;">
                <div style="display: none; @media (min-width: 768px) { display: flex; } align-items: center; gap: 8px; font-size: 12px; color: var(--text-muted);">
                    <span>SmartCell</span>
                    <span style="color: #333;">•</span>
                    <span class="badge badge-green" style="padding: 2px 8px;">${breadcrumb}</span>
                </div>
                
                <div style="width: 1px; height: 20px; background-color: var(--border);"></div>
                
                <button style="color: var(--text-muted);"><i data-lucide="sun"></i></button>
                
                <div style="position: relative; cursor: pointer; color: var(--text-muted);">
                    <i data-lucide="bell"></i>
                    <span style="position: absolute; top: -5px; right: -5px; background: #ef4444; color: #fff; font-size: 9px; min-width: 16px; height: 16px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800;">3</span>
                </div>
                
                <div style="position: relative; cursor: pointer; color: var(--text-muted);">
                    <i data-lucide="shopping-basket"></i>
                    <span style="position: absolute; top: -5px; right: -5px; background: var(--accent-green); color: #000; font-size: 9px; min-width: 16px; height: 16px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800;">7</span>
                </div>
            </div>
        `;
        
        document.querySelector('.main-wrapper')?.prepend(header);
    }

    initIcons() {
        if (window.lucide) {
            window.lucide.createIcons();
        }
    }
}

export const ui = new SharedUI();
