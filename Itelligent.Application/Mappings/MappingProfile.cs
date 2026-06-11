using AutoMapper;
using Itelligent.Application.DTOs.Articles;
using Itelligent.Application.DTOs.Auth;
using Itelligent.Application.DTOs.Comments;
using Itelligent.Domain.Entities;

namespace Itelligent.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<Article, ArticleDto>()
            .ForMember(d => d.AuthorUsername, o => o.MapFrom(s => s.Author.Username));

        CreateMap<Article, ArticleDetailDto>()
            .ForMember(d => d.AuthorUsername, o => o.MapFrom(s => s.Author.Username));

        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.User.Username));
    }
}
