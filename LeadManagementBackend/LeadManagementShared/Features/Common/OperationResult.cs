namespace LeadManagementSystem.Features.Common;

// A simple wrapper that holds the result of any operation: did it succeed or fail, and why?
// "sealed record" means this is an immutable (unchangeable) data object that can't be inherited
public sealed record OperationResult(bool Success, string Message)
{
    // Shortcut to create a successful result with a message
    public static OperationResult Ok(string message) => new(true, message);

    // Shortcut to create a failed result with an error message
    public static OperationResult Fail(string message) => new(false, message);
}

// A generic version that can also carry a value (e.g., the created object or fetched data)
// The <T> means "this can hold any type of value" — like a box that fits anything
public sealed record OperationResult<T>(bool Success, string Message, T? Value)
{
    // Shortcut to create a successful result that includes a value and a message
    public static OperationResult<T> Ok(T value, string message) => new(true, message, value);

    // Shortcut to create a failed result with no value, just an error message
    public static OperationResult<T> Fail(string message) => new(false, message, default);
}

/*
 * FILE SUMMARY — Features/Common/OperationResult.cs (Shared Library)
 * This file defines a simple "result wrapper" pattern used throughout the application.
 * Instead of throwing exceptions for business rule failures, methods return an OperationResult
 * that says whether the operation succeeded or failed, along with a human-readable message.
 * The generic version (OperationResult<T>) can also carry a data value on success.
 * As part of the shared library, both microservices and the main app use this to communicate
 * outcomes in a clean, consistent way.
 */
