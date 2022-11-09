namespace Cadastro.API.Models.Response
{
    public class Response<T>
    {
        protected Response()
        {
        }
        private Response(T data, int qtdTotalItens = 0, int? qtdItensPorPaginas = null, int? paginaAtual = null)
        {
            Data = data;
            QtdTotalItens = qtdTotalItens;
            QtdItensPorPaginas = qtdItensPorPaginas;
            PaginaAtual = paginaAtual;
            Sucesso = true;
        }
        private Response(string erro)
        {
            Sucesso = false;
            Erro = erro;
        }

        public T Data { get; private set; }
        public bool Sucesso { get; private set; }
        public string Erro { get; private set; }
        public int QtdTotalItens { get; private set; }
        public int? QtdItensPorPaginas { get; private set; }
        public int? PaginaAtual { get; private set; }
        public int? QtdTotalPaginas
        {
            get
            {
                if (QtdTotalItens <= 0 || QtdItensPorPaginas <= 0)
                    return null;
                var total = QtdTotalItens / QtdItensPorPaginas;
                total = total > 0 ? total : 1;
                return total;
            }
        }

        public static Response<T> SuccessResult(T data, int qtdTotalItens = 0)
        {
            return new Response<T>(data, qtdTotalItens);
        }

        public static Response<T> SuccessResult(T data, int qtdTotalItens = 0, int qtdItensPorPaginas = 0, int paginaAtual = 0)
        {
            return new Response<T>(data, qtdTotalItens, qtdItensPorPaginas, paginaAtual);
        }

        public static Response<T> ErrorResult(string errorMessage)
        {
            return new Response<T>(errorMessage);
        }
    }
}
