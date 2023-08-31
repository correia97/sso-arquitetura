FROM nginx:1.25.1-alpine as base

FROM node:19-buster-slim as build-step
WORKDIR /app
USER root
COPY package.json .
RUN npm install -g @angular/cli@16.1.0 --force && \
        npm install --force

COPY . .
RUN ng b --configuration=production

FROM base as final
COPY --from=build-step /app/default.conf /etc/nginx/conf.d/default.conf 
COPY --from=build-step /app/dist/cadastro.angular/ /usr/share/nginx/html