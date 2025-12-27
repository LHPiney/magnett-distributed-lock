# Arranque local

## Prerrequisitos
- .NET SDK 10
- Podman en ejecución
- Node.js y npm instalados

## Pasos
1. Construir la imagen del portal Angular:
   ```bash
   cd src/apps/management-app
   npm run build
   podman build -t management-app:latest .
   ```
   
   O usar el script proporcionado:
   ```bash
   cd src/apps/management-app
   chmod +x build-image.sh
   ./build-image.sh
   ```

2. Configurar Podman para Aspire (opcional, si Aspire no detecta Podman automáticamente):
   - Crear alias: `alias docker=podman` (añadir a `~/.bashrc` o `~/.zshrc` para persistencia)
   - O configurar variable de entorno: `export DOCKER_HOST=unix://$XDG_RUNTIME_DIR/podman/podman.sock`

3. Ejecutar toda la orquestación:
   ```bash
   cd src/aspire/AppHost/AppHost
   dotnet run
   ```

## URLs
- Dashboard Aspire: http://localhost:15000
- Portal: http://localhost:3000
- API REST: http://localhost:8080/v1/ping
- gRPC: http://localhost:8081 (h2c)
- RabbitMQ UI: http://localhost:15672
- Keycloak: http://localhost:8090
- Postgres: localhost:5433 (externo) / postgres:5432 (interno)
- Memcached: memcached:11211 (red interna)

## Credenciales dev
- Keycloak admin: admin / admin123
- Keycloak usuario: devadmin / devpass
- RabbitMQ: dev / devpass
- Postgres: dev / devpass (db `locksdb`)

## Validación rápida
- Portal responde en http://localhost:3000
- Login portal via Keycloak (usuario devadmin / devpass).
- API responde `GET /v1/ping`.
- gRPC responde `Ping` con mensaje `pong`.
- Worker activo (logs de latido).

## Troubleshooting
- Regenerar portal: repetir paso de construcción de imagen con Podman.
- Reset Postgres: eliminar carpeta `src/infra/postgres-data` y volver a levantar.
- Reset Keycloak: eliminar `~/.config/keycloak` si se hubiera creado y relanzar contenedor.
- RabbitMQ colas: reiniciar contenedor desde AppHost o `podman rm -f rabbitmq`.
- Si Aspire no detecta Podman: crear alias `docker` → `podman` o configurar variable de entorno `DOCKER_HOST`.
- Verificar imagen construida: `podman images | grep management-app`
