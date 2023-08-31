using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Octokit.Clients;
using Itmo.Dev.Asap.Github.Octokit.Extensions;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class GithubRepositoryService : IGithubRepositoryService
{
    private readonly IGithubClientProvider _clientProvider;
    private readonly ILogger<GithubRepositoryService> _logger;
    private readonly GithubApiClient _apiClient;

    public GithubRepositoryService(
        IGithubClientProvider clientProvider,
        ILogger<GithubRepositoryService> logger,
        GithubApiClient apiClient)
    {
        _clientProvider = clientProvider;
        _logger = logger;
        _apiClient = apiClient;
    }

    public async Task<GithubRepositoryModel?> FindByIdAsync(
        long organizationId,
        long repositoryId,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        return await _apiClient.FindRepositoryByIdAsync(repositoryId, token, cancellationToken);
    }

    public async Task AddTeamPermissionAsync(
        long organizationId,
        long repositoryId,
        long teamId,
        RepositoryPermission permission,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        GithubOrganizationModel? organization = await _apiClient
            .FindOrganizationByIdAsync(organizationId, token, cancellationToken);

        if (organization is null)
        {
            _logger.LogCritical(
                "Failed to load organization = {OrganizationId}, but somehow got it's client",
                organizationId);

            return;
        }

        GithubRepositoryModel? repository = await _apiClient
            .FindRepositoryByIdAsync(repositoryId, token, cancellationToken);

        if (repository is null)
        {
            _logger.LogError("Failed to load repository = {RepositoryId}", repositoryId);
            return;
        }

        await _apiClient.UpdateTeamPermissionsAsync(
            organizationId,
            teamId,
            organization.Name,
            repository.Name,
            permission,
            token,
            cancellationToken);
    }

    public async Task<AddPermissionResult> AddUserPermissionAsync(
        long organizationId,
        long repositoryId,
        long userId,
        RepositoryPermission permission,
        CancellationToken cancellationToken)
    {
        string permissionString = permission.ToGithubApiString();

        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        GithubUserModel? user = await _apiClient.FindUserByIdAsync(userId, token, cancellationToken);

        if (user is null)
            return AddPermissionResult.Failed;

        bool isCollaborator = await client.Repository.Collaborator.IsCollaborator(repositoryId, user.Username);

        if (isCollaborator)
            return AddPermissionResult.AlreadyCollaborator;

        GithubOrganizationModel? organization = await _apiClient
            .FindOrganizationByIdAsync(organizationId, token, cancellationToken);

        if (organization is null)
        {
            _logger.LogCritical(
                "Failed to load organization = {OrganizationId}, but somehow loaded it's client",
                organizationId);

            return AddPermissionResult.Failed;
        }

        GithubRepositoryModel? repository = await _apiClient
            .FindRepositoryByIdAsync(repositoryId, token, cancellationToken);

        if (repository is null)
            return AddPermissionResult.Failed;

        RepositoryInvitation invitation = await client.Repository.Collaborator.Invite(
            organization.Name,
            repository.Name,
            user.Username,
            new CollaboratorRequest(permissionString));

        if (invitation is null)
        {
            _logger.LogInformation(
                "Adding permission {Permission} for {Username} in {OrganizationName}/{RepositoryName}",
                permission,
                user.Username,
                organization.Name,
                repository.Name);

            await client.Repository.Collaborator.Add(
                organization.Name,
                repository.Name,
                user.Username,
                new CollaboratorRequest(permissionString));

            return AddPermissionResult.Invited;
        }

        if (DateTimeOffset.UtcNow.Subtract(invitation.CreatedAt) < TimeSpan.FromDays(7))
            return AddPermissionResult.Pending;

        _logger.LogInformation(
            "Invitation for {Username} in {OrganizationName}/{RepositoryName} is expired, renewing",
            user.Username,
            organization,
            repository.Name);

        await client.Repository.Invitation.Delete(repositoryId, invitation.Id);

        await client.Repository.Collaborator.Add(
            organization.Name,
            repository.Name,
            user.Username,
            new CollaboratorRequest(permissionString));

        return AddPermissionResult.ReInvited;
    }

    public async Task<long?> CreateRepositoryFromTemplateAsync(
        long organizationId,
        string newRepositoryName,
        long templateRepositoryId,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        GithubOrganizationModel? organization = await _apiClient
            .FindOrganizationByIdAsync(organizationId, token, cancellationToken);

        if (organization is null)
        {
            _logger.LogCritical(
                "Failed to load organization = {OrganizationId}, but somehow loaded it's client",
                organizationId);

            return null;
        }

        GithubRepositoryModel? templateRepository = await _apiClient
            .FindRepositoryByIdAsync(templateRepositoryId, token, cancellationToken);

        if (templateRepository is null)
            return null;

        var userRepositoryFromTemplate = new NewRepositoryFromTemplate(newRepositoryName)
        {
            Owner = organization.Name,
            Description = null,
            Private = true,
        };

        _logger.LogInformation(
            "Creating repository {OrganizationName}/{RepositoryName} from {Template}",
            organization,
            newRepositoryName,
            templateRepository.Name);

        Repository createdRepository = await client.Repository.Generate(
            organization.Name,
            templateRepository.Name,
            userRepositoryFromTemplate);

        return createdRepository.Id;
    }
}