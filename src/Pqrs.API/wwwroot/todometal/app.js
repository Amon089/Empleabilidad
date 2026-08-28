/**
 * Estructuras y Montajes Todo Metal SAS - Frontend Application Logic
 */

document.addEventListener("DOMContentLoaded", () => {
  renderServices();
  renderProjects();
});

// Navigation
function switchView(viewId) {
  document.querySelectorAll(".view-section").forEach((sec) => sec.classList.remove("active"));
  const target = document.getElementById(`view-${viewId}`);
  if (target) {
    target.classList.add("active");
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  document.querySelectorAll(".nav-link").forEach((link) => {
    link.classList.remove("active");
    if (link.getAttribute("href") === `#${viewId}`) {
      link.classList.add("active");
    }
  });
}

// Render Services
function renderServices() {
  const homeGrid = document.getElementById("home-services-grid");
  const fullGrid = document.getElementById("full-services-grid");
  const services = window.TODOMETAL_DEMO_DATA.services;

  const html = services
    .map(
      (s) => `
    <div class="card srv-card">
      <div class="srv-icon">${s.icon}</div>
      <div>
        <h3>${s.title}</h3>
        <p>${s.summary}</p>
        <ul class="srv-list">
          ${s.details.map((d) => `<li>✓ ${d}</li>`).join("")}
        </ul>
        <button class="btn btn-outline btn-sm" onclick="switchView('quote')">📐 Request Quote for ${s.title.split(" ")[0]}</button>
      </div>
    </div>
  `
    )
    .join("");

  if (homeGrid) homeGrid.innerHTML = html;
  if (fullGrid) fullGrid.innerHTML = html;
}

// Render Projects Portfolio
function renderProjects() {
  const homeGrid = document.getElementById("home-projects-grid");
  const fullGrid = document.getElementById("full-projects-grid");
  const projects = window.TODOMETAL_DEMO_DATA.projects;

  const html = projects
    .map(
      (p) => `
    <div class="card proj-card">
      <div class="proj-img-wrapper">
        <img src="${p.image}" alt="${p.title}">
        <span class="proj-badge">${p.category}</span>
      </div>
      <div class="proj-body">
        <h4>${p.title}</h4>
        <p class="proj-client">🏛️ ${p.client}</p>
        <p class="proj-tonnage">⚖️ ${p.tonnage} (${p.year})</p>
        <p class="proj-desc">${p.description}</p>
        <span class="badge badge-success">${p.status}</span>
      </div>
    </div>
  `
    )
    .join("");

  if (homeGrid) homeGrid.innerHTML = html;
  if (fullGrid) fullGrid.innerHTML = html;
}

// Quote Form Submit
async function handleQuoteSubmit(event) {
  event.preventDefault();
  const btn = document.getElementById("btn-submit-quote");
  btn.innerText = "⏳ Processing Request...";
  btn.disabled = true;

  await new Promise((resolve) => setTimeout(resolve, 800));

  const refId = "COT-" + Math.floor(10000 + Math.random() * 90000);
  const newRequest = {
    id: refId,
    projectType: document.getElementById("q-type").value,
    dimensions: document.getElementById("q-dimensions").value,
    location: document.getElementById("q-location").value,
    status: "En Revisión Técnica",
    date: new Date().toISOString().split("T")[0]
  };

  if (window.TODOMETAL_DEMO_DATA.demoRequests) {
    window.TODOMETAL_DEMO_DATA.demoRequests.unshift(newRequest);
  }

  document.getElementById("quote-ref-number").innerText = refId;
  document.getElementById("quote-form").classList.add("hidden");
  document.getElementById("quote-success-card").classList.remove("hidden");

  btn.innerText = "Submit Request";
  btn.disabled = false;
}

function resetQuoteForm() {
  document.getElementById("quote-form").reset();
  document.getElementById("quote-form").classList.remove("hidden");
  document.getElementById("quote-success-card").classList.add("hidden");
}

// Public PQRS Submit
async function handlePqrsSubmit(event) {
  event.preventDefault();
  const btn = document.getElementById("btn-submit-pqrs");
  btn.innerText = "⏳ Radicando...";
  btn.disabled = true;

  const ticketData = {
    type: document.getElementById("pqrs-type").value,
    subject: document.getElementById("pqrs-subject").value,
    description: document.getElementById("pqrs-description").value,
    contactEmail: document.getElementById("pqrs-email").value,
    contactName: document.getElementById("pqrs-name").value
  };

  try {
    const res = await window.SaaSApi.submitTicket("todo-metal-key-456", ticketData);
    document.getElementById("radicado-number").innerText = res.ticketNumber || "PQRS-" + Math.floor(1000 + Math.random() * 9000);
    document.getElementById("pqrs-form").classList.add("hidden");
    document.getElementById("pqrs-success-card").classList.remove("hidden");
  } catch (err) {
    alert("Error al radicar PQRS: " + err.message);
  } finally {
    btn.innerText = "📩 Submit Corporate PQRS";
    btn.disabled = false;
  }
}

function resetPqrsForm() {
  document.getElementById("pqrs-form").reset();
  document.getElementById("pqrs-form").classList.remove("hidden");
  document.getElementById("pqrs-success-card").classList.add("hidden");
}

function openWidgetChat() {
  if (window.PqrsWidget) {
    window.PqrsWidget.toggleChat();
  }
}
