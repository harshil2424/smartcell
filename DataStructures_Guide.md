# Data Structures in SmartCell: Implementation Guide

This document provides a comprehensive explanation of how, where, and why specific Data Structures are utilized within the SmartCell application. It is designed to explain the core logic bridging the C# backend and JavaScript frontend.

---

## 1. Hash Table (With Linear Probing)

### **Why is it used?**
In standard arrays, searching for a specific inventory item by its name or SKU requires a linear time complexity of **O(n)**. By implementing a Hash Table, we map the item's custom unique key directly to an array index, allowing us to achieve an average lookup, insertion, and deletion time complexity of **O(1)**. 

### **Where is it located?**
*   **Mathematical Logic (Backend):** `Services/DataStructures/HashingService.cs`
*   **State & Endpoints (Backend):** `Controllers/DataStructuresController.cs`
*   **Visual Representation (Frontend):** `wwwroot/js/data-structures/hashing.js`

### **How does the code work?**
The hashing methodology is built around **Sum of ASCII Modulo Division** and resolves collisions via **Linear Probing**.

1.  **Hash Calculation:** When an item is selected for hashing, the C# service takes its `Name` (e.g., "MacBook") and iterates through every character, summing their ASCII integer values.
    ```csharp
    int asciiSum = 0;
    foreach (char c in key) asciiSum += (int)c;
    ```
2.  **Modulo operation:** The table size is deliberately set to `13`. Prime numbers are mathematically proven to result in fewer collisions when used as the divisor. The initial index is calculated as:
    ```csharp
    int initialIndex = asciiSum % 13;
    ```
3.  **Collision Resolution (Linear Probing):** If the calculated index is already populated by another product (a Hash Collision), the code enters a `while` loop, stepping forward by `1` index until it finds an absolutely empty `null` slot, wrapping around the end of the array using modulo arithmetic:
    ```csharp
    while (data.HashingTable[actualIndex] != null && steps < 13)
    {
        actualIndex = (actualIndex + 1) % 13; // Stepping forward +1
        steps++;
    }
    ```
4.  **Client-Server Handoff:** The C# service returns a detailed `HashResult` object containing the `initialIndex`, `finalIndex`, and the exact probe steps taken. The `hashing.js` file then loops over this metadata object to visually animate the exact steps the backend took.

---

## 2. Queue (Strict FIFO algorithm)

### **Why is it used?**
To manage active delivery operations. The **FIFO (First-In, First-Out)** principle is the fundamentally correct operational logic for order management—the first customer who places an order must naturally be the very first one whose order is processed and shipped to guarantee fairness.

### **Where is it located?**
*   **Mathematical Logic (Backend):** `Services/DataStructures/QueueService.cs`
*   **State & Endpoints (Backend):** `Controllers/DataStructuresController.cs`
*   **Visual Representation (Frontend):** `wwwroot/js/data-structures/queue.js`

### **How does the code work?**
The generic List dynamically acts as an active contiguous memory Queue. The constraints of a queue allow mutations on ONLY the front (Dequeue) or the rear (Enqueue).

1.  **Enqueueing (Insertion at Rear):** When a new order is received, it is blindly appended to the tail of the array list without displacing any active elements.
    ```csharp
    data.DeliveryQueue.Add(item); // Pushed exactly to the end
    ```
2.  **Dequeueing (Deletion at Front):** When the warehouse worker clicks "Process Next", they are not allowed to select *which* order to process. The C# backend strictly targets the `[0]` index (the absolute front of the line). 
    ```csharp
    var o = data.DeliveryQueue[0]; // Strict FIFO: isolate the front
    data.DeliveryQueue.RemoveAt(0); // Pop it out
    data.QueueInProgress.Add(o); // Transition to next state
    ```
3.  By tightly isolating this logic inside `QueueService.cs`, the integrity of the data structure is preserved across the entire backend ecosystem, ensuring no rogue client frontend can arbitrarily skip the line.

---

## Summary
By enforcing **Separation of Concerns**, the logic dictating *how* the arrays and queues operate is safely hidden inside the fully compiled **C# .NET Minimal API Services**. The frontend JavaScript exists purely as a "Thin Client"—it contains zero mathematical intelligence, acting only to request JSON data and visually update DOM coordinate boundaries for the teacher presentation!
