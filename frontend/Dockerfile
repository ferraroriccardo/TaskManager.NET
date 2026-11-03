# Multi-stage build: Node compila React in file statici, nginx li serve.

# ---- Stage 1: build ----
FROM node:22-slim AS build
WORKDIR /app

# Dipendenze prima del codice, per sfruttare la cache di Docker
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# Le variabili VITE_* vengono "cotte" nel bundle JavaScript al momento della build
ARG VITE_API_URL=http://localhost:5080
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ---- Stage 2: serve ----
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
