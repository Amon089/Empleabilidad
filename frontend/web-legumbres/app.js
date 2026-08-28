/**
 * Leggumbres La Escoba - Frontend Application Logic
 */

let cart = [];
let currentCategory = "all";

document.addEventListener("DOMContentLoaded", () => {
  initFarmers();
  initCategoryPills();
  renderCatalog();
  updateAuthUI();
});

// View Navigation
function switchView(viewId, scrollTarget = null) {
  document.querySelectorAll(".view-section").forEach((sec) => {
    sec.classList.remove("active");
  });

  const targetView = document.getElementById(`view-${viewId}`);
  if (targetView) {
    targetView.classList.add("active");
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  if (scrollTarget) {
    setTimeout(() => {
      const elem = document.getElementById(scrollTarget);
      if (elem) elem.scrollIntoView({ behavior: "smooth" });
    }, 100);
  }

  // Update nav active styles
  document.querySelectorAll(".nav-link").forEach((link) => {
    link.classList.remove("active");
    if (link.getAttribute("href") === `#${viewId}`) {
      link.classList.add("active");
    }
  });

  if (viewId === "customer") {
    renderCustomerPortal();
  }
}

// Farmers Section
function initFarmers() {
  const container = document.getElementById("farmers-container");
  if (!container) return;

  const farmers = window.LEGGUMBRES_DEMO_DATA.farmers;
  container.innerHTML = farmers
    .map(
      (f) => `
    <div class="card farmer-card">
      <div class="farmer-avatar">👨‍🌾</div>
      <div>
        <h4>${f.name}</h4>
        <span class="badge badge-success">${f.location}</span>
        <p class="farmer-spec">Especialidad: ${f.specialty}</p>
        <blockquote class="farmer-quote">"${f.quote}"</blockquote>
      </div>
    </div>
  `
    )
    .join("");
}

// Category Pills
function initCategoryPills() {
  const container = document.getElementById("category-pills");
  if (!container) return;

  const categories = window.LEGGUMBRES_DEMO_DATA.categories;
  container.innerHTML = categories
    .map(
      (cat) => `
    <button class="pill-btn ${cat.id === currentCategory ? "active" : ""}" onclick="filterByCategory('${cat.id}')">
      ${cat.icon} ${cat.name}
    </button>
  `
    )
    .join("");
}

function filterByCategory(catId) {
  currentCategory = catId;
  initCategoryPills();
  renderCatalog();
}

function filterCatalog() {
  renderCatalog();
}

// Render Catalog Products
function renderCatalog() {
  const grid = document.getElementById("products-grid");
  if (!grid) return;

  const searchTerm = (document.getElementById("catalog-search")?.value || "").toLowerCase().trim();
  let products = window.LEGGUMBRES_DEMO_DATA.products;

  if (currentCategory !== "all") {
    products = products.filter((p) => p.category === currentCategory);
  }

  if (searchTerm) {
    products = products.filter(
      (p) => p.name.toLowerCase().includes(searchTerm) || p.description.toLowerCase().includes(searchTerm)
    );
  }

  if (!products.length) {
    grid.innerHTML = `
      <div class="empty-state grid-col-span-full">
        <p>🥬 No se encontraron productos agrícolas que coincidan con la búsqueda.</p>
      </div>
    `;
    return;
  }

  grid.innerHTML = products
    .map(
      (p) => `
    <div class="card product-card">
      <div class="product-img-wrapper">
        <img src="${p.image}" alt="${p.name}">
        <span class="product-badge">${p.badge}</span>
      </div>
      <div class="product-body">
        <h4>${p.name}</h4>
        <p class="product-farmer">👨‍🌾 ${p.farmer}</p>
        <p class="product-desc">${p.description}</p>
        <div class="product-price-row">
          <div>
            <strong class="product-price">$${p.price.toLocaleString("es-CO")}</strong>
            <span class="product-unit">/ ${p.unit}</span>
          </div>
          <button class="btn btn-primary btn-sm" onclick="addToCart('${p.id}')">+ Agregar</button>
        </div>
      </div>
    </div>
  `
    )
    .join("");
}

// Cart Logic
function addToCart(productId) {
  const product = window.LEGGUMBRES_DEMO_DATA.products.find((p) => p.id === productId);
  if (!product) return;

  const existing = cart.find((item) => item.product.id === productId);
  if (existing) {
    existing.quantity++;
  } else {
    cart.push({ product: product, quantity: 1 });
  }

  updateCartUI();
  openCartModal();
}

function updateCartQuantity(productId, delta) {
  const item = cart.find((i) => i.product.id === productId);
  if (!item) return;

  item.quantity += delta;
  if (item.quantity <= 0) {
    cart = cart.filter((i) => i.product.id !== productId);
  }

  updateCartUI();
}

function updateCartUI() {
  const countElem = document.getElementById("cart-count");
  const totalItems = cart.reduce((acc, i) => acc + i.quantity, 0);
  if (countElem) countElem.innerText = totalItems;

  const container = document.getElementById("cart-items-container");
  const subtotalElem = document.getElementById("cart-subtotal");

  if (!container) return;

  if (!cart.length) {
    container.innerHTML = `<div class="empty-state"><p>Tu carrito está vacío.</p></div>`;
    if (subtotalElem) subtotalElem.innerText = "$0";
    return;
  }

  let subtotal = 0;
  container.innerHTML = cart
    .map((item) => {
      const itemTotal = item.product.price * item.quantity;
      subtotal += itemTotal;
      return `
      <div class="cart-item">
        <img src="${item.product.image}" alt="${item.product.name}">
        <div class="cart-item-info">
          <h5>${item.product.name}</h5>
          <small>$${item.product.price.toLocaleString()} / ${item.product.unit}</small>
        </div>
        <div class="cart-qty-controls">
          <button onclick="updateCartQuantity('${item.product.id}', -1)">-</button>
          <span>${item.quantity}</span>
          <button onclick="updateCartQuantity('${item.product.id}', 1)">+</button>
        </div>
        <strong class="cart-item-price">$${itemTotal.toLocaleString()}</strong>
      </div>
    `;
    })
    .join("");

  if (subtotalElem) subtotalElem.innerText = `$${subtotal.toLocaleString("es-CO")}`;
}

// Modals
function openCartModal() {
  updateCartUI();
  document.getElementById("modal-cart")?.classList.remove("hidden");
}

function closeCartModal(event) {
  document.getElementById("modal-cart")?.classList.add("hidden");
}

function openCheckoutModal() {
  if (!cart.length) {
    alert("Agrega productos al carrito antes de continuar.");
    return;
  }
  closeCartModal();
  updateCheckoutTotal();
  document.getElementById("checkout-success")?.classList.add("hidden");
  document.getElementById("checkout-form")?.classList.remove("hidden");
  document.getElementById("modal-checkout")?.classList.remove("hidden");
}

function closeCheckoutModal() {
  document.getElementById("modal-checkout")?.classList.add("hidden");
}

function updateCheckoutTotal() {
  const deliveryType = document.getElementById("chk-delivery")?.value || "domicilio";
  const subtotal = cart.reduce((acc, i) => acc + i.product.price * i.quantity, 0);
  const deliveryFee = deliveryType === "domicilio" ? 4500 : 0;
  const total = subtotal + deliveryFee;

  const amountElem = document.getElementById("chk-total-amount");
  if (amountElem) amountElem.innerText = `$${total.toLocaleString("es-CO")}`;
}

async function processCheckout(event) {
  event.preventDefault();
  const name = document.getElementById("chk-name").value;
  const address = document.getElementById("chk-address").value;
  const deliveryType = document.getElementById("chk-delivery").value;

  const pedId = "PED-" + Math.floor(10000 + Math.random() * 90000);
  const subtotal = cart.reduce((acc, i) => acc + i.product.price * i.quantity, 0);
  const deliveryFee = deliveryType === "domicilio" ? 4500 : 0;
  const itemsText = cart.map((i) => `${i.quantity}x ${i.product.name}`).join(", ");

  const newOrder = {
    id: pedId,
    date: new Date().toISOString().split("T")[0],
    items: itemsText,
    total: subtotal + deliveryFee,
    status: "Confirmado",
    deliveryType: deliveryType === "domicilio" ? "Domicilio Directo" : "Recogida en Centro de Acopio"
  };

  window.LEGGUMBRES_DEMO_DATA.demoOrders.unshift(newOrder);

  // Clear cart
  cart = [];
  updateCartUI();

  document.getElementById("chk-ped-id").innerText = pedId;
  document.getElementById("checkout-form").classList.add("hidden");
  document.getElementById("checkout-success").classList.remove("hidden");
}

function finishCheckout() {
  closeCheckoutModal();
  switchView("customer");
}

// Auth UI Logic
function openAuthModal(mode = "login") {
  toggleAuthMode(mode);
  document.getElementById("modal-auth")?.classList.remove("hidden");
}

function closeAuthModal() {
  document.getElementById("modal-auth")?.classList.add("hidden");
}

function toggleAuthMode(mode) {
  const loginForm = document.getElementById("login-form");
  const regForm = document.getElementById("register-form");
  const title = document.getElementById("auth-title");

  if (mode === "login") {
    loginForm?.classList.remove("hidden");
    regForm?.classList.add("hidden");
    if (title) title.innerText = "Iniciar Sesión";
  } else {
    loginForm?.classList.add("hidden");
    regForm?.classList.remove("hidden");
    if (title) title.innerText = "Crear Cuenta Demo";
  }
}

async function handleLoginSubmit(event) {
  event.preventDefault();
  const email = document.getElementById("login-email").value;
  const pass = document.getElementById("login-pass").value;

  try {
    const user = await window.SaaSAuth.loginSimulated(email, pass, "leggumbres");
    updateAuthUI();
    closeAuthModal();
    switchView("customer");
  } catch (err) {
    alert(err.message);
  }
}

async function handleRegisterSubmit(event) {
  event.preventDefault();
  const name = document.getElementById("reg-name").value;
  const email = document.getElementById("reg-email").value;
  const pass = document.getElementById("reg-pass").value;

  try {
    const user = await window.SaaSAuth.registerSimulated(name, email, pass, "leggumbres");
    updateAuthUI();
    closeAuthModal();
    switchView("customer");
  } catch (err) {
    alert(err.message);
  }
}

function logoutUser() {
  window.SaaSAuth.clearSession("leggumbres");
  updateAuthUI();
  switchView("home");
}

function updateAuthUI() {
  const container = document.getElementById("auth-nav-container");
  const user = window.SaaSAuth.getUser("leggumbres");

  if (!container) return;

  if (user) {
    container.innerHTML = `
      <button class="btn btn-outline" onclick="switchView('customer')">👤 ${user.fullName}</button>
    `;
  } else {
    container.innerHTML = `
      <button class="btn btn-outline" onclick="openAuthModal('login')">Iniciar Sesión</button>
    `;
  }
}

// Portal Customer
function renderCustomerPortal() {
  const user = window.SaaSAuth.getUser("leggumbres");
  if (!user) {
    openAuthModal("login");
    return;
  }

  const welcome = document.getElementById("customer-welcome-msg");
  if (welcome) welcome.innerText = `Bienvenido(a), ${user.fullName}`;

  document.getElementById("prof-name").innerText = user.fullName;
  document.getElementById("prof-email").innerText = user.email;

  // Render Orders
  const ordersTbody = document.getElementById("customer-orders-tbody");
  const orders = window.LEGGUMBRES_DEMO_DATA.demoOrders;

  if (ordersTbody) {
    ordersTbody.innerHTML = orders
      .map(
        (o) => `
      <tr>
        <td><strong>${o.id}</strong></td>
        <td>${o.date}</td>
        <td>${o.items}</td>
        <td><small>${o.deliveryType}</small></td>
        <td><strong>$${o.total.toLocaleString("es-CO")}</strong></td>
        <td><span class="badge ${o.status === "Entregado" ? "badge-success" : "badge-warning"}">${o.status}</span></td>
      </tr>
    `
      )
      .join("");
  }
}

function switchPortalTab(tabName) {
  document.querySelectorAll(".portal-tab-content").forEach((tab) => tab.classList.remove("active"));
  document.querySelectorAll(".tab-btn").forEach((btn) => btn.classList.remove("active"));

  document.getElementById(`tab-${tabName}`)?.classList.add("active");
  event.target.classList.add("active");
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
    btn.innerText = "📩 Radicar PQRS Oficial";
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
