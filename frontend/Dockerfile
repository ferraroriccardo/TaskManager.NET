# Multi-stage build: Node compiles React into static files, nginx serves them.

# ---- Stage 1: build ----
FROM node:22-slim AS build
WORKDIR /app

# Dependencies before the code, to take advantage of Docker's cache
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# VITE_* variables are "baked" into the JavaScript bundle at build time
ARG VITE_API_URL=http://localhost:5080
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ---- Stage 2: serve ----
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
