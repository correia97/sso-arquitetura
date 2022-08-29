FROM docker.elastic.co/beats/metricbeat:7.10.2

USER root

COPY ./metricbeat.docker.yml /usr/share/metricbeat/metricbeat.yml

RUN chown root:metricbeat /usr/share/metricbeat/metricbeat.yml

USER metricbeat 