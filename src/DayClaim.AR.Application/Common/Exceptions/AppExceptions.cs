namespace DayClaim.AR.Application.Common.Exceptions;

public class NotFoundException(string entity, object key)
    : Exception($"{entity} with id '{key}' was not found.");

public class ForbiddenAccessException(string message = "You do not have permission to perform this action.")
    : Exception(message);

public class ValidationAppException(IDictionary<string, string[]> errors) : Exception("One or more validation failures occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}

public class ConflictException(string message) : Exception(message);
