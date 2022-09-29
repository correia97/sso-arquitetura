namespace Cadastro.API.Models.Response
{
    public class Response<T>
    {
        protected Response()
        {
        }
        private Response(T data)
        {
            Data = data;
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

        public static Response<T> SuccessResult(T data)
        {
            return new Response<T>(data);
        }

        public static Response<T> ErrorResult(string errorMessage)
        {
            return new Response<T>(errorMessage);
        }
    }
}
