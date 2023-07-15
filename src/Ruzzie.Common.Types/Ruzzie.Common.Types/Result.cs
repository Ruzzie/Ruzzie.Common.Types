using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Ruzzie.Common.Types.Diagnostics;

namespace Ruzzie.Common.Types;

public static class Result
{
    public static Result<TError, T> Ok<TError, T>(T ok)
    {
        return Result<TError, T>.Ok(ok);
    }

    public static Result<TError, T> Err<TError, T>(TError err)
    {
        return Result<TError, T>.Err(err);
    }

    public static Result<TError, T> AsOk<TError, T>(this T ok)
    {
        return Result<TError, T>.Ok(ok);
    }

    public static Result<TError, T> AsErr<TError, T>(this TError err)
    {
        return Result<TError, T>.Err(err);
    }
}

public delegate TU OnErr<out TU, TError>(in TError err);

public delegate TU OnOk<out TU, T>(in T ok);

/// <summary>
/// Result is a type that represents either success (Ok) or failure (Err).
/// </summary>
/// <typeparam name="TError">The type of the Err value.</typeparam>
/// <typeparam name="TOk">The type of the Ok value.</typeparam>
/// <remarks>
/// The default is Err. When <see cref="Result{TError,T}" /> is initialized as default, it will throw a <exception cref="PanicException{TError}"></exception> when trying to obtain the err value.
/// This such that chaining and composing results that are ok should work with the default.
/// </remarks>
[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[SkipLocalsInit]
public readonly record struct Result<TError, TOk> : ISerializable
{
    private const    string VariantFieldName = "variant";
    private const    string ValueFieldName   = "ok";
    private const    string ErrFieldName     = "err";
    private readonly TError _err;

    private readonly bool          _initialized = false;
    private readonly TOk           _value;
    private readonly ResultVariant _variant;

    private string DebuggerDisplay => $"{_variant}({ToString()})";

    /// <summary>
    /// Gets the error value.
    /// </summary>
    /// <value>
    /// The error value.
    /// </value>
    /// <exception cref="PanicException{TError}">When the result is not initialized (initialized as default).</exception>
    private TError ErrValue
    {
        get
        {
            if (!_initialized)
            {
                throw ResultPanicExtensions
                    .CreatePanicExceptionForErr($"Result is uninitialized. You cannot obtain the Err value.", _err);
            }

            return _err;
        }
    }

    private Result(in TError err, in TOk ok, ResultVariant variant)
    {
        _variant     = variant;
        _err         = err;
        _value       = ok;
        _initialized = true;
    }

    public Result(TError err) : this(err, default!, ResultVariant.Err)
    {
    }

    public Result(TOk ok) : this(default!, ok, ResultVariant.Ok)
    {
    }

    //Deserialize
    private Result(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
        _variant = (ResultVariant)serializationInfo.GetByte(VariantFieldName);
        if (_variant == ResultVariant.Ok)
        {
            //note: decide on whether to panic or leave this as is, since it is possible that the caller intended to serialize a null value
            _value       = (TOk?)serializationInfo.GetValue(ValueFieldName, typeof(TOk))!;
            _err         = default!;
            _initialized = true;
        }
        else
        {
            //note: decide on whether to panic or leave this as is, since it is possible that the caller intended to serialize a null value
            _err         = (TError?)serializationInfo.GetValue(ErrFieldName, typeof(TError))!;
            _value       = default!;
            _initialized = true;
        }
    }

    //Serialize
    /// <inheritdoc />
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(VariantFieldName, (byte)_variant);
        if (IsOk())
        {
            info.AddValue(ValueFieldName, _value);
        }
        else
        {
            info.AddValue(ErrFieldName, _err);
        }
    }

    /// Experimental deconstruct
    public void Deconstruct(out TError? err, out TOk? value, out bool isErr)
    {
        err   = _err;
        value = _value;
        isErr = IsErr();
    }

    public override string ToString()
    {
        switch (_variant)
        {
            case ResultVariant.Err:
                return _err?.ToString() ?? "";
            case ResultVariant.Ok:
                return _value?.ToString() ?? "";
            default:
                return "";
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOk()
    {
        return _variant == ResultVariant.Ok;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsErr()
    {
        return _variant == ResultVariant.Err;
    }

    /// <summary>
    /// Deconstruct a Result to a tuple of options.
    /// </summary>
    public static implicit operator (Option<TError> err, Option<TOk> ok)(in Result<TError, TOk> res)
    {
        return (err: res.Err(), ok: res.Ok());
    }

    public static implicit operator Option<TOk>(in Result<TError, TOk> res)
    {
        return res.Ok();
    }

    public static implicit operator Option<TError>(in Result<TError, TOk> res)
    {
        return res.Err();
    }

    public static implicit operator Result<TError, TOk>(in TOk ok)
    {
        return new Result<TError, TOk>(ok);
    }

    public static implicit operator Result<TError, TOk>(in TError err)
    {
        return new Result<TError, TOk>(err);
    }

    public static Result<TError, TOk> Ok(in TOk value)
    {
        return new Result<TError, TOk>(value);
    }

    public static Result<TError, TOk> Err(in TError value)
    {
        return new Result<TError, TOk>(value);
    }

    /// <summary>
    ///Converts from Result{TError, T} to Option{T}.
    /// Converts this into an Option{T}, discarding the error, if any.
    /// </summary>
    public Option<TOk> Ok()
    {
        if (IsOk())
        {
            return Option<TOk>.Some(_value);
        }

        return Option<TOk>.None;
    }

    /// <summary>
    ///Converts from Result{TError, T} to Option{T}.
    /// Converts this into an Option{T}, discarding the success value, if any.
    /// </summary>
    public Option<TError> Err()
    {
        if (IsErr())
        {
            return Option<TError>.Some(ErrValue);
        }

        return Option<TError>.None;
    }


    /// <summary>
    ///  Control flow based on pattern matching.
    /// </summary>
    /// <typeparam name="TU">The output type of the match.</typeparam>
    /// <param name="onErr">Calls this when the type is Err.</param>
    /// <param name="onOk">Calls this when the type is right.</param>
    /// <returns></returns>
    public TU Match<TU>(Func<TError, TU> onErr, Func<TOk, TU> onOk)
    {
        return IsOk() ? onOk(_value) : onErr(ErrValue);
    }

    /// <summary>
    ///  Control flow based on pattern matching.
    /// </summary>
    /// <typeparam name="TU">The output type of the match.</typeparam>
    /// <param name="onErr">Calls this when the type is Err.</param>
    /// <param name="onOk">Calls this when the type is right.</param>
    /// <returns></returns>
    public TU Match<TU>(OnErr<TU, TError> onErr, OnOk<TU, TOk> onOk)
    {
        return IsOk() ? onOk(_value) : onErr(ErrValue);
    }

    public unsafe TU Match<TU>(delegate*<in TError, TU> onErr, delegate*<in TOk, TU> onOk)
    {
        return IsOk() ? onOk(_value) : onErr(ErrValue);
    }

    public unsafe TU Match<TU>(delegate*<TError, TU> onErr, delegate*<TOk, TU> onOk)
    {
        return IsOk() ? onOk(_value) : onErr(ErrValue);
    }

    /// <summary>
    ///Calls op if the result is Ok, otherwise returns the Err value of self.
    ///This function can be used for control flow based on Result values.
    /// </summary>
    public void For(Action<TError> onErr, Action<TOk> onOk)
    {
        if (IsOk())
        {
            onOk(_value);
        }
        else
        {
            onErr(ErrValue);
        }
    }

    /// Map current <see cref="Result{TError,TOk}"/> to a new <see cref="Result{TError,TOk}"/> where error is mapped with <paramref name="selectErr"/> and ok is mapped with <paramref name="selectOk"/>.
    public Result<TF, TU> Map<TF, TU>(Func<TError, TF> selectErr, Func<TOk, TU> selectOk)
    {
        return IsOk()
                   ? Result<TF, TU>.Ok(selectOk(_value))
                   : Result<TF, TU>.Err(selectErr(ErrValue));
    }

    /// <summary>
    ///Maps a Result{TErr,T} to Result{TErr,TU} by applying a function to a contained Ok value, leaving an Err value untouched.
    ///  This function can be used to compose the results of two functions.
    /// </summary>
    public Result<TError, TU> MapOk<TU>(Func<TOk, TU> mapResultTo)
    {
        return IsOk() ? Result<TError, TU>.Ok(mapResultTo(_value)) : Result<TError, TU>.Err(ErrValue);
    }

    /// <summary>
    ///Maps a Result{TErr,T} to Result{TF,T} by applying a function to a contained Err value, leaving an Ok value untouched.
    ///    This function can be used to pass through a successful result while handling an error.
    /// </summary>
    public Result<TF, TOk> MapErr<TF>(Func<TError, TF> handleError)
    {
        return IsOk() ? Result<TF, TOk>.Ok(_value) : Result<TF, TOk>.Err(handleError(ErrValue));
    }

    /// <summary>
    ///Returns res if the result is Ok, otherwise returns the Err value of self.
    /// </summary>
    public Result<TError, TU> And<TU>(Result<TError, TU> result)
    {
        return IsOk() ? result : Result<TError, TU>.Err(ErrValue);
    }

    /// <summary>
    ///Returns res if the result is Err, otherwise returns the Ok value of self.
    ///    Arguments passed to or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use or_else, which is lazily evaluated.
    /// </summary>
    public Result<TF, TOk> Or<TF>(Result<TF, TOk> result)
    {
        return IsOk() ? Result<TF, TOk>.Ok(_value) : result;
    }

    /// <summary>
    ///Returns res if the result is Err, otherwise returns the Ok value of self.
    ///    Arguments passed to or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use or_else, which is lazily evaluated.
    /// </summary>
    public Result<TF, TOk> Or<TF>(in Result<TF, TOk> result)
    {
        return IsOk() ? Result<TF, TOk>.Ok(_value) : result;
    }

    /// <summary>
    ///Calls op if the result is Err, otherwise returns the Ok value of self.
    ///    This function can be used for control flow based on result values.
    /// </summary>
    public Result<TF, TOk> OrElse<TF>(Func<TError, Result<TF, TOk>> op)
    {
        return IsOk() ? Result<TF, TOk>.Ok(_value) : op(ErrValue);
    }

    /// <summary>
    ///Calls op if the result is Err, otherwise returns the Ok value of self.
    ///    This function can be used for control flow based on result values.
    /// </summary>
    public Result<TF, TOk> OrElse<TF>(OnErr<Result<TF, TOk>, TError> op)
    {
        return IsOk() ? Result<TF, TOk>.Ok(_value) : op(ErrValue);
    }

    /// <summary>
    ///Unwraps a result, yielding the content of an Ok. Else, it returns optb.
    ///    Arguments passed to unwrap_or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use unwrap_or_else, which is lazily evaluated.
    /// </summary>
    public TOk UnwrapOr(TOk optb)
    {
        //Could use match function, but I assume this is faster.
        return IsOk() ? _value : optb;
    }

    /// <summary>
    ///Unwraps a result, yielding the content of an Ok. If the value is an Err then it calls op with its value.
    /// </summary>
    public TOk UnwrapOrElse(Func<TError, TOk> op)
    {
        //Could use match function, but I assume this is faster.
        return IsOk() ? _value : op(ErrValue);
    }

    /// <summary>
    ///Unwraps a result, yielding the content of an Ok. If the value is an Err then it calls op with its value.
    /// </summary>
    public TOk UnwrapOrElse(OnErr<TOk, TError> op)
    {
        //Could use match function, but I assume this is faster.
        return IsOk() ? _value : op(ErrValue);
    }

    /// <summary>
    ///Joins two results to a tuple of Ok values when both are Ok value, returns the first error otherwise.
    ///  This function can be used to compose results of 2 functions.
    /// </summary>
    public Result<TError, (TOk, TU)> JoinOk<TU>(Result<TError, TU> secondResult)
    {
        if (!IsOk())
        {
            return _err;
        }

        if (secondResult.IsOk())
        {
            return (_value, secondResult._value);
        }

        return secondResult._err;
    }

    /// <summary>
    ///Joins two results to a tuple of Ok values when both are Ok value, returns the first error otherwise.
    ///  This function can be used to compose results.
    /// </summary>
    public unsafe Result<TError, (TOk, TU)> AndJoinOk<TU>(delegate*<Result<TError, TU>> secondAction)
    {
        if (!IsOk())
        {
            return _err;
        }

        var secondResult = secondAction();

        if (secondResult.IsOk())
        {
            return (_value, secondResult._value);
        }

        return secondResult._err;
    }

    /// <summary>
    ///Joins two results to a tuple of Ok values when both are Ok value, returns the first error otherwise.
    ///  This function can be used to compose results.
    /// </summary>
    public Result<TError, (TOk, TU)> AndJoinOk<TU>(Func<Result<TError, TU>> secondAction)
    {
        if (!IsOk())
        {
            return _err;
        }

        return MapOk2(secondAction(), (f, s) => (f, s));
    }


    /// When 2 results are Ok the map function will be called. The first error will be called otherwise.
    /// this can be used to compose results.
    public Result<TError, TNewOk> MapOk2<TNewOk, TU>(Result<TError, TU> secondResult, Func<TOk, TU, TNewOk> map)
    {
        if (!IsOk())
        {
            return _err;
        }

        if (secondResult.IsOk())
        {
            return map(_value, secondResult._value);
        }

        return secondResult._err;
    }

    /// <summary>
    /// Deconstructs the Result type to the given out parameters and returns if the Result was Ok.
    /// </summary>
    /// <remarks>This makes it easier to consume in imperative code.</remarks>
    public bool IsOk(out TOk ok, out TError err, TOk okDefault, TError errDefault)
    {
        switch (_variant)
        {
            default:
            case ResultVariant.Err:
                ok  = okDefault;
                err = ErrValue;
                return false;

            case ResultVariant.Ok:
                ok  = _value;
                err = errDefault;
                return true;
        }
    }

    /// <summary>
    /// Deconstructs the Result type to the given out parameters and returns if the Result was Err.
    /// </summary>
    /// <remarks>This makes it easier to consume in imperative code.</remarks>
    public bool IsErr(out TOk ok, out TError err, TOk okDefault, TError errDefault)
    {
        return !IsOk(out ok, out err, okDefault, errDefault);
    }

    public (Option<TError> error, Option<TOk> ok) GetValue()
    {
        if (IsOk())
        {
            return (error: Option<TError>.None, ok: _value);
        }

        return (error: ErrValue, ok: Option<TOk>.None);
    }

    private enum ResultVariant : byte
    {
        // The default variant is error.
        //   When obtaining Error value a check sill be done to check whether the type is initialized
        Err = 0
      , Ok  = 1
    }

    #region AndThen

    /// <summary>
    ///Calls op if the result is Ok, otherwise returns the Err value of self.
    ///This function can be used for control flow based on Result values.
    /// </summary>
    public Result<TError, TU> AndThen<TU>(Func<TOk, Result<TError, TU>> op)
    {
        return IsOk() ? op(_value) : Result<TError, TU>.Err(ErrValue);
    }

    /// <summary>
    ///Calls op if the result is Ok, otherwise returns the Err value of self.
    ///This function can be used for control flow based on Result values.
    /// </summary>
    public Result<TError, TU> AndThen<TU>(OnOk<Result<TError, TU>, TOk> op)
    {
        return IsOk() ? op(_value) : Result<TError, TU>.Err(ErrValue);
    }

    /// <summary>
    ///Calls op if the result is Ok, otherwise returns the Err value of self.
    ///This function can be used for control flow based on Result values.
    /// </summary>
    public unsafe Result<TError, TU> AndThen<TU>(delegate*<in TOk, Result<TError, TU>> onOk)
    {
        return IsOk() ? onOk(_value) : Result<TError, TU>.Err(ErrValue);
    }

    /// <summary>
    ///Calls op if the result is Ok, otherwise returns the Err value of self.
    ///This function can be used for control flow based on Result values.
    /// </summary>
    public unsafe Result<TError, TU> AndThen<TU>(delegate*<TOk, Result<TError, TU>> onOk)
    {
        return IsOk() ? onOk(_value) : Result<TError, TU>.Err(ErrValue);
    }

    #endregion
}