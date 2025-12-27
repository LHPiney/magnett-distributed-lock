#!/bin/sh
set -e

ENV_FILE="/usr/share/nginx/html/env.js"

cat > "$ENV_FILE" <<EOF
window['NG_APP_OIDC_ISSUER'] = '${NG_APP_OIDC_ISSUER:-http://keycloak:8080/realms/locks}';
window['NG_APP_OIDC_CLIENT_ID'] = '${NG_APP_OIDC_CLIENT_ID:-portal}';
window['NG_APP_API_BASE_URL'] = '${NG_APP_API_BASE_URL:-http://locks-api:8080}';
EOF

exec nginx -g 'daemon off;'

