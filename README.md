# SmartCell: Intelligent Warehouse & Delivery Management System

SmartCell is an advanced, framework-less, monolithic web application engineered entirely from scratch. This project is purposely designed to synthesize concepts from **four separate computer science domains** into one cohesive, interactive codebase. 

## 🚀 The 4 Intersecting Domains

### 1. Operating Systems (Algorithms)
- **Concept:** CPU Process Scheduling Simulator.
- **Implementation:** The *Delivery Scheduler Dashboard* visualizes algorithmic sorting for delivery dispatches mimicking standard Operating Systems task-management architectures.
- **Algorithms Used:**
  - **SJF (Shortest Job First):** Minimizes collective waiting times by placing orders with smaller product quantities ahead of massive, time-consuming orders.
  - **Priority Scheduling:** High/Medium/Low priority statuses dictate the pre-eminence of a task execution, mirroring OS interrupt ranking systems. 

### 2. Data Structures (Logic)
- **Concept:** Information Storage & Retrieval Optimizations.
- **Implementation:** Custom-built logic deployed inside the specialized `.NET` services (`HashingService` and `QueueService`).
- **Algorithms Used:**
  - **Hash Table (Linear Probing):** Products are mapped to storage indices using an ASCII string-summation algorithm with a modulo 13 limit. Collisions are actively resolved via backend linear probing.
  - **FIFO Queue:** Customer dispatches are actively enforced using First-In-First-Out (FIFO) structural bounds, preventing arbitrary item pops directly from the C# List array mechanisms.

### 3. Web Development (Frontend)
- **Concept:** Modular DOM manipulations crossing over Asynchronous APIs.
- **Implementation:** An incredibly swift "Thin Client" frontend without React, Angular, or Vue.
- **Techniques Used:**
  - **ES6 API Modules:** Pure `import` and `export` to strictly organize features (`os/`, `data-structures/`, `core/`).
  - **Responsive Layouts:** Harnessing modern Flexbox and CSS Grids along with Tailwind for immaculate aesthetics. 
  - **Fetch API:** Handling Promises to communicate asynchronously strictly with the C# backend. 

### 4. .NET and C# (Backend)
- **Concept:** Robust, OOP-driven RESTful Systems Architecture.
- **Implementation:** Built on **.NET 8.0 Minimal APIs**.
- **Techniques Used:**
  - **Dependency Injection (DI):** Absolute eradication of "God Classes". Core file readers and logic servers are cleanly injected into specific endpoint Controllers.
  - **Repository Pattern / Service Layers:** The domains correctly separate HTTP networking logic (Controllers) from mathematical processing (Services). 
  - **JSON Serialization:** Emulating a NoSQL database seamlessly by reading/writing to local document storage on-the-fly (`data.json`) via `System.Text.Json`.

---

## 🛠️ Installation & Setup

Ensure that you have [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download) installed on your machine.

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/smartcell.git
   cd smartcell
   ```

2. **Run the Application:**
   ```bash
   dotnet run
   ```

3. **Access the Application:**
   Navigate natively to `http://localhost:5111` in your browser.

> Note: The app uses `data.json` recursively for storage. You don't need to link MS SQL Server or PostgreSQL down the line; everything functions out of the box dynamically!
