namespace Itmo.Dev.Asap.Github.Common.Exceptions;

public class UnexpectedOperationResultException : AsapGithubException
{
    public UnexpectedOperationResultException() : base("Operation finished with unexpected result") { }
}