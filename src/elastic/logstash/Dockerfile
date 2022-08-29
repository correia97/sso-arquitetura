FROM docker.elastic.co/logstash/logstash:7.17.6

USER root
COPY ./pipelines/ /usr/share/logstash/pipeline/

RUN chown root:logstash /usr/share/logstash/pipeline/logstash.conf

USER logstash