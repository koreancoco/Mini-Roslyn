namespace CodeAnalysis.Syntax.InternalSyntax
{
    public enum SpecialType : sbyte
    {
        /// <summary>
        /// Indicates a non-special type (default value).
        /// </summary>
        None = 0,
        /// <summary>
        /// Number 无符号整型字面量，支持十六进制格式
        /// </summary>
        System_UInt64 = 16,
        /// <summary>
        /// Number 双精度浮点数字面量
        /// </summary>
        System_Double = 19,
    }
}