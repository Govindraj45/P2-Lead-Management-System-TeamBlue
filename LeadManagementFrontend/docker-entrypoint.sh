#!/bin/sh
# Substitute API_BACKEND_URL into nginx config template at container startup.
# Only replaces ${API_BACKEND_URL}, leaving nginx variables ($host, $uri, etc.) intact.
envsubst '${API_BACKEND_URL}' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf
exec nginx -g 'daemon off;'
