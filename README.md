# 🏦 Home Banking API

Una API RESTful robusta y segura para un sistema de Home Banking, desarrollada con **.NET 10** y orientada a buenas prácticas de ingeniería de software. 

Este proyecto fue diseñado con un fuerte enfoque en la seguridad transaccional, el rendimiento y la escalabilidad, aplicando patrones de arquitectura limpia y principios SOLID.

## 🏗️ Arquitectura y Principios SOLID

El código base está estructurado para ser altamente mantenible y testeable, respetando los estándares de la industria:

* **S - Single Responsibility Principle (SRP):** Desacoplamiento total del código. Los Controladores (`Controllers`) operan como una capa encargada exclusivamente de gestionar peticiones HTTP. Toda la lógica matemática, validaciones de negocio y reglas financieras viven aisladas en la **Capa de Servicios** (`Services`).
* **D - Dependency Inversion Principle (DIP):** Los controladores no dependen de implementaciones concretas, sino de abstracciones (ej. `ITransactionService`). Esto se gestiona a través del contenedor de **Inyección de Dependencias** nativo de .NET, logrando un sistema modular y preparado para la implementación de pruebas unitarias.

## ✨ Características Principales (Features)

* **🔐 Seguridad y Autenticación:** * Implementación de **Tokens JWT** (JSON Web Tokens) para proteger endpoints privados.
  * Hashing criptográfico unidireccional de contraseñas utilizando **BCrypt**, garantizando que no existan credenciales en texto plano en la base de datos.
  * Defensa contra escalada de privilegios (validación estricta de propiedad de cuentas cruzando datos del Token y la BD).
* **🛡️ Validación Robusta:** Uso de **Data Annotations** en los DTOs para interceptar y rechazar automáticamente operaciones fraudulentas (ej. intentar depósitos negativos o nulos), devolviendo errores `400 Bad Request` antes de sobrecargar la lógica de negocio.
* **⚡ Rendimiento Optimizado:** Paginación dinámica en el historial de transacciones utilizando consultas diferidas de Entity Framework (`Skip` y `Take`), previniendo cuellos de botella en la memoria del servidor ante historiales masivos.
* **⚙️ Gestión de Base de Datos:** Migraciones automatizadas y **Data Seeding** inicial para inyectar usuarios y cuentas de prueba directamente en MySQL.
* **🌐 CORS Configurado:** Políticas de seguridad preparadas para integraciones fluidas con aplicaciones Front-End (React, Vite, etc.).

## 🛠️ Stack Tecnológico

* **Framework:** .NET 10 (C#)
* **ORM:** Entity Framework Core
* **Base de Datos:** MySQL
* **Seguridad:** JWT Authentication, BCrypt.Net-Next
* **Documentación:** Swagger / OpenAPI
* **Servidor Web:** Kestrel