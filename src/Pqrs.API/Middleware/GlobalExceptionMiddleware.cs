using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pqrs.Application.Exceptions;

namespace Pqrs.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        string code = "INTERNAL_SERVER_ERROR";
        string message = "An unexpected error occurred.";
        int statusCode = StatusCodes.Status500InternalServerError;

        if (exception is NotFoundException nfe)
        {
            statusCode = StatusCodes.Status404NotFound;
            code = nfe.Code;
            message = nfe.Message;
        }
        else if (exception is UnauthorizedTenantException ute)
        {
            statusCode = StatusCodes.Status403Forbidden;
            code = ute.Code;
            message = ute.Message;
        }
        else if (exception is ValidationException ve)
        {
            statusCode = StatusCodes.Status400BadRequest;
            code = ve.Code;
            message = ve.Message;
        }
        else if (exception is AppException ae)
        {
            statusCode = StatusCodes.Status400BadRequest;
            code = ae.Code;
            message = ae.Message;
        }

        context.Response.StatusCode = statusCode;

        var errorResponse = new
        {
            error = new
            {
                code = code,
                message = message
            }
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
