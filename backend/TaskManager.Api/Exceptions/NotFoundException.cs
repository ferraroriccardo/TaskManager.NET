namespace TaskManager.Api.Exceptions;

// Eccezione "di dominio": i service la lanciano quando una risorsa non esiste.
// Il GlobalExceptionHandler la trasforma in una risposta HTTP 404.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
