# Plan de Proyecto: Plataforma de Publicación de Artículos — Itelligent

## 1. Visión Arquitectónica (Clean Architecture)

Para asegurar un código limpio, escalable y mantenible, la solución se estructurará en cuatro capas principales siguiendo los principios de Clean Architecture.

* **`Itelligent.Domain` (Core):** Contendrá las entidades del negocio (`User`, `Article`, `Comment`), enumeradores (`Role: Admin, User`) y las interfaces de los repositorios. Cero dependencias externas.
* **`Itelligent.Application`:** Casos de uso de la aplicación. Aquí residirán los DTOs, la configuración de **AutoMapper**, las interfaces de servicios externos (ej. `IJwtProvider`) y la lógica de negocio (patrón de Servicios o CQRS con MediatR).
* **`Itelligent.Infrastructure`:** Implementación de acceso a datos. Configuración de **Entity Framework Core** para **SQLite**, implementaciones de los repositorios, y el servicio de generación de tokens JWT.
* **`Itelligent.Presentation` (Web/API):** Capa de entrada. Contendrá:
  * Controladores de API REST (`/api/auth`, `/api/articles`).
  * Vistas Razor y controladores MVC para el Frontend.
  * Middleware global para el manejo de excepciones y logs.
  * Configuración de **Swagger**.
* **`Itelligent.Tests`:** Proyectos de pruebas unitarias divididos por capas, utilizando **xUnit** y **Moq**.

---

## 2. Stack Tecnológico

| Categoría      | Tecnología / Librería                                        |
|----------------|--------------------------------------------------------------|
| **Backend**    | .NET 8 SDK / net8.0 (C#)                                     |
| **Arquitectura** | Clean Architecture                                         |
| **Base de Datos** | SQLite (Code-First con Entity Framework Core 9.x)         |
| **ORM**        | Entity Framework Core + LINQ                                 |
| **Mapeo**      | AutoMapper 12.x (via Extensions package)                     |
| **Seguridad**  | JWT Bearer Authentication + BCrypt.Net-Next (hashing)        |
| **Frontend**   | HTML5, Bootstrap 5, Razor Views, jQuery (AJAX + DOM)         |
| **Pruebas**    | xUnit, Moq, EF Core InMemory                                 |
| **Docs API**   | Swagger / OpenAPI (Swashbuckle.AspNetCore)                   |
| **Extras opcionales** | OData — omitido (marcado como opcional, agrega complejidad innecesaria) |

> **Nota de implementación:** el proyecto usa SDK .NET 8 (`net8.0`). Los paquetes EF Core 9.x son compatibles con net8.0. `Microsoft.AspNetCore.Authentication.JwtBearer` debe ser versión 8.x y `Swashbuckle.AspNetCore` versión 6.x (las versiones 9.x de ambos requieren .NET 9+). El template de Presentation es `dotnet new mvc` (no `webapi`) porque el proyecto es híbrido API + Razor Views.

---

## 3. Modelos de Dominio (Entidades)

### `User`
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; } // Admin | User
    public ICollection<Article> Articles { get; set; }
    public ICollection<Comment> Comments { get; set; }
}
```

### `Article`
```csharp
public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Content { get; set; }
    public DateTime PublishedAt { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; }
    public ICollection<Comment> Comments { get; set; }
}
```

### `Comment`
```csharp
public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime PublishedAt { get; set; }
    public int ArticleId { get; set; }
    public Article Article { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}
