// This tells C# which folder/group this code belongs to
namespace LeadManagementSystem.Features.Common;

// This is a simple result object that tells you if something worked or failed
// "record" is a special C# type that holds data and cannot be changed after creation
// It has two pieces of info: Success (true/false) and a Message (text explaining what happened)
public sealed record OperationResult(bool Success, string Message)
{
    // A shortcut to create a "success" result with a message
    public static OperationResult Ok(string message) => new(true, message);

    // A shortcut to create a "failure" result with a message
    public static OperationResult Fail(string message) => new(false, message);
}

// This is the same as above, but it can also carry a value (like an ID or data)
// The <T> means it can hold any type of data — a number, a string, an object, etc.
public sealed record OperationResult<T>(bool Success, string Message, T? Value)
{
    // A shortcut to create a "success" result that also includes a value
    public static OperationResult<T> Ok(T value, string message) => new(true, message, value);

    // A shortcut to create a "failure" result with no value (just an error message)
    public static OperationResult<T> Fail(string message) => new(false, message, default);
}

/*
 * FILE SUMMARY: OperationResult.cs
 *
 * This file defines a simple "result" object used throughout the entire application.
 * Every time the app does something (create a lead, update a status, delete a record),
 * it returns an OperationResult to say whether it worked or failed, along with a message.
 * The generic version (OperationResult<T>) can also carry data back, like the ID of a newly created lead.
 * This is the standard way the app communicates success or failure between different layers.
 */
