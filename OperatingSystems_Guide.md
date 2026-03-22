# Operating Systems (OS) in SmartCell: Implementation Guide

This document provides a comprehensive explanation of how classic Operating System CPU scheduling algorithms are practically implemented and visualized within the SmartCell application.

---

## 1. Shortest Job First (SJF) Scheduling

### **Why is it used?**
In Operating Systems, the **SJF algorithm** is mathematically proven to provide the absolute minimum average waiting time for a given set of processes. In SmartCell, we map "CPU Burst Time" to an order's "Product Quantity." By processing the smallest, fastest orders first, we prevent massive bulk orders from permanently blocking quick, single-item deliveries (a problem known as the *Convoy Effect*).

### **Where is it located?**
*   **Algorithmic Logic & UI:** `wwwroot/js/os/scheduler-algorithms.js`

### **How does the code work?**
The CPU scheduler accesses the current array of pending orders and executes an **ascending sort** based exclusively on the quantity.

1.  **Sorting Logic (Burst Time tracking):** 
    ```javascript
    // We treat `order.qty` as the CPU Burst Execution Time
    let scheduled = [...pendingOrders].sort((a, b) => a.qty - b.qty);
    ```
2.  **Turnaround Time Calculation:** The algorithm dynamically loops through the newly sorted array to calculate exactly when each process (delivery) will finish executing, tracking the cumulative "CPU clock" time just like an OS generating a Gantt chart.
    ```javascript
    let currentTime = 0; // Baseline CPU clock 
    scheduled.forEach(order => {
        let burstTime = order.qty;
        let waitingTime = currentTime;
        currentTime += burstTime; // Simulate CPU clock advancing
        let turnaroundTime = waitingTime + burstTime;
    });
    ```

---

## 2. Priority Scheduling (Starvation / Preemption Simulation)

### **Why is it used?**
An Operating System cannot treat all processes equally—a critical system kernel interrupt MUST preempt a standard background document printing task. In SmartCell, this translates to VIP "High Priority" customer orders bypassing regular "Medium" or "Low" priority queue entries regardless of when they arrived.

### **Where is it located?**
*   **Algorithmic Logic & UI:** `wwwroot/js/os/scheduler-algorithms.js`

### **How does the code work?**
The scheduler converts string-based severity labels into absolute numerical weights that the CPU can mathematically rank.

1.  **Priority Mapping (User Mode vs Kernel Mode):** We declare a dictionary assigning strict numerical hardware-level importance.
    ```javascript
    const priorityWeight = { 'High': 1, 'Medium': 2, 'Low': 3 };
    ```
2.  **Execution Sorting:** The algorithm ranks the pending order array exactly according to the integer mapping. High priority (1) process IDs shift straight to the front of the CPU execution line.
    ```javascript
    let scheduled = [...pendingOrders].sort((a, b) => {
        return priorityWeight[a.priority] - priorityWeight[b.priority];
    });
    ```
3.  **Visual Processing:** The frontend immediately re-renders the DOM tables based exactly on this new array state, providing the user with a distinct visual representation of process tracking.

---

## Summary
By isolating the OS logic into a dedicated file (`scheduler-algorithms.js`), we clearly demonstrate the mapping between theoretical textbook OS concepts—like CPU *burst times*, *turnaround times*, and *preemptive prioritization*—and modern, real-world JavaScript DOM data arrays.
