using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Filia.Application.Common.Models;
using Filia.Application.Files.Commands.UploadFile;
using Filia.Application.Files.Queries.GetFileById;
using Filia.Application.Files.Queries.GetFilesList;
using Filia.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Filia.IntegrationTests;

public class FilesControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public FilesControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static MultipartFormDataContent BuildUploadContent(string fileName, string content, string? folder = null)
    {
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(content));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "File", fileName);

        if (folder is not null)
            form.Add(new StringContent(folder), "FolderPath");

        return form;
    }

    [Fact]
    public async Task Upload_ThenGetById_ShouldReturnStoredFile()
    {
        using var content = BuildUploadContent("hello.txt", "hello integration test");

        var uploadResponse = await _client.PostAsync("/api/v1/files", content);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<UploadFileResult>();
        uploaded.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/v1/files/{uploaded!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content.ReadFromJsonAsync<FileDetailsDto>();
        dto!.FileName.Should().Be("hello.txt");
    }

    [Fact]
    public async Task Upload_ThenDelete_ShouldReturnNotFoundAfterwards()
    {
        using var content = BuildUploadContent("to-delete.txt", "bye");
        var uploadResponse = await _client.PostAsync("/api/v1/files", content);
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<UploadFileResult>();

        var deleteResponse = await _client.DeleteAsync($"/api/v1/files/{uploaded!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/files/{uploaded.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_ShouldReturnPaginatedResults()
    {
        using var content = BuildUploadContent("list-me.txt", "content", folder: "reports");
        await _client.PostAsync("/api/v1/files", content);

        var response = await _client.GetAsync("/api/v1/files?folderPath=reports&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PaginatedList<FileListItemDto>>();
        page!.Items.Should().Contain(f => f.FileName == "list-me.txt");
    }

    [Fact]
    public async Task GetById_WithUnknownId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/files/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
