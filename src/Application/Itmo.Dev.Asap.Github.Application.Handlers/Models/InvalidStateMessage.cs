using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Models;

public readonly struct InvalidStateMessage
{
    private readonly string _operationPrefix;
    private readonly SubmissionStateDto _state;

    public InvalidStateMessage(string operationPrefix, SubmissionStateDto state)
    {
        _operationPrefix = operationPrefix;
        _state = state;
    }

    public override string ToString()
    {
        return $"{_operationPrefix} cannot be processed for submission in {_state} state";
    }

    public static implicit operator string(InvalidStateMessage message)
        => message.ToString();
}