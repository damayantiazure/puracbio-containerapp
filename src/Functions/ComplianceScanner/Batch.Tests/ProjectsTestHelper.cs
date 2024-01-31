#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Tests;

public class ProjectsTestHelper
{
    public static Project CreateProjectWithParameters(string name, string id, string description, Uri url)
    {
        var project = new Project
        {
            Name = name,
            Id = id,
            Description = description,
            Url = url
        };

        return project;
    }

    private static Project CreateDummyProject()
    {
        var fixture = new Fixture();
        return fixture.Create<Project>();
    }

    public static IEnumerable<Project> CreateMultipleProjectsResponse(int number)
    {
        return Enumerable
            .Range(0, number)
            .Select(_ => CreateDummyProject());
    }
    public static IEnumerable<Project> CreateMultipleProjectsResponseAsync(int number)
    {
        return Enumerable
            .Range(0, number)
            .Select(_ => CreateDummyProject());
    }

    [Fact]
    public void CreateMultipleProjectsResponseShouldCreateGivenNumberOfProjects()
    {
        var projects = CreateMultipleProjectsResponse(2);
        projects.Count().ShouldBe(2);
    }
}