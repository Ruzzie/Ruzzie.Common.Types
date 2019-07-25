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


    public interface IResult<TError, T>
    {
        ///Returns true if the result is Ok.
        bool IsOk { get; }
        
        ///Returns true if the result is Err.
        bool IsErr { get; }

        ///Converts from Result{TError, T} to Option{T}.
        /// Converts this into an Option{T}, discarding the error, if any.
        Option<T> Ok();

        ///Converts from Result{TError, T} to Option{T}.
        /// Converts this into an Option{T}, discarding the success value, if any.
        Option<TError> Err();
        TU Match<TU>(Func<TError, TU> onErr, Func<T, TU> onOk);
        Result<TF, TU> Select<TF, TU>(Func<TError, TF> selectErr, Func<T, TU> selectOk);

        ///Maps a Result{TErr,T} to Result{TErr,TU} by applying a function to a contained Ok value, leaving an Err value untouched.
        ///  This function can be used to compose the results of two functions.
        Result<TError, TU> Map<TU>(Func<T, TU> mapResultTo);

        ///Maps a Result{TErr,T} to Result{TF,T} by applying a function to a contained Err value, leaving an Ok value untouched.
        ///    This function can be used to pass through a successful result while handling an error.
        Result<TF, T> MapErr<TF>(Func<TError, TF> handleError);

        ///Returns res if the result is Ok, otherwise returns the Err value of self.
        Result<TError, TU> And<TU>(Result<TError, TU> result);

        ///Calls op if the result is Ok, otherwise returns the Err value of self.
        ///This function can be used for control flow based on Result values.
        Result<TError, TU> AndThen<TU>(Func<T, Result<TError, TU>> op);

        ///Returns res if the result is Err, otherwise returns the Ok value of self.
        ///    Arguments passed to or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use or_else, which is lazily evaluated.
        Result<TF, T> Or<TF>(Result<TF, T> result);

        ///Calls op if the result is Err, otherwise returns the Ok value of self.
        ///    This function can be used for control flow based on result values.
        Result<TF, T> OrElse<TF>(Func<TError, Result<TF, T>> op);

        ///Unwraps a result, yielding the content of an Ok. Else, it returns optb.
        ///    Arguments passed to unwrap_or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use unwrap_or_else, which is lazily evaluated.
        T UnwrapOr(T optb);

        ///Unwraps a result, yielding the content of an Ok. If the value is an Err then it calls op with its value.
        T UnwrapOrElse(Func<TError, T> op);
    }

    /// <summary>
    /// Result is a type that represents either success (Ok) or failure (Err).
    /// </summary>
    /// <typeparam name="TError">The type of the Err value.</typeparam>
    /// <typeparam name="T">The type of the Ok value.</typeparam>
    /// <remarks>The default is Err. When <see cref="Result{TError,T}"/> is initialized as default, it will throw a <exception cref="PanicException{TError}"></exception> when trying to obtain the err value.
    /// This such that chaining and composing results that are ok should work with the default.
    /// </remarks>
    [Serializable]
    public readonly struct Result<TError, T>  : IEitherValueType<TError, T>, IEquatable<Result<TError, T>>, IResult<TError, T>, ISerializable
    {
        private readonly bool _initialized;
        private readonly ResultVariant _variant;
        private readonly T _value;
        private readonly TError _err;
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
                    throw ResultPanicExtensions.CreatePanicExceptionForErr("Result is uninitialized. You cannot obtain the Err value.", _err);
                }
                return _err;
            }
        }

        private Result(in TError err, in T ok, in ResultVariant variant)
        {
            _variant = variant;
            _err = err;
            _value = ok;
            _initialized = true;
        }

        public Result(TError err) : this(err, default!, ResultVariant.Err)
        {
        }

        public Result(T ok) : this(default!, ok, ResultVariant.Ok)
        {
        }

        public static implicit operator (Option<TError> err, Option<T> ok)(in Result<TError,T> res)
        {
            return (err: res.Err(), ok: res.Ok());
        }

        public static implicit operator Option<T>(in Result<TError,T> res)
        {
            return res.Ok();
        }

        public static implicit operator Option<TError>(in Result<TError,T> res)
        {
            return res.Err();
        }

        //public static implicit operator Result<TError, T>(in T ok) => new Result<TError, T>(ok);
        //public static implicit operator Result<TError, T>(in TError err) => new Result<TError, T>(err);

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

        public bool Equals(Result<TError, T> other)
        {
            if (!_initialized && !other._initialized)
            {
                return true;
            }

            switch (_variant)
            {
                case ResultVariant.Ok:
                    return EqualityComparer<T>.Default.Equals(_value, other._value);
                case ResultVariant.Err:
                    return EqualityComparer<TError>.Default.Equals(_err, other._err);
                default:
                    return _initialized == other._initialized && _variant == other._variant &&
                           EqualityComparer<T>.Default.Equals(_value, other._value) &&
                           EqualityComparer<TError>.Default.Equals(_err, other._err);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Result<TError, T> other && Equals(other);
        }

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

        public TU Match<TU>(Func<TError, TU> onErr, Func<T, TU> onOk)
        {
            return IsOk ? onOk(_value) : onErr(ErrValue);
        }

        public TU Match<TU>(Func<T, TU> onOk, Func<TError, TU> onErr)
        {
            return IsOk ? onOk(_value) : onErr(ErrValue);
        }

        public Result<TF, TU> Select<TF, TU>(Func<TError, TF> selectErr, Func<T, TU> selectOk)
        {
            return IsOk
                ? Result<TF, TU>.Ok(selectOk(_value))
                : Result<TF, TU>.Err(selectErr(ErrValue));
        }

        ///Maps a Result{TErr,T} to Result{TErr,TU} by applying a function to a contained Ok value, leaving an Err value untouched.
        ///  This function can be used to compose the results of two functions.
        public Result<TError, TU> Map<TU>(Func<T, TU> mapResultTo)
        {
            return IsOk ? Result<TError, TU>.Ok(mapResultTo(_value)) : Result<TError, TU>.Err(ErrValue);
        }

        ///Maps a Result{TErr,T} to Result{TF,T} by applying a function to a contained Err value, leaving an Ok value untouched.
        ///    This function can be used to pass through a successful result while handling an error.
        public Result<TF, T> MapErr<TF>(Func<TError, TF> handleError)
        {
            return IsOk ? Result<TF, T>.Ok(_value) : Result<TF, T>.Err(handleError(ErrValue));
        }

        ///Returns res if the result is Ok, otherwise returns the Err value of self.
        public Result<TError, TU> And<TU>(Result<TError, TU> result)
        {
            return IsOk ? result : Result<TError, TU>.Err(ErrValue);
        }

        ///Calls op if the result is Ok, otherwise returns the Err value of self.
        ///This function can be used for control flow based on Result values.
        public Result<TError, TU> AndThen<TU>(Func<T, Result<TError, TU>> op)
        {
            return IsOk ? op(_value) : Result<TError, TU>.Err(ErrValue);
        }

        ///Returns res if the result is Err, otherwise returns the Ok value of self.
        ///    Arguments passed to or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use or_else, which is lazily evaluated.
        public Result<TF, T> Or<TF>(Result<TF, T> result)
        {
            return IsOk ? Result<TF, T>.Ok(_value) : result;
        }

        ///Calls op if the result is Err, otherwise returns the Ok value of self.
        ///    This function can be used for control flow based on result values.
        public Result<TF, T> OrElse<TF>(Func<TError, Result<TF, T>> op)
        {
            return IsOk ? Result<TF, T>.Ok(_value) : op(ErrValue);
        }

        ///Unwraps a result, yielding the content of an Ok. Else, it returns optb.
        ///    Arguments passed to unwrap_or are eagerly evaluated; if you are passing the result of a function call, it is recommended to use unwrap_or_else, which is lazily evaluated.
        public T UnwrapOr(T optb)
        {
            //Could use match function, but I assume this is faster.
            return IsOk ? _value : optb;
        }

        ///Unwraps a result, yielding the content of an Ok. If the value is an Err then it calls op with its value.
        public T UnwrapOrElse(Func<TError, T> op)
        {
            //Could use match function, but I assume this is faster.
            return IsOk ? _value : op(ErrValue);
        }

        private enum ResultVariant : byte
        {
            Ok = 1,
            Err = 0,
        }

        private const string VariantFieldName = "variant";
        private const string ValueFieldName = "ok";
        private const string ErrFieldName = "err";

        //Serialize
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(VariantFieldName, (byte) _variant);
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
            _variant = (ResultVariant) serializationInfo.GetByte(VariantFieldName);
            if (_variant == ResultVariant.Ok)
            {
                _value = (T) serializationInfo.GetValue(ValueFieldName, typeof(T));
                _err = default!;
                _initialized = true;
            }
            else
            {
                _err = (TError) serializationInfo.GetValue(ErrFieldName, typeof(TError));
                _value = default!;
                _initialized = true;
            }
        }
    }
}