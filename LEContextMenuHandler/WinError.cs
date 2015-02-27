namespace LEContextMenuHandler
{
    internal static class WinError
    {
        public const int S_OK = 0x0000;
        public const int S_FALSE = 0x0001;
        public const int E_FAIL = -2147467259;
        public const int E_INVALIDARG = -2147024809;
        public const int E_OUTOFMEMORY = -2147024882;
        public const int STRSAFE_E_INSUFFICIENT_BUFFER = -2147024774;
        public const uint SEVERITY_SUCCESS = 0;
        public const uint SEVERITY_ERROR = 1;

        /// <summary>
        ///     Create an HRESULT value from component pieces.
        /// </summary>
        /// <param name="sev">The severity to be used</param>
        /// <param name="fac">The facility to be used</param>
        /// <param name="code">The error number</param>
        /// <returns>A HRESULT constructed from the above 3 values</returns>
        public static int MAKE_HRESULT(uint sev, uint fac, uint code)
        {
            return (int)((sev << 31) | (fac << 16) | code);
        }
    }
}