namespace Node.Utility;

internal static class ExceptionPolicy
{
    public static bool IsFatal(Exception ex)
    {
        return ex is OutOfMemoryException or StackOverflowException or AccessViolationException;
    }
}