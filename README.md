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
- **Contenedores:** Docker & Docker Compose para orquestación completa.

## 📖 Documentación Detallada
Consulta nuestro índice de documentación en [`docs/README.md`](./docs/README.md)
*   [Guía de Despliegue](./docs/infrastructure/DEPLOYMENT.md)
*   [Solución de Problemas Docker](./docs/troubleshooting/DOCKER_ERRORS.md)

---

## 🛠️ Instalación y Configuración

### 1. Prerrequisitos

Asegúrate de tener instaladas las siguientes herramientas:

| Herramienta | Versión mínima | Descarga |
|---|---|---|
| **Docker Desktop** | 4.x o superior | [docker.com](https://www.docker.com/) |
| **.NET SDK** | 10.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/) |
| **Git** | última versión | [git-scm.com](https://git-scm.com/) |

---

### 2. Método A: Levantar con Docker (Recomendado)

La forma más rápida de iniciar un entorno consistente con Base de Datos y Proxy.

```bash
# Levantar todo el stack
docker compose -f docker-compose.dev.yml up -d

# Probar Health Check
curl http://localhost:8080/health
```

---

### 3. Método B: Ejecución Manual (Local)

Ideal para desarrollo activo y depuración rápida sin contenedores para la aplicación.

```bash
# 1. Navegar a la carpeta de la API
cd SmartSpaces.API

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar la aplicación
dotnet run
```

> ℹ️ **Nota:** Para el método manual, necesitas una instancia de PostgreSQL corriendo localmente. Consulta la **[Guía de Instalación Local](./docs/guides/LOCAL_SETUP.md)** para configurar la base de datos y migraciones.

> ⚠️ **`dotnet run` sin más puede fallar con `RAG_BASE_URL no configurado`** si corres con `--no-launch-profile` (por ejemplo, para forzar el bind a `0.0.0.0` — ver más abajo). Ese flag también se salta `ASPNETCORE_ENVIRONMENT=Development` que trae `launchSettings.json`, y sin eso no carga `appsettings.Development.json` (donde vive `RAG_BASE_URL`). Si usas `--no-launch-profile`, exporta ambas variables:
> ```bash
> export ASPNETCORE_ENVIRONMENT="Development"
> export ASPNETCORE_URLS="http://0.0.0.0:5274"
> dotnet run --no-launch-profile
> ```

---

### 3.1 Probar contra un dispositivo físico (app móvil o tablet real)

Por default, `dotnet run` escucha solo en `localhost` (`launchSettings.json` → `applicationUrl`), inalcanzable desde un celular/tablet en la misma red. Dos formas de exponerlo sin tocar ese archivo compartido:

**Opción A — bind a `0.0.0.0` + IP de LAN:**
```bash
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://0.0.0.0:5274"
dotnet run --no-launch-profile
```
Requiere firewall abierto en el puerto y el dispositivo en la misma red. Si corres esto dentro de **WSL** (Windows), la IP de LAN de Windows no llega sola a WSL — necesitas además reenviar el puerto desde Windows (PowerShell como administrador):
```powershell
netsh interface portproxy add v4tov4 listenaddress=0.0.0.0 listenport=5274 connectaddress=<IP_INTERNA_WSL> connectport=5274
New-NetFirewallRule -DisplayName "SmartSpaces API Dev" -Direction Inbound -LocalPort 5274 -Protocol TCP -Action Allow
```
La IP interna de WSL (`hostname -I` dentro de WSL) cambia en cada reinicio — hay que rehacer el proxy.

**Opción B — túnel (ngrok), más simple:**
```bash
ngrok http 5274
```
Da una URL pública (`https://algo.ngrok-free.app`) que tunela directo, sin firewall ni portproxy, funciona hasta en datos móviles. La URL cambia cada reinicio de `ngrok`; no lo dejes corriendo sin vigilancia (expone el backend local a internet mientras el túnel viva).

---

### 4. Verificar el Estado

Abre **[http://localhost:8080/swagger](http://localhost:8080/swagger)** (Docker) o **[http://localhost:5274/swagger](http://localhost:5274/swagger)** (Manual) para explorar la documentación interactiva de la API.

---

## 📦 Comandos Disponibles

| Comando | Descripción |
|---|---|
| `docker compose -f docker-compose.dev.yml up -d` | Inicia el stack de desarrollo completo |
| `docker compose -f docker-compose.dev.yml logs -f api` | Ver logs de la API en tiempo real |
| `dotnet build` | Compila la solución localmente |
| `dotnet test` | Ejecuta las pruebas unitarias y de integración |
| `dotnet ef migrations add Nombre` | Crea una nueva migración de base de datos |

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
