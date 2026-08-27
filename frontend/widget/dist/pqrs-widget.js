(function () {
  const cssStyles = `
:host {
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  font-size: 14px;
  line-height: 1.5;
  color: #1f2937;
  box-sizing: border-box;
}
*, *:before, *:after { box-sizing: border-box; }
.pqrs-launcher {
  position: fixed;
  bottom: 24px;
  right: 24px;
  width: 60px;
  height: 60px;
  border-radius: 30px;
  background: linear-gradient(135deg, #2563eb, #1d4ed8);
  color: #ffffff;
  border: none;
  box-shadow: 0 4px 14px rgba(37, 99, 235, 0.4);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999999;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}
.pqrs-launcher:hover { transform: scale(1.05); box-shadow: 0 6px 20px rgba(37, 99, 235, 0.5); }
.pqrs-launcher:focus-visible { outline: 3px solid #60a5fa; outline-offset: 3px; }
.pqrs-launcher svg { width: 28px; height: 28px; fill: currentColor; }
.pqrs-container {
  position: fixed;
  bottom: 96px;
  right: 24px;
  width: 380px;
  max-width: calc(100vw - 32px);
  height: 580px;
  max-height: calc(100vh - 120px);
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15), 0 1px 3px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  z-index: 999998;
  opacity: 0;
  transform: translateY(20px) scale(0.95);
  pointer-events: none;
  transition: opacity 0.25s ease, transform 0.25s ease;
}
.pqrs-container.open { opacity: 1; transform: translateY(0) scale(1); pointer-events: auto; }
.pqrs-header {
  background: linear-gradient(135deg, #1e293b, #0f172a);
  color: #ffffff;
  padding: 16px 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}
.pqrs-header-title { font-weight: 600; font-size: 16px; display: flex; align-items: center; gap: 8px; }
.pqrs-header-subtitle { font-size: 11px; color: #94a3b8; }
.pqrs-close-btn { background: transparent; border: none; color: #94a3b8; cursor: pointer; padding: 4px; border-radius: 6px; display: flex; align-items: center; justify-content: center; }
.pqrs-close-btn:hover { color: #ffffff; background: rgba(255, 255, 255, 0.1); }
.pqrs-body { flex: 1; overflow-y: auto; padding: 16px; background-color: #f8fafc; display: flex; flex-direction: column; gap: 12px; }
.pqrs-msg { max-width: 85%; padding: 12px 16px; border-radius: 14px; font-size: 13.5px; word-wrap: break-word; animation: fadeIn 0.2s ease-in-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(6px); } to { opacity: 1; transform: translateY(0); } }
.pqrs-msg-bot { align-self: flex-start; background: #ffffff; color: #334155; border: 1px solid #e2e8f0; border-bottom-left-radius: 4px; box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05); }
.pqrs-msg-user { align-self: flex-end; background: #2563eb; color: #ffffff; border-bottom-right-radius: 4px; }
.pqrs-source { font-size: 11px; color: #64748b; margin-top: 6px; padding-top: 6px; border-top: 1px dashed #cbd5e1; display: flex; align-items: center; gap: 4px; }
.pqrs-feedback { background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 10px; padding: 12px; margin-top: 8px; display: flex; flex-direction: column; gap: 8px; }
.pqrs-feedback-text { font-size: 12px; font-weight: 500; color: #1e40af; }
.pqrs-feedback-btns { display: flex; gap: 8px; }
.pqrs-btn { padding: 8px 14px; border-radius: 8px; font-size: 12px; font-weight: 600; border: none; cursor: pointer; transition: background 0.15s ease; }
.pqrs-btn-primary { background: #2563eb; color: #ffffff; }
.pqrs-btn-primary:hover { background: #1d4ed8; }
.pqrs-btn-secondary { background: #ffffff; color: #475569; border: 1px solid #cbd5e1; }
.pqrs-btn-secondary:hover { background: #f1f5f9; }
.pqrs-form { background: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; display: flex; flex-direction: column; gap: 12px; }
.pqrs-form-title { font-size: 14px; font-weight: 600; color: #0f172a; }
.pqrs-field { display: flex; flex-direction: column; gap: 4px; }
.pqrs-label { font-size: 12px; font-weight: 500; color: #475569; }
.pqrs-input, .pqrs-textarea { width: 100%; padding: 8px 12px; border: 1px solid #cbd5e1; border-radius: 6px; font-size: 13px; font-family: inherit; }
.pqrs-input:focus, .pqrs-textarea:focus { outline: none; border-color: #2563eb; box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15); }
.pqrs-input.error, .pqrs-textarea.error { border-color: #ef4444; }
.pqrs-error-msg { font-size: 11px; color: #ef4444; }
.pqrs-success-card { background: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 12px; padding: 20px; text-align: center; display: flex; flex-direction: column; align-items: center; gap: 10px; }
.pqrs-success-icon { width: 48px; height: 48px; background: #22c55e; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; }
.pqrs-radicado-badge { background: #dcfce7; color: #15803d; font-family: monospace; font-size: 16px; font-weight: 700; padding: 6px 14px; border-radius: 8px; border: 1px dashed #86efac; }
.pqrs-footer { padding: 12px; background: #ffffff; border-top: 1px solid #e2e8f0; display: flex; gap: 8px; }
.pqrs-chat-input { flex: 1; padding: 10px 14px; border: 1px solid #cbd5e1; border-radius: 20px; font-size: 13px; outline: none; }
.pqrs-send-btn { width: 40px; height: 40px; border-radius: 20px; background: #2563eb; color: #ffffff; border: none; cursor: pointer; display: flex; align-items: center; justify-content: center; }
.pqrs-send-btn:disabled { background: #94a3b8; cursor: not-allowed; }
.pqrs-loading-dots { display: inline-flex; gap: 4px; }
.pqrs-dot { width: 6px; height: 6px; background: #94a3b8; border-radius: 50%; animation: bounce 1.4s infinite ease-in-out both; }
.pqrs-dot:nth-child(1) { animation-delay: -0.32s; }
.pqrs-dot:nth-child(2) { animation-delay: -0.16s; }
@keyframes bounce { 0%, 80%, 100% { transform: scale(0); } 40% { transform: scale(1.0); } }
@media (max-width: 480px) { .pqrs-container { bottom: 12px; right: 12px; left: 12px; width: calc(100vw - 24px); height: calc(100vh - 80px); } }
`;

  class WidgetApiClient {
    constructor(tenantKey, baseUrl) {
      this.tenantKey = tenantKey;
      this.baseUrl = baseUrl || (typeof window !== "undefined" ? window.location.origin + "/api/v1/widget" : "/api/v1/widget");
    }

    async ragSearch(query) {
      const response = await fetch(`${this.baseUrl}/rag-search`, {
        method: "POST",
        headers: { "Content-Type": "application/json", "X-Widget-Key": this.tenantKey },
        body: JSON.stringify({ query })
      });
      if (!response.ok) throw new Error(`HTTP Error ${response.status}`);
      return await response.json();
    }

    async createTicket(ticketData) {
      const response = await fetch(`${this.baseUrl}/tickets`, {
        method: "POST",
        headers: { "Content-Type": "application/json", "X-Widget-Key": this.tenantKey },
        body: JSON.stringify(ticketData)
      });
      if (!response.ok) throw new Error(`HTTP Error ${response.status}`);
      return await response.json();
    }
  }

  class WidgetState {
    constructor(tenantKey) {
      this.tenantKey = tenantKey;
      this.phase = "chat";
      this.messages = [];
      this.loading = false;
      this.ticketNumber = null;
      this.lastQuery = "";
      this.subscribers = [];
    }

    subscribe(cb) { this.subscribers.push(cb); }
    notify() { this.subscribers.forEach(cb => cb(this)); }

    addMessage(sender, text, sources = []) {
      this.messages.push({ sender, text, sources });
      this.notify();
    }

    setLoading(loading) { this.loading = loading; this.notify(); }
    setPhase(phase) { this.phase = phase; this.notify(); }
    setTicketSuccess(num) { this.ticketNumber = num; this.phase = "success"; this.notify(); }
  }

  class WidgetUI {
    constructor(shadowRoot, state, baseUrl) {
      this.shadow = shadowRoot;
      this.state = state;
      this.api = new WidgetApiClient(state.tenantKey, baseUrl);
      this.isOpen = false;
      this.init();
    }

    init() {
      this.renderLauncher();
      this.renderContainer();
      this.state.subscribe(() => this.renderContent());
    }

    renderLauncher() {
      const btn = document.createElement("button");
      btn.className = "pqrs-launcher";
      btn.setAttribute("aria-label", "Abrir asistente de PQRS");
      btn.innerHTML = `<svg viewBox="0 0 24 24"><path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H6l-2 2V4h16v12z"/></svg>`;
      btn.addEventListener("click", () => this.toggleWidget());
      this.shadow.appendChild(btn);
    }

    toggleWidget() {
      this.isOpen = !this.isOpen;
      if (this.container) {
        if (this.isOpen) {
          this.container.classList.add("open");
          this.shadow.querySelector(".pqrs-chat-input")?.focus();
        } else {
          this.container.classList.remove("open");
        }
      }
    }

    renderContainer() {
      const container = document.createElement("div");
      container.className = "pqrs-container";
      container.innerHTML = `
        <div class="pqrs-header">
          <div>
            <div class="pqrs-header-title">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z"/></svg>
              Asistente Virtual PQRS
            </div>
            <div class="pqrs-header-subtitle">Respuesta inmediata & Soporte 24/7</div>
          </div>
          <button class="pqrs-close-btn" aria-label="Cerrar widget">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
          </button>
        </div>
        <div class="pqrs-body" id="pqrs-body"></div>
        <div class="pqrs-footer" id="pqrs-footer">
          <input type="text" class="pqrs-chat-input" placeholder="Escribe tu pregunta o consulta..." aria-label="Escribe tu pregunta">
          <button class="pqrs-send-btn" aria-label="Enviar pregunta">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor"><path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/></svg>
          </button>
        </div>
      `;

      container.querySelector(".pqrs-close-btn").addEventListener("click", () => this.toggleWidget());
      const sendBtn = container.querySelector(".pqrs-send-btn");
      const chatInput = container.querySelector(".pqrs-chat-input");

      const handleSend = () => {
        const text = chatInput.value.trim();
        if (text && !this.state.loading) {
          chatInput.value = "";
          this.handleUserQuestion(text);
        }
      };

      sendBtn.addEventListener("click", handleSend);
      chatInput.addEventListener("keydown", (e) => { if (e.key === "Enter") handleSend(); });

      this.shadow.appendChild(container);
      this.container = container;
      this.state.addMessage("bot", "¡Hola! Bienvenid@ a nuestra plataforma de atención. ¿En qué te podemos ayudar hoy?");
    }

    async handleUserQuestion(query) {
      this.state.lastQuery = query;
      this.state.addMessage("user", query);
      this.state.setLoading(true);

      try {
        const res = await this.api.ragSearch(query);
        this.state.setLoading(false);

        if (res.resolved && res.answer) {
          this.state.addMessage("bot", res.answer, res.sources || []);
          this.renderRagFeedback();
        } else {
          this.state.addMessage("bot", "No encontré una respuesta exacta en nuestra base de conocimientos. ¿Deseas hacer otra pregunta o prefieres radicar una PQRS?");
          this.renderUnresolvedFeedback();
        }
      } catch (err) {
        this.state.setLoading(false);
        this.state.addMessage("bot", "Ocurrió un inconveniente temporal. Puedes continuar en el chat o radicar tu PQRS a continuación.");
        this.renderUnresolvedFeedback();
      }
    }

    renderRagFeedback() {
      const body = this.shadow.querySelector("#pqrs-body");
      const feedbackEl = document.createElement("div");
      feedbackEl.className = "pqrs-feedback";
      feedbackEl.innerHTML = `
        <div class="pqrs-feedback-text">¿Esta respuesta resolvió tu inquietud?</div>
        <div class="pqrs-feedback-btns">
          <button class="pqrs-btn pqrs-btn-primary" id="btn-rag-yes">¡Sí, gracias!</button>
          <button class="pqrs-btn pqrs-btn-secondary" id="btn-rag-no">No, radicar PQRS</button>
        </div>
      `;

      feedbackEl.querySelector("#btn-rag-yes").addEventListener("click", () => {
        feedbackEl.remove();
        this.state.addMessage("bot", "¡Excelente! Nos alegra haberte ayudado. Que tengas un excelente día.");
      });

      feedbackEl.querySelector("#btn-rag-no").addEventListener("click", () => {
        feedbackEl.remove();
        this.state.setPhase("ticket-form");
      });

      body.appendChild(feedbackEl);
      body.scrollTop = body.scrollHeight;
    }

    renderUnresolvedFeedback() {
      const body = this.shadow.querySelector("#pqrs-body");
      const feedbackEl = document.createElement("div");
      feedbackEl.className = "pqrs-feedback";
      feedbackEl.innerHTML = `
        <div class="pqrs-feedback-btns">
          <button class="pqrs-btn pqrs-btn-secondary" id="btn-try-again">💬 Seguir en el Chat</button>
          <button class="pqrs-btn pqrs-btn-primary" id="btn-go-ticket">📝 Radicar PQRS</button>
        </div>
      `;

      feedbackEl.querySelector("#btn-try-again").addEventListener("click", () => {
        feedbackEl.remove();
        const chatInput = this.shadow.querySelector(".pqrs-chat-input");
        chatInput?.focus();
      });

      feedbackEl.querySelector("#btn-go-ticket").addEventListener("click", () => {
        feedbackEl.remove();
        this.state.setPhase("ticket-form");
      });

      body.appendChild(feedbackEl);
      body.scrollTop = body.scrollHeight;
    }

    renderContent() {
      const body = this.shadow.querySelector("#pqrs-body");
      const footer = this.shadow.querySelector("#pqrs-footer");
      if (!body) return;

      body.innerHTML = "";

      this.state.messages.forEach(msg => {
        const msgDiv = document.createElement("div");
        msgDiv.className = `pqrs-msg pqrs-msg-${msg.sender}`;
        msgDiv.innerText = msg.text;

        if (msg.sources && msg.sources.length > 0) {
          const sourceSpan = document.createElement("div");
          sourceSpan.className = "pqrs-source";
          sourceSpan.innerHTML = `📄 Fuente: ${msg.sources.map(s => s.title).join(", ")}`;
          msgDiv.appendChild(sourceSpan);
        }
        body.appendChild(msgDiv);
      });

      if (this.state.loading) {
        const loadingDiv = document.createElement("div");
        loadingDiv.className = "pqrs-msg pqrs-msg-bot";
        loadingDiv.innerHTML = `Buscando información <div class="pqrs-loading-dots"><span class="pqrs-dot"></span><span class="pqrs-dot"></span><span class="pqrs-dot"></span></div>`;
        body.appendChild(loadingDiv);
      }

      if (this.state.phase === "ticket-form") {
        footer.style.display = "none";
        this.renderTicketForm(body);
      } else if (this.state.phase === "success") {
        footer.style.display = "none";
        this.renderSuccessCard(body);
      } else {
        footer.style.display = "flex";
      }

      body.scrollTop = body.scrollHeight;
    }

    renderTicketForm(container) {
      const formDiv = document.createElement("div");
      formDiv.className = "pqrs-form";
      formDiv.innerHTML = `
        <div class="pqrs-form-title">Radicar PQRS</div>
        <div class="pqrs-field">
          <label class="pqrs-label">Tu Nombre *</label>
          <input type="text" id="tf-name" class="pqrs-input" placeholder="Ej: Juan Pérez">
          <div class="pqrs-error-msg" id="err-name"></div>
        </div>
        <div class="pqrs-field">
          <label class="pqrs-label">Correo Electrónico *</label>
          <input type="email" id="tf-email" class="pqrs-input" placeholder="juan@ejemplo.com">
          <div class="pqrs-error-msg" id="err-email"></div>
        </div>
        <div class="pqrs-field">
          <label class="pqrs-label">Asunto *</label>
          <input type="text" id="tf-subject" class="pqrs-input" placeholder="Ej: Reclamación sobre entrega">
          <div class="pqrs-error-msg" id="err-subject"></div>
        </div>
        <div class="pqrs-field">
          <label class="pqrs-label">Descripción Detallada *</label>
          <textarea id="tf-desc" class="pqrs-textarea" rows="3" placeholder="Describe claramente tu solicitud..."></textarea>
          <div class="pqrs-error-msg" id="err-desc"></div>
        </div>
        <div style="display: flex; gap: 8px; margin-top: 8px;">
          <button type="button" class="pqrs-btn pqrs-btn-primary" id="btn-submit-ticket" style="flex:1;">Enviar PQRS</button>
          <button type="button" class="pqrs-btn pqrs-btn-secondary" id="btn-cancel-ticket">Volver al Chat</button>
        </div>
      `;

      if (this.state.lastQuery) {
        formDiv.querySelector("#tf-subject").value = this.state.lastQuery.substring(0, 40);
        formDiv.querySelector("#tf-desc").value = this.state.lastQuery;
      }

      formDiv.querySelector("#btn-cancel-ticket").addEventListener("click", () => this.state.setPhase("chat"));

      const submitBtn = formDiv.querySelector("#btn-submit-ticket");
      submitBtn.addEventListener("click", async () => {
        const name = formDiv.querySelector("#tf-name").value.trim();
        const email = formDiv.querySelector("#tf-email").value.trim();
        const subject = formDiv.querySelector("#tf-subject").value.trim();
        const desc = formDiv.querySelector("#tf-desc").value.trim();

        formDiv.querySelectorAll(".pqrs-error-msg").forEach(el => el.innerText = "");
        formDiv.querySelectorAll(".pqrs-input, .pqrs-textarea").forEach(el => el.classList.remove("error"));

        let valid = true;
        if (!name) { formDiv.querySelector("#err-name").innerText = "Ingresa tu nombre."; formDiv.querySelector("#tf-name").classList.add("error"); valid = false; }
        if (!email || !email.includes("@")) { formDiv.querySelector("#err-email").innerText = "Correo inválido."; formDiv.querySelector("#tf-email").classList.add("error"); valid = false; }
        if (!subject) { formDiv.querySelector("#err-subject").innerText = "Ingresa el asunto."; formDiv.querySelector("#tf-subject").classList.add("error"); valid = false; }
        if (!desc) { formDiv.querySelector("#err-desc").innerText = "Ingresa la descripción."; formDiv.querySelector("#tf-desc").classList.add("error"); valid = false; }

        if (!valid) return;

        submitBtn.disabled = true;
        submitBtn.innerText = "Enviando...";

        try {
          const ticket = await this.api.createTicket({ customerName: name, customerEmail: email, subject, description: desc });
          const radicadoNumber = ticket.ticketNumber || `PQRS-${ticket.id.substring(0, 6).toUpperCase()}`;
          this.state.setTicketSuccess(radicadoNumber);
        } catch (err) {
          submitBtn.disabled = false;
          submitBtn.innerText = "Enviar PQRS";
          alert("No se pudo enviar la solicitud. Por favor verifica tus datos e intenta nuevamente.");
        }
      });

      container.appendChild(formDiv);
    }

    renderSuccessCard(container) {
      const card = document.createElement("div");
      card.className = "pqrs-success-card";
      card.innerHTML = `
        <div class="pqrs-success-icon"><svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"></polyline></svg></div>
        <div style="font-weight: 700; font-size: 16px; color: #166534;">¡Solicitud Registrada!</div>
        <div style="font-size: 13px; color: #374151;">Tu PQRS fue procesada correctamente.</div>
        <div class="pqrs-radicado-badge">Radicado: ${this.state.ticketNumber}</div>
        <button class="pqrs-btn pqrs-btn-primary" id="btn-finish-pqrs" style="margin-top: 10px; width: 100%;">Finalizar</button>
      `;

      card.querySelector("#btn-finish-pqrs").addEventListener("click", () => {
        this.state.setPhase("chat");
        this.state.addMessage("bot", "¡Gracias por comunicarte con nosotros!");
      });

      container.appendChild(card);
    }
  }

  // Self-initialization
  const scriptTag = document.currentScript || document.querySelector("script[data-tenant]");
  const tenantKey = scriptTag ? scriptTag.getAttribute("data-tenant") : null;
  let baseUrl = scriptTag?.getAttribute("data-api");
  if (!baseUrl) {
    if (scriptTag && scriptTag.src) {
      try {
        const u = new URL(scriptTag.src);
        baseUrl = u.origin + "/api/v1/widget";
      } catch (e) {}
    }
    if (!baseUrl && typeof window !== "undefined") {
      baseUrl = window.location.origin + "/api/v1/widget";
    }
  }

  if (!tenantKey) {
    console.error("[PQRS Widget Error] Missing 'data-tenant' attribute on widget script tag.");
    return;
  }

  if (document.getElementById("pqrs-widget-root")) return;

  const host = document.createElement("div");
  host.id = "pqrs-widget-root";
  document.body.appendChild(host);

  const shadow = host.attachShadow({ mode: "open" });
  const styleEl = document.createElement("style");
  styleEl.textContent = cssStyles;
  shadow.appendChild(styleEl);

  const state = new WidgetState(tenantKey);
  new WidgetUI(shadow, state, baseUrl);
  console.log(`[PQRS Widget] Successfully loaded for tenant key: ${tenantKey}`);
})();
