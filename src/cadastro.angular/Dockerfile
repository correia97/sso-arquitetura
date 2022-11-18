FROM nginx:1.23.1-alpine as base

FROM node:19-buster-slim as build-step
WORKDIR /app
USER root
COPY package.json .
RUN npm install -g @angular/cli@14.2.3 --force && \
        npm install --force

COPY . .
RUN ng b -c production

FROM base as final
COPY --from=build-step /app/default.conf /etc/nginx/conf.d/default.conf 
COPY --from=build-step /app/dist/cadastro.angular/ /usr/share/nginx/html