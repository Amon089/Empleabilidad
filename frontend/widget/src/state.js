export class WidgetState {
  constructor(tenantKey) {
    this.tenantKey = tenantKey;
    this.sessionId = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2);
    this.phase = "chat"; // 'chat', 'ticket-form', 'success', 'error'
    this.messages = [];
    this.loading = false;
    this.ragResolved = false;
    this.ticketCreated = false;
    this.ticketNumber = null;
    this.lastQuery = "";
    this.subscribers = [];
  }

  subscribe(callback) {
    this.subscribers.push(callback);
  }

  notify() {
    this.subscribers.forEach(cb => cb(this));
  }

  addMessage(sender, text, sources = []) {
    this.messages.push({ sender, text, sources, timestamp: new Date() });
    this.notify();
  }

  setLoading(loading) {
    this.loading = loading;
    this.notify();
  }

  setPhase(phase) {
    this.phase = phase;
    this.notify();
  }

  setTicketSuccess(ticketNumber) {
    this.ticketCreated = true;
    this.ticketNumber = ticketNumber;
    this.phase = "success";
    this.notify();
  }
}
