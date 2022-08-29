FROM docker.elastic.co/kibana/kibana:8.4.0

USER root

ADD ./kibana.yml /usr/share/kibana/config/kibana.yml

RUN chown kibana /usr/share/kibana/config/kibana.yml

USER kibana

