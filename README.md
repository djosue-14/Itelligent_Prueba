# Itelligent — Plataforma de Publicación de Artículos

Plataforma web para publicar, leer y comentar artículos. Construida con .NET 10, Clean Architecture, SQLite, JWT y Bootstrap 5.

---

## Requisitos previos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- `dotnet-ef` instalado globalmente:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Cómo ejecutar el proyecto

```bash
# 1. Clonar o descargar el repositorio
cd ItelligentPlatform

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar la aplicación
dotnet run --project Itelligent.Presentation
```

La base de datos SQLite (`itelligent.db`) se crea **automáticamente** al iniciar.  
La aplicación estará disponible en la URL que muestre la consola (normalmente `http://localhost:5244`).

### Ejecutar los tests

```bash
dotnet test
```

---

## Credenciales de administrador

Al iniciar por primera vez se crea automáticamente un usuario administrador:

| Campo | Valor |
|---|---|
| **Usuario** | `admin` |
| **Contraseña** | `Admin123!` |

> El administrador puede editar y eliminar **cualquier** artículo, no solo los propios.

---

## Pantallas disponibles

| URL | Descripción | ¿Requiere sesión? |
|---|---|---|
| `/` | Inicio — feed de últimos artículos | No |
| `/auth/login` | Iniciar sesión / Registrarse | No |
| `/article/manage` | Gestión de artículos (crear, editar, eliminar) | Sí |
| `/article/detail/{id}` | Detalle de un artículo y sus comentarios | No (comentar sí) |
| `/swagger` | Documentación interactiva de la API | No |

---

## Guía de uso por pantalla

### Pantalla de inicio (`/`)

Muestra todos los artículos publicados ordenados del más reciente al más antiguo.

- Cada tarjeta muestra: **Título**, **Autor**, **Fecha** y **Resumen**.
- El botón **Ver artículo** lleva al detalle completo.
- Si hay más artículos de los que caben en la página, aparecen los botones **Página anterior** y **Página siguiente** (5 artículos por página).

---

### Pantalla de Login (`/auth/login`)

#### Iniciar sesión
| Campo | Descripción |
|---|---|
| **Usuario** | Tu nombre de usuario registrado |
| **Contraseña** | Tu contraseña |

Después de iniciar sesión correctamente serás redirigido al inicio y verás tu nombre en la barra de navegación.

#### Registrarse (botón "Regístrate")
Se abre un popup con el formulario de registro:

| Campo | Regla de validación |
|---|---|
| **Usuario** | Mínimo **3 caracteres**. No puede repetirse (único). Solo se permiten caracteres de texto. |
| **Contraseña** | Mínimo **6 caracteres**. |
| **Confirmar contraseña** | Debe ser **exactamente igual** a la contraseña. |

> Los usuarios registrados por este formulario tienen rol **User** y solo pueden editar/eliminar sus propios artículos.

---

### Pantalla de gestión de artículos (`/article/manage`)

Accesible solo si has iniciado sesión. Muestra todos los artículos publicados en la plataforma con las siguientes columnas:

| Columna | Descripción |
|---|---|
| Autor | Usuario que publicó el artículo |
| Título | Título del artículo |
| Fecha de publicación | Fecha y hora en formato `DD/MM/AAAA HH:MM` |
| Acciones | Botones disponibles según tu rol |

#### Acciones disponibles

| Botón | Disponible para | Descripción |
|---|---|---|
| **Ver detalle** | Todos | Abre el artículo completo |
| **Editar** | Solo el autor del artículo o Admin | Abre el modal de edición |
| **Eliminar** | Solo el autor del artículo o Admin | Elimina el artículo (pide confirmación) |

> Si no eres el autor ni el administrador, los botones Editar y Eliminar no aparecen.

#### Crear un nuevo artículo (botón "Nuevo artículo")

Se abre un modal con los siguientes campos:

| Campo | Regla de validación |
|---|---|
| **Título** | Mínimo **3 caracteres**. Máximo 200. Requerido. |
| **Resumen** | Mínimo **10 caracteres**. Máximo 500. Requerido. Aparece en el feed y en la tarjeta del inicio. |
| **Contenido** | Mínimo **10 caracteres**. Sin límite superior. Requerido. Es el cuerpo completo del artículo. |

