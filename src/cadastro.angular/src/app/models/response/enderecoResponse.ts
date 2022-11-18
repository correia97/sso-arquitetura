
export interface EnderecoResponse {
    id: number;
    rua: string;
    numero: number | null;
    cep: string;
    complemento: string;
    bairro: string;
    cidade: string;
    uf: string;
}