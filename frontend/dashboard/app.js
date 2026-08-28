/**
 * Dashboard Administrativo SaaS Multi-Tenant - Application Logic
 */

let activeTenant = "leggumbres";
let ticketsData = [];
let kbData = [];
let signalrConnection = null;

const DEMO_TENANT_TICKETS = {
  leggumbres: [
    {
      id: "tk-101",
      ticketNumber: "PQRS-8490",
      contactName: "María López",
      contactEmail: "maria.lopez@ejemplo.com",
      subject: "Retraso de 2 horas en entrega de pedido de verduras",
      description: "El domiciliario llegó con retraso y dos plátanos venían magullados.",
      type: "Queja",
      priority: "HIGH",
      sentiment: "NEGATIVE",
      aiSummary: "Cliente reporta retraso en domicilio y producto magullado.",
      status: "PENDING",
      date: "2026-08-27"
    },
    {
      id: "tk-102",
      ticketNumber: "PQRS-8495",
      contactName: "Carlos Pérez",
      contactEmail: "carlos.perez@ejemplo.com",
      subject: "Excelente calidad en la papa sabanera y aguacate Hass",
      description: "Quería felicitar al equipo por la frescura del producto recibido hoy.",
      type: "Felicitación",
      priority: "LOW",
      sentiment: "POSITIVE",
      aiSummary: "Cliente felicita por excelente calidad y frescura.",
      status: "RESOLVED",
      date: "2026-08-26"
    }
  ],
  "todo-metal": [
    {
      id: "tk-201",
      ticketNumber: "PQRS-9201",
      contactName: "Ing. Roberto Silva",
      contactEmail: "rsilva@constructora.com",
      subject: "Solicitud de dossier de calidad e inspección AWS D1.1",
      description: "Requerimos los certificados de tintas penetrantes del puente vehicular.",
      type: "Petición",
      priority: "MEDIUM",
      sentiment: "NEUTRAL",
      aiSummary: "Solicitud de certificados de inspección de soldadura en obra.",
      status: "IN_PROGRESS",
      date: "2026-08-27"
    },
    {
      id: "tk-202",
      ticketNumber: "PQRS-9205",
      contactName: "Arq. Diana Morales",
      contactEmail: "dmorales@alcaldia.gov.co",
      subject: "Inconveniente con acceso de grúas a la obra del parque industrial",
      description: "Retraso en izamiento por falta de permiso de movilización vial.",
      type: "Queja",
      priority: "HIGH",
      sentiment: "NEGATIVE",
      aiSummary: "Retraso en izamiento de estructura por permisos de grúa.",
      status: "PENDING",
      date: "2026-08-27"
    }
  ]
};

const DEMO_TENANT_KB = {
  leggumbres: [
    { id: "kb-1", title: "informacion_empresa", content: "Leggumbres La Escoba es una empresa dedicada a conectar campesinos con hogares...", status: "Activo" },
    { id: "kb-2", title: "productos", content: "Comercializamos productos agrícolas frescos: Papa, Yuca, Plátano, Tomate, Cebolla...", status: "Activo" },
    { id: "kb-3", title: "pedidos_y_entregas", content: "Entregas a domicilio de 6:00 AM a 2:00 PM o recogida en centro de acopio bodega 12...", status: "Activo" }
  ],
  "todo-metal": [
    { id: "kb-10", title: "informacion_empresa", content: "Estructuras y Montajes Todo Metal SAS desarrolla proyectos de ingeniería metálica...", status: "Activo" },
    { id: "kb-11", title: "servicios", content: "Fabricación y montaje de estructuras metálicas, puentes, naves industriales y obras civiles...", status: "Activo" },
    { id: "kb-12", title: "estructuras_y_puentes", content: "Puentes vehiculares sismorresistentes bajo norma NSR-10 y soldadura AWS D1.1...", status: "Activo" }
  ]
};

document.addEventListener("DOMContentLoaded", () => {
  checkAdminAuth();
});

function setLoginPreset(email, pass) {
  document.getElementById("admin-email").value = email;
  document.getElementById("admin-pass").value = pass;
}

