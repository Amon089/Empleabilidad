#!/bin/bash
# Script de Despliegue Automatizado en Kubernetes

set -e

echo "🚀 Iniciando despliegue de la Plataforma SaaS Multi-Tenant en Kubernetes..."

# 1. Aplicar todos los manifiestos mediante Kustomize
kubectl apply -k k8s/

echo "⏳ Esperando que la base de datos PostgreSQL + pgvector esté lista..."
kubectl rollout status deployment/pqrs-postgres-deployment -n pqrs-saas --timeout=120s

echo "⏳ Esperando que los pods de la API de ASP.NET Core estén listos..."
kubectl rollout status deployment/pqrs-api-deployment -n pqrs-saas --timeout=120s

echo "✅ ¡Despliegue en Kubernetes completado con éxito!"
echo ""
echo "📊 Estado de los recursos:"
kubectl get all -n pqrs-saas
