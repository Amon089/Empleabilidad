using System;

namespace Pqrs.Application.Exceptions;

public class AppException : Exception
{
    public string Code { get; }

    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key) 
        : base($"{entity.ToUpper()}_NOT_FOUND", $"{entity} with key '{key}' was not found.")
    {
    }
}

public class UnauthorizedTenantException : AppException
{
    public UnauthorizedTenantException(string message = "Access to resource of another tenant is prohibited.") 
        : base("UNAUTHORIZED_TENANT_ACCESS", message)
    {
    }
}

public class ValidationException : AppException
{
    public ValidationException(string message) 
        : base("VALIDATION_ERROR", message)
    {
    }
}
