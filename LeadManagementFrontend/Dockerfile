# ---- Stage 1: Build the React app ----
# Start with Node.js 20 (Alpine = lightweight Linux) to compile the frontend
FROM node:20-alpine AS build
# Set the working directory inside the container
WORKDIR /app

# Copy package files first and install dependencies
# This is done separately so Docker can cache this layer (faster rebuilds)
COPY package.json package-lock.json* ./
RUN npm install

# Copy all source code and build the production-ready React app
COPY . .
RUN npm run build

# ---- Stage 2: Serve the built app with Nginx ----
# Use a lightweight Nginx image to serve the static HTML/JS/CSS files
FROM nginx:alpine AS runtime
# Copy the built React files into Nginx's web directory
COPY --from=build /app/dist /usr/share/nginx/html
# Copy the Nginx config template (has a placeholder for the API URL)
COPY nginx.conf.template /etc/nginx/conf.d/default.conf.template
# Copy the startup script that fills in the API URL at runtime
COPY docker-entrypoint.sh /docker-entrypoint.sh
# Make the startup script executable
RUN chmod +x /docker-entrypoint.sh

# Let Docker know this container serves web traffic on port 80
EXPOSE 80
# Run the startup script when the container starts (it configures Nginx then starts it)
ENTRYPOINT ["/docker-entrypoint.sh"]

# ==========================================================================
# SUMMARY:
# This Dockerfile builds the React frontend and serves it with Nginx. Stage 1
# installs npm packages and creates the production bundle. Stage 2 copies that
# bundle into an Nginx container along with a config template and startup
# script. The startup script injects the backend API URL at runtime so the
# same image can work in any environment (local, staging, production).
# ==========================================================================