```

---

## 4. Diseño de APIs

### API 1 — Autenticación y Usuarios (`/api/auth`)

| Método | Endpoint            | Auth requerida | Descripción                                   |
|--------|---------------------|----------------|-----------------------------------------------|
| POST   | `/api/auth/register` | No            | Registra un nuevo usuario                     |
| POST   | `/api/auth/login`    | No            | Autentica y retorna un token JWT              |
| GET    | `/api/auth/me`       | Sí (Bearer)   | Retorna la información del usuario autenticado|

**Consideraciones:**
* Implementar JWT Bearer para autenticación y autorización.
* Los usuarios pueden tener roles: `Admin` o `User`.
* Hashear contraseñas con **BCrypt.Net-Next** (interfaz `IPasswordHasher` en Application, implementación `BcryptPasswordHasher` en Infrastructure).
* El registro público (`POST /api/auth/register`) siempre asigna rol `User`. El rol `Admin` se asigna únicamente por seed de base de datos.
* La respuesta de login/register devuelve `AuthResponseDto { Token, User }` donde `User` incluye `{ Id, Username, Role }`.
* El token JWT se puede enviar tanto por header `Authorization: Bearer` (para AJAX) como por cookie `jwt_token` (para soporte MVC).

### API 2 — Artículos y Comentarios (`/api/articles`)

| Método | Endpoint                        | Auth requerida      | Descripción                                    |
|--------|---------------------------------|---------------------|------------------------------------------------|
| GET    | `/api/articles`                 | No                  | Lista artículos con paginación                 |
| GET    | `/api/articles/{id}`            | No                  | Detalle de artículo y sus comentarios          |
| POST   | `/api/articles`                 | Sí (Bearer)         | Crea un nuevo artículo                         |
| PUT    | `/api/articles/{id}`            | Sí (autor o admin)  | Edita un artículo                              |
| DELETE | `/api/articles/{id}`            | Sí (autor o admin)  | Elimina un artículo                            |
| POST   | `/api/articles/{id}/comments`   | Sí (Bearer)         | Agrega un comentario a un artículo             |

**Consideraciones:**
* Enfoque **Code-First** con EF Core + SQLite.
* **AutoMapper** para transformar entidades en DTOs de respuesta.
* Validar que solo el autor o un Admin pueda editar/eliminar artículos (retorna `403 Forbidden`).
* La paginación usa parámetros `page` (default 1) y `pageSize` (default 10, máx 50). La respuesta incluye `{ Items, TotalCount, Page, PageSize, TotalPages, HasPrevious, HasNext }`.
* `GET /api/articles/{id}` devuelve `ArticleDetailDto` con lista de comentarios incluidos.
* La UI oculta los botones Editar/Eliminar en la vista de gestión cuando el artículo no pertenece al usuario activo (verificación client-side con `currentUser().id`).

---

## 5. Pantallas (Frontend Razor + Bootstrap + jQuery)

| Pantalla                      | Descripción                                                                                       |
|-------------------------------|---------------------------------------------------------------------------------------------------|
| **Login**                     | Formulario con Usuario y Contraseña. Botón "Registrate" despliega modal/popup de registro.       |
| **Registro**                  | Modal con campos: Usuario, Contraseña, Confirmar contraseña. Botones: Registrar / Cancelar.      |
| **Últimos artículos** (Home)  | Feed paginado con Título, Autor, Fecha y resumen. Botones "Página anterior / Página siguiente".  |
| **Detalle de artículo**       | Contenido completo. Toggle jQuery "Mostrar/Ocultar comentarios". Formulario AJAX para comentar.  |
| **Gestión de artículos**      | Tabla con Autor, Título, Fecha. Acciones: Ver detalle, Editar, Eliminar. Botón "Nuevo artículo". |

---

## 6. Product Backlog & Estrategia Incremental

### Epic 1: Foundation & Setup
**Objetivo:** Establecer la estructura base, la base de datos y la arquitectura del proyecto.

* **US 1.1 — Estructura de la Solución:** Como desarrollador, quiero crear los proyectos de la solución bajo Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Presentation`, `Tests`) para organizar el código correctamente.
* **US 1.2 — Configuración de Base de Datos:** Como desarrollador, quiero configurar EF Core con **SQLite**, crear las entidades base (`User`, `Article`, `Comment`) y generar la primera migración Code-First.
* **US 1.3 — Infraestructura Transversal:** Como desarrollador, quiero configurar AutoMapper, el Middleware de manejo de errores global y Swagger para documentar las APIs.

### Epic 2: Autenticación y Gestión de Usuarios (API 1)
**Objetivo:** Implementar la seguridad y el control de acceso.

* **US 2.1 — API de Registro y Login:** Como usuario anónimo, quiero endpoints (`POST /api/auth/register`, `POST /api/auth/login`) para poder crear una cuenta y obtener un token JWT.
* **US 2.2 — Autorización e Identidad:** Como sistema, quiero validar los tokens JWT en cada petición protegida y diferenciar roles (Admin vs User) para restringir el acceso a ciertos endpoints (ej. `GET /api/auth/me`).
* **US 2.3 — UI de Autenticación:** Como usuario, quiero ver una pantalla de Login y un modal Bootstrap/jQuery para registrarme en el sistema.

