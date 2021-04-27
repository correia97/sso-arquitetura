FROM nginx:1.19.5 as base

FROM node:14.15.3-buster as build-step
WORKDIR /app
USER root
COPY package.json .
RUN npm install -g @angular/cli@11.0.5 && \
        npm install

COPY . .
RUN ng b --prod

FROM base as final
COPY --from=build-step /app/default.conf /etc/nginx/conf.d/default.conf 
COPY --from=build-step /app/dist/angularkeycloakprotect/ /usr/share/nginx/html