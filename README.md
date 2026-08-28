# Plataforma SaaS Multi-Tenant de PQRS con IA

Sistema completo SaaS multi-tenant compuesto por un **Backend en ASP.NET Core (.NET 10)** con **PostgreSQL + pgvector**, **Widget reutilizable en Vanilla JS + Shadow DOM**, **2 Sitios Web de Demostración** con diseño profesional y un **Dashboard de Gestión Multi-Tenant con Notificaciones SignalR en tiempo real**.

Para ver las instrucciones detalladas paso a paso sobre cómo probar cada flujo, consulta la [📖 Guía de Uso Completa (COMO_USAR.md)](file:///c:/Users/moren/OneDrive/Documents/riwi/C%23/Empleavilidad/COMO_USAR.md).  
Para aprender a registrar nuevas empresas y pegar el widget en otros sitios web, consulta la [🏢 Guía de Creación de Tenants e Incrustación de Widget (GUIA_NUEVO_TENANT_Y_WIDGET.md)](file:///c:/Users/moren/OneDrive/Documents/riwi/C%23/Empleavilidad/GUIA_NUEVO_TENANT_Y_WIDGET.md).  
Para desplegar la infraestructura en un clúster de Kubernetes, consulta la [☸️ Guía Completa de Despliegue en Kubernetes (GUIA_DESPLIEGUE_KUBERNETES.md)](file:///c:/Users/moren/OneDrive/Documents/riwi/C%23/Empleavilidad/GUIA_DESPLIEGUE_KUBERNETES.md).

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
│   ├── widget/                       # Widget Reutilizable (Vanilla JS + Shadow DOM con doble botón flotante)
│   │   ├── src/ (styles.css, api.js, state.js, ui.js, index.js)
│   │   └── dist/ (pqrs-widget.js)
│   ├── web-legumbres/                # Sitio Web Tenant A - Estilo "Cosecha Orgánica" (Leggumbres La Escoba)
│   │   └── index.html, styles.css
│   ├── web-todo-metal/               # Sitio Web Tenant B - Estilo "Industrial Precision" (Todo Metal SAS B2B)
│   │   └── index.html, styles.css, app.js
│   └── dashboard/                    # Console SPA de Gestión Multi-Tenant con Aislamiento
│       └── index.html, styles.css, app.js
│
├── src/
│   ├── Pqrs.Domain/                  # Entidades, Enums e Interfaces base
│   ├── Pqrs.Application/             # Casos de uso, Servicios DTOs y Lógica RAG/Triaje
│   ├── Pqrs.Infrastructure/          # PqrsDbContext (EF Core pgvector), AiService, Queue
│   └── Pqrs.API/                     # REST API Controllers, Middlewares, SignalR, StaticFiles
│       └── wwwroot/                  # Servidor estático integrado para Frontend
│           ├── widget/               # pqrs-widget.js (Doble Lanzador IA y Radicación Directa)
│           ├── legumbres/            # Web Leggumbres (Agro Style)
│           ├── todometal/            # Web Todo Metal (Industrial Style B2B Showcase)
│           └── dashboard/            # Admin Dashboard SPA con Login Multitenant
│
└── tests/
    ├── Pqrs.UnitTests/               # Pruebas Unitarias Aprobadas (100%)
    └── Pqrs.IntegrationTests/        # Pruebas de Integración Aprobadas (100%)
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

1. **Documentación Swagger OpenAPI**: `http://localhost:5050/swagger` (o `http://localhost:5000/swagger`)
2. **Web Tenant A (Leggumbres La Escoba - Agro Style)**: `http://localhost:5050/legumbres/index.html`
   - *Widget Key*: `leggumbres-key-123` / `pk_live_escoba_12345`
3. **Web Tenant B (Estructuras Todo Metal SAS - Industrial Style B2B Showcase)**: `http://localhost:5050/todometal/index.html`
   - *Widget Key*: `todo-metal-key-456` / `pk_live_todometal_67890`
4. **Dashboard de Gestión Multi-Tenant**: `http://localhost:5050/dashboard/index.html`
   - **Login Admin Leggumbres**: `admin@leggumbres.local` | Clave: `Password123!`
   - **Login Admin Todo Metal**: `admin@todometal.local` | Clave: `Password123!`
   - **Login Super Admin Global**: `admin@saas.com` | Clave: `Password123!`

---

## 🛠️ Características Destacadas Implementadas

1. **Dataset RAG Entrenado con 300 Preguntas y Respuestas**:
   - 150 Q&As completas sobre frutas, verduras, hortalizas, legumbres, granos, hierbas, horarios de 6am-2pm, recogida en Bodega 12 y garantías para **Leggumbres La Escoba**.
   - 150 Q&As completas sobre estructuras metálicas, aceros ASTM A36/A572, puentes vehiculares/peatonales, obras civiles, demolición, norma NSR-10, soldadura AWS D1.1, NDT y garantía Ley 1796 para **Todo Metal SAS**.
2. **Doble Botón Flotante en el Widget**:
   - 🔵 **Lanzador Azul (`🤖 Asistente IA`)**: Chatbot conversacional enfocado en respuestas RAG instantáneas.
   - 🟢 **Lanzador Verde (`📝 Radicar PQRS`)**: Apertura directa del formulario oficial de radicación de PQRS.
3. **Formato Limpio y Natural de la IA**:
   - Respuestas estructuradas en viñetas sin volcado de identificadores técnicos o dataset tags (`P101:`, `R101:`, `Q&As`).
4. **Seguridad y Aislamiento por Credencial en el Dashboard**:
   - `admin@leggumbres.local` bloquea el panel exclusivamente para Leggumbres La Escoba.
   - `admin@todometal.local` bloquea el panel exclusivamente para Todo Metal SAS.
   - `admin@saas.com` permite alternar dinámicamente entre todos los tenants.

---

## 🧪 Pruebas Automatizadas

```bash
dotnet test
```

- **16/16 Pruebas Automatizadas Exitosas (100% de éxito)**.
