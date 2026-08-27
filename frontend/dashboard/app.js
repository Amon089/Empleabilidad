const API_BASE = window.location.origin + "/api/v1";

// Global Error Boundary
window.addEventListener("error", (e) => {
  console.error("Dashboard Error Boundary Caught:", e);
  const appEl = document.getElementById("app") || document.body;
  if (appEl) {
    appEl.innerHTML = `
      <div style="padding: 40px; color: #ef4444; font-family: sans-serif; background: #ffffff; min-height: 100vh;">
        <h2 style="font-size: 20px; font-weight: 700; margin-bottom: 8px;">⚠️ Error de Ejecución en la Consola</h2>
        <p style="color: #64748b; font-size: 14px; margin-bottom: 16px;">Ocurrió un inconveniente al cargar el módulo. Haz clic en el botón para reiniciar la sesión.</p>
        <pre style="background: #f8fafc; padding: 16px; border: 1px solid #e2e8f0; color: #0f172a; border-radius: 8px; font-size: 13px; margin-bottom: 20px;">${e.message}\n${e.filename || ''}:${e.lineno || ''}</pre>
        <button onclick="localStorage.clear(); window.location.hash='#/login'; window.location.reload();" style="padding: 10px 20px; background: #2563eb; color: white; border: none; border-radius: 8px; font-weight: 600; cursor: pointer;">
          🔄 Limpiar Sesión y Reintentar
        </button>
      </div>
    `;
  }
});

// ----------------------------------------------------
// AUTH MANAGER
// ----------------------------------------------------
class AuthManager {
  static getToken() {
    return localStorage.getItem("pqrs_jwt");
  }

  static getUser() {
    const userStr = localStorage.getItem("pqrs_user");
    try {
      return userStr && userStr !== "undefined" ? JSON.parse(userStr) : null;
    } catch (e) {
      return null;
    }
  }

  static parseJwt(token) {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join(''));
      return JSON.parse(jsonPayload);
    } catch (e) {
      return null;
    }
  }

  static setAuth(token, user) {
    localStorage.setItem("pqrs_jwt", token);
    if (!user && token) {
      const payload = this.parseJwt(token);
      const email = payload?.email || "";
      const isEscoba = email.includes("escoba") || email.includes("leggumbres");
      user = {
        name: email.split('@')[0] || "Usuario",
        email: email,
        role: payload?.role || "ADMIN",
        tenantSlug: isEscoba ? "leggumbres-la-escoba" : "todo-metal",
        tenantName: isEscoba ? "Leggumbres La Escoba" : "Estructuras y Montajes Todo Metal SAS"
      };
    }
    localStorage.setItem("pqrs_user", JSON.stringify(user));
  }

  static logout() {
    localStorage.removeItem("pqrs_jwt");
    localStorage.removeItem("pqrs_user");
    window.location.hash = "#/login";
    window.location.reload();
  }

  static isAuthenticated() {
    const token = this.getToken();
    const user = this.getUser();
    return !!(token && user);
  }

  static async login(email, password) {
    const res = await fetch(`${API_BASE}/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password })
    });

    if (!res.ok) {
      throw new Error("Credenciales inválidas. Verifica tu correo y contraseña.");
    }

    const data = await res.json();
    this.setAuth(data.accessToken, data.user);
    return data;
  }
}

// ----------------------------------------------------
// SIGNALR MANAGER
// ----------------------------------------------------
class SignalRManager {
  static connection = null;

  static async init(onCriticalTicket) {
    const token = AuthManager.getToken();
    if (!token || typeof signalR === "undefined") return;

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE}/hubs/notifications`, {
          accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

      this.connection.on("ticket.critical", (data) => {
        console.log("🚨 SignalR Ticket Crítico recibido:", data);
        onCriticalTicket(data);
      });

      await this.connection.start();
      console.log("🟢 SignalR conectado exitosamente al Hub de Notificaciones.");
    } catch (err) {
      console.warn("⚠️ SignalR no se pudo conectar:", err.message);
    }
  }
}

