import cssStyles from "./styles.css?raw";
import { WidgetState } from "./state.js";
import { WidgetUI } from "./ui.js";

(function initWidget() {
  // Find current script tag to extract data-tenant
  const scriptTag = document.currentScript || document.querySelector("script[data-tenant]");
  const tenantKey = scriptTag ? scriptTag.getAttribute("data-tenant") : null;

  if (!tenantKey) {
    console.error("[PQRS Widget Error] Missing required 'data-tenant' attribute on widget script tag.");
    return;
  }

  // Avoid duplicate initialization
  if (document.getElementById("pqrs-widget-root")) {
    return;
  }

  // Create host element
  const host = document.createElement("div");
  host.id = "pqrs-widget-root";
  document.body.appendChild(host);

  // Create Shadow Root for full style encapsulation
  const shadow = host.attachShadow({ mode: "open" });

  // Inject encapsulated CSS styles
  const styleEl = document.createElement("style");
  styleEl.textContent = cssStyles;
  shadow.appendChild(styleEl);

  // Initialize State & UI
  const state = new WidgetState(tenantKey);
  new WidgetUI(shadow, state);

  console.log(`[PQRS Widget] Successfully initialized for tenant: ${tenantKey}`);
})();
