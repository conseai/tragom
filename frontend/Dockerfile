# ---------- dev: used by docker-compose.yml (ng serve with live reload) ----------
FROM node:24-alpine AS dev
WORKDIR /app
ENV NG_CLI_ANALYTICS=false
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
EXPOSE 4200
# --poll makes file watching reliable on Docker Desktop (Windows/macOS) bind mounts
CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--poll", "2000"]

# ---------- build: production bundle ----------
FROM node:24-alpine AS build
WORKDIR /app
ENV NG_CLI_ANALYTICS=false
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npx ng build --configuration production

# ---------- prod: static files served by nginx, /api proxied to the API ----------
FROM nginx:1.29-alpine AS prod
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/travel-web/browser /usr/share/nginx/html
EXPOSE 80
