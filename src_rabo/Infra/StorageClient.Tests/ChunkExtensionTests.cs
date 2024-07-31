using AutoFixture;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Linq;
using Xunit;

namespace Rabobank.Compliancy.Infra.StorageClient.Tests;

public class ChunkExtensionTests
{
    [Fact]
    public void CanSplitListInChunks()
    {
        var fixture = new Fixture();
        var entities = fixture.CreateMany<PipelineRegistration>(250).ToList();
        var result = entities.ToChunksOf(100);

        Assert.Equal(3, result.Count());
        Assert.Equal(50, result.Last().Count());
    }
}