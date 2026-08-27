import { WidgetApiClient } from "./api.js";

export class WidgetUI {
  constructor(shadowRoot, state) {
    this.shadow = shadowRoot;
    this.state = state;
    this.api = new WidgetApiClient(state.tenantKey);
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
    btn.innerHTML = `
      <svg viewBox="0 0 24 24">
        <path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H6l-2 2V4h16v12z"/>
      </svg>
    `;

    btn.addEventListener("click", () => this.toggleWidget());
    this.shadow.appendChild(btn);
    this.launcherBtn = btn;
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
            <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z"/>
            </svg>
            Asistente Virtual PQRS
          </div>
          <div class="pqrs-header-subtitle">Respuesta inmediata & Soporte 24/7</div>
        </div>
        <button class="pqrs-close-btn" aria-label="Cerrar widget">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      </div>
      <div class="pqrs-body" id="pqrs-body"></div>
      <div class="pqrs-footer" id="pqrs-footer">
        <input type="text" class="pqrs-chat-input" placeholder="Escribe tu pregunta o consulta..." aria-label="Escribe tu pregunta">
        <button class="pqrs-send-btn" aria-label="Enviar pregunta">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
            <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
          </svg>
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
    chatInput.addEventListener("keydown", (e) => {
      if (e.key === "Enter") handleSend();
    });

    this.shadow.appendChild(container);
    this.container = container;

    // Initial greeting
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
        this.state.ragResolved = true;
        this.renderRagFeedback();
      } else {
        this.state.addMessage("bot", "No contamos con suficiente información en nuestra base de conocimientos para responder con precisión. Te invitamos a radicar una PQRS para que un agente se ponga en contacto contigo.");
        this.state.setPhase("ticket-form");
      }
    } catch (err) {
      this.state.setLoading(false);
      this.state.addMessage("bot", "Ocurrió un error al buscar en la base de conocimientos. Por favor intenta radicar tu PQRS.");
      this.state.setPhase("ticket-form");
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
        <button class="pqrs-btn pqrs-btn-secondary" id="btn-rag-no">No, necesito radicar PQRS</button>
      </div>
    `;

    feedbackEl.querySelector("#btn-rag-yes").addEventListener("click", () => {
      feedbackEl.remove();
      this.state.addMessage("bot", "¡Excelente! Nos alegra haberte ayudado. Que tengas un feliz día.");
    });

    feedbackEl.querySelector("#btn-rag-no").addEventListener("click", () => {
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

    // Render Messages Log
    this.state.messages.forEach(msg => {
      const msgDiv = document.createElement("div");
      msgDiv.className = `pqrs-msg pqrs-msg-${msg.sender}`;
      msgDiv.setAttribute("aria-live", "polite");
      msgDiv.innerText = msg.text;

      if (msg.sources && msg.sources.length > 0) {
        const sourceSpan = document.createElement("div");
        sourceSpan.className = "pqrs-source";
        sourceSpan.innerHTML = `📄 Fuente: ${msg.sources.map(s => s.title).join(", ")}`;
        msgDiv.appendChild(sourceSpan);
      }

      body.appendChild(msgDiv);
    });

    // Render Loading Indicator
    if (this.state.loading) {
      const loadingDiv = document.createElement("div");
      loadingDiv.className = "pqrs-msg pqrs-msg-bot";
      loadingDiv.innerHTML = `Buscando información <div class="pqrs-loading-dots"><span class="pqrs-dot"></span><span class="pqrs-dot"></span><span class="pqrs-dot"></span></div>`;
      body.appendChild(loadingDiv);
    }

    // Render Ticket Form Phase
    if (this.state.phase === "ticket-form") {
      footer.style.display = "none";
      this.renderTicketForm(body);
    } 
    // Render Success Phase
    else if (this.state.phase === "success") {
      footer.style.display = "none";
      this.renderSuccessCard(body);
    } 
    else {
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
        <input type="text" id="tf-subject" class="pqrs-input" placeholder="Ej: Solicitud / Reclamación">
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

    // Pre-fill subject / description if user previously entered a query
    if (this.state.lastQuery) {
      formDiv.querySelector("#tf-subject").value = this.state.lastQuery.substring(0, 40);
      formDiv.querySelector("#tf-desc").value = this.state.lastQuery;
    }

    formDiv.querySelector("#btn-cancel-ticket").addEventListener("click", () => {
      this.state.setPhase("chat");
    });

    const submitBtn = formDiv.querySelector("#btn-submit-ticket");
    submitBtn.addEventListener("click", async () => {
      const name = formDiv.querySelector("#tf-name").value.trim();
      const email = formDiv.querySelector("#tf-email").value.trim();
      const subject = formDiv.querySelector("#tf-subject").value.trim();
      const desc = formDiv.querySelector("#tf-desc").value.trim();

      // Clear previous errors
      formDiv.querySelectorAll(".pqrs-error-msg").forEach(el => el.innerText = "");
      formDiv.querySelectorAll(".pqrs-input, .pqrs-textarea").forEach(el => el.classList.remove("error"));

      let valid = true;
      if (!name) {
        formDiv.querySelector("#err-name").innerText = "Ingresa tu nombre.";
        formDiv.querySelector("#tf-name").classList.add("error");
        valid = false;
      }
      if (!email || !email.includes("@")) {
        formDiv.querySelector("#err-email").innerText = "Ingresa un correo electrónico válido.";
        formDiv.querySelector("#tf-email").classList.add("error");
        valid = false;
      }
      if (!subject) {
        formDiv.querySelector("#err-subject").innerText = "Ingresa el asunto.";
        formDiv.querySelector("#tf-subject").classList.add("error");
        valid = false;
      }
      if (!desc) {
        formDiv.querySelector("#err-desc").innerText = "Ingresa la descripción.";
        formDiv.querySelector("#tf-desc").classList.add("error");
        valid = false;
      }

      if (!valid) return;

      submitBtn.disabled = true;
      submitBtn.innerText = "Registrando...";

      try {
        const ticket = await this.api.createTicket({
          customerName: name,
          customerEmail: email,
          subject: subject,
          description: desc
        });

        const radicadoNumber = ticket.ticketNumber || `PQRS-${ticket.id.substring(0, 6).toUpperCase()}`;
        this.state.setTicketSuccess(radicadoNumber);
      } catch (err) {
        submitBtn.disabled = false;
        submitBtn.innerText = "Enviar PQRS";
        alert("Ocurrió un error al registrar la solicitud. Por favor intenta de nuevo.");
      }
    });

    container.appendChild(formDiv);
  }

  renderSuccessCard(container) {
    const card = document.createElement("div");
    card.className = "pqrs-success-card";
    card.innerHTML = `
      <div class="pqrs-success-icon">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
          <polyline points="20 6 9 17 4 12"></polyline>
        </svg>
      </div>
      <div style="font-weight: 700; font-size: 16px; color: #166534;">¡Solicitud Registrada!</div>
      <div style="font-size: 13px; color: #374151;">Tu PQRS fue registrada correctamente en nuestro sistema.</div>
      <div class="pqrs-radicado-badge">Radicado: ${this.state.ticketNumber}</div>
      <div style="font-size: 11px; color: #6b7280; margin-top: 4px;">Te enviaremos actualizaciones a tu correo electrónico.</div>
      <button class="pqrs-btn pqrs-btn-primary" id="btn-finish-pqrs" style="margin-top: 10px; width: 100%;">Finalizar</button>
    `;

    card.querySelector("#btn-finish-pqrs").addEventListener("click", () => {
      this.state.setPhase("chat");
      this.state.addMessage("bot", "¡Gracias por comunicarte con nosotros! Si tienes otra consulta, escribe aquí.");
    });

    container.appendChild(card);
  }
}
