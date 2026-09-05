using AutoMapper;
using Filia.Application.Files.Queries.GetFileById;
using Filia.Application.Files.Queries.GetFilesList;
using Filia.Domain.Entities;

namespace Filia.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<FileItem, FileDetailsDto>();
        CreateMap<FileItem, FileListItemDto>();
    }
}
