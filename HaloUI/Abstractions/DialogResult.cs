using System;

namespace HaloUI.Abstractions;

public readonly record struct DialogResult(bool IsCancelled, object? Value)
{
    public bool IsSuccess => !IsCancelled;

    public static DialogResult Success(object? value = null) => new(false, value);

    public static DialogResult Cancel() => new(true, null);

    public DialogResult<T> As<T>()
    {
        if (IsCancelled)
        {
            return DialogResult<T>.Cancel();
        }

        if (Value is T typed)
        {
            return DialogResult<T>.Success(typed);
        }

        return DialogResult<T>.Success((T?)Convert.ChangeType(Value, typeof(T))!);
    }
}

public readonly record struct DialogResult<T>(bool IsCancelled, T? Value)
{
    public bool IsSuccess => !IsCancelled;

    public T ValueOrThrow()
    {
        if (IsCancelled)
        {
            throw new InvalidOperationException("Dialog was cancelled.");
        }

        if (Value is null)
        {
            throw new InvalidOperationException("Dialog returned no value.");
        }

        return Value;
    }

    public static DialogResult<T> Success(T? value = default) => new(false, value);

    public static DialogResult<T> Cancel() => new(true, default);
}