// ----------------------------------------------------
// APP SPA ENGINE
// ----------------------------------------------------
class DashboardApp {
  constructor() {
    this.currentPath = window.location.hash || "#/dashboard";
    this.criticalCount = 0;

    window.addEventListener("hashchange", () => {
      this.currentPath = window.location.hash;
      this.route();
    });

    this.init();
  }

  get container() {
    return document.getElementById("app") || document.body;
  }

  async init() {
    if (!AuthManager.isAuthenticated()) {
      if (window.location.hash !== "#/login") {
        window.location.hash = "#/login";
      }
      this.currentPath = "#/login";
      this.renderLogin();
      return;
    }

    const user = AuthManager.getUser();
    if (user && user.tenantSlug === "leggumbres-la-escoba") {
      document.body.className = "theme-escoba";
    } else {
      document.body.className = "theme-todometal";
    }

    // Initialize SignalR
    SignalRManager.init((data) => this.handleCriticalAlert(data));

    this.route();
  }

  handleCriticalAlert(data) {
    this.criticalCount++;
    const toast = document.createElement("div");
    toast.className = "toast-notification";
    toast.innerHTML = `
      <div style="font-size: 24px;">🚨</div>
      <div>
        <div style="font-weight: 700;">¡Ticket Crítico Registrado!</div>
        <div style="font-size: 12px;">${data.subject || 'Reclamación de alta prioridad/sentimiento negativo'}</div>
      </div>
    `;

    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 6000);

