/**
 * Antigravity SaaS Platform - API Client (Shared Module)
 */
window.SaaSApi = (function () {
  const BASE_URL = window.location.origin;

  async function request(endpoint, options = {}) {
    const headers = options.headers || {};
    if (options.body && typeof options.body === "object") {
      headers["Content-Type"] = "application/json";
      options.body = JSON.stringify(options.body);
    }
    options.headers = headers;

    try {
      const response = await fetch(`${BASE_URL}${endpoint}`, options);
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Error HTTP ${response.status}`);
      }
      const data = await response.json();
      return data;
    } catch (err) {
      console.warn(`[SaaSApi] Error en ${endpoint}:`, err.message);
      throw err;
    }
  }

  return {
    submitTicket: async function (widgetKey, ticketData) {
      return await request("/api/v1/widget/tickets", {
        method: "POST",
        headers: {
          "X-Widget-Key": widgetKey
        },
        body: ticketData
      });
    },

    searchRag: async function (widgetKey, query, sessionId = "") {
      return await request("/api/v1/widget/rag-search", {
        method: "POST",
        headers: {
          "X-Widget-Key": widgetKey
        },
        body: {
          query: query,
          sessionId: sessionId
        }
      });
    },

    getTicketsAdmin: async function (token, tenantSlug) {
      return await request(`/api/v1/tickets?tenantSlug=${tenantSlug}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
    },

    getKbArticlesAdmin: async function (token, tenantSlug) {
      return await request(`/api/v1/knowledge-base?tenantSlug=${tenantSlug}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
    }
  };
})();
