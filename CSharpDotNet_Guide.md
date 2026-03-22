# C# and .NET in SmartCell: Implementation Guide

This document extensively details the Object-Oriented Programming (OOP) architectures, design patterns, and framework-specific features utilized within the **.NET 8.0 Minimal API** backend of the SmartCell project.

---

## 1. Dependency Injection (DI) & Inversion of Control (IoC)

### **Why is it used?**
Dependency Injection is the absolute cornerstone of modern C# application architecture. Without DI, a controller would have to manually instantiate its own services, creating "tight coupling" where components are permanently fused together and impossible to unit test. By injecting *Interfaces* rather than solid classes, the codebase is modular, highly scalable, and loosely coupled.

### **Where is it located?**
*   **Registration:** `Program.cs` 
*   **Injection:** The constructor functions (`public DataStructuresController(IHashingService hashingService)`) of every Controller and Service.

### **How does the code work?**
1.  **The IoC Container:** In `Program.cs`, we tell the .NET engine exactly how to resolve dependencies. 
    ```csharp
    builder.Services.AddSingleton<IJsonStorageService, JsonStorageService>();
    builder.Services.AddScoped<IHashingService, HashingService>();
    ```
2.  By defining the core file-system storage as a **Singleton**, we guarantee that whether the `OrderService` or the `InventoryService` asks for data, .NET provides the exact same, synchronized, thread-safe tracking object to both.
3.  The controllers never use the `new` keyword to create services. They simply request the interface `IHashingService` in their constructor, and .NET automatically injects the active instance at runtime.

---

## 2. Separation of Concerns (Service/Repository Pattern)

### **Why is it used?**
To brutally avoid the "God Class" anti-pattern. A Controller’s only responsibilities should be receiving a network HTTP request, routing variables, and returning an HTTP Status (200 OK, 400 Bad Request). If business logic or mathematical algorithms are inside a Controller, the architecture violates the **Single Responsibility Principle** of SOLID OOP design.

### **Where is it located?**
*   **Controllers:** the `Controllers/` directory manages network traffic routes.
*   **Services:** The `Services/` directory (specialized by domain: `Core/`, `Inventory/`, `Orders/`, `DataStructures/`) manages the actual brains of the operation.

### **How does the code work?**
When the frontend clicks "Enqueue Order":
1.  The `[HttpPost("queue/enqueue")]` method in `DataStructuresController.cs` intercepts the raw JSON payload and automatically deserializes it into a strongly typed C# `QueueItem` Model.
2.  The Controller performs **zero logic**—it immediately passes the object to `_queueService.EnqueueAsync(item)`.
3.  The Service processes the queue rules, assigns IDs, generates dates, and saves the data.
4.  The Service returns control to the Controller, which simply returns `Ok()`.

---

## 3. Asynchronous Programming (Async / Await)

### **Why is it used?**
To prevent Thread Blocking. When the C# server is reading or writing to the physical `data.json` file on the hard drive, that I/O operation takes significantly longer than CPU math. By making the operations asynchronous, the C# thread is instantly released to serve other customer HTTP requests while waiting for the hard drive to finish spinning.

### **Where is it located?**
*   Pervasive across the entire backend: `Task<IActionResult>`, `await _storage.GetStoreDataAsync()`.

### **How does the code work?**
```csharp
public async Task<HashResult> HashItemAsync(long itemId) {
    var data = await _storage.GetStoreDataAsync(); // Thread pauses and yields back to the OS pool here!
    // ... Math execution resumes once data is fully loaded into RAM
}
```

---

## 4. Custom NoSQL Implementation via JSON Serialization

### **Why is it used?**
Instead of the massive overhead of connecting to an external SQL Server database, SmartCell implements its own lightweight, persistent Document Database (NoSQL style) utilizing native C# serialization.

### **Where is it located?**
*   `Services/Core/JsonStorageService.cs`

### **How does the code work?**
1.  Using `System.Text.Json`, the C# backend opens a raw `FIleStream` directly to `data.json`.
2.  `JsonSerializer.DeserializeAsync<StoreData>()` actively reads the untyped byte strings from the file and instantly magically maps them to nested, strictly-typed C# class hierarchies defined in `Models/StoreData.cs` (e.g. converting a string `[ ]` into a `List<Order>`). 
3.  After the services dynamically manipulate the Objects in RAM, `SerializeAsync` overwrites the file with stringified JSON to maintain persistence across server reboots.

---

## Summary
The SmartCell .NET backend is a textbook display of enterprise-grade **Object-Oriented Architecture**. By strictly adhering to SOLID principles, utilizing extensive interface-based **Dependency Injection**, isolating logic into specialized **Services**, and maintaining an asynchronous, non-blocking thread architecture, the backend easily scales from a single student project to a massive production warehouse framework.
