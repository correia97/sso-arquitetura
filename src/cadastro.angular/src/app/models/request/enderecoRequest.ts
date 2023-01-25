import { EnderecoResponse } from "../response/enderecoResponse";

export class EnderecoRequest {
    id!: number;
    rua!: string;
    numero!: number | null;
    cep!: string;
    complemento!: string;
    bairro!: string;
    cidade!: string;
    uf!: string;

    constructor(endereco: EnderecoResponse) {
        this.id = endereco.id;
        this.rua = endereco.rua;
        this.numero = endereco.numero;
        this.cep = endereco.cep;
        this.complemento = endereco.complemento;
        this.bairro = endereco.bairro;
        this.cidade = endereco.cidade;
        this.uf = endereco.uf;
    }
}