### Epic 3: Core de Artículos (API 2 — Parte 1)
**Objetivo:** Permitir la creación y administración de contenido.

* **US 3.1 — CRUD de Artículos (API):** Como autor/admin, quiero endpoints protegidos para crear (`POST`), editar (`PUT`) y eliminar (`DELETE`) mis artículos, validando que solo el dueño o un admin pueda modificarlos.
* **US 3.2 — UI de Gestión de Artículos:** Como autor, quiero una pantalla de administración (tabla HTML con Bootstrap) donde pueda ver mis artículos y tenga botones para crear, editar y eliminar.
* **US 3.3 — Pruebas Unitarias (Artículos):** Como desarrollador, quiero escribir pruebas con xUnit y Moq para la lógica de creación y validación de permisos de artículos.

### Epic 4: Portal Público y Participación (API 2 — Parte 2)
**Objetivo:** Visualización del contenido y sistema de comentarios.

* **US 4.1 — Catálogo de Artículos Paginado:** Como visitante, quiero un endpoint (`GET /api/articles`) que devuelva la lista de artículos ordenados por fecha con paginación (y opcionalmente OData).
* **US 4.2 — UI de Últimos Artículos:** Como visitante, quiero ver la página principal con un feed responsive de los últimos artículos (título, resumen, autor, fecha) y botones de paginación.
* **US 4.3 — API de Comentarios:** Como usuario autenticado, quiero un endpoint (`POST /api/articles/{id}/comments`) para agregar comentarios a un artículo.
* **US 4.4 — Detalle de Artículo y UI de Comentarios:** Como visitante, quiero leer el artículo completo. Como usuario autenticado, quiero un toggle jQuery "Mostrar/Ocultar comentarios" y un formulario AJAX para publicar comentarios.

---

## 6b. Decisiones de Implementación (no estaban en el plan original)

### Gestión de sesión client-side (`wwwroot/js/auth.js`)
No estaba planificado en el diseño original. Se implementó un archivo JS global cargado en el layout que:
- En cada carga de página llama a `GET /api/auth/me` para validar el token (si existe en `localStorage`).
- Si el servidor retorna `401`, limpia automáticamente `localStorage` y la cookie.
- Renderiza el estado del navbar (usuario, botón Gestión, Cerrar sesión) según el resultado.
- Expone funciones globales: `getToken()`, `authHeader()`, `isLoggedIn()`, `currentUser()`, `appLogout()`.

### Logout completamente client-side
El plan original incluía una ruta server-side `/auth/logout`. En la implementación final, el logout es **puramente client-side** mediante `appLogout()`:
1. Elimina `jwt_token` y `jwt_user` de `localStorage`.
2. Invalida la cookie `jwt_token`.
3. Redirige a `/auth/login`.

Esto resuelve el problema donde el logout server-side borraba la cookie pero no `localStorage`, provocando que al recargar la página el token siguiera siendo válido y el navbar permaneciera autenticado.

### Interfaz `IPasswordHasher` en Application
Para cumplir Clean Architecture, Application no puede referenciar directamente `BCrypt.Net-Next` (que es un paquete de Infrastructure). Se introdujo la interfaz `IPasswordHasher` en `Application/Interfaces/` implementada por `BcryptPasswordHasher` en `Infrastructure/Security/`.

### Seed de Admin con hash estático
El hash de la contraseña del Admin se pre-computó estáticamente (`$2b$11$...`) para evitar que EF Core regenere la migración en cada ejecución (BCrypt genera un salt distinto cada vez).

### Excepciones de dominio tipadas
Se agregó `Application/Exceptions/` con:
- `ForbiddenException` → el middleware retorna `403 Forbidden`.
- `NotFoundException` → el middleware retorna `404 Not Found`.

El `ExceptionHandlingMiddleware` mapea cada tipo de excepción a su código HTTP correspondiente, devolviendo JSON `{ "error": "mensaje" }`.

### Auto-migración al iniciar
`Program.cs` ejecuta `db.Database.Migrate()` al arrancar la aplicación, de modo que la base de datos se crea/actualiza automáticamente sin necesidad de ejecutar `dotnet ef database update` manualmente.