function checkAdminAuth() {
  const user = window.SaaSAuth.getUser("saas_admin");
  const mainContent = document.getElementById("dash-main-content");
  const authHeader = document.getElementById("admin-auth-header");
  const modalAuth = document.getElementById("modal-admin-auth");
  const tenantSelect = document.getElementById("tenant-select");

  if (user) {
    if (mainContent) mainContent.classList.remove("hidden");
    if (modalAuth) modalAuth.classList.add("hidden");

    // Enforce Tenant Scope depending on user credentials
    if (user.email.includes("leggumbres")) {
      activeTenant = "leggumbres";
      if (tenantSelect) {
        tenantSelect.value = "leggumbres";
        tenantSelect.disabled = true;
      }
    } else if (user.email.includes("todometal")) {
      activeTenant = "todo-metal";
      if (tenantSelect) {
        tenantSelect.value = "todo-metal";
        tenantSelect.disabled = true;
      }
    } else {
      // Global SaaS Admin
      if (tenantSelect) {
        tenantSelect.disabled = false;
        activeTenant = tenantSelect.value || "leggumbres";
      }
    }

    if (authHeader) {
      const scopeBadge = user.email.includes("leggumbres")
        ? "🥦 Admin Leggumbres"
        : user.email.includes("todometal")
        ? "🏗️ Admin Todo Metal"
        : "⚡ Super Admin Global";

      authHeader.innerHTML = `
        <div class="admin-user-pill" style="display:flex; align-items:center; gap:10px; background:#f1f5f9; padding:6px 12px; border-radius:8px; font-size:0.85rem;">
          <span style="font-weight:700;">${scopeBadge} (${user.email})</span>
          <button class="btn btn-outline-danger btn-sm" onclick="logoutAdmin()">🚪 Salir</button>
        </div>
      `;
    }

    loadTenantData();
    initSignalR();
  } else {
    if (mainContent) mainContent.classList.add("hidden");
    if (modalAuth) modalAuth.classList.remove("hidden");
    if (authHeader) authHeader.innerHTML = "";
  }
}

async function handleAdminLogin(event) {
  event.preventDefault();
  const email = document.getElementById("admin-email").value.trim().toLowerCase();
  const pass = document.getElementById("admin-pass").value;

  if (!email || !pass) {
    alert("Ingresa tu correo y contraseña.");
    return;
  }

  let scopeTenant = "saas_global";
  if (email.includes("leggumbres")) scopeTenant = "leggumbres";
  if (email.includes("todometal")) scopeTenant = "todo-metal";

  try {
    await window.SaaSAuth.loginSimulated(email, pass, "saas_admin", scopeTenant);
    checkAdminAuth();
  } catch (err) {
    alert(err.message);
  }
}

function logoutAdmin() {
  window.SaaSAuth.clearSession("saas_admin");
  checkAdminAuth();
}

function changeActiveTenant() {
  const tenantSelect = document.getElementById("tenant-select");
  if (tenantSelect && !tenantSelect.disabled) {
    activeTenant = tenantSelect.value;
    loadTenantData();
  }
}

function loadTenantData() {
  ticketsData = [...(DEMO_TENANT_TICKETS[activeTenant] || [])];
  kbData = [...(DEMO_TENANT_KB[activeTenant] || [])];
  renderKpis();
  renderTickets();
  renderKb();
}

// KPI Metrics Calculations
function renderKpis() {
  const total = ticketsData.length;
  const pending = ticketsData.filter((t) => t.status === "PENDING").length;
  const progress = ticketsData.filter((t) => t.status === "IN_PROGRESS").length;
  const resolved = ticketsData.filter((t) => t.status === "RESOLVED").length;
  const high = ticketsData.filter((t) => t.priority === "HIGH").length;
  const negative = ticketsData.filter((t) => t.sentiment === "NEGATIVE").length;

  document.getElementById("kpi-total").innerText = total;
  document.getElementById("kpi-pending").innerText = pending;
  document.getElementById("kpi-progress").innerText = progress;
  document.getElementById("kpi-resolved").innerText = resolved;
  document.getElementById("kpi-high").innerText = high;
  document.getElementById("kpi-negative").innerText = negative;
}

