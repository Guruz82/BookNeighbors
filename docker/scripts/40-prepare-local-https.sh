#!/bin/sh
set -eu

mkdir -p /etc/nginx/certs
if [ ! -f /etc/nginx/certs/localhost.key ]; then
    openssl req -x509 -nodes -newkey rsa:2048 -sha256 -days 365 \
      -keyout /etc/nginx/certs/localhost.key \
      -out /etc/nginx/certs/localhost.crt \
      -subj '/CN=localhost' \
      -addext 'subjectAltName=DNS:localhost,IP:127.0.0.1' >/dev/null 2>&1
fi

# Blazor legge questo file all'avvio; URL pubblico e configurabile al runtime.
PUBLIC_URL="${PUBLIC_URL:-https://localhost:8443}"
case "$PUBLIC_URL" in
  https://*) ;;
  *) echo 'PUBLIC_URL deve iniziare con https://' >&2; exit 1 ;;
esac
printf '{ "AppOptions": { "Services": { "ApiBaseUrl": "%s/", "SignalRHubPath": "hubs/loan-chat" } } }\n' "${PUBLIC_URL%/}" \
  > /usr/share/nginx/html/appsettings.json
