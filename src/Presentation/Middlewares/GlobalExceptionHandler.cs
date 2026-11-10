using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MinhaAgendaApi.Application.Exceptions;
using MinhaAgendaApi.Domain.Exceptions;

namespace MinhaAgendaApi.Presentation.Middlewares;

// captura qualquer excecao nao tratada e devolve um ProblemDetails (RFC 7807)
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Dados inválidos"),
            DomainException => (StatusCodes.Status400BadRequest, "Regra de negócio violada"),
            CredenciaisInvalidasException or UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado"),
            AcessoNegadoException => (StatusCodes.Status403Forbidden, "Acesso negado"),
            RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erro não tratado");

        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            // em erro 500 nao expomos detalhes internos
            Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message
        };

        if (exception is ValidationException validacao)
        {
            problema.Detail = "Um ou mais campos são inválidos.";
            problema.Extensions["erros"] = validacao.Errors
                .Select(e => new { campo = e.PropertyName, mensagem = e.ErrorMessage });
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problema, cancellationToken);

        return true;
    }
}
