namespace Cadastro.MVC.Models.Response
{
    public class Response<T>
    {
        public Response()
        {
        }
        private Response(T data, int qtdTotalItens = 0, int? qtdtensPorPaginas = null, int? paginaAtual = null)
        {
            Data = data;
            QtdTotalItens = qtdTotalItens;
            QtdItensPorPaginas = qtdtensPorPaginas;
            PaginaAtual = paginaAtual;
            Sucesso = true;
        }
        private Response(string erro)
        {
            Sucesso = false;
            Erro = erro;
        }

        public T Data { get; set; }
        public bool Sucesso { get; set; }
        public string Erro { get; set; }
        public int QtdTotalItens { get; set; }
        public int? QtdItensPorPaginas { get; set; }
        public int? PaginaAtual { get; set; }
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

        public static Response<T> SuccessResult(T data)
        {
            return new Response<T>(data, 0);
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
