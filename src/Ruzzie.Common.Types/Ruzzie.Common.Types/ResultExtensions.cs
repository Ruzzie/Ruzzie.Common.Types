namespace Ruzzie.Common.Types;

public static class ResultExtensions
{
    ///<summary>
    ///Maps a Result{TError,T} to {TU} by applying a function to a contained Ok value, or a fallback function to a contained Err value.
    ///This function can be used to unpack a successful result while handling an error.
    ///</summary>
    public static TU MapOrElse<TU, TError, T>(this Result<TError, T> self
                                            , Func<TError, TU>       fallback
                                            , Func<T, TU>            mapResultTo)
    {
        return self.Match(fallback, mapResultTo);
    }

    /// <summary>
    ///Returns the contained value or a default
    ///Consumes the self argument then, if Ok, returns the contained value, otherwise if Err, returns the default value for that type.
    /// </summary>
    public static T UnwrapOrDefault<TError, T>(this Result<TError, T> self) where T : struct
    {
        return self.UnwrapOr(default);
    }

    /// <summary>
    ///Returns the contained value or a default
    ///Consumes the self argument then, if Ok, returns the contained value, otherwise if Err, returns the default value for that type.
    /// </summary>
    public static T UnwrapOrDefault<TError, T>(this Result<TError, T> self, T @default)
    {
        return self.UnwrapOr(@default);
    }
    

    public static Result<TErr, TOk> ApplyCompose<TA, TB, TOk, TErr>(this Func<TA, Result<TErr, TB>> f1
                                                                  , Func<TB, Result<TErr, TOk>>     f2
                                                                  , TA                              value)

    {
        return f1(value).AndThen(f2);
    }
    

    public static unsafe Result<TErr, TOk> ApplyCompose<TA, TB, TOk, TErr>(delegate*<TA, Result<TErr, TB>>  f1
                                                                         , delegate*<TB, Result<TErr, TOk>> f2
                                                                         , TA                               value)

    {
        return f1(value).AndThen(f2);
    }
}