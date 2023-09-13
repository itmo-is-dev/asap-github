using Itmo.Dev.Asap.Core.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Core.Models;
using Itmo.Dev.Asap.Github.Application.Core.Services.SubjectCourses;
using Itmo.Dev.Asap.Github.Integrations.Core.Mapping;

namespace Itmo.Dev.Asap.Github.Integrations.Core.Services;

public class SubjectCourseService : ISubjectCourseService
{
    private readonly Asap.Core.SubjectCourses.SubjectCourseService.SubjectCourseServiceClient _client;

    public SubjectCourseService(Asap.Core.SubjectCourses.SubjectCourseService.SubjectCourseServiceClient client)
    {
        _client = client;
    }

    public async Task UpdateMentorsAsync(
        Guid subjectCourseId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var request = new UpdateMentorsRequest
        {
            SubjectCourseId = subjectCourseId.ToString(),
            UserIds = { userIds.Select(x => x.ToString()) },
        };

        await _client.UpdateMentorsAsync(request, cancellationToken: cancellationToken);
    }

    public async Task<GetSubjectCourseStudentsResponse> GetSubjectCourseStudents(
        GetSubjectCourseStudentsRequest request,
        CancellationToken cancellationToken)
    {
        var grpcRequest = new GetStudentsRequest
        {
            SubjectCourseId = request.SubjectCourseId.ToString(),
            PageToken = request.PageToken,
            PageSize = request.PageSize,
        };

        GetStudentsResponse response = await _client
            .GetStudentsAsync(grpcRequest, cancellationToken: cancellationToken);

        StudentDto[] students = response.Students
            .Select(x => x.ToDto())
            .ToArray();

        return new GetSubjectCourseStudentsResponse(students, response.PageToken);
    }
}