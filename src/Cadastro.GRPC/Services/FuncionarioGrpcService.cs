using Grpc.Core;

namespace Cadastro.GRPC.Services
{
    public class FuncionarioGrpcService : FuncionarioGrpc.FuncionarioGrpcBase
    {
        private readonly ILogger<FuncionarioGrpcService> _logger;
        public FuncionarioGrpcService(ILogger<FuncionarioGrpcService> logger)
        {
            _logger = logger;
        }

        public override Task<BooleanResult> Cadastrar(FuncionarioRequest request, ServerCallContext context)
        {
            //try
            //{
            //    var result = _service.Cadastrar(funcionario);
            //    return Ok(result);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Post");
            //    return BadRequest(ex);
            //}
            return Task.FromResult(new BooleanResult { Result = true });
        }
        public override Task<BooleanResult> Atualizar(FuncionarioRequest request, ServerCallContext context)
        {
            //try
            //{
            //    var result = _service.Cadastrar(funcionario);
            //    return Ok(result);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Post");
            //    return BadRequest(ex);
            //}
            return Task.FromResult(new BooleanResult { Result = true });
        }
    }
}
