
[![.NET](https://github.com/correia97/sso/actions/workflows/dotnet.yml/badge.svg)](https://github.com/correia97/sso/actions/workflows/dotnet.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=correia97_sso&metric=coverage)](https://sonarcloud.io/dashboard?id=correia97_sso) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=correia97_sso&metric=alert_status)](https://sonarcloud.io/dashboard?id=correia97_sso)
[![CircleCI](https://dl.circleci.com/status-badge/img/gh/correia97/sso/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/gh/correia97/sso/tree/master)
[![codecov](https://codecov.io/gh/correia97/sso/branch/master/graph/badge.svg?token=m08IwTWZcU)](https://codecov.io/gh/correia97/sso)

# Exemplos de projetos em .NET com algum SSO
Projeto de estudo para proteger projetos ASP.Net core 6 com KeyCloak<br/><br/>
Formado por um Projeto Asp.Net MVC Core, um projeto ASP.Net API, Keycloak e PostgreSQl onde a Autenticação do Projeto MVC é feita através do padrão OpenId com o KeyCloak e a API é acessada com o authorization token também gerado pelo Keycloak <br/>
<br/>
<br/>

## Preparação do ambiente

>### Pré Requisitos
>- Instalação do docker e docker-compose
>- Instalação do openssl

<details>
  <summary> Criação dos certificados</summary>
Executar os comandos a baixo dentro da pasta "src/nginx/certificate" para criar os certificados auto assinados.

```sh
openssl req -x509 -out mvc.localhost.crt -keyout mvc.localhost.key \
  -newkey rsa:2048 -nodes -sha256 -days 1024 \
  -subj '/CN=mvc.localhost' -extensions EXT -config <( \
   printf "[dn]\nCN=mvc.localhost\n[req]\ndistinguished_name = dn\n[EXT]\nsubjectAltName=DNS:mvc.localhost\nkeyUsage=digitalSignature\nextendedKeyUsage=serverAuth")

openssl req -x509 -out api.localhost.crt -keyout api.localhost.key \
  -newkey rsa:2048 -nodes -sha256 -days 1024 \
  -subj '/CN=api.localhost' -extensions EXT -config <( \
   printf "[dn]\nCN=api.localhost\n[req]\ndistinguished_name = dn\n[EXT]\nsubjectAltName=DNS:api.localhost\nkeyUsage=digitalSignature\nextendedKeyUsage=serverAuth")

openssl req -x509 -out angular.localhost.crt -keyout angular.localhost.key \
  -newkey rsa:2048 -nodes -sha256 -days 1024 \
  -subj '/CN=angular.localhost' -extensions EXT -config <( \
   printf "[dn]\nCN=angular.localhost\n[req]\ndistinguished_name = dn\n[EXT]\nsubjectAltName=DNS:angular.localhost\nkeyUsage=digitalSignature\nextendedKeyUsage=serverAuth")

openssl req -x509 -out keycloak.localhost.crt -keyout keycloak.localhost.key \
  -newkey rsa:2048 -nodes -sha256 -days 1024 \
  -subj '/CN=keycloak.localhost' -extensions EXT -config <( \
   printf "[dn]\nCN=keycloak.localhost\n[req]\ndistinguished_name = dn\n[EXT]\nsubjectAltName=DNS:keycloak.localhost\nkeyUsage=digitalSignature\nextendedKeyUsage=serverAuth")
```

</details>
<br />

>### Instale os certficados na maquina como confiável 
<details>
  <summary>Windows</summary>

Execute o comando win+R e digite mmc

![Executar](asset/gerenciador_certificado_windows00.png)

Arquivo -> Adicionar/remover snap-in 
![Adicionar snap-in](asset/gerenciador_certificado_windows01.png)

Certificados
![Certificado](asset/gerenciador_certificado_windows02.png)

Conta de computador
![Conta de computador](asset/gerenciador_certificado_windows03.png)

Concluir
![Concluir](asset/gerenciador_certificado_windows04.png)

Confirmação 
![Confirmação](asset/gerenciador_certificado_windows05.png)

Importar certificado em "Autoridades de certificação raiz confiáveis"
![Importar certificado](asset/gerenciador_certificado_windows06.png)

Inicio d importação
![importação](asset/gerenciador_certificado_windows07.png)

Localizar o certificado
![Localizar o certificado](asset/gerenciador_certificado_windows08.png)

Avança
![Avança](asset/gerenciador_certificado_windows09.png)

Concluir
![Concluir](asset/gerenciador_certificado_windows10.png)
</details>
<br />
<details>
  <summary> Linux</summary>

Copie os certificados para pasta "/usr/local/share/ca-certificates/"

Execute o comando
 ```
 sudo update-ca-certificates
 ```
</details><br /><br />
## Executando o Projeto

Execute o comando a baixo na raiz do repositório
```bash
docker-compose build
docker-compose up -d 
```


### Criar um usuário para aplicação
<details>
<summary>Via API do Keycloak e Postman
</summary>

- Importe no postman a Collection "SetupUserKeycloak.postman_collection.json" que está na pasta raiz do projeto
- Execute as reuisições em sequência
 	- Obter token
 	- Criar usuário
 	- Recuperar usuário
 	- Recuperar grupos
 	- Adicionar usuário ao grupo
 	- Cadastrar senha para o usuário

</details>
<br />
<details>
<summary>Via Interface do Keycloak
</summary>


> Navegue até a url **https://keycloak.localhost**
![home](asset/01%20HomeKeycloak.PNG)

> Faça login no Keycloak
![Login](asset/02%20LoginKeyCloak.PNG)

> Altere para o realm Sample
![Realm](asset/03%20RealmKeyCloak.PNG)

> Navega até a opção Usuários
![Usuarios](asset/04%20UsuariosKeycloak.PNG)

> Cadastre um novo usuário associado ao grupo admin
![Novousuario](asset/05%20RegistroUsuariosKeycloak.PNG)
![Novousuario-grupo](asset/05%20RegistroUsuariosKeycloak-grupo.PNG)

> Crie uma Senha para o usuário e desmaque a opção de temporario
![Senhausuario](asset/06%20SenhaUsuariosKeycloak.PNG)



</details>
<br >

> Navege até a url do projeto MVC **https://mvc.localhost** e clique no menu Privacy
![HomeMVC](asset/07%20HomeDoMVC.PNG)

> Faça o Login com o usuário que foi criado no Keycloak
![LoginUsuario](asset/02%20LoginKeyCloak.PNG)

> Navegue na aplicação autenticada
![HomeAutenticada](asset/09%20HomeAutenticada.PNG)


> Navege até a url do projeto Angular **https://angular.localhost** e clique no botão login
![HomeMVC](asset/10%20AngularHome.PNG)

> Faça o Login com o usuário que foi criado no Keycloak
![LoginUsuario](asset/02%20LoginKeyCloak.PNG)


> Navegue na aplicação autenticada
![HomeAutenticada](asset/11%20AngularClaims.PNG)

> Home consumindo a API
![HomeAutenticada](asset/12%20AngularHomeAutenticada.PNG)

<br/>
<br/>

## Dependências

#### API

```bash
Microsoft.AspNetCore.Authentication.JwtBearer
```


#### MVC
```bash
Microsoft.AspNetCore.Authentication.Cookies 
Microsoft.AspNetCore.Authentication.OpenIdConnect
```



#### Angular
```bash
Angular
angular-auth-oidc-client
jwt-decode
bootstrap
```

### Fluxo de Dados
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
<br/>
<br/>

### Referências
[Keycloak](https://www.keycloak.org/docs/latest/getting_started/) <br/>
[Exemplo Auth0 API](https://auth0.com/docs/quickstart/backend/aspnet-core-webapi) <br/>
[Exemplo Auth0 MVC](https://auth0.com/docs/quickstart/webapp/aspnet-core-3)
