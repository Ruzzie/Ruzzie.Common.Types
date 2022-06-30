using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ruzzie.Common.Types
{
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

    public interface IResult<TError, T>
    {
        /// <summary>
        ///Returns true if the result is Ok.
        /// </summary>
        bool IsOk { get; }

        /// <summary>
        ///Returns true if the result is Err.
        /// </summary>
        bool IsErr { get; }

        /// <summary>
        ///Converts from Result{TError, T} to Option{T}.
        /// Converts this into an Option{T}, discarding the error, if any.
        /// </summary>
        Option<T> Ok();

        /// <summary>
        ///Converts from Result{TError, T} to Option{T}.
        /// Converts this into an Option{T}, discarding the success value, if any.
        /// </summary>
        Option<TError> Err();

        TU Match<TU>(Func<TError, TU> onErr, Func<T, TU> onOk);

        /// <summary>
        ///  Control flow based on pattern matching.
        /// </summary>
        /// <typeparam name="TU">The output type of the match.</typeparam>
        /// <param name="onErr">Calls this when the type is Err.</param>
        /// <param name="onOk">Calls this when the type is right.</param>
        /// <returns></returns>
        TU Match<TU>(OnErr<TU, TError> onErr, OnOk<TU, T> onOk);

        /// <summary>
        ///Maps a Result{TErr,T} to Result{TErr,TU} by applying a function to a contained Ok value, leaving an Err value untouched.
        ///  This function can be used to compose the results of two functions.
        /// </summary>
        Result<TError, TU> MapOk<TU>(Func<T, TU> mapResultTo);

        /// <summary>
        ///Maps a Result{TErr,T} to Result{TF,T} by applying a function to a contained Err value, leaving an Ok value untouched.
        ///    This function can be used to pass through a successful result while handling an error.
        /// </summary>
        Result<TF, T> MapErr<TF>(Func<TError, TF> handleError);

        /// <summary>
        ///Returns res if the result is Ok, otherwise returns the Err value of self.
        /// </summary>
        Result<TError, TU> And<TU>(Result<TError, TU> result);

        /// <summary>
        ///Calls op if the result is Ok, otherwise returns the Err value of self.
        ///This function can be used for control flow based on Result values.
        /// </summary>
        Result<TError, TU> AndThen<TU>(Func<T, Result<TError, TU>> op);

        /// <summary>
        ///Returns res if the result is Err, otherwise returns the Ok value of self.
        ///    Arguments passed to or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use or_else, which is lazily evaluated.
        /// </summary>
        Result<TF, T> Or<TF>(Result<TF, T> result);

        /// <summary>
        ///Calls op if the result is Err, otherwise returns the Ok value of self.
        ///    This function can be used for control flow based on result values.
        /// </summary>
        Result<TF, T> OrElse<TF>(Func<TError, Result<TF, T>> op);

        /// <summary>
        ///Unwraps a result, yielding the content of an Ok. Else, it returns optb.
        ///    Arguments passed to unwrap_or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use unwrap_or_else, which is lazily evaluated.
        /// </summary>
        T UnwrapOr(T optb);

        /// <summary>
        ///Unwraps a result, yielding the content of an Ok. If the value is an Err then it calls op with its value.
        /// </summary>
        T UnwrapOrElse(Func<TError, T> op);

        /// <summary>
        ///Joins two results to a tuple of Ok values when both are Ok value, returns the first error otherwise.
        ///  This function can be used to compose results of 2 functions.
        /// </summary>
        Result<TError, (T, TU)> JoinOk<TU>(Result<TError, TU> secondResult);

        /// When 2 results are Ok the map function will be called. The first error will be called otherwise.
        /// this can be used to compose results.
        Result<TError, TNewOk> MapOk2<TNewOk, TU>(Result<TError, TU> secondResult, Func<T, TU, TNewOk> map);
    }

    /// <summary>
    /// Result is a type that represents either success (Ok) or failure (Err).
    /// </summary>
    /// <typeparam name="TError">The type of the Err value.</typeparam>
    /// <typeparam name="T">The type of the Ok value.</typeparam>
    /// <remarks>
    /// The default is Err. When <see cref="Result{TError,T}" /> is initialized as default, it will throw a <exception cref="PanicException{TError}"></exception> when trying to obtain the err value.
    /// This such that chaining and composing results that are ok should work with the default.
    /// </remarks>
    [Serializable]
    public readonly struct Result<TError, T> : IEitherValueType<TError, T>, IEquatable<Result<TError, T>>
                                             , IResult<TError, T>, ISerializable
    {
        private readonly bool          _initialized;
        private readonly ResultVariant _variant;
        private readonly T             _value;
        private readonly TError        _err;

        /// <inheritdoc />
        public bool IsOk => _variant == ResultVariant.Ok;

        /// <inheritdoc />
        public bool IsErr => _variant == ResultVariant.Err;

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

        private Result(in TError err, in T ok, ResultVariant variant)
        {
            _variant     = variant;
            _err         = err;
            _value       = ok;
            _initialized = true;
        }

        public Result(TError err) : this(err, default!, ResultVariant.Err)
        {
        }

        public Result(T ok) : this(default!, ok, ResultVariant.Ok)
        {
        }

        public static implicit operator (Option<TError> err, Option<T> ok)(in Result<TError, T> res)
        {
            return (err: res.Err(), ok: res.Ok());
        }

        public static implicit operator Option<T>(in Result<TError, T> res)
        {
            return res.Ok();
        }

        public static implicit operator Option<TError>(in Result<TError, T> res)
        {
            return res.Err();
        }

        public static implicit operator Result<TError, T>(in T      ok)  => new Result<TError, T>(ok);
        public static implicit operator Result<TError, T>(in TError err) => new Result<TError, T>(err);

        public static Result<TError, T> Ok(in T value)
        {
            return new Result<TError, T>(default!, value, ResultVariant.Ok);
        }

        public static Result<TError, T> Err(in TError value)
        {
            return new Result<TError, T>(value, default!, ResultVariant.Err);
        }

        /// <inheritdoc />
        public Option<T> Ok()
        {
            if (IsOk)
            {
                return Option<T>.Some(_value);
            }

            return Option<T>.None;
        }

        /// <inheritdoc />
        public Option<TError> Err()
        {
            if (IsErr)
            {
                return Option<TError>.Some(ErrValue);
            }

            return Option<TError>.None;
        }

        /// <inheritdoc />
        public bool Equals(Result<TError, T> other)
        {
            if (!_initialized && !other._initialized)
            {
                return true;
            }

            return _variant switch
            {
                ResultVariant.Ok  => EqualityComparer<T>.Default.Equals(_value, other._value)
              , ResultVariant.Err => EqualityComparer<TError>.Default.Equals(_err, other._err)
              , _ => (_initialized == other._initialized                       && _variant == other._variant &&
                      EqualityComparer<T>.Default.Equals(_value, other._value) &&
                      EqualityComparer<TError>.Default.Equals(_err, other._err))
            };
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Result<TError, T> other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance. When the result is <see cref="ResultVariant.Ok"/> this returns the hashcode of the {T} type value. When the result is <see cref="ResultVariant.Err"/> this returns the hashcode of the {TError} type value.
        /// When not initialized this returns 0.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            if (!_initialized)
            {
                return 0;
            }

            return IsOk ? _value?.GetHashCode() ?? 0 : _err?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Result<TError, T> left, Result<TError, T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Result<TError, T> left, Result<TError, T> right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc cref="IEither{TLeft,TRight}" />
        public TU Match<TU>(Func<TError, TU> onErr, Func<T, TU> onOk)
        {
            return IsOk ? onOk(_value) : onErr(ErrValue);
        }

        /// <inheritdoc/>
        public TU Match<TU>(OnErr<TU, TError> onErr, OnOk<TU, T> onOk)
        {
            return IsOk ? onOk(_value) : onErr(ErrValue);
        }

        public unsafe TU Match<TU>(delegate*<in TError, TU> onErr, delegate*<in T, TU> onOk)
        {
            return IsOk ? onOk(_value) : onErr(ErrValue);
        }

        public unsafe TU Match<TU>(delegate*<TError, TU> onErr, delegate*<T, TU> onOk)
        {
            return IsOk ? onOk(_value) : onErr(ErrValue);
        }

        /// <inheritdoc />
        public void For(Action<TError> onErr, Action<T> onOk)
        {
            if (IsOk)
            {
                onOk(_value);
            }
            else
            {
                onErr(ErrValue);
            }
        }

        /// <inheritdoc cref="IEither{TLeft,TRight}" />
        public Result<TF, TU> Map<TF, TU>(Func<TError, TF> selectErr, Func<T, TU> selectOk)
        {
            return IsOk
                       ? Result<TF, TU>.Ok(selectOk(_value))
                       : Result<TF, TU>.Err(selectErr(ErrValue));
        }

        /// <summary>
        ///Maps a Result{TErr,T} to Result{TErr,TU} by applying a function to a contained Ok value, leaving an Err value untouched.
        ///  This function can be used to compose the results of two functions.
        /// </summary>
        public Result<TError, TU> MapOk<TU>(Func<T, TU> mapResultTo)
        {
            return IsOk ? Result<TError, TU>.Ok(mapResultTo(_value)) : Result<TError, TU>.Err(ErrValue);
        }

        /// <summary>
        ///Maps a Result{TErr,T} to Result{TF,T} by applying a function to a contained Err value, leaving an Ok value untouched.
        ///    This function can be used to pass through a successful result while handling an error.
        /// </summary>
        public Result<TF, T> MapErr<TF>(Func<TError, TF> handleError)
        {
            return IsOk ? Result<TF, T>.Ok(_value) : Result<TF, T>.Err(handleError(ErrValue));
        }

        /// <summary>
        ///Returns res if the result is Ok, otherwise returns the Err value of self.
        /// </summary>
        public Result<TError, TU> And<TU>(Result<TError, TU> result)
        {
            return IsOk ? result : Result<TError, TU>.Err(ErrValue);
        }

        /// <summary>
        ///Calls op if the result is Ok, otherwise returns the Err value of self.
        ///This function can be used for control flow based on Result values.
        /// </summary>
        public Result<TError, TU> AndThen<TU>(Func<T, Result<TError, TU>> op)
        {
            return IsOk ? op(_value) : Result<TError, TU>.Err(ErrValue);
        }

        /// <summary>
        ///Calls op if the result is Ok, otherwise returns the Err value of self.
        ///This function can be used for control flow based on Result values.
        /// </summary>
        public Result<TError, TU> AndThen<TU>(OnOk<Result<TError, TU>, T> op)
        {
            return IsOk ? op(_value) : Result<TError, TU>.Err(ErrValue);
        }

        public unsafe Result<TError, TU> AndThen<TU>(delegate*<in T, Result<TError, TU>> onOk)
        {
            return IsOk ? onOk(_value) : Result<TError, TU>.Err(ErrValue);
        }

        public unsafe Result<TError, TU> AndThen<TU>(delegate*<T, Result<TError, TU>> onOk)
        {
            return IsOk ? onOk(_value) : Result<TError, TU>.Err(ErrValue);
        }

        /// <summary>
        ///Returns res if the result is Err, otherwise returns the Ok value of self.
        ///    Arguments passed to or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use or_else, which is lazily evaluated.
        /// </summary>
        public Result<TF, T> Or<TF>(Result<TF, T> result)
        {
            return IsOk ? Result<TF, T>.Ok(_value) : result;
        }

        /// <summary>
        ///Returns res if the result is Err, otherwise returns the Ok value of self.
        ///    Arguments passed to or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use or_else, which is lazily evaluated.
        /// </summary>
        public Result<TF, T> Or<TF>(in Result<TF, T> result)
        {
            return IsOk ? Result<TF, T>.Ok(_value) : result;
        }

        /// <summary>
        ///Calls op if the result is Err, otherwise returns the Ok value of self.
        ///    This function can be used for control flow based on result values.
        /// </summary>
        public Result<TF, T> OrElse<TF>(Func<TError, Result<TF, T>> op)
        {
            return IsOk ? Result<TF, T>.Ok(_value) : op(ErrValue);
        }

        /// <summary>
        ///Calls op if the result is Err, otherwise returns the Ok value of self.
        ///    This function can be used for control flow based on result values.
        /// </summary>
        public Result<TF, T> OrElse<TF>(OnErr<Result<TF, T>, TError> op)
        {
            return IsOk ? Result<TF, T>.Ok(_value) : op(ErrValue);
        }

        /// <summary>
        ///Unwraps a result, yielding the content of an Ok. Else, it returns optb.
        ///    Arguments passed to unwrap_or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use unwrap_or_else, which is lazily evaluated.
        /// </summary>
        public T UnwrapOr(T optb)
        {
            //Could use match function, but I assume this is faster.
            return IsOk ? _value : optb;
        }

        /// <summary>
        ///Unwraps a result, yielding the content of an Ok. If the value is an Err then it calls op with its value.
        /// </summary>
        public T UnwrapOrElse(Func<TError, T> op)
        {
            //Could use match function, but I assume this is faster.
            return IsOk ? _value : op(ErrValue);
        }

        /// <inheritdoc/>
        public Result<TError, (T, TU)> JoinOk<TU>(Result<TError, TU> secondResult)
        {
            if (!IsOk)
            {
                return _err;
            }

            if (secondResult.IsOk)
            {
                return (_value, secondResult._value);
            }

            return secondResult._err;
        }

        /// <summary>
        ///Joins two results to a tuple of Ok values when both are Ok value, returns the first error otherwise.
        ///  This function can be used to compose results.
        /// </summary>
        public unsafe Result<TError, (T, TU)> AndJoinOk<TU>(delegate*<Result<TError, TU>> secondAction)
        {
            if (!IsOk)
            {
                return _err;
            }

            var secondResult = secondAction();

            if (secondResult.IsOk)
            {
                return (_value, secondResult._value);
            }

            return secondResult._err;
        }

        /// <summary>
        ///Joins two results to a tuple of Ok values when both are Ok value, returns the first error otherwise.
        ///  This function can be used to compose results.
        /// </summary>
        public Result<TError, (T, TU)> AndJoinOk<TU>(Func<Result<TError, TU>> secondAction)
        {
            if (!IsOk)
            {
                return _err;
            }

            return MapOk2(secondAction(), (f, s) => (f, s));
        }

        /// <inheritdoc/>
        public Result<TError, TNewOk> MapOk2<TNewOk, TU>(Result<TError, TU> secondResult, Func<T, TU, TNewOk> map)
        {
            if (!IsOk)
            {
                return _err;
            }

            if (secondResult.IsOk)
            {
                return map(_value, secondResult._value);
            }

            return secondResult._err;
        }

        /// <summary>
        ///Unwraps a result, yielding the content of an Ok. If the value is an Err then it calls op with its value.
        /// </summary>
        public T UnwrapOrElse(OnErr<T, TError> op)
        {
            //Could use match function, but I assume this is faster.
            return IsOk ? _value : op(ErrValue);
        }

        /// experimental
        [Obsolete("will be removed. you can use Ok() which returns an option, and the IsOk property returns a bool")]
        public bool TryIsOk(out Option<T> value)
        {
            if (IsOk)
            {
                value = _value;
                return true;
            }

            value = Option<T>.None;
            return false;
        }

        /// experimental
        public bool TryIsOk(out T? value)
        {
            if (IsOk)
            {
                value = _value;
                return true;
            }

            value = default;
            return false;
        }

        /// experimental
        public bool TryIsOkOr(out T value, T orValue)
        {
            if (IsOk)
            {
                value = _value;
                return true;
            }

            value = orValue;
            return false;
        }

        /// experimental
        public (bool isError, bool isOk) GetValue(out Option<T> okValue, out Option<TError> errValue)
        {
            if (IsOk)
            {
                okValue  = _value;
                errValue = Option<TError>.None;
                return (false, true);
            }

            okValue  = Option<T>.None;
            errValue = ErrValue;
            return (true, false);
        }
        
        /// experimental
        public (bool isError, bool isOk) GetValue(out T okValue,  T okDefault, out TError errValue, TError errDefault)
        {
            if (IsOk)
            {
                okValue  = _value;
                errValue = errDefault;
                return (false, true);
            }

            okValue  = okDefault;
            errValue = ErrValue;
            return (true, false);
        }

        public (Option<TError> error, Option<T> ok) GetValue()
        {
            if (IsOk)
            {
                return (error: Option<TError>.None, ok: _value);
            }

            return (error: ErrValue, ok: Option<T>.None);
        }

        /// experimental
        public (bool isError, bool isOk) GetValue(out T? okValue, out TError? errValue)
        {
            if (IsOk)
            {
                okValue  = _value;
                errValue = default;
                return (false, true);
            }

            okValue  = default;
            errValue = ErrValue;
            return (true, false);
        }

        private enum ResultVariant : byte
        {
            Ok  = 1
          , Err = 0
           ,
        }

        private const string VariantFieldName = "variant";
        private const string ValueFieldName   = "ok";
        private const string ErrFieldName     = "err";

        //Serialize
        /// <inheritdoc />
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(VariantFieldName, (byte)_variant);
            if (IsOk)
            {
                info.AddValue(ValueFieldName, _value);
            }
            else
            {
                info.AddValue(ErrFieldName, ErrValue);
            }
        }

        //Deserialize
        private Result(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            _variant = (ResultVariant)serializationInfo.GetByte(VariantFieldName);
            if (_variant == ResultVariant.Ok)
            {
                //note: decide on whether to panic or leave this as is, since it is possible that the caller intended to serialize a null value
                _value       = (T?)serializationInfo.GetValue(ValueFieldName, typeof(T))!;
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
    }
}