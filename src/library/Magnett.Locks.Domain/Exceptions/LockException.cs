namespace Magnett.Locks.Domain.Exceptions;

public abstract class LockException : Exception
{
    protected LockException(string message) : base(message) { }
    protected LockException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class UnauthorizedException : LockException
{
    public UnauthorizedException(string message = "Unauthorized") : base(message) { }
}

public sealed class InvalidArgumentException : LockException
{
    public InvalidArgumentException(string message) : base(message) { }
}

public sealed class AlreadyLockedException : LockException
{
    public AlreadyLockedException(string message = "Resource is already locked") : base(message) { }
}

public sealed class LockNotFoundException : LockException
{
    public LockNotFoundException(string message = "Lock not found") : base(message) { }
}

public sealed class LockConflictException : LockException
{
    public LockConflictException(string message = "Lock conflict detected") : base(message) { }
}

public sealed class BackendUnavailableException : LockException
{
    public BackendUnavailableException(string message = "Backend service unavailable") : base(message) { }
    public BackendUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}

