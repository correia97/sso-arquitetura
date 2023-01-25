import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
    httpDebug: 'full',
    stages: [
        { duration: '10s', target: 250 }, 
        { duration: '15s', target: 500 }, 
        { duration: '20s', target: 700 }, 
        { duration: '10s', target: 300 }, 
        { duration: '15s', target: 350 }, 
        { duration: '35s', target: 1225 }, 
        { duration: '40s', target: 1200 }, 
    ],
    thresholds: {
        http_req_failed: ['rate<0.01'], // http errors should be less than 1%
        http_req_duration: ['p(90) < 800', 'p(95) < 1200', 'p(99.9) < 2200'], // 95% of requests should be below 200ms

      },
};

const BASE_URL = 'http://localhost:56701';

export default () => {
    const loginRes = http.post(`http://192.168.0.17:8088/realms/Sample/protocol/openid-connect/token`, {
        grant_type: "password",
        client_id: "exemplo1",
        username: "drstranger@marvel.com",
        password: "12345@Mudar",
        client_secret: "hVWQNWWgaazBxwdBFiFdmJXFwsaVUHPY"
    });

    check(loginRes, {
        'logged in successfully': (resp) => resp.json('access_token') !== '',
    });

    const correlationId = generate_guid();
    const userId = generate_guid();

    const headers = {
        headers: {
            Authorization: `Bearer ${loginRes.json('access_token')}`,
            'Content-Type': "application/json",
            'correlationId': `${correlationId}`
        },
    };

    const cadastroPayload = JSON.stringify({
        userid: `${userId}`,
        matricula: "Inserido",
        cargo: "Admin",
        nome: "Paulo Eduardo",
        sobreNome: "Correia",
        email: `${userId}@gmail.com`,
        dataNascimento: "2011-11-11"
    });

    const cadastroResult = http.post(`${BASE_URL}/api/v1/Funcionario/funcionario`, cadastroPayload, headers);
    check(cadastroResult, { 'Cadastrado com sucesso': (obj) => obj.json('sucesso') });

    const atualizacaoPayload = JSON.stringify({
        userid: `${userId}`,
        matricula: "Atualizado",
        cargo: "Admin",
        nome: "Paulo Eduardo",
        sobreNome: "Correia",
        email: `${userId}@gmail.com`,
        dataNascimento: "2011-11-11",
        telefones: [{
            telefone: "11900000000",
            ddi: "+55"
        },
        {
            telefone: "1127272727",
            ddi: "+55"
        }],
        enderecoComercial: {
            rua: "Rua Teste",
            numero: 47,
            cep: "04600000",
            complemento: "",
            bairro: "Vila Santana",
            cidade: "São Paulo",
            uf: "SP",
            tipoEndereco: 1
        },
        enderecoResidencial: {
            rua: "Rua Teste",
            numero: 47,
            cep: "04600000",
            complemento: "",
            bairro: "Vila Santana",
            cidade: "São Paulo",
            uf: "SP",
            tipoEndereco: 1
        }
    });



    const atualizacaoResult = http.patch(`${BASE_URL}/api/v1/Funcionario/funcionario`, atualizacaoPayload, headers);
    check(atualizacaoResult, { 'Atualizado com sucesso': (obj) => obj.json('sucesso')});

    sleep(1);
};

function generate_guid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}