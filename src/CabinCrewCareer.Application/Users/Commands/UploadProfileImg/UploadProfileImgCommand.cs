using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.Users.Commands.UploadProfileImg
{
    public record UploadProfileImgCommand(IFormFile File) : IRequest<Response<GetUserResponse>>;
}
