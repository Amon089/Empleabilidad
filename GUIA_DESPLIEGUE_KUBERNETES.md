# ☸️ Guía Completa de Despliegue en Kubernetes (k8s)

Esta guía explica cómo desplegar la **Plataforma SaaS Multi-Tenant de PQRS con IA** en cualquier clúster de Kubernetes (Minikube, Kind, Docker Desktop Kubernetes, AKS, GKE, EKS).

---

## 📁 Estructura del Entorno Kubernetes (`k8s/`)

La carpeta `k8s/` contiene todos los manifiestos nativos declarativos organizados para producción:

```text
k8s/
├── 01-namespace.yaml             # Namespace dedicado 'pqrs-saas'
├── 02-postgres-secret.yaml       # Credenciales seguras de PostgreSQL
├── 03-postgres-pvc.yaml          # Almacenamiento persistente (PersistentVolumeClaim 10Gi)
├── 04-postgres-deployment.yaml   # PostgreSQL 16 + extensión pgvector (ankane/pgvector)
├── 05-postgres-service.yaml      # Servicio ClusterIP para la BD (Puerto 5432)
├── 06-api-secret.yaml            # Secretos de la API (GEMINI_API_KEY, Jwt__SecretKey)
├── 07-api-configmap.yaml         # ConfigMap de la API (Cadenas de conexión, IA Provider)
├── 08-api-deployment.yaml        # Despliegue API en ASP.NET Core (2 réplicas HA + Health Probes)
├── 09-api-service.yaml           # Servicio ClusterIP para la API (Puerto 80 -> 8080)
├── 10-api-hpa.yaml               # Escalado Horizontal Automático (HPA de 2 a 10 pods)
├── 11-ingress.yaml               # NGINX Ingress con soporte de WebSockets para SignalR
├── kustomization.yaml            # Manifiesto Kustomize para despliegue de comando único
└── deploy.sh                     # Helper script de automatización
```

---

## 🚀 Despliegue en 1 Solo Paso

### Opción A: Usando `kubectl` con Kustomize (Recomendado)

Desde la raíz del proyecto, ejecuta:

```bash
kubectl apply -k k8s/
```

### Opción B: Usando el Script de Despliegue

```bash
chmod +x k8s/deploy.sh
./k8s/deploy.sh
```

---

## 🛠️ Verificación del Despliegue

1. **Verificar el estado del Namespace y Pods**:
   ```bash
   kubectl get pods -n pqrs-saas
   ```
   *Debe mostrar los Pods de PostgreSQL y las Réplicas de la API en estado `RUNNING`.*

2. **Verificar los Servicios y el Ingress**:
   ```bash
   kubectl get svc,ingress -n pqrs-saas
   ```

3. **Verificar el Escalado Automático (HPA)**:
   ```bash
   kubectl get hpa -n pqrs-saas
   ```

---

## 🌐 Configuración de Dominio Local (`/etc/hosts`)

Para probar la plataforma localmente a través del Ingress Ingress (`pqrs.local`), mapea la dirección IP de tu clúster o `127.0.0.1`:

### En Linux / macOS (`/etc/hosts`):
```text
127.0.0.1 pqrs.local
```

### En Windows (`C:\Windows\System32\drivers\etc\hosts`):
```text
127.0.0.1 pqrs.local
```

Una vez mapeado, podrás acceder a:
- 🥦 **Web Leggumbres**: `http://pqrs.local/legumbres/index.html`
- 🏗️ **Web Todo Metal**: `http://pqrs.local/todometal/index.html`
- 📊 **Dashboard Multi-Tenant**: `http://pqrs.local/dashboard/index.html`
- 📖 **Swagger OpenAPI**: `http://pqrs.local/swagger`

---

## 🔐 Configuración de Claves de Producción

Antes de desplegar en un clúster de producción (AWS/GCP/Azure):

1. Edita `k8s/06-api-secret.yaml` e ingresa tu **`GEMINI_API_KEY`** oficial.
2. Edita `k8s/02-postgres-secret.yaml` e ingresa una contraseña de PostgreSQL de alta complejidad.