// Render Tickets Inbox Table
function renderTickets() {
  const tbody = document.getElementById("tickets-tbody");
  if (!tbody) return;

  const filterStatus = document.getElementById("filter-status")?.value || "ALL";
  const filterPriority = document.getElementById("filter-priority")?.value || "ALL";
  const filterSentiment = document.getElementById("filter-sentiment")?.value || "ALL";

  let filtered = [...ticketsData];

  if (filterStatus !== "ALL") filtered = filtered.filter((t) => t.status === filterStatus);
  if (filterPriority !== "ALL") filtered = filtered.filter((t) => t.priority === filterPriority);
  if (filterSentiment !== "ALL") filtered = filtered.filter((t) => t.sentiment === filterSentiment);

  if (!filtered.length) {
    tbody.innerHTML = `<tr><td colspan="8" class="text-center p-4">No hay PQRS que coincidan con los filtros.</td></tr>`;
    return;
  }

  tbody.innerHTML = filtered
    .map(
      (t) => `
    <tr>
      <td><strong>${t.ticketNumber}</strong></td>
      <td>
        <div>${t.contactName}</div>
        <small class="text-muted">${t.contactEmail}</small>
      </td>
      <td>${t.subject}</td>
      <td><span class="badge badge-secondary">${t.type}</span></td>
      <td>${getPriorityBadge(t.priority)}</td>
      <td>${getSentimentBadge(t.sentiment)}</td>
      <td>${getStatusBadge(t.status)}</td>
      <td>
        <button class="btn btn-outline btn-sm" onclick="openTicketModal('${t.id}')">Ver Detalle</button>
      </td>
    </tr>
  `
    )
    .join("");
}

function getPriorityBadge(priority) {
  switch (priority) {
    case "HIGH": return '<span class="badge badge-danger">Alta</span>';
    case "MEDIUM": return '<span class="badge badge-warning">Media</span>';
    case "LOW": return '<span class="badge badge-info">Baja</span>';
    default: return priority;
  }
}

function getSentimentBadge(sentiment) {
  switch (sentiment) {
    case "NEGATIVE": return '<span class="badge badge-danger">Negativo 😡</span>';
    case "NEUTRAL": return '<span class="badge badge-secondary">Neutro 😐</span>';
    case "POSITIVE": return '<span class="badge badge-success">Positivo 😃</span>';
    default: return sentiment;
  }
}

function getStatusBadge(status) {
  switch (status) {
    case "PENDING": return '<span class="badge badge-warning">Pendiente</span>';
    case "IN_PROGRESS": return '<span class="badge badge-info">En Proceso</span>';
    case "RESOLVED": return '<span class="badge badge-success">Resuelta</span>';
    default: return status;
  }
}

// Render RAG Knowledge Base Table
function renderKb() {
  const tbody = document.getElementById("kb-tbody");
  if (!tbody) return;

  if (!kbData.length) {
    tbody.innerHTML = `<tr><td colspan="4" class="text-center p-4">No hay artículos registrados para este tenant.</td></tr>`;
    return;
  }

  tbody.innerHTML = kbData
    .map(
      (article) => `
    <tr>
      <td><strong>${article.title}</strong></td>
      <td>${article.content.substring(0, 80)}...</td>
      <td><span class="badge badge-success">${article.status}</span></td>
      <td>
        <button class="btn btn-outline-danger btn-sm" onclick="deleteKbArticle('${article.id}')">Eliminar</button>
      </td>
    </tr>
  `
    )
    .join("");
}

// Tabs Switching
function switchDashTab(tabId) {
  document.querySelectorAll(".dash-tab-btn").forEach((btn) => btn.classList.remove("active"));
  document.querySelectorAll(".dash-tab-content").forEach((c) => c.classList.remove("active"));

  const targetTab = document.getElementById(`dashtab-${tabId}`);
  if (targetTab) targetTab.classList.add("active");

  const evtBtn = event ? event.target : null;
  if (evtBtn) evtBtn.classList.add("active");
}

