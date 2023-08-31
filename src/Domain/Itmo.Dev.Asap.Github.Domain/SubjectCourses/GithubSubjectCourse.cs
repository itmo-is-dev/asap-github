using RichEntity.Annotations;

namespace Itmo.Dev.Asap.Github.Domain.SubjectCourses;

public partial class GithubSubjectCourse : IEntity<Guid>
{
    public GithubSubjectCourse(
        Guid id,
        long organizationId,
        long templateRepositoryId,
        long mentorTeamId) : this(id)
    {
        OrganizationId = organizationId;
        TemplateRepositoryId = templateRepositoryId;
        MentorTeamId = mentorTeamId;
    }

    public long OrganizationId { get; }

    public long TemplateRepositoryId { get; }

    public long MentorTeamId { get; set; }
}