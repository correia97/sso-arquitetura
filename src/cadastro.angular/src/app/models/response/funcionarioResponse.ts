import { EnderecoResponse } from './enderecoResponse';
import { TelefoneResponse } from './telefoneResponse';

export interface FuncionarioResponse {
    userId: string;
    matricula: string;
    cargo: string;
    nome: string;
    sobreNome: string;
    email: string;
    dataNascimento: Date | null;
    telefones: TelefoneResponse[];
    enderecoComercial: EnderecoResponse;
    enderecoResidencial: EnderecoResponse;
    ativo: boolean;
}

