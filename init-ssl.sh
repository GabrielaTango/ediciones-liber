#!/bin/bash
# Script para obtener certificados SSL con Let's Encrypt
# Uso: ./init-ssl.sh

set -e

# Cargar variables del .env
if [ -f .env ]; then
    export $(grep -v '^#' .env | xargs)
fi

DOMAIN=${DOMAIN:-edicionesliber.com.ar}
EMAIL=${CERTBOT_EMAIL:-admin@$DOMAIN}

echo "=== Inicializando SSL para $DOMAIN ==="

# 1. Crear config temporal de nginx sin SSL para obtener el certificado
echo "-> Creando config temporal de nginx (solo HTTP)..."
cat > nginx/conf.d/default.conf << 'TMPCONF'
server {
    listen 80;
    server_name edicionesliber.com.ar www.edicionesliber.com.ar;

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 200 'Configurando SSL...';
        add_header Content-Type text/plain;
    }
}
TMPCONF

# 2. Levantar nginx
echo "-> Levantando nginx..."
docker compose up -d nginx

# 3. Esperar que nginx esté listo
sleep 5

# 4. Obtener certificado
echo "-> Obteniendo certificado SSL con Certbot..."
docker compose run --rm certbot certonly \
    --webroot \
    --webroot-path=/var/www/certbot \
    --email "$EMAIL" \
    --agree-tos \
    --no-eff-email \
    -d "$DOMAIN" \
    -d "www.$DOMAIN"

# 5. Restaurar config de producción con SSL
echo "-> Restaurando config de producción con SSL..."
cat > nginx/conf.d/default.conf << 'PRODCONF'
# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name edicionesliber.com.ar www.edicionesliber.com.ar;

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 301 https://$host$request_uri;
    }
}

# HTTPS server
server {
    listen 443 ssl http2;
    server_name edicionesliber.com.ar www.edicionesliber.com.ar;

    ssl_certificate /etc/letsencrypt/live/edicionesliber.com.ar/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/edicionesliber.com.ar/privkey.pem;

    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 1d;
    ssl_session_tickets off;

    add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
    add_header X-Frame-Options DENY always;
    add_header X-Content-Type-Options nosniff always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    location / {
        proxy_pass http://frontend:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /api/ {
        proxy_pass http://backend:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 300s;
        proxy_connect_timeout 10s;
        proxy_send_timeout 300s;
        client_max_body_size 10M;
    }
}
PRODCONF

# 6. Recargar nginx con la nueva config
echo "-> Recargando nginx..."
docker compose exec nginx nginx -s reload

echo ""
echo "=== SSL configurado exitosamente ==="
echo "El sitio deberia estar disponible en https://$DOMAIN"
