namespace MinhaAgendaApi.Application.Exceptions;

// recurso pedido nao existe -> HTTP 404
public class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string mensagem) : base(mensagem) { }
}

// usuario logado nao pode mexer neste recurso -> HTTP 403
public class AcessoNegadoException : Exception
{
    public AcessoNegadoException(string mensagem) : base(mensagem) { }
}

// email ou senha errados -> HTTP 401
public class CredenciaisInvalidasException : Exception
{
    public CredenciaisInvalidasException() : base("E-mail ou senha inválidos.") { }
}
