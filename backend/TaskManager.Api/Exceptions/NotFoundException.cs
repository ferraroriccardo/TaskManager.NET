namespace TaskManager.Api.Exceptions;

// "Domain" exception: services throw it when a resource doesn't exist.
// The GlobalExceptionHandler turns it into an HTTP 404 response.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
