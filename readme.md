
[![.NET](https://github.com/correia97/sso/actions/workflows/dotnet.yml/badge.svg)](https://github.com/correia97/sso/actions/workflows/dotnet.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=correia97_sso&metric=coverage)](https://sonarcloud.io/dashboard?id=correia97_sso) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=correia97_sso&metric=alert_status)](https://sonarcloud.io/dashboard?id=correia97_sso)
[![CircleCI](https://dl.circleci.com/status-badge/img/gh/correia97/sso/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/gh/correia97/sso/tree/master)
[![codecov](https://codecov.io/gh/correia97/sso/branch/master/graph/badge.svg?token=m08IwTWZcU)](https://codecov.io/gh/correia97/sso)

# Exemplos de projetos em .NET com algum SSO

### Fluxo de Navegação
```mermaid
sequenceDiagram
	autonumber
	actor Funcionario
	participant MVC
	participant API
	participant Keycloak
	participant RabbitMQ
	participant Worker
	
	alt logged
			Funcionario->>+MVC: Get Home  
			MVC-->>+API: Get Wheather
			API->>+Keycloak: Is Token Valid?
			Keycloak-->>-API: True
			API->>RabbitMQ: publish	message		
			Worker->>+RabbitMQ: Get message	
			Worker->>+RabbitMQ: publish Event
			API-->>-MVC: json result
			MVC-->>-Funcionario: Html 
  else not logged      
			Funcionario->>+MVC: Get Home  
			MVC-->>-Funcionario: Html 
	end
	
```


### Fluxo de login
```mermaid
sequenceDiagram
	autonumber
	actor Funcionario
	participant MVC
	participant API
	participant Keycloak
	participant RabbitMQ
	participant Worker
	

      Funcionario->>+MVC: Login
      MVC->>+Keycloak: Redirect
      Keycloak-->>-MVC: User Token
			MVC-->>-Funcionario: Html with User Token

	
```
