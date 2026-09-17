using System.Net;
using System.Text.Json;
using Filia.Application.Common.Exceptions;
using ValidationException = Filia.Application.Common.Exceptions.ValidationException;

namespace Filia.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Validation Failed", ex.Errors);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Not Found", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string title, object detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            title,
            status = (int)statusCode,
            detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
