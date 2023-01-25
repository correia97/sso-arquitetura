import { FuncionarioResponse } from '../response/funcionarioResponse';
import { EnderecoRequest } from './enderecoRequest';
import { TelefoneRequest } from './telefoneRequest';

export class FuncionarioRequest {
    userId!: string;
    matricula!: string;
    cargo!: string;
    nome!: string;
    sobreNome!: string;
    email!: string;
    dataNascimento!: Date | null;
    telefones!: TelefoneRequest[];
    enderecoComercial!: EnderecoRequest;
    enderecoResidencial!: EnderecoRequest;
    ativo!: boolean;

   
    constructor (funcionario: FuncionarioResponse){
this.ativo = funcionario.ativo;
this.cargo = funcionario.cargo;
this.dataNascimento = funcionario.dataNascimento;
this.email = funcionario.email;
this.matricula = funcionario.matricula;
this.nome =  funcionario.nome;
this.sobreNome = funcionario.sobreNome;
this.userId = funcionario.userId;
this.enderecoComercial = new EnderecoRequest(funcionario.enderecoComercial);

    }

}
