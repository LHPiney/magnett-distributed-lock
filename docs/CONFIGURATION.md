# Configuración de Credenciales

## Estructura de Configuración

El proyecto usa un sistema mixto de configuración que combina:
- **appsettings.json**: Configuración no sensible (puertos, imágenes, etc.)
- **appsettings.Development.json**: Credenciales y configuración sensible (ignorado por git)
- **Variables de entorno**: Prioridad más alta, sobrescribe archivos de configuración

## Archivos de Configuración

### `appsettings.json` (en repositorio)
Contiene configuración no sensible:
- Imágenes de contenedores
- Puertos
- Nombres de bases de datos
- Configuración de endpoints

### `appsettings.Development.json` (NO en repositorio)
Contiene credenciales y configuración sensible:
- Usuarios y contraseñas de servicios
- Configuración específica de desarrollo

**Importante**: Este archivo está en `.gitignore` y no se sube al repositorio.

## Configuración Inicial

1. Copiar el archivo de ejemplo:
   ```bash
   cd src/aspire/AppHost/AppHost
   cp appsettings.Development.json.example appsettings.Development.json
   ```

2. Editar `appsettings.Development.json` con tus credenciales:
   ```json
   {
     "Credentials": {
       "Postgres": {
         "User": "dev",
         "Password": "devpass"
       },
       "RabbitMQ": {
         "User": "dev",
         "Password": "devpass"
       },
       "Keycloak": {
         "Admin": "admin",
         "AdminPassword": "admin123"
       }
     },
     "Keycloak": {
       "Realm": "locks",
       "ClientId": "portal"
     }
   }
   ```

## Prioridad de Configuración

La configuración se lee en este orden (mayor prioridad primero):

1. **Variables de entorno** (más alta prioridad)
2. `appsettings.Development.json`
3. `appsettings.json`
4. **Valores por defecto** (solo para desarrollo local)

## Variables de Entorno Disponibles

Puedes sobrescribir cualquier valor usando variables de entorno:

```bash
# Postgres
export POSTGRES_USER=mi_usuario
export POSTGRES_PASSWORD=mi_password

# RabbitMQ
export RABBITMQ_USER=mi_usuario
export RABBITMQ_PASSWORD=mi_password

# Keycloak
export KEYCLOAK_ADMIN=admin
export KEYCLOAK_ADMIN_PASSWORD=mi_password_admin
```

## Producción

Para producción, se recomienda:
1. Usar variables de entorno del sistema/hosting
2. O crear `appsettings.Production.json` (también en `.gitignore`)
3. Usar servicios de gestión de secretos (Azure Key Vault, AWS Secrets Manager, etc.)

## Seguridad

- ✅ `appsettings.Development.json` está en `.gitignore`
- ✅ `appsettings.Production.json` está en `.gitignore`
- ✅ Credenciales no están en el código fuente
- ✅ Valores por defecto solo para desarrollo local
- ⚠️ En producción, usar siempre variables de entorno o servicios de secretos

