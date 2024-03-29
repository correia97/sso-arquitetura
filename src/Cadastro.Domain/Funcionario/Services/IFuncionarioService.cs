﻿using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadastro.Domain.Services
{
    public interface IFuncionarioService
    {
        Task Cadastrar(Funcionario funcionario);
        Task Atualizar(Funcionario funcionario);
        Task Remover(Guid id);
        Task Desativar(Guid id);
        Task<Funcionario> ObterPorId(Guid id);
        Task<(IEnumerable<Funcionario>, int)> ObterTodos(int pagina, int qtdItens);
    }
}