### DTOs adicionales no planificados
| DTO | Descripción |
|---|---|
| `AuthResponseDto` | Envuelve `Token` + `UserDto` en la respuesta de login/register |
| `ArticleDetailDto` | Hereda de `ArticleDto` y agrega `List<CommentDto> Comments` |
| `PagedArticlesDto` | Envuelve la lista paginada con metadatos (`TotalCount`, `TotalPages`, `HasPrevious`, `HasNext`) |

### Estructura real de carpetas (difiere de la propuesta original)
```
Itelligent.Presentation/
├── Controllers/
│   ├── Api/
│   │   ├── AuthController.cs       ← /api/auth/*
│   │   └── ArticlesController.cs   ← /api/articles/*
│   └── Mvc/
│       ├── HomeMvcController.cs    ← / (Home)
│       ├── AuthMvcController.cs    ← /auth/*
│       └── ArticleMvcController.cs ← /article/*
├── Views/
│   ├── Home/         ← HomeMvcController → Index.cshtml
│   ├── AuthMvc/      ← AuthMvcController → Login.cshtml
│   └── ArticleMvc/   ← ArticleMvcController → Manage.cshtml, Detail.cshtml
└── wwwroot/js/
    ├── site.js
    └── auth.js       ← gestión global de sesión (nuevo)
```
> Las carpetas de vistas se nombran por el nombre del controlador MVC (sin sufijo `Controller`). Por eso `AuthMvcController` → carpeta `AuthMvc/` y `ArticleMvcController` → carpeta `ArticleMvc/`.

---

## 7. Guía de Inicio Rápido (Comandos CLI)

```bash
# Crear la solución
dotnet new sln -n ItelligentPlatform

# Crear los proyectos
dotnet new classlib -n Itelligent.Domain
dotnet new classlib -n Itelligent.Application
dotnet new classlib -n Itelligent.Infrastructure
dotnet new mvc -n Itelligent.Presentation        # mvc, no webapi — proyecto híbrido
dotnet new xunit -n Itelligent.Tests

# Agregar proyectos a la solución
dotnet sln add Itelligent.Domain/Itelligent.Domain.csproj
dotnet sln add Itelligent.Application/Itelligent.Application.csproj
dotnet sln add Itelligent.Infrastructure/Itelligent.Infrastructure.csproj
dotnet sln add Itelligent.Presentation/Itelligent.Presentation.csproj
dotnet sln add Itelligent.Tests/Itelligent.Tests.csproj

# Referencias entre proyectos (Clean Architecture)
dotnet add Itelligent.Application/Itelligent.Application.csproj reference Itelligent.Domain/Itelligent.Domain.csproj
dotnet add Itelligent.Infrastructure/Itelligent.Infrastructure.csproj reference Itelligent.Application/Itelligent.Application.csproj
dotnet add Itelligent.Presentation/Itelligent.Presentation.csproj reference Itelligent.Application/Itelligent.Application.csproj
dotnet add Itelligent.Presentation/Itelligent.Presentation.csproj reference Itelligent.Infrastructure/Itelligent.Infrastructure.csproj

# Referencias adicionales para Tests
dotnet add Itelligent.Tests/Itelligent.Tests.csproj reference Itelligent.Application/Itelligent.Application.csproj
dotnet add Itelligent.Tests/Itelligent.Tests.csproj reference Itelligent.Infrastructure/Itelligent.Infrastructure.csproj

# Paquetes NuGet
dotnet add Itelligent.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add Itelligent.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add Itelligent.Infrastructure package BCrypt.Net-Next               # hashing contraseñas
dotnet add Itelligent.Infrastructure package System.IdentityModel.Tokens.Jwt
dotnet add Itelligent.Infrastructure package Microsoft.Extensions.Configuration.Abstractions
dotnet add Itelligent.Application package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add Itelligent.Presentation package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add Itelligent.Presentation package Swashbuckle.AspNetCore          # Swagger UI
dotnet add Itelligent.Presentation package Microsoft.EntityFrameworkCore.Design  # migrations
dotnet add Itelligent.Tests package Moq
dotnet add Itelligent.Tests package xunit
dotnet add Itelligent.Tests package Microsoft.NET.Test.Sdk
dotnet add Itelligent.Tests package Microsoft.EntityFrameworkCore.InMemory  # tests de repositorio

# Primera migración EF Core (desde Itelligent.Presentation como startup project)
dotnet ef migrations add InitialCreate --project Itelligent.Infrastructure --startup-project Itelligent.Presentation
dotnet ef database update --project Itelligent.Infrastructure --startup-project Itelligent.Presentation
```

