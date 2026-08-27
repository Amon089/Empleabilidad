# Plataforma SaaS Multi-Tenant de PQRS con IA

Sistema completo SaaS multi-tenant compuesto por un **Backend en ASP.NET Core (.NET 10)** con **PostgreSQL + pgvector**, **Widget reutilizable en Vanilla JS + Shadow DOM**, **2 Sitios Web de Demostración** con diseño profesional y un **Dashboard de Gestión Multi-Tenant con Notificaciones SignalR en tiempo real**.

Para ver las instrucciones detalladas paso a paso sobre cómo probar cada flujo, consulta la [📖 Guía de Uso Completa (COMO_USAR.md)](file:///c:/Users/moren/OneDrive/Documents/riwi/C%23/Empleavilidad/COMO_USAR.md).

---

## 📁 Estructura Completa del Proyecto

```text
Empleavilidad/
├── COMO_USAR.md                      # 📖 Manual detallado de uso y pruebas paso a paso
├── README.md                         # Documentación general de arquitectura y URLs
├── Dockerfile                        # Multi-stage Dockerfile para Pqrs.API
├── docker-compose.yml                # Orquestación de API y PostgreSQL con pgvector
│
├── frontend/                         # Código fuente del Frontend
│   ├── widget/                       # Widget Reutilizable (Vanilla JS + Shadow DOM)
│   │   ├── src/ (styles.css, api.js, state.js, ui.js, index.js)
│   │   └── dist/ (pqrs-widget.js)
│   ├── web-legumbres/                # Sitio Web Tenant A - Estilo "Cosecha Orgánica"
│   │   └── index.html, styles.css
│   ├── web-todo-metal/               # Sitio Web Tenant B - Estilo "Industrial Precision"
│   │   └── index.html, styles.css
│   └── dashboard/                    # Console SPA de Gestión Multi-Tenant
│       └── index.html, styles.css, app.js
│
├── src/
│   ├── Pqrs.Domain/                  # Entidades, Enums e Interfaces base
│   ├── Pqrs.Application/             # Casos de uso, Servicios DTOs y Lógica RAG/Triaje
│   ├── Pqrs.Infrastructure/          # PqrsDbContext (EF Core pgvector), AiService, Queue
│   └── Pqrs.API/                     # REST API Controllers, Middlewares, SignalR, StaticFiles
│       └── wwwroot/                  # Servidor estático integrado para Frontend
│           ├── widget/               # pqrs-widget.js
│           ├── legumbres/            # Web Leggumbres (Agro Style)
│           ├── todometal/            # Web Todo Metal (Industrial Style)
│           └── dashboard/            # Admin Dashboard SPA
│
└── tests/
    ├── Pqrs.UnitTests/               # 7 Pruebas Unitarias Aprobadas (100%)
    └── Pqrs.IntegrationTests/        # 5 Pruebas de Integración Aprobadas (100%)
```

---

## 🚀 Cómo Ejecutar la Aplicación Completa

Inicia la API REST y el servidor estático de Frontend con un solo comando:

```bash
dotnet run --project src/Pqrs.API/Pqrs.API.csproj
```

O utilizando Docker Compose con PostgreSQL + pgvector:

```bash
docker compose up --build
```

---

## 🌐 URLs de Acceso a la Demostración

Una vez iniciada la aplicación, accede a cada componente desde el navegador:

1. **Documentación Swagger OpenAPI**: `http://localhost:5000/swagger`
2. **Web Tenant A (Leggumbres La Escoba - Agro Style)**: `http://localhost:5000/legumbres/index.html`
   - *Widget Key*: `pk_live_escoba_12345`
3. **Web Tenant B (Estructuras Todo Metal SAS - Industrial Style)**: `http://localhost:5000/todometal/index.html`
   - *Widget Key*: `pk_live_todometal_67890`
4. **Dashboard de Gestión Multi-Tenant**: `http://localhost:5000/dashboard/index.html`
   - **Login Tenant A (Leggumbres)**: `admin@leggumbres.local` | Clave: `Password123!`
   - **Login Tenant B (Todo Metal)**: `admin@todometal.local` | Clave: `Password123!`

---

## 🛠️ Características del Frontend Implementado

1. **Widget Reutilizable (Vanilla JS + Shadow DOM)**:
   - Encapsulamiento total CSS dentro del Shadow DOM.
   - Apertura y cierre accesible mediante botón flotante (`aria-label`).
   - Chat interactivo con respuestas RAG y badges de fuentes de conocimiento.
   - Botones de retroalimentación (`[Sí, gracias!]` para desviación sin ticket / `[No, radicar PQRS]` para escalamiento).
   - Formulario PQRS con validación en línea y preservación de datos en reintentos.
   - Generación de número de radicado oficial `PQRS-XXXX`.
2. **Web Leggumbres La Escoba (Agro Style / Cosecha Orgánica)**:
   - Identidad agrícola/fresca (Verdes orgánicos `#14532d`, `#16a34a`, tarjetas de cosecha, proceso "From Seed to Kitchen").
   - Inserta `<script src="/widget/pqrs-widget.js" data-tenant="pk_live_escoba_12345"></script>`.
3. **Web Estructuras y Montajes Todo Metal SAS (Industrial Style / Precision Engineering)**:
   - Identidad industrial (Pizarra `#0a0e17`, tarjetas `#131924`, acentos amarillo oro `#eab308`).
   - Inserta `<script src="/widget/pqrs-widget.js" data-tenant="pk_live_todometal_67890"></script>`.
4. **Dashboard de Gestión Multi-Tenant**:
   - Cambia dinámicamente de identidad (Colores, Nombre y Métricas) según el JWT del usuario autenticado.
   - Conexión a SignalR Hub `/api/v1/hubs/notifications` con notificaciones emergentes `ticket.critical` en tiempo real.
   - Filtros de estado/prioridad, resumen IA, histórico de estados y botón de prueba de seguridad de aislamiento multi-tenant (HTTP 404).

---

## 🧪 Pruebas Automatizadas

```bash
dotnet test
```

- **12/12 Pruebas Exitosas (100% de éxito)**.
