using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.PullRequestEvents;

public readonly struct InvalidStateMessage
{
    private readonly string _operationPrefix;
    private readonly SubmissionState _state;

    public InvalidStateMessage(string operationPrefix, SubmissionState state)
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