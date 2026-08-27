# 📖 Guía de Uso Completa - Plataforma SaaS Multi-Tenant de PQRS con IA

Esta guía explica paso a paso cómo iniciar, interactuar y evaluar todas las funcionalidades de la plataforma SaaS Multi-Tenant de PQRS con IA (RAG previo, Triaje asíncrono, Notificaciones SignalR en tiempo real y Aislamiento Multi-Tenant de datos).

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

La API y todos los portales estáticos iniciarán automáticamente en **`http://localhost:5000`**.

### Opción B: Ejecución con Docker Compose (Recomendado para PostgreSQL + pgvector)

```bash
docker compose up --build
```

Esto desplegará:
- **Contenedor PostgreSQL 16 + pgvector**: `localhost:5432`
- **Contenedor ASP.NET Core API + Frontend**: `http://localhost:5000`

---

## 🌐 Paso 2: Portales y URLs de Acceso

Una vez iniciado el servidor, abre tu navegador e ingresa a las siguientes direcciones:

| Componente | URL de Acceso | Descripción |
| :--- | :--- | :--- |
| **Documentación Swagger** | `http://localhost:5000/swagger` | Explorador interactivo de la API REST |
| **Web Leggumbres (Tenant A)** | `http://localhost:5000/legumbres/index.html` | Sitio agro/orgánico con Widget `pk_live_escoba_12345` |
| **Web Todo Metal (Tenant B)** | `http://localhost:5000/todometal/index.html` | Sitio industrial con Widget `pk_live_todometal_67890` |
| **Dashboard Multi-Tenant** | `http://localhost:5000/dashboard/index.html` | Consola de administración y atención de tickets |

---

## 💬 Paso 3: Probar el Widget PQRS con IA (Demostración RAG y Desviación)

### 3.1 Probar Respuesta RAG Existente y Desviación Operativa

1. Entra a la **Web Leggumbres La Escoba** (`http://localhost:5000/legumbres/index.html`).
2. Haz clic en el **botón flotante del launcher** (esquina inferior derecha) para abrir el widget.
3. Escribe la pregunta:
   > *"¿Cuáles son los horarios de entrega?"*
4. **Resultado**: El widget consulta la base de conocimiento vectorial de Leggumbres y responde:
   > *"Zonas de cobertura: Ciudad Principal y Municipios aledaños. Horarios de entrega: lunes a sábado de 6:00 AM a 2:00 PM."* junto con el distintivo de fuente (`📄 Fuente: Zonas de cobertura y horarios de entrega`).
5. Aparecerá la pregunta: *"¿Esta respuesta resolvió tu inquietud?"*:
   - Haz clic en **`[¡Sí, gracias!]`**.
   - El widget muestra un mensaje de agradecimiento y **finaliza la interacción sin crear ningún ticket en la base de datos**. Esto demuestra el **ahorro operativo del RAG (Desviación)**.

### 3.2 Probar Aislamiento RAG (Evitar Filtración entre Tenants)

1. En la misma Web de **Leggumbres**, abre el widget y pregunta sobre el negocio del otro tenant:
   > *"¿Cómo solicito una visita técnica para una estructura metálica o un puente?"*
2. **Resultado**: El backend evalúa la similitud y, al no encontrar información en la base de conocimientos de Leggumbres, responde:
   > *"No contamos con suficiente información en nuestra base de conocimientos para responder esta consulta."*
3. **Validación de Seguridad**: El sistema **NO** utilizó la información de *Todo Metal SAS*, demostrando el aislamiento total de datos.

### 3.3 Escalatorio a Radicación de PQRS

1. Tras recibir la respuesta de falta de información, haz clic en **`[No, radicar PQRS]`**.
2. El widget cambiará al formulario de radicación.
3. Diligencia los campos:
   - **Nombre**: `Carlos Gómez`
   - **Correo**: `carlos@example.com`
   - **Asunto**: `Pedido Incompleto`
   - **Descripción**: `Mi pedido llegó sin las papas criollas solicitadas.`
4. Haz clic en **`[Enviar PQRS]`**.
5. **Resultado**: Se muestra la pantalla de éxito con el número de radicado oficial generado por el backend (`Radicado: PQRS-XXXX`).

