#!/bin/sh
# This script runs when the frontend Docker container starts up.
# It replaces the ${API_BACKEND_URL} placeholder in the Nginx config template
# with the actual backend URL from the environment variable.
# Only ${API_BACKEND_URL} is replaced — Nginx variables like $host and $uri are left alone.
envsubst '${API_BACKEND_URL}' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf
# Start Nginx in the foreground (so Docker can track the process and restart if it crashes)
exec nginx -g 'daemon off;'

# ==========================================================================
# SUMMARY:
# This is the startup script for the frontend Docker container. It takes the
# Nginx config template, replaces the API_BACKEND_URL placeholder with the
# real backend URL from the environment variable, and then starts Nginx.
# Running Nginx with "daemon off" keeps it in the foreground so Docker can
# monitor it and restart the container if Nginx crashes.
# ==========================================================================
