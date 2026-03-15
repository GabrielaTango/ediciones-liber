#!/bin/sh
set -e

DOMAIN="edicionesliber.com.ar"
EMAIL="${CERTBOT_EMAIL:-admin@$DOMAIN}"

# Wait for nginx to be ready
sleep 10

# Request cert if no renewal config exists (means certbot never issued one)
if [ ! -f "/etc/letsencrypt/renewal/$DOMAIN.conf" ]; then
  echo "Requesting certificate from Let's Encrypt..."
  certbot certonly --webroot \
    --webroot-path=/var/www/certbot \
    -d "$DOMAIN" \
    -d "www.$DOMAIN" \
    --email "$EMAIL" \
    --agree-tos \
    --no-eff-email \
    --force-renewal
  echo "Certificate obtained."
fi

# Renewal loop
trap exit TERM
while :; do
  certbot renew
  sleep 12h & wait $!
done
