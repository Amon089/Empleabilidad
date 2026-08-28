# 📖 Guía Técnica: Cómo Añadir un Nuevo Tenant e Incrustar el Widget Web

Esta guía explica detalladamente el proceso paso a paso para **crear e incorporar una nueva empresa (Tenant)** en la plataforma SaaS Multi-Tenant y **cómo incrustar su widget de IA y PQRS en cualquier página web externa** (HTML estático, React, Vue, Angular, WordPress, etc.).

---

## 🏢 PARTE 1: Cómo Añadir un Nuevo Tenant en la Plataforma

Para registrar una nueva empresa en el sistema, se requieren 3 componentes en la base de datos:
1. **El Registro del Tenant** (Datos de la empresa, API Key/Widget Key y Dominios CORS permitidos).
2. **Los Usuarios Administradores / Agentes** asociados al `TenantId`.
3. **La Base de Conocimiento RAG** (Artículos de preguntas frecuentes y sus vectores de embedding).

---

### Paso 1.1: Registrar el Tenant en la Base de Datos

Puedes agregarlo mediante código C# (por ejemplo en `DbInitializer.cs`) o ejecutando una consulta SQL directa en PostgreSQL.

#### Opción A: Registro en C# (EF Core - `DbInitializer.cs`)

```csharp
var nuevoTenant = new Tenant
{
    Id = Guid.NewGuid(),
    Name = "Restaurante La Casona",
    Slug = "la-casona",
    WidgetPublicKey = "la-casona-key-789", // Clave pública única para el widget
    AllowedOrigins = new List<string>
    {
        "https://lacasona.com",
        "https://www.lacasona.com",
        "http://localhost:3000" // Dominios autorizados por CORS
    },
    IsActive = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

context.Tenants.Add(nuevoTenant);
await context.SaveChangesAsync();
```

#### Opción B: Inserción en SQL Directo (PostgreSQL)

```sql
INSERT INTO "Tenants" ("Id", "Name", "Slug", "WidgetPublicKey", "AllowedOrigins", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (
    gen_random_uuid(),
    'Restaurante La Casona',
    'la-casona',
    'la-casona-key-789',
    '["https://lacasona.com", "http://localhost:3000"]',
    true,
    NOW(),
    NOW()
);
```

---

### Paso 1.2: Crear los Usuarios de Gestión (Admin / Agente)

Cada empresa requiere de al menos un usuario administrador o agente registrado con el hash de contraseña seguro (BCrypt):

```csharp
var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

var usuarioAdmin = new User
{
    Id = Guid.NewGuid(),
    TenantId = nuevoTenant.Id,
    Name = "Admin La Casona",
    Email = "admin@lacasona.com",
    PasswordHash = passwordHash,
    Role = UserRole.ADMIN,
    IsActive = true
};

context.Users.Add(usuarioAdmin);
await context.SaveChangesAsync();
```

---

### Paso 1.3: Indexar la Base de Conocimientos RAG con Embeddings

Para que el chatbot con IA pueda responder preguntas específicas de la nueva empresa, se deben insertar artículos e indexar sus vectores con `IAiService`:

```csharp
var articulosNuevos = new List<(string Title, string Content)>
{
    (
        "menu_y_platos",
        "En Restaurante La Casona ofrecemos menú ejecutivo, carnes a la parrilla, opciones vegetarianas y postres tradicionales. Atendemos de lunes a domingo de 12:00 PM a 10:00 PM."
    ),
    (
        "reservas_y_eventos",
        "Aceptamos reservas para eventos especiales con 24 horas de anticipación a través de nuestro sitio web o teléfono oficial. Capacidad máxima para salones de 80 personas."
    )
};

foreach (var art in articulosNuevos)
{
    // Generar el vector embedding de 1536 dimensiones
    var embedding = await aiService.GenerateEmbeddingAsync($"{art.Title}\n{art.Content}");

    context.KnowledgeBaseArticles.Add(new KnowledgeBaseArticle
    {
        Id = Guid.NewGuid(),
        TenantId = nuevoTenant.Id,
        Title = art.Title,
        Content = art.Content,
        Embedding = new Pgvector.Vector(embedding),
        IsActive = true
    });
}

await context.SaveChangesAsync();
```

---

## 🔌 PARTE 2: Cómo Incrustar el Widget en Cualquier Página Web

Una vez creado el Tenant en la plataforma, puedes integrar el widget conversacional y de radicación de PQRS en **cualquier sitio web público** pegando una simple etiqueta `<script>`.

---

### Paso 2.1: Código de Inserción HTML

Agrega el siguiente bloque de código antes del cierre de la etiqueta `</body>` en la página web de la empresa:

```html
<!-- Widget SaaS Multi-Tenant de PQRS e Inteligencia Artificial -->
<script 
  src="http://localhost:5050/widget/pqrs-widget.js" 
  data-tenant="la-casona-key-789">
</script>
```

> **Nota**: Reemplaza `la-casona-key-789` por el valor de `WidgetPublicKey` configurado para esa empresa y ajusta el dominio del servidor si está desplegado en producción (ej: `https://api.tusaas.com/widget/pqrs-widget.js`).

---

### Paso 2.2: Características del Widget Inyectado

1. **Aislamiento Shadow DOM**:
   - El archivo `pqrs-widget.js` crea un Shadow DOM (`attachShadow({ mode: 'open' })`) totalmente encapsulado.
   - **No interfiere con el CSS, Bootstrap, Tailwind o estilos globales** de la página web huésped.

2. **Doble Botón Flotante Automático**:
   - En la esquina inferior derecha aparecerán automáticamente dos lanzadores:
     - 🔵 **Lanzador Azul (`🤖 Consultar Asistente IA`)**: Abre el chatbot interactivo para resolver dudas mediante el RAG exclusivo de la empresa.
     - 🟢 **Lanzador Verde (`📝 Radicar PQRS`)**: Abre directamente el Formulario Oficial de Radicación de PQRS.

3. **Multi-Tenant Automático**:
   - Cada petición realizada desde el widget envía el encabezado HTTP `X-Widget-Key` con la clave provista en `data-tenant`.
   - El middleware del backend (`TenantResolutionMiddleware`) resuelve el `TenantId` automáticamente y aísla el contexto de datos.

---

## 💻 Ejemplo Práctico de una Página Web Externa

```html
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <title>Restaurante La Casona - Sitio Oficial</title>
</head>
<body>

  <h1>Bienvenido a Restaurante La Casona</h1>
  <p>Disfruta de la mejor gastronomía en un ambiente acogedor.</p>

  <!-- INYECCIÓN DEL WIDGET MULTI-TENANT -->
  <script 
    src="http://localhost:5050/widget/pqrs-widget.js" 
    data-tenant="la-casona-key-789">
  </script>

</body>
</html>
```

---

## 📊 PARTE 3: Acceso al Dashboard de Administración

Para gestionar los tickets radicados por los clientes del nuevo tenant:

1. Ingresa al Dashboard: `http://localhost:5050/dashboard/index.html`.
2. Inicia sesión con el correo registrado (ej: `admin@lacasona.com` / `Password123!`).
3. El panel detectará las credenciales y mostrará **exclusivamente la bandeja de PQRS, métricas y artículos RAG** de *Restaurante La Casona*.
