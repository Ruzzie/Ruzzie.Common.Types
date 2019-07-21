# Ruzzie.Common.Types
Some functional types:
- readonly struct Either<TLeft, TRight>
- class Left<TLeft, TRight>
- class Right<TLeft, TRight>
- readonly struct Option<TValue>, for optional values Some or None
- readonly struct Result<TError, T>, for (rust) result style values

Best used with non nullable reference types.