// Ticket Modal Drawer
function openTicketModal(ticketId) {
  const ticket = ticketsData.find((t) => t.id === ticketId);
  if (!ticket) return;

  document.getElementById("det-radicado").innerText = ticket.ticketNumber;
  document.getElementById("det-body").innerHTML = `
    <div style="display:flex; flex-direction:column; gap:12px;">
      <div style="background:#f8fafc; padding:12px; border-radius:8px; border:1px solid #e2e8f0;">
        <p><strong>Solicitante:</strong> ${ticket.contactName} (${ticket.contactEmail})</p>
        <p><strong>Fecha de Radicación:</strong> ${ticket.date}</p>
        <p><strong>Tipo:</strong> ${ticket.type}</p>
      </div>

      <div style="background:#eff6ff; padding:12px; border-radius:8px; border:1px solid #bfdbfe;">
        <p style="font-weight:700; color:#1e40af;">🤖 Resumen de Triaje IA:</p>
        <p style="font-size:0.9rem; color:#1e3a8a;">${ticket.aiSummary}</p>
        <div style="display:flex; gap:8px; margin-top:8px;">
          ${getPriorityBadge(ticket.priority)}
          ${getSentimentBadge(ticket.sentiment)}
        </div>
      </div>

      <div>
        <p><strong>Asunto:</strong> ${ticket.subject}</p>
        <p style="margin-top:6px; color:#475569;">${ticket.description}</p>
      </div>

      <div class="form-group" style="margin-top:12px;">
        <label>Cambiar Estado de PQRS:</label>
        <select id="change-status-select" class="form-group" style="padding:8px; width:100%; border-radius:6px; border:1px solid #cbd5e1;">
          <option value="PENDING" ${ticket.status === "PENDING" ? "selected" : ""}>Pendiente</option>
          <option value="IN_PROGRESS" ${ticket.status === "IN_PROGRESS" ? "selected" : ""}>En Proceso</option>
          <option value="RESOLVED" ${ticket.status === "RESOLVED" ? "selected" : ""}>Resuelta</option>
        </select>
      </div>
    </div>
  `;

  document.getElementById("det-footer").innerHTML = `
    <button class="btn btn-outline" onclick="closeTicketModal()">Cancelar</button>
    <button class="btn btn-primary" onclick="updateTicketStatus('${ticket.id}')">💾 Actualizar Estado</button>
  `;

  document.getElementById("modal-ticket-detail").classList.remove("hidden");
}

function updateTicketStatus(ticketId) {
  const newStatus = document.getElementById("change-status-select").value;
  const ticket = ticketsData.find((t) => t.id === ticketId);
  if (ticket) {
    ticket.status = newStatus;
    renderKpis();
    renderTickets();
    closeTicketModal();
  }
}

function closeTicketModal() {
  document.getElementById("modal-ticket-detail").classList.add("hidden");
}

// KB Article Modals
function openNewKbModal() {
  document.getElementById("kb-form").reset();
  document.getElementById("modal-kb").classList.remove("hidden");
}

function closeKbModal() {
  document.getElementById("modal-kb").classList.add("hidden");
}

function handleKbSubmit(event) {
  event.preventDefault();
  const title = document.getElementById("kb-title").value.trim();
  const content = document.getElementById("kb-content").value.trim();

  if (!title || !content) return;

  const newArticle = {
    id: "kb-" + Date.now(),
    title,
    content,
    status: "Activo"
  };

  kbData.unshift(newArticle);
  renderKb();
  closeKbModal();
}

function deleteKbArticle(kbId) {
  if (confirm("¿Estás seguro de eliminar este artículo de la Base de Conocimientos?")) {
    kbData = kbData.filter((k) => k.id !== kbId);
    renderKb();
  }
}

// SignalR Setup for High Priority / Negative Sentiment Tickets
function initSignalR() {
  if (typeof signalR === "undefined") return;
  if (signalrConnection) return;

  try {
    signalrConnection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/tickets")
      .withAutomaticReconnect()
      .build();

    signalrConnection.on("ReceiveCriticalTicket", (data) => {
      document.getElementById("sig-radicado").innerText = data.ticketNumber || "PQRS-CRITICA";
      document.getElementById("sig-subject").innerText = data.subject || "Ticket Crítico en tiempo real";
      document.getElementById("modal-signalr-alert").classList.remove("hidden");
    });

    signalrConnection.start().catch(() => {});
  } catch (e) {}
}

function dismissSignalRAlert() {
  document.getElementById("modal-signalr-alert").classList.add("hidden");
}
