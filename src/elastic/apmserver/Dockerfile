FROM docker.elastic.co/apm/apm-server:7.17.6

USER root

COPY ./apm-server.docker.yml /usr/share/apm-server/apm-server.yml

RUN chown root:apm-server /usr/share/apm-server/apm-server.yml

USER apm-server