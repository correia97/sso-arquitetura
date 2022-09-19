FROM nginx:1.23.1-alpine as base

FROM node:18.9.0-buster as build-step
WORKDIR /app
USER root
COPY package.json .
RUN npm install -g @angular/cli@14.2.3 --force && \
        npm install --force

COPY . .
RUN ng b -c production

FROM base as final
COPY --from=build-step /app/default.conf /etc/nginx/conf.d/default.conf 
COPY --from=build-step /app/dist/angularkeycloakprotect/ /usr/share/nginx/html