---

## 📊 Paso 4: Consola de Administración y Dashboard Multi-Tenant

Accede a `http://localhost:5000/dashboard/index.html`.

### 4.1 Credenciales de Demostración

Puedes hacer clic en los botones de **Acceso Rápido Demo** o ingresar manualmente:

- **Tenant A - Leggumbres La Escoba**:
  - **Correo**: `admin@leggumbres.local`
  - **Contraseña**: `Password123!`
- **Tenant B - Estructuras Todo Metal SAS**:
  - **Correo**: `admin@todometal.local`
  - **Contraseña**: `Password123!`

### 4.2 Probar la Consola de Leggumbres (Tenant A)

1. Inicia sesión con `admin@leggumbres.local`.
2. Observa cómo el dashboard adopta el tema y colores agrícolas de Leggumbres.
3. Verás las métricas de atención y en el listado de tickets aparecerá el ticket que acabas de radicar en la Web de Leggumbres.
4. Haz clic en el ticket para ver su detalle:
   - Podrás ver la **Clasificación y Resumen Generado por IA (Triaje Asíncrono)**.
   - Podrás cambiar el estado (ej. a `IN_PROGRESS` o `RESOLVED`) y actualizar la prioridad.

### 4.3 Probar la Consola de Todo Metal (Tenant B)

1. Cierra sesión e inicia sesión con `admin@todometal.local`.
2. Observa cómo el dashboard cambia dinámicamente de identidad (Colores Azul/Pizarra industrial de Todo Metal).
3. Verás únicamente los tickets y métricas correspondientes a Todo Metal SAS. El ticket de Leggumbres **NO** aparece en esta lista.

### 4.4 Probar Notificaciones SignalR en Tiempo Real (`ticket.critical`)

1. Mantén abierta la consola de **Todo Metal** en el navegador.
2. En otra pestaña del navegador, abre la **Web Todo Metal** (`http://localhost:5000/todometal/index.html`).
3. Abre el widget y radica un ticket crítico, por ejemplo:
   - **Asunto**: `Problema grave de corrosión en puente`
   - **Descripción**: `Se evidencia corrosión severa en los pernos de la unión estructural del puente.`
4. Regresa a la pestaña de la consola de Todo Metal:
   - En pocos segundos, verás aparecer una **alerta flotante roja (Toast)**: `🚨 ¡Ticket Crítico Registrado!`.
   - La lista de tickets y el contador de críticos se actualizarán automáticamente sin necesidad de recargar la página.

---

## 🛡️ Paso 5: Prueba de Seguridad de Aislamiento Multi-Tenant

Para comprobar que un tenant no puede acceder directamente a recursos de otro tenant mediante la API REST:

1. Dentro de la consola del Dashboard (ej. sesión de Leggumbres), ve a la pantalla de detalle de cualquier ticket.
2. Haz clic en el botón rojo **`[🛡️ Probar Aislamiento Multi-Tenant]`**.
3. El frontend intentará realizar una petición HTTP GET enviando el token JWT de Leggumbres para solicitar un recurso inexistente o pertenciente a otro tenant.
4. **Resultado**: El sistema mostrará la confirmación:
   > *"🛡️ PRUEBA DE SEGURIDAD EXITOSA: El backend rechazó la petición de otro tenant con un estado HTTP 404 NOT FOUND (Aislamiento Total)."*

---

## 🧪 Paso 6: Ejecutar la Suite de Pruebas Automatizadas

El proyecto incluye 12 pruebas automatizadas que verifican la arquitectura y el aislamiento.

Para ejecutarlas:

```bash
dotnet test
```

### Resultados de las Pruebas:
- `Pqrs.UnitTests`: **7/7 Aprobadas** (Pruebas unitarias de servicios, triaje y RAG).
- `Pqrs.IntegrationTests`: **5/5 Aprobadas** (Pruebas de integración E2E de aislamiento multi-tenant, autenticación JWT y rechazo de accesos cruzados).
- **Total**: **12/12 Pruebas Exitosas (100% de éxito)**.
