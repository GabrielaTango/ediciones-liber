#!/bin/sh
set -e

DOMAIN="edicionesliber.com.ar"
CERT_PATH="/etc/letsencrypt/live/$DOMAIN"

# If real certs don't exist, create a temporary self-signed cert so nginx can start
if [ ! -f "$CERT_PATH/fullchain.pem" ]; then
  echo "No SSL certs found. Creating temporary self-signed certificate..."
  apk add --no-cache openssl
  mkdir -p "$CERT_PATH"
  openssl req -x509 -nodes -days 1 \
    -newkey rsa:2048 \
    -keyout "$CERT_PATH/privkey.pem" \
    -out "$CERT_PATH/fullchain.pem" \
    -subj "/CN=$DOMAIN"
  echo "Temporary certificate created. Certbot will replace it."
fi

exec nginx -g "daemon off;"
