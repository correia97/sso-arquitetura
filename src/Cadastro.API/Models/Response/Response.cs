namespace Cadastro.API.Models.Response
{
    public class Response<T>
    {
        protected Response()
        {
        }
        public T Data { get; private set; }
        public bool Sucesso { get; private set; }
        public string Erro { get; private set; }

        public static Response<T> SuccessResult(T data)
        {
            return new Response<T>()
            {
                Sucesso = true,
                Data = data
            };
        }

        public static Response<T> ErrorResult(string errorMessage)
        {
            return new Response<T>()
            {
                Sucesso = false,
                Erro = errorMessage
            };
        }
    }
}
