namespace Microsoft.FSharp.Compiler

#if EXTENSIONTYPING


open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.AbstractIL.IL

/// Stores and transports aggregated list of errors reported by the type provider
type internal TypeProviderError =
    inherit System.Exception
    
    /// creates new instance of TypeProviderError that represents one error
    new : (int * string) * string * range -> TypeProviderError
    /// creates new instance of TypeProviderError that represents collection of errors
    new : int * string * range * seq<string> -> TypeProviderError
    member Number : int
    member Range : range
    member ContextualErrorMessage : string
    /// creates new instance of TypeProviderError with specified type\method names
    member WithContext : string * string -> TypeProviderError
    /// creates new instance of TypeProviderError based on current instance information(message)
    member MapText : (string -> int * string) * string * range -> TypeProviderError
    /// provides uniform way to process aggregated errors
    member Iter : (TypeProviderError -> unit) -> unit


/// This struct wraps a value produced by a type provider to properly attribute any failures.
[<NoEquality; NoComparison; Class>]
type internal Tainted<'T, 'TOwner> =

    /// Create an initial tainted value
    static member CreateAll : ('TOwner * ILScopeRef) list -> Tainted<'TOwner, 'TOwner> list

    /// A type provider that produced the value
    member TypeProvider : Tainted<'TOwner, 'TOwner>

    /// Test to report for the name of the type provider that produced the value
    member TypeProviderDesignation : string

    /// The ILScopeRef of the runtime assembly reference for type provider that produced the value
    member TypeProviderAssemblyRef : ILScopeRef

    /// Apply an operation. Any exception will be attributed to the type provider with an error located at the given range
    member PApply : ('T -> 'U) * range:range -> Tainted<'U, 'TOwner>

    /// Apply an operation. Any exception will be attributed to the type provider with an error located at the given range
    member PApply2 : ('T -> 'U1 * 'U2) * range:range -> Tainted<'U1, 'TOwner> * Tainted<'U2, 'TOwner> 

    /// Apply an operation. Any exception will be attributed to the type provider with an error located at the given range
    member PApply3 : ('T -> 'U1 * 'U2 * 'U3) * range:range -> Tainted<'U1, 'TOwner> * Tainted<'U2, 'TOwner>  * Tainted<'U3, 'TOwner>

    /// Apply an operation. Any exception will be attributed to the type provider with an error located at the given range
    member PApply4 : ('T -> 'U1 * 'U2 * 'U3 * 'U4) * range:range -> Tainted<'U1, 'TOwner> * Tainted<'U2, 'TOwner>  * Tainted<'U3, 'TOwner> * Tainted<'U4, 'TOwner>

    /// Apply an operation. No exception may be raised by 'f'
    member PApplyNoFailure : f: ('T -> 'U) -> Tainted<'U, 'TOwner>

    /// Apply an operation. Any exception will be attributed to the type provider with an error located at the given range
    member PApplyWithProvider : ('T * 'TOwner -> 'U) * range:range -> Tainted<'U, 'TOwner>

    /// Apply an operation that returns an array. Unwrap array. Any exception will be attributed to the type provider with an error located at the given range.  String is method name of thing-returning-array, to diagnostically attribute if it is null
    member PApplyArray : ('T -> 'U[]) * string * range:range -> Tainted<'U, 'TOwner>[]

    /// Apply an operation that returns an option. Unwrap option. Any exception will be attributed to the type provider with an error located at the given range
    member PApplyOption : ('T -> 'U option) * range:range -> Tainted<'U, 'TOwner> option

    /// Apply an operation and 'untaint' the result. The result must be marshalable. Any exception will be attributed to the type provider with an error located at the given range
    member PUntaint : ('T -> 'U) * range:range -> 'U

    /// Apply an operation and 'untaint' the result. This can be used if the return type 
    /// is guaranteed not to be implemented by a type provider
    member PUntaintNoFailure : ('T -> 'U) -> 'U

    /// Conditionally coerce the value
    member OfType<'U> : unit -> Tainted<'U, 'TOwner> option

    /// Assert that the value is of 'U and coerce the value.
    /// If corecion fails, the failuer will be blamed on a type provider
    member Coerce<'U> : range:range -> Tainted<'U, 'TOwner>

/// This struct wraps a value produced by a type provider to properly attribute any failures.
type internal TaintedProvider<'T> = Tainted<'T, ITypeProvider>

/// ?
type internal TaintedCheckingProvider<'T> = Tainted<'T, ITypeCheckingProvider>


[<RequireQualifiedAccess>]
module internal Tainted =
    /// Test whether the tainted value is null
    val (|Null|_|) : TaintedProvider<'T> -> unit option when 'T : null
    /// Test whether the tainted value equals given value. 
    /// Failure in call to equality operation will be blamed on type provider of first operand
    val Eq : TaintedProvider<'T> -> 'T -> bool when 'T : equality
    /// Test whether the tainted value equals given value. Type providers are ignored (equal tainted values produced by different type providers are equal)
    /// Failure in call to equality operation will be blamed on type provider of first operand
    val EqTainted : TaintedProvider<'T> -> TaintedProvider<'T> -> bool when 'T : equality and 'T : not struct
    /// Compute the hash value for the tainted value
    val GetHashCodeTainted : TaintedProvider<'T> -> int when 'T : equality

#endif
