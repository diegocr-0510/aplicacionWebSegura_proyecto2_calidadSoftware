# NovaTech Store

Sistema web para la gestión y consulta de productos tecnológicos, desarrollado con ASP.NET Core MVC y enfoque en seguridad.

---

## 📌 Descripción

NovaTech Store permite administrar productos informáticos y usuarios dentro de un entorno seguro, aplicando autenticación, control de acceso por roles y buenas prácticas de seguridad.

---

## 🛠️ Tecnologías

* ASP.NET Core MVC
* Entity Framework Core
* PostgreSQL
* ASP.NET Identity
* Bootstrap

---

## 🔐 Seguridad

* Autenticación con Identity
* Contraseñas encriptadas
* Control de acceso por roles (RBAC)
* Protección CSRF
* Validaciones frontend y backend
* Prevención de SQL Injection

---

## 👥 Roles

* **SuperAdmin:** acceso total
* **Registrador:** gestiona productos
* **Auditor:** solo lectura

---

## 📦 Funcionalidades

* Login y logout
* CRUD de productos
* CRUD de usuarios
* Asignación de roles

---

## 🚀 Ejecución

```bash
git clone <URL_DEL_REPO>
cd Proyecto2Seguridad.Web
dotnet restore
dotnet ef database update
dotnet run
```

---

## 🔑 Usuario de prueba

Usuario: admin
Contraseña: Admin123*S

---

## 📊 Estado

✔ Productos
✔ Usuarios
✔ Roles
🚧 Auditoría
🚧 API JWT
🚧 Seguridad avanzada

---

## 📄 Proyecto académico

UTN - Seguridad en Sistemas
