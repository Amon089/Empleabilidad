/**
 * Leggumbres La Escoba - Frontend Application Logic
 */

let cart = [];

document.addEventListener("DOMContentLoaded", () => {
  renderCatalog();
});

function getDemoProducts() {
  const data = window.LEGGUMBRES_DEMO_DATA || window.LEGUMBRES_DEMO_DATA;
  if (!data || !data.products) return [];
  return data.products;
}

// Navigation
function switchView(viewId, targetElementId) {
  document.querySelectorAll(".view-section").forEach((sec) => sec.classList.remove("active"));
  const target = document.getElementById(`view-${viewId}`);
  if (target) {
    target.classList.add("active");
    if (targetElementId) {
      const el = document.getElementById(targetElementId);
      if (el) el.scrollIntoView({ behavior: "smooth" });
    } else {
      window.scrollTo({ top: 0, behavior: "smooth" });
    }
  }

  document.querySelectorAll(".nav-link").forEach((link) => {
    link.classList.remove("active");
    if (link.getAttribute("href") === `#${viewId}`) {
      link.classList.add("active");
    }
  });
}

// Render Catalog
function renderCatalog() {
  const homeGrid = document.getElementById("home-catalog-grid");
  const fullGrid = document.getElementById("full-catalog-grid");
  const products = getDemoProducts();

  const homeHtml = products
    .slice(0, 6)
    .map((p) => createProductCard(p))
    .join("");

  const fullHtml = products.map((p) => createProductCard(p)).join("");

  if (homeGrid) homeGrid.innerHTML = homeHtml;
  if (fullGrid) fullGrid.innerHTML = fullHtml;
}

function createProductCard(p) {
  const title = p.name || p.title || "Producto Orgánico";
  const farmer = p.farmer || p.origin || "Campesinos Locales";
  const badgeText = p.badge || p.category || "Fresco";

  return `
    <div class="card prod-card">
      <div class="prod-img-wrapper">
        <img src="${p.image}" alt="${title}">
        <span class="badge badge-success prod-cat">${badgeText}</span>
      </div>
      <div class="prod-body">
        <h4>${title}</h4>
        <p class="prod-origin">🌱 Productores: ${farmer}</p>
        <p class="prod-desc">${p.description}</p>
        <div class="prod-footer">
          <span class="prod-price">$${p.price.toLocaleString()} COP / ${p.unit}</span>
          <button class="btn btn-primary btn-sm" onclick="addToCart('${p.id}')">+ Agregar</button>
        </div>
      </div>
    </div>
  `;
}

function filterCatalog(category) {
  document.querySelectorAll(".filter-chip").forEach((btn) => btn.classList.remove("active"));
  if (event) event.target.classList.add("active");

  const products = getDemoProducts();
  let filtered = products;

  if (category !== "ALL") {
    const catLower = category.toLowerCase();
    filtered = products.filter((p) => {
      const pCat = (p.category || "").toLowerCase();
      if (catLower.includes("verdura") && (pCat.includes("verdura") || pCat.includes("tuberculo"))) return true;
      if (catLower.includes("fruta") && pCat.includes("fruta")) return true;
      if (catLower.includes("grano") && pCat.includes("grano")) return true;
      return pCat.includes(catLower);
    });
  }

  const fullGrid = document.getElementById("full-catalog-grid");
  if (fullGrid) fullGrid.innerHTML = filtered.map((p) => createProductCard(p)).join("");
}

// Cart Functionality
function addToCart(productId) {
  const products = getDemoProducts();
  const product = products.find((p) => p.id === productId);
  if (!product) return;

  const existing = cart.find((item) => item.id === productId);
  if (existing) {
    existing.quantity++;
  } else {
    cart.push({ ...product, quantity: 1 });
  }

  updateCartBadge();
  alert(`¡${product.name || product.title} agregado a tu canasta campesina!`);
}

function updateCartBadge() {
  const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
  const badge = document.getElementById("cart-count");
  if (badge) badge.innerText = totalItems;
}

function openCartModal() {
  renderCartModal();
  document.getElementById("modal-cart").classList.remove("hidden");
}

function closeCartModal(event) {
  if (event && event.target.className !== "modal-backdrop") return;
  document.getElementById("modal-cart").classList.add("hidden");
}

function renderCartModal() {
  const container = document.getElementById("cart-items-container");
  const totalEl = document.getElementById("cart-total-price");

  if (!cart.length) {
    container.innerHTML = `<p class="text-center text-muted p-4">Tu canasta está vacía. ¡Agrega productos frescos del campo!</p>`;
    totalEl.innerText = "$0 COP";
    return;
  }

  let total = 0;
  container.innerHTML = cart
    .map((item) => {
      const subtotal = item.price * item.quantity;
      total += subtotal;
      const title = item.name || item.title;
      return `
      <div style="display:flex; justify-content:space-between; align-items:center; padding:10px 0; border-bottom:1px solid #f1f5f9;">
        <div>
          <strong>${title}</strong>
          <br><small class="text-muted">$${item.price.toLocaleString()} x ${item.quantity} ${item.unit}</small>
        </div>
        <div style="display:flex; align-items:center; gap:8px;">
          <strong style="color:#166534;">$${subtotal.toLocaleString()} COP</strong>
          <button class="btn btn-outline-danger btn-sm" onclick="removeFromCart('${item.id}')">&times;</button>
        </div>
      </div>
    `;
    })
    .join("");

  totalEl.innerText = `$${total.toLocaleString()} COP`;
}

function removeFromCart(id) {
  cart = cart.filter((item) => item.id !== id);
  updateCartBadge();
  renderCartModal();
}

function checkoutCart() {
  if (!cart.length) {
    alert("Tu canasta está vacía.");
    return;
  }

  const orderId = "PED-" + Math.floor(10000 + Math.random() * 90000);
  alert(`¡Pedido ${orderId} registrado con éxito! Tu mercado fresco se entregará en tu domicilio.`);
  cart = [];
  updateCartBadge();
  document.getElementById("modal-cart").classList.add("hidden");
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
    const res = await window.SaaSApi.submitTicket("leggumbres-key-123", ticketData);
    document.getElementById("radicado-number").innerText = res.ticketNumber || "PQRS-" + Math.floor(1000 + Math.random() * 9000);
    document.getElementById("pqrs-form").classList.add("hidden");
    document.getElementById("pqrs-success-card").classList.remove("hidden");
  } catch (err) {
    alert("Error al radicar PQRS: " + err.message);
  } finally {
    btn.innerText = "📩 Enviar PQRS Oficial";
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
