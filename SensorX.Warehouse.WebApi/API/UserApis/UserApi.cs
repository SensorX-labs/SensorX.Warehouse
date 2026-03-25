using SensorX.Warehouse.Application.Commands.UserModule.Login;
using SensorX.Warehouse.Application.Commands.UserModule.SignUpWithLocalAccount;
using SensorX.Warehouse.Application.Commands.UserModule.UpdateProfile;
using SensorX.Warehouse.Application.Commands.UserModule.VerifyToken;
using SensorX.Warehouse.Application.Common.ResponseClient;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Warehouse.WebApi.API.UserApis.Request;

namespace SensorX.Warehouse.WebApi.API.UserApis
{
    public static class UserApi
    {
        public static RouteGroupBuilder MapUserApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/users").WithTags("Users");

            api.MapPost("/registerLocalAccount", RegisterLocalAccount).WithOpenApi();
            api.MapPut("/updateProfile/{id:long}", UpdateProfile).WithOpenApi();
            api.MapPost("/login", Login).WithOpenApi();
            api.MapPost("/verifyToken", VerifyToken).WithOpenApi();
            return api;
        }
        private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> RegisterLocalAccount(
            [FromBody] SignUpWithLocalAccountCommand command,
            [FromServices] IMediator mediator
        )
        {
            Result result = await mediator.Send(command);
            return result ? TypedResults.Ok() : TypedResults.BadRequest(result.Error);
        }

        private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> UpdateProfile(
            [FromRoute] long id,
            [FromBody] UpdateProfileRequest request,
            [FromServices] IMediator mediator
        )
        {
            var command = new UpdateProfileCommand(id, request.Name, request.UrlAvatar);
            Result result = await mediator.Send(command);
            return result ? TypedResults.Ok() : TypedResults.BadRequest(result.Error);
        }

        private static async Task<Results<Ok<string>, BadRequest<string>, ProblemHttpResult>> Login(
            [FromBody] LoginRequest request,
            [FromServices] IMediator mediator
        )
        {
            string deviceInfo = $"{request.Device} - {request.Browser}";
            var command = new LoginCommand(
                request.Email,
                request.Password,
                deviceInfo,
                request.IpAddress
            );

            Result<string> result = await mediator.Send(command);
            return result ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Error);
        }

        private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> VerifyToken(
            [FromBody] VerifyTokenRequest request,
            [FromServices] IMediator mediator
        )
        {
            if (!Enum.IsDefined(typeof(TypeOfVerificationToken), request.Type))
            {
                return TypedResults.BadRequest("Invalid verification token type.");
            }

            TypeOfVerificationToken type = (TypeOfVerificationToken)request.Type;
            var command = new VerifyTokenCommand(request.Token, request.Type);

            Result result = await mediator.Send(command);
            return result ? TypedResults.Ok() : TypedResults.BadRequest(result.Error);
        }
    }
}

