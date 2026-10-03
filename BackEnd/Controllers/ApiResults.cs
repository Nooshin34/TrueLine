using Microsoft.AspNetCore.Mvc;
using TrueLine.Application.Services;

namespace TrueLine.Api.Controllers;

internal static class ApiResults
{
    public static ActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
    {
        return result.Error switch
        {
            ServiceError.NotFound => result.Message is null
                ? controller.NotFound()
                : controller.NotFound(result.Message),
            ServiceError.BadRequest => controller.BadRequest(result.Message),
            ServiceError.Unauthorized => result.Message is null
                ? controller.Unauthorized()
                : controller.Unauthorized(result.Message),
            ServiceError.Conflict => controller.Conflict(result.Message),
            ServiceError.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden),
            ServiceError.StorageFailed => controller.Problem(
                detail: result.Message,
                statusCode: StatusCodes.Status502BadGateway),
            _ => controller.Problem(),
        };
    }
}
