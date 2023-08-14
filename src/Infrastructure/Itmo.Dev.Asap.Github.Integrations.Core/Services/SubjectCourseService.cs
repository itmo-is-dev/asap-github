using Itmo.Dev.Asap.Core.Models;
using Itmo.Dev.Asap.Core.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Core.Models;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Integrations.Core.Mapping;
using System.Runtime.CompilerServices;

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

    public async IAsyncEnumerable<StudentDto> GetSubjectCourseStudentIds(
        Guid subjectCourseId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new GetStudentsRequest { SubjectCourseId = subjectCourseId.ToString() };
        GetStudentsResponse response = await _client.GetStudentsAsync(request, cancellationToken: cancellationToken);

        foreach (Student student in response.Students)
        {
            yield return student.ToDto();
        }
    }
}