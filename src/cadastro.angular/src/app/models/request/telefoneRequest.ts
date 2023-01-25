import { TelefoneResponse } from "../response/telefoneResponse";


export class TelefoneRequest {
    id!: number;
    ddi!: string;
    telefone!: string;

    constructor(telefone: TelefoneResponse) {
this.id = telefone.id;
this.ddi = telefone.ddi;
this.telefone = telefone.telefone;
    }
}