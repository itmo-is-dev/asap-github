﻿namespace Itmo.Dev.Asap.Github.Application.Invites;

public class GithubInviteBackgroundServiceConfiguration
{
    public TimeSpan Delay => new TimeSpan(Hours, Minutes, 0);

    public int Hours { get; set; }

    public int Minutes { get; set; }
}