---

### Pantalla de detalle de artículo (`/article/detail/{id}`)

Muestra el artículo completo: título, autor, fecha y contenido.

#### Sección de comentarios

Por defecto los comentarios están **ocultos**. El botón central **Mostrar comentarios** los despliega. Al presionarlo de nuevo cambia a **Ocultar comentarios**.

Al mostrar los comentarios verás:

- **Contador**: `Comentarios (N)` con el total actual.
- **Cada comentario** muestra: `Publicado por {usuario} el {fecha}` y el texto del comentario.

#### Agregar un comentario

Para comentar debes tener sesión iniciada. El formulario aparece automáticamente si estás autenticado.

| Campo | Regla de validación |
|---|---|
| **Comentario** | Mínimo **1 carácter**. Máximo 1000. Requerido. |

El comentario se publica vía AJAX (sin recargar la página) y aparece inmediatamente en la lista. El contador se actualiza al instante.

> Si no tienes sesión iniciada, verás un mensaje con enlace a login en lugar del formulario.

---

## Barra de navegación

| Elemento | Visible cuando | Acción |
|---|---|---|
| **Itelligent** (logo) | Siempre | Lleva al inicio |
| **Artículos** | Siempre | Lleva al feed de artículos |
| **Gestión** | Solo con sesión activa | Lleva a la gestión de artículos |
| **{usuario} (rol)** | Solo con sesión activa | Muestra tu nombre y rol |
| **Cerrar sesión** | Solo con sesión activa | Cierra la sesión completamente |
| **Iniciar sesión** | Sin sesión | Lleva al login |

---

## API REST

La documentación interactiva completa está disponible en **`/swagger`** mientras la aplicación esté en modo desarrollo.

### Endpoints principales

#### Autenticación
| Método | Endpoint | Auth | Descripción |
|---|---|---|---|
| POST | `/api/auth/register` | No | Crear cuenta nueva |
| POST | `/api/auth/login` | No | Iniciar sesión, obtiene JWT |
| GET | `/api/auth/me` | Sí | Info del usuario activo |

#### Artículos
| Método | Endpoint | Auth | Descripción |
|---|---|---|---|
| GET | `/api/articles?page=1&pageSize=10` | No | Lista paginada |
| GET | `/api/articles/{id}` | No | Detalle con comentarios |
| POST | `/api/articles` | Sí | Crear artículo |
| PUT | `/api/articles/{id}` | Sí (autor/admin) | Editar artículo |
| DELETE | `/api/articles/{id}` | Sí (autor/admin) | Eliminar artículo |
| POST | `/api/articles/{id}/comments` | Sí | Agregar comentario |

Para usar los endpoints protegidos en Swagger:
1. Ejecuta `POST /api/auth/login` con tus credenciales.
2. Copia el valor del campo `token` de la respuesta.
3. Haz clic en el botón **Authorize** (candado) en la parte superior de Swagger.
4. Escribe `Bearer {tu_token}` y haz clic en **Authorize**.

---

## Errores comunes

| Error | Causa | Solución |
|---|---|---|
| `401 Unauthorized` | Token ausente o expirado (dura 60 min) | Inicia sesión nuevamente |
| `403 Forbidden` | Intentas editar/eliminar un artículo ajeno | Solo el autor o Admin pueden hacerlo |
| `400 Bad Request` | Campos inválidos o incompletos | Revisa las reglas de validación de cada campo |
| `Username already taken` | El nombre de usuario ya existe | Elige un nombre de usuario diferente |
| `Invalid credentials` | Usuario o contraseña incorrectos | Verifica tus datos de acceso |

---

## Estructura del proyecto

```
ItelligentPlatform/
├── Itelligent.Domain/          # Entidades, enums, interfaces de repositorios
├── Itelligent.Application/     # Servicios, DTOs, interfaces (lógica de negocio)
├── Itelligent.Infrastructure/  # EF Core, repositorios, JWT, BCrypt
├── Itelligent.Presentation/    # Controladores API + MVC, Vistas Razor, wwwroot
└── Itelligent.Tests/           # Pruebas unitarias (xUnit + Moq)
```
