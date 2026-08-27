const getApiBaseUrl = () => {
  if (typeof document !== 'undefined') {
    const script = document.querySelector('script[data-tenant]');
    if (script && script.src) {
      try {
        const url = new URL(script.src);
        return `${url.origin}/api/v1/widget`;
      } catch (e) {}
    }
  }
  if (typeof window !== 'undefined' && window.location && window.location.origin) {
    return `${window.location.origin}/api/v1/widget`;
  }
  return "/api/v1/widget";
};

export class WidgetApiClient {
  constructor(tenantKey) {
    this.tenantKey = tenantKey;
  }

  async ragSearch(query) {
    const baseUrl = getApiBaseUrl();
    const response = await fetch(`${baseUrl}/rag-search`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Widget-Key": this.tenantKey
      },
      body: JSON.stringify({ query })
    });

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`);
    }

    return await response.json();
  }

  async createTicket(ticketData) {
    const baseUrl = getApiBaseUrl();
    const response = await fetch(`${baseUrl}/tickets`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Widget-Key": this.tenantKey
      },
      body: JSON.stringify(ticketData)
    });

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`);
    }

    return await response.json();
  }
}
