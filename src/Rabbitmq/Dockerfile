FROM rabbitmq:3.12-management-alpine

RUN rabbitmq-plugins enable --offline rabbitmq_management
RUN rabbitmq-plugins enable --offline rabbitmq_shovel rabbitmq_shovel_management
RUN rabbitmq-plugins enable rabbitmq_prometheus
