namespace MinhaAgendaApi.Domain.Exceptions;

//excecao personalizada para violacao de regras de negocio
public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem){}
}