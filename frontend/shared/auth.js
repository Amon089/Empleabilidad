/**
 * Antigravity SaaS Platform - Auth & Session Manager (Shared Module)
 */
window.SaaSAuth = (function () {
  const TOKEN_KEY_PREFIX = "pqrs_token_";
  const USER_KEY_PREFIX = "pqrs_user_";

  function getStorageKey(prefix, tenantSlug) {
    return `${prefix}${tenantSlug || "default"}`;
  }

  return {
    getToken: function (tenantSlug) {
      return localStorage.getItem(getStorageKey(TOKEN_KEY_PREFIX, tenantSlug));
    },
    getUser: function (tenantSlug) {
      const userStr = localStorage.getItem(getStorageKey(USER_KEY_PREFIX, tenantSlug));
      if (!userStr) return null;
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    },
    saveSession: function (token, user, tenantSlug) {
      localStorage.setItem(getStorageKey(TOKEN_KEY_PREFIX, tenantSlug), token);
      localStorage.setItem(getStorageKey(USER_KEY_PREFIX, tenantSlug), JSON.stringify(user));
    },
    clearSession: function (tenantSlug) {
      localStorage.removeItem(getStorageKey(TOKEN_KEY_PREFIX, tenantSlug));
      localStorage.removeItem(getStorageKey(USER_KEY_PREFIX, tenantSlug));
    },
    isLoggedIn: function (tenantSlug) {
      return !!this.getToken(tenantSlug);
    },
    loginSimulated: async function (email, password, tenantSlug, role = "CLIENT") {
      // Simulate network latency
      await new Promise((resolve) => setTimeout(resolve, 600));

      if (!email || !password) {
        throw new Error("Por favor ingresa tu correo y contraseña.");
      }

      const fakeToken = "demo_jwt_token_" + btoa(email) + "_" + Date.now();
      const nameParts = email.split("@")[0].replace(".", " ");
      const formattedName = nameParts.charAt(0).toUpperCase() + nameParts.slice(1);

      const user = {
        email: email,
        fullName: formattedName.length > 2 ? formattedName : "Usuario Demo",
        role: role,
        tenantSlug: tenantSlug,
        createdAt: new Date().toISOString()
      };

      this.saveSession(fakeToken, user, tenantSlug);
      return user;
    },
    registerSimulated: async function (fullName, email, password, tenantSlug) {
      await new Promise((resolve) => setTimeout(resolve, 700));

      if (!fullName || !email || !password) {
        throw new Error("Todos los campos son obligatorios.");
      }

      const fakeToken = "demo_jwt_token_" + btoa(email) + "_" + Date.now();
      const user = {
        fullName: fullName,
        email: email,
        role: "CLIENT",
        tenantSlug: tenantSlug,
        createdAt: new Date().toISOString()
      };

      this.saveSession(fakeToken, user, tenantSlug);
      return user;
    }
  };
})();
