# Space AI API Core 🌌

![.NET 10](https://img.shields.io/badge/.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

Bienvenido al núcleo de **Space AI**, la API robusta y escalable diseñada bajo los más altos estándares de arquitectura para potenciar el ecosistema Space AI.

---

## 👨‍💻 Desarrollador y Equipo

- **Desarrollador Principal:** Emmanuel Ortiz Reyes
- **Equipo de Desarrollo:** Lattice Systems

---

## 🚀 Tecnologías y Arquitectura

Este proyecto implementa una **Arquitectura Limpia (Clean Architecture)** orientada a dominios para garantizar la mantenibilidad:
- **Framework:** .NET 10.0 (C# 13).
- **Arquitectura:** Modular con desacoplamiento de capas (Domain, Application, Infrastructure, API).
- **Persistencia:** PostgreSQL 15 con Entity Framework Core.
- **Patrones:** CQRS con MediatR, Repository Pattern, y FluentValidation.
- **Contenedores:** Docker Compose para levantar PostgreSQL en desarrollo local (la API en sí corre nativa con `dotnet run`, no está containerizada).

## 📖 Documentación del ecosistema
Este repo es uno de los 4 subproyectos de SpaceIA. La fuente de verdad del proyecto completo vive en la raíz del monorepo:
- [`docs/00_SPACEIA_SOURCE_OF_TRUTH.txt`](../docs/00_SPACEIA_SOURCE_OF_TRUTH.txt) — alcance, decisiones, prioridades.
- [`docs/03_API_MINIMUM_CONTRACTS.txt`](../docs/03_API_MINIMUM_CONTRACTS.txt) — contratos mínimos de API.
- [`docs/04_AZURE_MIGRATION_CHECKLIST.txt`](../docs/04_AZURE_MIGRATION_CHECKLIST.txt) — despliegue a Azure.

---

## 🛠️ Instalación y Configuración

### 1. Prerrequisitos

| Herramienta | Versión mínima | Descarga |
|---|---|---|
| **Docker Desktop** | 4.x o superior | [docker.com](https://www.docker.com/) (solo para levantar Postgres — si ya tienes un Postgres 15 local puedes saltarte esto) |
| **.NET SDK** | 10.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/) |
| **Git** | última versión | [git-scm.com](https://git-scm.com/) |

---

### 2. Base de datos: PostgreSQL

```bash
docker compose -f docker-compose.dev.yml up -d
docker compose -f docker-compose.dev.yml ps   # confirmar que "postgres" está healthy
```
Esto levanta un Postgres 15 en `localhost:5432` con los valores de `.env` (copia `.env.example` si no existe). Si ya tienes tu propio Postgres 15 corriendo, puedes omitir este paso y apuntar el connection string del siguiente paso a tu instancia.

---

### 3. Secretos locales: `dotnet user-secrets`

Este proyecto usa [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) para credenciales de desarrollo — nunca van al repo. Ya está configurado (`SmartSpaces.API.csproj` trae `UserSecretsId`), solo falta cargarlos:

```bash
cd SmartSpaces.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=smartspaces;Username=postgres;Password=postgres"
dotnet user-secrets set "JwtSettings:Secret" "<genera tu propio secret local, min 32 caracteres>"
dotnet user-secrets set "JwtSettings:Issuer" "SmartSpaces"
dotnet user-secrets set "JwtSettings:Audience" "SmartSpacesClient"
```

`appsettings.Development.json.example` documenta el resto de valores no-sensibles (`RAG_BASE_URL`, `AllowedOrigins`, `MQTT_*`, `AZURE_SPEECH_VOICE`) — ya vienen precargados en tu `appsettings.Development.json` local (gitignored). Si prefieres no usar user-secrets, puedes copiar el `.example` a `appsettings.Development.json` y completar `ConnectionStrings`/`JwtSettings` ahí directamente — sigue siendo seguro, el archivo real está en `.gitignore`.

Credenciales opcionales (síntesis de voz, MQTT del carrito) también van por `user-secrets` si las necesitas — ver `docs/04_AZURE_MIGRATION_CHECKLIST.txt` §2 para la lista completa de claves.

---

### 4. Ejecutar la API

```bash
cd SmartSpaces.API
dotnet restore
dotnet run
```

Por defecto (`launchSettings.json`, perfil `http`) queda escuchando en `http://localhost:5043` (perfil `https`: `https://localhost:7024`).

> ⚠️ **`dotnet run --no-launch-profile` puede fallar con `RAG_BASE_URL no configurado`.** Ese flag se salta `ASPNETCORE_ENVIRONMENT=Development` que trae `launchSettings.json`, y sin eso no carga `appsettings.Development.json`/user-secrets. Si necesitas `--no-launch-profile` (por ejemplo, para forzar el bind a `0.0.0.0`), exporta el entorno explícitamente:
> ```bash
> export ASPNETCORE_ENVIRONMENT="Development"
> export ASPNETCORE_URLS="http://0.0.0.0:5043"
> dotnet run --no-launch-profile
> ```

---

### 4.1 Probar contra un dispositivo físico (app móvil o tablet real)

Por default, `dotnet run` escucha solo en `localhost` (`launchSettings.json` → `applicationUrl`), inalcanzable desde un celular/tablet en la misma red. Dos formas de exponerlo sin tocar ese archivo compartido:

**Opción A — bind a `0.0.0.0` + IP de LAN:**
```bash
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://0.0.0.0:5043"
dotnet run --no-launch-profile
```
Requiere firewall abierto en el puerto y el dispositivo en la misma red. Si corres esto dentro de **WSL** (Windows), la IP de LAN de Windows no llega sola a WSL — necesitas además reenviar el puerto desde Windows (PowerShell como administrador):
```powershell
netsh interface portproxy add v4tov4 listenaddress=0.0.0.0 listenport=5043 connectaddress=<IP_INTERNA_WSL> connectport=5043
New-NetFirewallRule -DisplayName "SmartSpaces API Dev" -Direction Inbound -LocalPort 5043 -Protocol TCP -Action Allow
```
La IP interna de WSL (`hostname -I` dentro de WSL) cambia en cada reinicio — hay que rehacer el proxy.

**Opción B — túnel (ngrok), más simple:**
```bash
ngrok http 5043
```
Da una URL pública (`https://algo.ngrok-free.app`) que tunela directo, sin firewall ni portproxy, funciona hasta en datos móviles. La URL cambia cada reinicio de `ngrok`; no lo dejes corriendo sin vigilancia (expone el backend local a internet mientras el túnel viva).

---

### 5. Verificar el Estado

Abre **[http://localhost:5043/swagger](http://localhost:5043/swagger)** para explorar la documentación interactiva de la API y confirmar que levantó correctamente (no hay endpoint `/health` implementado — Swagger es el smoke-test disponible hoy).

---

## 📦 Comandos Disponibles

| Comando | Descripción |
|---|---|
| `docker compose -f docker-compose.dev.yml up -d` | Levanta Postgres para desarrollo local |
| `docker compose -f docker-compose.dev.yml logs -f postgres` | Ver logs de Postgres en tiempo real |
| `dotnet user-secrets list` | Ver los secretos locales configurados (desde `SmartSpaces.API/`) |
| `dotnet build` | Compila la solución localmente |
| `dotnet test` | Ejecuta las pruebas unitarias y de integración |
| `dotnet ef migrations add Nombre` | Crea una nueva migración de base de datos |
| `dotnet ef database update` | Aplica migraciones pendientes contra Postgres |

---

## 🗂️ Estructura del Proyecto

```
SmartSpaces.sln
├── SmartSpaces.API/            → Punto de entrada, Controladores y Configuración
├── SmartSpaces.Application/    → Lógica de Negocio (Commands, Queries, Mappings)
├── SmartSpaces.Domain/         → Entidades, Excepciones y Reglas de Negocio
├── SmartSpaces.Infrastructure/ → Acceso a Datos, Migraciones y Servicios Externos
├── SmartSpaces.Shared/         → DTOs y utilidades comunes
└── SmartSpaces.UnitTests/      → Suite de pruebas automáticas
```

---

*Desarrollado con innovación y dedicación por Lattice Systems.*