### Configuración del DbContext con SQLite (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=itelligent.db"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "ItelligentPlatform",
    "Audience": "ItelligentUsers",
    "ExpirationMinutes": 60
  }
}
```

### Registro del DbContext en `Program.cs`
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## 8. Estructura de Carpetas Sugerida

```
ItelligentPlatform/
├── Itelligent.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Article.cs
│   │   └── Comment.cs
│   ├── Enums/
│   │   └── Role.cs
│   └── Interfaces/
│       ├── IUserRepository.cs
│       ├── IArticleRepository.cs
│       └── ICommentRepository.cs
├── Itelligent.Application/
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Articles/
│   │   └── Comments/
│   ├── Interfaces/
│   │   └── IJwtProvider.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   └── Services/
│       ├── AuthService.cs
│       ├── ArticleService.cs
│       └── CommentService.cs
├── Itelligent.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   ├── ArticleRepository.cs
│   │   └── CommentRepository.cs
│   └── Security/
│       └── JwtProvider.cs
├── Itelligent.Presentation/
│   ├── Controllers/
│   │   ├── Api/
│   │   │   ├── AuthController.cs
│   │   │   └── ArticlesController.cs
│   │   └── Mvc/
│   │       ├── HomeController.cs
│   │       └── ArticleController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Views/
│       ├── Home/          # Últimos artículos
│       ├── Article/       # Detalle + Gestión
│       └── Auth/          # Login + Registro (modal)
└── Itelligent.Tests/
    ├── Application/
    │   └── ArticleServiceTests.cs
    └── Infrastructure/
        └── ArticleRepositoryTests.cs
```

---

## 9. Criterios de Aceptación Generales

* Todas las rutas protegidas deben retornar `401 Unauthorized` si no se envía token JWT válido.
* Solo el autor o un usuario con rol `Admin` puede editar o eliminar un artículo (retorna `403 Forbidden` en caso contrario).
* La paginación del catálogo de artículos debe soportar parámetros `page` y `pageSize`.
* Los comentarios por defecto deben estar ocultos en la vista de detalle; el toggle jQuery cambia el texto del botón entre "Mostrar comentarios" y "Ocultar comentarios".
* El formulario de comentarios debe enviarse vía AJAX sin recargar la página.
* La base de datos SQLite (`itelligent.db`) se crea automáticamente al arrancar la aplicación (auto-migrate en `Program.cs`).

### Criterios adicionales implementados (no estaban en el plan original)

* El registro público (`/api/auth/register`) siempre crea usuarios con rol `User`. El rol `Admin` solo se asigna por seed.
* El usuario Admin por defecto (`admin` / `Admin123!`) se crea automáticamente en la migración inicial via `HasData()`.
* El logout limpia `localStorage` y la cookie del lado del cliente antes de redirigir; no hay ruta server-side de logout.
* `auth.js` verifica en cada carga de página si el token sigue siendo válido usando `GET /api/auth/me`. Si el servidor retorna `401`, la sesión se limpia automáticamente.
* Los botones Editar/Eliminar en la pantalla de Gestión solo se renderizan cuando `currentUser().id === article.authorId` o `currentUser().role === 'Admin'`.
* La vista de detalle muestra el contador de comentarios: `Comentarios (N)`, actualizado en tiempo real al publicar.
* Las pruebas unitarias cubren 9 casos: 5 en `ArticleServiceTests` (xUnit + Moq) y 4 en `ArticleRepositoryTests` (xUnit + EF InMemory).

---

## 10. Validaciones de campos implementadas

| Campo | Regla |
|---|---|
| `Username` (registro) | Mínimo 3 caracteres. Único en la base de datos. |
| `Password` (registro) | Mínimo 6 caracteres. |
| `ConfirmPassword` | Debe ser igual a `Password` (validación client-side y server-side). |
| `Article.Title` | Mínimo 3 caracteres. Máximo 200. Requerido. |
| `Article.Summary` | Mínimo 10 caracteres. Máximo 500. Requerido. |
| `Article.Content` | Mínimo 10 caracteres. Sin límite superior. Requerido. |
| `Comment.Text` | Mínimo 1 carácter. Máximo 1000. Requerido. |
| `page` (paginación) | Mínimo 1 (clampeado). |
| `pageSize` (paginación) | Entre 1 y 50 (clampeado). |
