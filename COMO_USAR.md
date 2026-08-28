# 📖 Guía de Uso Completa - Plataforma SaaS Multi-Tenant de PQRS con IA

Esta guía explica paso a paso cómo iniciar, interactuar y evaluar todas las funcionalidades de la plataforma SaaS Multi-Tenant de PQRS con IA (RAG previo, Triaje asíncrono, Notificaciones SignalR en tiempo real, Aislamiento Multi-Tenant de datos y Doble Botón Flotante).

---

## 📌 Requisitos Previos

Asegúrate de contar con uno de los siguientes entornos instalados en tu equipo:

- **.NET 10 SDK** (para ejecución local)
- **Docker & Docker Compose** (para ejecución en contenedores con PostgreSQL + pgvector)

---

## 🚀 Paso 1: Iniciar la Aplicación

### Opción A: Ejecución Local (.NET SDK)

Abre una terminal en la raíz del proyecto y ejecuta:

```bash
dotnet run --project src/Pqrs.API/Pqrs.API.csproj
```

La API y todos los portales estáticos iniciarán automáticamente en **`http://localhost:5000`** o **`http://localhost:5050`**.

### Opción B: Ejecución con Docker Compose (Recomendado para PostgreSQL + pgvector)

```bash
docker compose up --build
```

Esto desplegará:
- **Contenedor PostgreSQL 16 + pgvector**: `localhost:5432`
- **Contenedor ASP.NET Core API + Frontend**: `http://localhost:5050`

---

## 🌐 Paso 2: Portales y URLs de Acceso

Una vez iniciado el servidor, abre tu navegador e ingresa a las siguientes direcciones:

| Componente | URL de Acceso | Descripción |
| :--- | :--- | :--- |
| **Documentación Swagger** | `http://localhost:5050/swagger` | Explorador interactivo de la API REST |
| **Web Leggumbres (Tenant A)** | `http://localhost:5050/legumbres/index.html` | Sitio agro/orgánico con Widget `leggumbres-key-123` |
| **Web Todo Metal (Tenant B)** | `http://localhost:5050/todometal/index.html` | Sitio industrial B2B con Widget `todo-metal-key-456` |
| **Dashboard Multi-Tenant** | `http://localhost:5050/dashboard/index.html` | Consola de administración y atención de tickets |

---

## 💬 Paso 3: Probar el Widget con Doble Botón Flotante

El widget flotante incluye dos botones de acceso en la esquina inferior derecha:

- 🔵 **Botón Azul (`🤖 Consultar Asistente IA`)**: Abre la ventana de consulta conversacional asistida por RAG.
- 🟢 **Botón Verde (`📝 Radicar PQRS`)**: Abre directamente el Formulario Oficial de Radicación de PQRS.

### 3.1 Probar Consulta RAG y Respuestas Formateadas

1. Entra a la **Web Leggumbres La Escoba** (`http://localhost:5050/legumbres/index.html`).
2. Haz clic en el **Botón Azul (`🤖 Consultar Asistente IA`)**.
3. Escribe la pregunta:
   > *"¿Qué productos vendes?"*
4. **Resultado**: La IA responde de forma amable y estructurada en viñetas limpias por categorías (Frutas, Verduras, Granos, Hierbas), sin mostrar códigos ni dataset tags.
5. Aparecerá la pregunta: *"¿Esta respuesta resolvió tu inquietud?"*:
   - Haz clic en **`[¡Sí, gracias!]`**.
   - El widget muestra un mensaje de agradecimiento y **finaliza la interacción sin crear ningún ticket en la base de datos** (Demostración del ahorro operativo RAG / Desviación).

### 3.2 Probar Aislamiento RAG entre Tenants

1. En la misma Web de **Leggumbres**, abre el chat IA y pregunta sobre el otro tenant:
   > *"¿Cómo solicito una visita técnica para un puente o una estructura metálica?"*
2. **Resultado**: El backend evalúa la base de conocimientos y, al no encontrar información en el tenant activo, responde:
   > *"No encuentro información suficiente para responder esta consulta."*
3. **Validación de Seguridad**: El sistema **NO** utilizó la base de conocimiento de *Todo Metal SAS*, garantizando el aislamiento multi-tenant.

### 3.3 Radicación Directa de PQRS

1. Haz clic en el **Botón Verde (`📝 Radicar PQRS`)**.
2. Se abrirá directamente el formulario oficial de radicación.
3. Diligencia los campos:
   - **Nombre**: `Carlos Gómez`
   - **Correo**: `carlos@example.com`
   - **Asunto**: `Pedido Incompleto`
   - **Descripción**: `Mi pedido llegó sin las papas criollas solicitadas.`
4. Haz clic en **`[Enviar PQRS]`**.
5. **Resultado**: Se muestra la confirmación con el número de radicado oficial generado por el backend (`Radicado: PQRS-XXXX`).

---

## 📊 Paso 4: Consola de Administración y Aislamiento por Credenciales

Accede a `http://localhost:5050/dashboard/index.html`.

### 4.1 Credenciales de Demostración y Aislamiento por Tenant

En el modal de inicio de sesión puedes usar los botones de **Acceso Rápido Demo**:

| Perfil | Correo | Contraseña | Alcance de Datos |
| :--- | :--- | :--- | :--- |
| 🥦 **Admin Leggumbres** | `admin@leggumbres.local` | `Password123!` | Exclusivo Leggumbres La Escoba (Fuerza tenant `leggumbres` y deshabilita selector) |
| 🏗️ **Admin Todo Metal** | `admin@todometal.local` | `Password123!` | Exclusivo Todo Metal SAS (Fuerza tenant `todo-metal` y deshabilita selector) |
| ⚡ **Super Admin Global** | `admin@saas.com` | `Password123!` | Acceso Global Multi-Tenant (Habilita el selector `🏢 Tenant Activo`) |

---

## 🧪 Paso 5: Suite de Pruebas Automatizadas

Para ejecutar las pruebas:

```bash
dotnet test
```

### Resultados de las Pruebas:
- **16/16 Pruebas Aprobadas (100% de éxito)**.
