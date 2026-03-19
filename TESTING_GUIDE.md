# SmartCell IMS: Complete Testing Guide

This guide will walk you through every feature of the new C# backend and integrated modules.

## Phase 1: Dashboard & Backend Verification
1.  **Launch the App**: Run `dotnet run` in the `SmartCell` directory.
2.  **Open Dashboard**: Go to `http://localhost:5111/index.html` (or your active port).
3.  **Check Persistence**: Notice the "System Initialized" activity. Refresh the page—the activity should remain. 
4.  **Verify Database**: Open `data.json` on your computer. It should perfectly match the data shown on the screen.

## Phase 2: Inventory Management
1.  **Add Product**: Go to "Add Item" page. Add a new product (e.g., "Pixel 8 Pro").
2.  **Verify Save**: Go to the "Products" page. The new item should appear immediately.
3.  **Edit/Delete**: Use the actions in the Products table. Changes are synced instantly to the C# backend.

## Phase 3: Integrated Orders & Logistics
1.  **Create Order**: Go to the **Orders** page.
2.  **New Fields**: Click "Create Order". Note the new **Delivery Date** and **Distance (km)** fields.
3.  **Manual Dispatch**: After creating the order, it will appear as "Pending". It will **NOT** be in the queue yet.
4.  **Send to Queue**: Hover over the order row to see the **Truck Icon**. Click it.
5.  **Confirm Enqueue**: Check the **Delivery Queue** page. The order should now be listed there for fulfillment.

## Phase 4: Algorithm Scheduling
1.  **Sync Data**: Go to the **Scheduler** page.
2.  **Live Fetch**: Note that the table automatically shows your pending orders.
3.  **Labels**: Observe that "Burst Time" is now **Distance (Burst)** and "Arrival" is **Date (Arrival)**.
4.  **Run Simulation**: 
    - Select **SJF** (Shortest Job First) and click "Run Schedule".
    - The Gantt chart will sort your orders based on the **Distance** you entered during order creation.
    - Switch to **Priority** and run again.

## Phase 5: Complete the Cycle
1.  **Processing**: Go back to the **Delivery Queue**.
2.  **Status Flow**: Move an order from "Pending" to "In Progress" then "Delivered".
3.  **Final Check**: Return to the **Dashboard**. You should see the "Revenue" and "Recent Activity" update to reflect the completed delivery.

---
**Everything is now centralized on your C# server. Happy testing!**
