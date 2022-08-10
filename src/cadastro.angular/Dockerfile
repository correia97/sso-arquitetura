FROM nginx:1.21.0-alpine as base

FROM node:16-buster as build-step
WORKDIR /app
USER root
COPY package.json .
RUN npm install -g @angular/cli@12.1.1 --force && \
        npm install --force

COPY . .
RUN ng b -c production

FROM base as final
COPY --from=build-step /app/default.conf /etc/nginx/conf.d/default.conf 
COPY --from=build-step /app/dist/angularkeycloakprotect/ /usr/share/nginx/html