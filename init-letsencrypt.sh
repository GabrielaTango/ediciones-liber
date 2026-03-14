#!/bin/bash
# Script para obtener el certificado Let's Encrypt por primera vez
# Uso: ./init-letsencrypt.sh [--staging]

set -e

DOMAINS="edicionesliber.com www.edicionesliber.com.ar"
EMAIL="admin@edicionesliber.com"  # Cambiar al email real
STAGING_ARG=""

if [ "$1" = "--staging" ]; then
    STAGING_ARG="--staging"
    echo ">>> Usando el entorno de staging de Let's Encrypt (para pruebas)"
fi

echo ">>> Levantando nginx sin SSL para el challenge..."

# Crear config temporal solo HTTP (sin bloque SSL)
cat > nginx/conf.d/default.conf <<'NGINX_CONF'
server {
    listen 80;
    server_name www.edicionesliber.com.ar edicionesliber.com;

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

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
    }
}
NGINX_CONF

docker compose up -d nginx

echo ">>> Solicitando certificado Let's Encrypt..."
docker compose run --rm certbot certonly \
    --webroot \
    --webroot-path=/var/www/certbot \
    --email "$EMAIL" \
    --agree-tos \
    --no-eff-email \
    $STAGING_ARG \
    -d edicionesliber.com \
    -d www.edicionesliber.com.ar

echo ">>> Restaurando config nginx con SSL..."

cat > nginx/conf.d/default.conf <<'NGINX_CONF'
server {
    listen 80;
    server_name www.edicionesliber.com.ar edicionesliber.com;

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 301 https://$host$request_uri;
    }
}

server {
    listen 443 ssl;
    server_name www.edicionesliber.com.ar edicionesliber.com;

    ssl_certificate /etc/letsencrypt/live/edicionesliber.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/edicionesliber.com/privkey.pem;

    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

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
    }
}
NGINX_CONF

docker compose restart nginx

echo ">>> Listo! Certificado SSL configurado."
