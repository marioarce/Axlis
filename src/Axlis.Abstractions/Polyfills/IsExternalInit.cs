// Polyfill required for C# 9 'init' accessor on netstandard2.0 targets.
// The type is part of the BCL in .NET 5+; define it here for older TFMs.
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
