export class Response<T> {
    data!: T;
    sucesso!: boolean;
    erro!: string;
    qtdTotalItens!: number;
    qtdItensPorPaginas!: number;
    paginaAtual!: number;
    qtdTotalPaginas: number = this.getQtdTotalPaginas();
    public getQtdTotalPaginas() {
        if (this.qtdTotalItens <= 0 || this.qtdItensPorPaginas <= 0) {
            return 0;
        }
        let total = Math.round(this.qtdTotalItens / this.qtdItensPorPaginas);
        total = total > 0 ? total : 1;
        return total;
    }
}