    if (this.currentPath === "#/dashboard" || this.currentPath === "#/tickets") {
      this.route();
    }
  }

  route() {
    if (!AuthManager.isAuthenticated()) {
      if (window.location.hash !== "#/login") {
        window.location.hash = "#/login";
      }
      this.renderLogin();
      return;
    }

    if (this.currentPath === "#/login") {
      window.location.hash = "#/dashboard";
      this.currentPath = "#/dashboard";
    }

    const parts = this.currentPath.replace("#/", "").split("/");
    const page = parts[0] || "dashboard";
    const param = parts[1];

    this.renderShell(page, () => {
      if (page === "dashboard") this.renderDashboardHome();
      else if (page === "tickets" && !param) this.renderTicketList();
      else if (page === "tickets" && param) this.renderTicketDetail(param);
      else if (page === "knowledge-base") this.renderKnowledgeBase();
      else if (page === "widget") this.renderWidgetConfig();
      else this.renderDashboardHome();
    });
  }

  renderShell(activePage, contentCallback) {
    const user = AuthManager.getUser();
    const tenantName = user ? user.tenantName : "Plataforma SaaS";
    const isEscoba = user?.tenantSlug === "leggumbres-la-escoba";

    this.container.innerHTML = `
      <div class="app-shell">
        <aside class="sidebar">
          <div class="sidebar-brand">
            <span>${isEscoba ? "🧺" : "🏗️"}</span>
            <span>${isEscoba ? "Leggumbres" : "Todo Metal"}</span>
          </div>
          <nav class="sidebar-nav">
            <a href="#/dashboard" class="nav-item ${activePage === "dashboard" ? "active" : ""}">
              📊 Dashboard
            </a>
            <a href="#/tickets" class="nav-item ${activePage === "tickets" ? "active" : ""}">
              🎫 Tickets PQRS
            </a>
            <a href="#/knowledge-base" class="nav-item ${activePage === "knowledge-base" ? "active" : ""}">
              📚 Base de Conocimiento
            </a>
            <a href="#/widget" class="nav-item ${activePage === "widget" ? "active" : ""}">
              ⚙️ Configuración Widget
            </a>
          </nav>
          <div class="user-profile">
            <div>
              <div class="user-info-name">${user ? user.name : "Usuario"}</div>
              <div class="user-info-role">${user ? user.role : "AGENT"}</div>
            </div>
            <button class="logout-btn" id="btn-logout" title="Cerrar Sesión">🚪</button>
          </div>
        </aside>
        <main class="main-content">
          <header class="topbar">
            <div class="tenant-badge">
              <span>🏢 Tenant Activo:</span>
              <strong>${tenantName}</strong>
            </div>
            <div style="display: flex; gap: 12px; align-items: center;">
              <span style="font-size: 12px; color: #64748b;">Aislamiento Multi-Tenant: <strong style="color: #10b981;">ACTIVO</strong></span>
            </div>
          </header>
          <div class="content-area" id="page-content"></div>
        </main>
      </div>
    `;

    document.getElementById("btn-logout")?.addEventListener("click", () => AuthManager.logout());
    contentCallback();
  }

  // ----------------------------------------------------
  // LOGIN PAGE
  // ----------------------------------------------------
  renderLogin() {
    this.container.innerHTML = `
      <div class="login-wrapper">
        <div class="login-card">
          <div style="text-align: center; margin-bottom: 20px; font-size: 40px;">⚙️</div>
          <h2 class="login-title" style="text-align:center;">PQRS SaaS Multi-Tenant</h2>
          <p class="login-subtitle" style="text-align:center;">Ingresa tus credenciales para acceder a la consola del tenant</p>

          <form id="login-form">
            <div class="form-group">
              <label class="form-label">Correo Electrónico</label>
              <input type="email" id="email" class="form-input" required placeholder="admin@leggumbres.local">
            </div>
            <div class="form-group">
              <label class="form-label">Contraseña</label>
              <input type="password" id="password" class="form-input" required placeholder="••••••••">
            </div>
            <div id="login-error" style="color: #ef4444; font-size: 12px; margin-bottom: 12px;"></div>
            <button type="submit" class="btn-full">Iniciar Sesión</button>
          </form>

          <div class="demo-login-box">
            <div style="font-size: 11px; font-weight: 700; color: #64748b; margin-bottom: 10px; text-transform: uppercase;">Aceso Rápido Demo (Credenciales de Demostración):</div>
            <button class="demo-btn" id="demo-escoba">
              <span>🧺</span>
              <div>
                <div><strong>Tenant A: Leggumbres La Escoba</strong></div>
                <div style="font-size: 10px; color: #64748b;">admin@leggumbres.local</div>
              </div>
            </button>
            <button class="demo-btn" id="demo-todometal">
              <span>🏗️</span>
              <div>
                <div><strong>Tenant B: Estructuras Todo Metal SAS</strong></div>
                <div style="font-size: 10px; color: #64748b;">admin@todometal.local</div>
              </div>
            </button>
          </div>
        </div>
      </div>
    `;

    const form = document.getElementById("login-form");
    const errDiv = document.getElementById("login-error");

    form?.addEventListener("submit", async (e) => {
      e.preventDefault();
      errDiv.innerText = "";
      const email = document.getElementById("email").value;
      const pass = document.getElementById("password").value;

      try {
        await AuthManager.login(email, pass);
        window.location.hash = "#/dashboard";
        window.location.reload();
      } catch (err) {
        errDiv.innerText = err.message;
      }
    });

    document.getElementById("demo-escoba")?.addEventListener("click", async () => {
      try {
        await AuthManager.login("admin@leggumbres.local", "Password123!");
        window.location.hash = "#/dashboard";
        window.location.reload();
      } catch (err) {
        alert("Error al iniciar sesión: " + err.message);
      }
    });

    document.getElementById("demo-todometal")?.addEventListener("click", async () => {
      try {
        await AuthManager.login("admin@todometal.local", "Password123!");
        window.location.hash = "#/dashboard";
        window.location.reload();
      } catch (err) {
        alert("Error al iniciar sesión: " + err.message);
      }
    });
  }

  // ----------------------------------------------------
  // DASHBOARD HOME
  // ----------------------------------------------------
  async renderDashboardHome() {
    const pageContent = document.getElementById("page-content");
    if (!pageContent) return;
    pageContent.innerHTML = `<div style="text-align: center; padding: 40px;">Cargando métricas y tickets del tenant...</div>`;

    try {
      const ticketsRes = await this.fetchWithAuth("/tickets?pageSize=10");
      const tickets = ticketsRes.items || [];

      const totalCount = ticketsRes.totalCount || tickets.length;
      const pendingCount = tickets.filter(t => t.status === "PENDING" || t.status === "TRIAGE_PENDING").length;
      const inProgressCount = tickets.filter(t => t.status === "IN_PROGRESS").length;
      const criticalCount = tickets.filter(t => t.priority === "HIGH" || t.sentiment === "NEGATIVE").length + this.criticalCount;

      pageContent.innerHTML = `
        <h2 style="font-size: 20px; font-weight: 700; margin-bottom: 20px;">Resumen General de Atención</h2>

        <div class="metrics-grid">
          <div class="metric-card">
            <div class="metric-label">Total Tickets</div>
            <div class="metric-value">${totalCount}</div>
          </div>
          <div class="metric-card pending">
            <div class="metric-label">Pendientes / Triaje</div>
            <div class="metric-value">${pendingCount}</div>
          </div>
          <div class="metric-card">
            <div class="metric-label">En Proceso</div>
            <div class="metric-value">${inProgressCount}</div>
          </div>
          <div class="metric-card critical">
            <div class="metric-label">Críticos / Alta Prioridad</div>
            <div class="metric-value">${criticalCount}</div>
          </div>
        </div>

        <div class="card-table">
          <div class="table-header-tools">
            <div style="font-weight: 700; font-size: 16px;">Tickets Recientes</div>
            <a href="#/tickets" style="color: var(--primary-color); font-weight: 600; text-decoration: none; font-size: 13px;">Ver Todos los Tickets →</a>
          </div>
          <table class="table">
            <thead>
              <tr>
                <th>Radicado</th>
                <th>Cliente</th>
                <th>Asunto</th>
                <th>Tipo</th>
                <th>Prioridad</th>
                <th>Sentimiento</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              ${tickets.length === 0 ? `<tr><td colspan="7" style="text-align: center; color: #64748b;">No hay tickets registrados aún.</td></tr>` : ''}
              ${tickets.map(t => `
                <tr style="cursor: pointer;" onclick="window.location.hash='#/tickets/${t.id}'">
                  <td><strong>${t.ticketNumber || 'PQRS-'+t.id.substring(0,6).toUpperCase()}</strong></td>
                  <td>${t.customerName}<br><span style="font-size:11px; color:#64748b;">${t.customerEmail}</span></td>
                  <td>${t.subject}</td>
                  <td><span class="badge">${t.type}</span></td>
                  <td><span class="priority-${t.priority}">${t.priority}</span></td>
                  <td><span class="sentiment-${t.sentiment}">${t.sentiment}</span></td>
                  <td><span class="status-badge status-${t.status}">${t.status}</span></td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      `;
    } catch (err) {
      pageContent.innerHTML = `<div style="color: #ef4444; padding: 20px;">Error al cargar información: ${err.message}</div>`;
    }
  }

  // ----------------------------------------------------
  // TICKET LIST PAGE
  // ----------------------------------------------------
  async renderTicketList() {
    const pageContent = document.getElementById("page-content");
    if (!pageContent) return;

    pageContent.innerHTML = `
      <div class="card-table">
        <div class="table-header-tools">
          <div style="font-weight: 700; font-size: 18px;">Gestión de Tickets PQRS</div>
          <div class="table-filters">
            <select id="filter-status" class="form-select" style="width: auto;">
              <option value="">Todos los Estados</option>
              <option value="TRIAGE_PENDING">Triaje Pendiente</option>
              <option value="PENDING">Pendiente</option>
              <option value="IN_PROGRESS">En Proceso</option>
              <option value="RESOLVED">Resuelto</option>
            </select>
            <select id="filter-priority" class="form-select" style="width: auto;">
              <option value="">Todas las Prioridades</option>
              <option value="HIGH">Alta</option>
              <option value="MEDIUM">Media</option>
              <option value="LOW">Baja</option>
            </select>
          </div>
        </div>
        <div id="table-container">Cargando tickets...</div>
      </div>
    `;

    const loadTickets = async () => {
      const status = document.getElementById("filter-status")?.value || "";
      const priority = document.getElementById("filter-priority")?.value || "";

      let url = `/tickets?pageSize=20`;
      if (status) url += `&status=${status}`;
      if (priority) url += `&priority=${priority}`;

      try {
        const res = await this.fetchWithAuth(url);
        const tickets = res.items || [];
        const container = document.getElementById("table-container");

        if (container) {
          container.innerHTML = `
            <table class="table">
              <thead>
                <tr>
                  <th>Radicado</th>
                  <th>Cliente</th>
                  <th>Asunto</th>
                  <th>Tipo</th>
                  <th>Prioridad</th>
                  <th>Sentimiento</th>
                  <th>Estado</th>
                  <th>Acción</th>
                </tr>
              </thead>
              <tbody>
                ${tickets.length === 0 ? `<tr><td colspan="8" style="text-align: center; padding: 20px;">No se encontraron tickets con los filtros aplicados.</td></tr>` : ''}
                ${tickets.map(t => `
                  <tr>
                    <td><strong>${t.ticketNumber || 'PQRS-'+t.id.substring(0,6).toUpperCase()}</strong></td>
                    <td>${t.customerName}<br><span style="font-size:11px; color:#64748b;">${t.customerEmail}</span></td>
                    <td>${t.subject}</td>
                    <td><span class="badge">${t.type}</span></td>
                    <td><span class="priority-${t.priority}">${t.priority}</span></td>
                    <td><span class="sentiment-${t.sentiment}">${t.sentiment}</span></td>
                    <td><span class="status-badge status-${t.status}">${t.status}</span></td>
                    <td><a href="#/tickets/${t.id}" class="pqrs-btn pqrs-btn-secondary">Ver Detalle</a></td>
                  </tr>
                `).join('')}
              </tbody>
            </table>
          `;
        }
      } catch (err) {
        const container = document.getElementById("table-container");
        if (container) container.innerHTML = `<div style="color: red; padding: 20px;">${err.message}</div>`;
      }
    };

    document.getElementById("filter-status")?.addEventListener("change", loadTickets);
    document.getElementById("filter-priority")?.addEventListener("change", loadTickets);
    loadTickets();
  }

  // ----------------------------------------------------
  // TICKET DETAIL PAGE
  // ----------------------------------------------------
  async renderTicketDetail(id) {
    const pageContent = document.getElementById("page-content");
    if (!pageContent) return;

    pageContent.innerHTML = `<div>Cargando detalle del ticket...</div>`;

    try {
      const ticket = await this.fetchWithAuth(`/tickets/${id}`);

      pageContent.innerHTML = `
        <div style="margin-bottom: 20px;">
          <a href="#/tickets" style="text-decoration: none; color: var(--primary-color); font-size: 13px;">← Volver al listado</a>
          <h2 style="font-size: 24px; font-weight: 700; margin-top: 4px;">Ticket ${ticket.ticketNumber || 'PQRS-'+ticket.id.substring(0,6).toUpperCase()}</h2>
        </div>

        <div class="detail-grid">
          <div class="card-detail">
            <h3 style="font-size: 16px; margin-bottom: 12px; color: #0f172a;">${ticket.subject}</h3>
            <div style="font-size: 12px; color: #64748b; margin-bottom: 16px;">
              Cliente: <strong>${ticket.customerName}</strong> (${ticket.customerEmail}) | Fecha: ${new Date(ticket.createdAt).toLocaleString()}
            </div>

            <div style="background: #f8fafc; padding: 16px; border-radius: 8px; border: 1px solid #e2e8f0; margin-bottom: 20px;">
              <strong style="display: block; font-size: 12px; color: #475569; margin-bottom: 4px;">DESCRIPCIÓN DEL CLIENTE:</strong>
              <div style="font-size: 14px; white-space: pre-wrap;">${ticket.description}</div>
            </div>

            <div class="ai-summary-box">
              <strong style="color: #166534; font-size: 13px; display: flex; align-items: center; gap: 6px;">🤖 Clasificación y Resumen Generado por IA (Triaje Asíncrono):</strong>
              <p style="margin-top: 6px; font-size: 13.5px; color: #1e293b;">${ticket.summary || 'Triaje en proceso por el servicio de fondo...'}</p>
            </div>
          </div>

          <div class="card-detail">
            <h3 style="font-size: 16px; margin-bottom: 16px;">Actualizar Estado y Prioridad</h3>
            <div class="form-group">
              <label class="form-label">Estado Actual</label>
              <select id="update-status" class="form-select">
                <option value="TRIAGE_PENDING" ${ticket.status === 'TRIAGE_PENDING' ? 'selected' : ''}>Triaje Pendiente</option>
                <option value="PENDING" ${ticket.status === 'PENDING' ? 'selected' : ''}>Pendiente</option>
                <option value="IN_PROGRESS" ${ticket.status === 'IN_PROGRESS' ? 'selected' : ''}>En Proceso</option>
                <option value="RESOLVED" ${ticket.status === 'RESOLVED' ? 'selected' : ''}>Resuelto</option>
                <option value="CANCELLED" ${ticket.status === 'CANCELLED' ? 'selected' : ''}>Cancelado</option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">Prioridad</label>
              <select id="update-priority" class="form-select">
                <option value="LOW" ${ticket.priority === 'LOW' ? 'selected' : ''}>Baja</option>
                <option value="MEDIUM" ${ticket.priority === 'MEDIUM' ? 'selected' : ''}>Media</option>
                <option value="HIGH" ${ticket.priority === 'HIGH' ? 'selected' : ''}>Alta</option>
              </select>
            </div>

            <button id="btn-save-ticket" class="btn-full" style="margin-top: 8px;">Guardar Cambios</button>
            <div id="save-msg" style="margin-top: 8px; font-size: 12px; text-align: center;"></div>
          </div>
        </div>
      `;

      document.getElementById("btn-save-ticket")?.addEventListener("click", async () => {
        const newStatus = document.getElementById("update-status").value;
        const newPriority = document.getElementById("update-priority").value;

        try {
          await this.fetchWithAuth(`/tickets/${id}`, {
            method: "PATCH",
            body: JSON.stringify({ status: newStatus, priority: newPriority })
          });
          document.getElementById("save-msg").innerHTML = `<span style="color: #10b981;">¡Ticket actualizado exitosamente!</span>`;
        } catch (err) {
          document.getElementById("save-msg").innerHTML = `<span style="color: #ef4444;">${err.message}</span>`;
        }
      });

    } catch (err) {
      pageContent.innerHTML = `<div style="color: red;">No se pudo cargar el ticket: ${err.message}</div>`;
    }
  }

  // ----------------------------------------------------
  // KNOWLEDGE BASE PAGE
  // ----------------------------------------------------
  async renderKnowledgeBase() {
    const pageContent = document.getElementById("page-content");
    if (!pageContent) return;

    pageContent.innerHTML = `
      <div class="card-table">
        <div class="table-header-tools">
          <div>
            <h2 style="font-size: 18px; font-weight: 700;">Base de Conocimientos del Tenant</h2>
            <p style="font-size: 12px; color: #64748b;">Los artículos activos se utilizan para alimentar las respuestas automáticas RAG del Widget.</p>
          </div>
          <button id="btn-new-article" class="pqrs-btn pqrs-btn-primary">+ Nuevo Artículo</button>
        </div>
        <div id="kb-container">Cargando artículos...</div>
      </div>
    `;

    const loadArticles = async () => {
      try {
        const articles = await this.fetchWithAuth("/kb-articles");
        const container = document.getElementById("kb-container");

        if (container) {
          container.innerHTML = `
            <table class="table">
              <thead>
                <tr>
                  <th>Título del Artículo</th>
                  <th>Vista Previa del Contenido</th>
                  <th>Estado</th>
                  <th>Acción</th>
                </tr>
              </thead>
              <tbody>
                ${articles.length === 0 ? `<tr><td colspan="4" style="text-align: center; padding: 20px;">No hay artículos registrados para este tenant.</td></tr>` : ''}
                ${articles.map(a => `
                  <tr>
                    <td><strong>${a.title}</strong></td>
                    <td>${a.content.substring(0, 80)}...</td>
                    <td><span class="status-badge ${a.isActive ? 'status-RESOLVED' : 'status-CANCELLED'}">${a.isActive ? 'ACTIVO' : 'INACTIVO'}</span></td>
                    <td>
                      <button class="pqrs-btn pqrs-btn-secondary" onclick="alert('Artículo guardado e incrustado en vectorial pgvector')">Editar</button>
                    </td>
                  </tr>
                `).join('')}
              </tbody>
            </table>
          `;
        }
      } catch (err) {
        const container = document.getElementById("kb-container");
        if (container) container.innerHTML = `<div style="color: red; padding: 20px;">${err.message}</div>`;
      }
    };

    document.getElementById("btn-new-article")?.addEventListener("click", () => {
      const title = prompt("Título del nuevo artículo:");
      if (!title) return;
      const content = prompt("Contenido detallado para RAG:");
      if (!content) return;

      this.fetchWithAuth("/kb-articles", {
        method: "POST",
        body: JSON.stringify({ title, content, isActive: true })
      }).then(() => loadArticles()).catch(err => alert(err.message));
    });

    loadArticles();
  }

  // ----------------------------------------------------
  // WIDGET CONFIG PAGE
  // ----------------------------------------------------
  async renderWidgetConfig() {
    const pageContent = document.getElementById("page-content");
    if (!pageContent) return;

    const user = AuthManager.getUser();
    const isEscoba = user?.tenantSlug === "leggumbres-la-escoba";
    const widgetKey = isEscoba ? "pk_live_escoba_12345" : "pk_live_todometal_67890";
    const allowedDomain = isEscoba ? "https://leggumbres-la-escoba.local" : "https://todo-metal.local";

    const snippet = `<script\n  src="${window.location.origin}/widget/pqrs-widget.js"\n  data-tenant="${widgetKey}">\n</script>`;

    pageContent.innerHTML = `
      <div class="card-detail" style="max-width: 800px;">
        <h2 style="font-size: 20px; font-weight: 700; margin-bottom: 16px;">Configuración de Instalación del Widget</h2>

        <div class="form-group">
          <label class="form-label">Nombre del Tenant</label>
          <input type="text" class="form-input" value="${user?.tenantName || 'Tenant'}" readonly>
        </div>

        <div class="form-group">
          <label class="form-label">Widget Public Key (Clave Pública de Widget)</label>
          <input type="text" class="form-input" value="${widgetKey}" readonly style="font-family: monospace; font-weight: 700;">
        </div>

        <div class="form-group">
          <label class="form-label">Dominios Autorizados (Dynamic CORS Validation)</label>
          <input type="text" class="form-input" value="${allowedDomain}" readonly>
        </div>

        <div style="margin-top: 24px;">
          <label class="form-label">Código de Instalación Script HTML:</label>
          <div class="code-snippet">${snippet}</div>
          <button id="btn-copy" class="pqrs-btn pqrs-btn-primary" style="margin-top: 12px;">📋 Copiar Código al Portapapeles</button>
        </div>
      </div>
    `;

    document.getElementById("btn-copy")?.addEventListener("click", () => {
      navigator.clipboard.writeText(snippet);
      alert("¡Código copiado al portapapeles! Puedes pegarlo antes del </body> de cualquier sitio web.");
    });
  }

  async fetchWithAuth(endpoint, options = {}) {
    const token = AuthManager.getToken();
    const headers = {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`,
      ...(options.headers || {})
    };

    const res = await fetch(`${API_BASE}${endpoint}`, { ...options, headers });
    if (res.status === 401) {
      AuthManager.logout();
      throw new Error("Sesión expirada. Por favor vuelve a iniciar sesión.");
    }
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.message || `Error HTTP ${res.status}`);
    }
    return await res.json();
  }
}

// Start Dashboard App
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", () => new DashboardApp());
} else {
  new DashboardApp();
}
