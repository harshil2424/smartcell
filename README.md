# SmartCell — Hospital Management System (Inventory & Algorithms)

SmartCell is a modern web-based inventory management and algorithmic simulation dashboard designed for hospital medical supplies. It combines robust inventory tracking with visual simulations of core data structures and scheduling algorithms.

## Key Modules

### 1. **Inventory Dashboard**
A premium dashboard overview showcasing total stock value, monthly orders, and delivery success rates. Includes category distribution and stock movement trends.

### 2. **Hash Storage Simulation**
Visualize how medical inventory is mapped to memory units using hashing algorithms.
- **Algorithm**: Sum of ASCII % 13.
- **Collision Handling**: Linear Probing (next available unit).

### 3. **Item Finder Pro**
A dedicated retrieval engine that demonstrates high-speed item discovery using derived hash addresses. Visualizes the look-up process and handling of memory collisions.

### 4. **Delivery Queue**
A FIFO (First-In-First-Out) task management system for processing supply orders. Simulates the flow from "Pending" to "In Progress" and "Delivered".

### 5. **Priority Scheduler**
An advanced simulation of CPU scheduling algorithms applied to inventory tasks. Includes Gantt chart visualizations for various priority-based execution models.

## Tech Stack
- **Backend**: .NET 8.0 Minimal APIs / C#
- **Frontend**: HTML5, Vanilla JavaScript, Tailwind CSS
- **Design**: Premium dark theme with glassmorphism and Lucide icons.
- **Data Store**: JSON-based persistent storage.

## Getting Started
1. Clone the repository.
2. Ensure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed.
3. Run the application:
   ```powershell
   dotnet run
   ```
4. Access the dashboard at `http://localhost:5xxx` (see terminal output for exact port).

---
© 2026 SmartCell Team
