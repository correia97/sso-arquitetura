import { EnderecoRequest } from './enderecoRequest';
import { TelefoneRequest } from './telefoneRequest';

export interface FuncionarioRequest {
    userId: string;
    matricula: string;
    cargo: string;
    nome: string;
    sobreNome: string;
    email: string;
    dataNascimento: string;
    telefones: TelefoneRequest[];
    enderecoComercial: EnderecoRequest;
    enderecoResidencial: EnderecoRequest;
    ativo: boolean;
}
