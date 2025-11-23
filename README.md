# 🏥 Pet Hospital API

API project designed to manage pets, owners, appointments, and hospital operations. The architecture follows a clean, modular structure with clear separation of concerns using Models, DataManager, Repositories, and API Controllers.

---

## 📌 **Project Overview**

PetHospitalAPI is a backend Web API developed using **C#** and **ASP.NET Core**. It provides CRUD operations for pets, appointments, and related entities commonly used in pet hospital systems.

The project architecture is structured to be clean, scalable, and easy to maintain, applying **Repository Pattern** and layered application design.

---

## 🧱 **Architecture Structure**

```
PetHospitalAPI.sln
│
├── Models             → Domain entities (Pet, Owner, Appointment ...)
├── DataManager        → Data access layer (DbContext / Data Handler)
├── Repositories       → Repository Interfaces + Implementations
├── Utility            → Helpers, shared utilities
└── PetHospitalApi     → Web API (Controllers, DI, startup)
```

---

## 🔹 **1. Models (Domain Layer)**

The **Models** folder contains all entity classes that represent the data structure in the application, such as:

* Pet
* Owner
* Appointment
* Service
* Invoice

These models map directly to database tables.

---

## 🔹 **2. DataManager (Database Access Layer)**

Responsible for:

* Managing database context
* Defining DbSets
* Handling Entity Framework operations

This layer acts as the entry point for database interaction.

---

## 🔹 **3. Repository Pattern**

This project uses the Repository Pattern to isolate data access from business logic.

### **Repository Interface Example:**

```
public interface IPetRepository
{
    Task<Pet> GetByIdAsync(int id);
    Task<IEnumerable<Pet>> GetAllAsync();
    Task AddAsync(Pet pet);
    void Update(Pet pet);
    void Delete(Pet pet);
}
```

### **Repository Implementation Example:**

```
public class PetRepository : IPetRepository
{
    private readonly ApplicationDbContext _context;

    public PetRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Pet> GetByIdAsync(int id) => await _context.Pets.FindAsync(id);

    public async Task<IEnumerable<Pet>> GetAllAsync() => await _context.Pets.ToListAsync();

    public async Task AddAsync(Pet pet) => await _context.Pets.AddAsync(pet);

    public void Update(Pet pet) => _context.Pets.Update(pet);

    public void Delete(Pet pet) => _context.Pets.Remove(pet);
}
```

---

## 🔹 **4. Web API Layer (Controllers)**

Controllers receive HTTP requests, call services/repositories, and return responses.

Example controller actions include:

* `GET /api/pets` – Get all pets
* `GET /api/pets/{id}` – Get pet by ID
* `POST /api/pets` – Add new pet
* `PUT /api/pets/{id}` – Update pet
* `DELETE /api/pets/{id}` – Remove pet

---

## 🔹 **5. Dependency Injection (DI)**

Repositories are registered in the DI container:

```
services.AddScoped<IPetRepository, PetRepository>();
services.AddScoped<IAppointmentRepository, AppointmentRepository>();
```

This ensures controllers receive the required dependencies automatically.

---

## 🔧 **Technologies Used**

* C#
* ASP.NET Core Web API
* Repository Pattern
* Dependency Injection
* Entity Framework Core (if configured)
* Git/GitHub

---

## 🚀 **How to Run the Project**

1. Clone the repository:

```
git clone <repo-url>
```

2. Restore dependencies:

```
dotnet restore
```

3. Update database (if migrations exist):

```
dotnet ef database update
```

4. Run the API:

```
dotnet run
```

5. The API will run on:

```
https://localhost:5001
http://localhost:5000
```

---

## 📌 **Next Improvements**

* Add DTO mapping using AutoMapper
* Add Service Layer for business logic
* Add Authentication (JWT)
* Add Logging & Exception Handling middleware
* Implement Unit Tests

---

## 🐾 **Conclusion**

This project provides a clean, structured API architecture applying best practices with repository pattern and layered design. It is suitable for growth, maintainability, and interview presentation.
