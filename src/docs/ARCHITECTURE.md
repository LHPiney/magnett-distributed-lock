# Arquitectura P0

- Orquestación: .NET Aspire (AppHost) en `src/aspire/AppHost`.
- Apps en contenedor:
  - `locks-api`: .NET 10 REST + gRPC (`/v1/*`, gRPC en 8081).
  - `audit-worker`: .NET 10 worker suscrito a RabbitMQ (placeholder).
  - `management-app`: Angular 21, imagen Docker/Podman (`management-app:latest`).
- Infra en contenedores declarados por AppHost:
  - Postgres 16 (host `postgres`, db `locksdb`, credenciales dev/devpass).
  - RabbitMQ 3.13 con UI 15672 (dev/devpass).
  - Memcached 1.6 (11211).
  - Keycloak 26 con import automático de `infra/keycloak/realm-export.json`.
- Red interna única manejada por Aspire.
- TLS: local opcional (h2c para gRPC); en producción deberá usarse TLS y front proxy.
- API versionada por ruta `/v1`.
- Configuración por variables de entorno (connection strings, issuer/audience, endpoints).

## Flujo
- Portal consume Keycloak (issuer `http://keycloak:8080/realms/locks`) y llama a `locks-api` con Bearer.
- `locks-api` usa Postgres/Memcached/RabbitMQ, expone health en `/v1/health` y readiness en `/v1/ready`.
- `audit-worker` consume RabbitMQ y persiste en Postgres (implementación posterior).

## Decisiones
- .NET usa container publish automático.
- Portal Angular usa Dockerfile multi-stage (build + nginx).
- Runtime de contenedores: Podman (compatible con Docker CLI).
- Ports: API 8080/8081, portal 3000, Keycloak 8080, RabbitMQ 5672/15672, Postgres 5432, Memcached 11211.

## Checklist para siguiente fase
- Añadir scripts de reset infra.
- Añadir pruebas de humo automatizadas.
- Ajustar TLS para producción (certs y reverse proxy).

