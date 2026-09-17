using Filia.Api.Contracts;
using Filia.Application.Common.Models;
using Filia.Application.Files.Commands.DeleteFile;
using Filia.Application.Files.Commands.UpdateFileMetadata;
using Filia.Application.Files.Commands.UploadFile;
using Filia.Application.Files.Queries.GetDownloadUrl;
using Filia.Application.Files.Queries.GetFileById;
using Filia.Application.Files.Queries.GetFilesList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Filia.Api.Controllers;

[ApiController]
[Route("api/v1/files")]
[Produces("application/json")]
public class FilesController(ISender sender) : ControllerBase
{
    [HttpGet("sample")]
    public ActionResult<string> SampleApi()
    {
        return Ok("ok");
    }

    /// <summary>Uploads a new file.</summary>
    [HttpPost]
    [RequestSizeLimit(500_000_000)]
    [ProducesResponseType(typeof(UploadFileResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<UploadFileResult>> Upload(
        [FromForm] UploadFileRequest request,
        CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        UploadFileResult result = null;
        try
        {
            result = await sender.Send(new UploadFileCommand
            {
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                Content = stream,
                FolderPath = request.FolderPath,
                Title = request.Title,
                Description = request.Description,
            }, cancellationToken);
        }
        catch (Exception ex)
        {
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    /// <summary>Gets metadata for a single file.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FileDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFileByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists files, optionally filtered by folder or search term.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<FileListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FileListItemDto>>> List(
        [FromQuery] string? folderPath,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetFilesListQuery
        {
            FolderPath = folderPath,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>Returns a short-lived presigned URL to download the file directly from RustFS.</summary>
    [HttpGet("{id:guid}/download-url")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetDownloadUrl(Guid id, CancellationToken cancellationToken)
    {
        var url = await sender.Send(new GetDownloadUrlQuery(id), cancellationToken);
        return Ok(new { downloadUrl = url });
    }

    /// <summary>Updates a file's display name and/or folder.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMetadata(
        Guid id,
        [FromBody] UpdateFileMetadataRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateFileMetadataCommand
        {
            Id = id,
            FileName = request.FileName,
            FolderPath = request.FolderPath
        }, cancellationToken);

        return NoContent();
    }

    /// <summary>Soft-deletes a file and removes the underlying object from storage.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteFileCommand(id), cancellationToken);
        return NoContent();
    }
}