FROM docker.elastic.co/beats/heartbeat:7.10.2

USER root

COPY /heartbeat.docker.yml /usr/share/heartbeat/heartbeat.yml

RUN  chown root:heartbeat /usr/share/heartbeat/heartbeat.yml

USER heartbeat