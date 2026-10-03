using Microsoft.AspNetCore.Mvc;
using TrueLine.Application.Common;

namespace TrueLine.Api.Controllers;

internal static class ApiResults
{
    public static ActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
    {
        return result.Error switch
        {
            ServiceError.NotFound => controller.NotFound(),
            ServiceError.BadRequest => controller.BadRequest(result.Message),
            ServiceError.Unauthorized => result.Message is null
                ? controller.Unauthorized()
                : controller.Unauthorized(result.Message),
            ServiceError.Conflict => controller.Conflict(result.Message),
            ServiceError.StorageFailed => controller.Problem(
                detail: result.Message,
                statusCode: StatusCodes.Status502BadGateway),
            _ => controller.Problem(),
        };
    }
}
