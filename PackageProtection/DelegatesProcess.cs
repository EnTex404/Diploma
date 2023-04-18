using static PackageProtection.Delegates;

namespace PackageProtection
{
    public static class DelegatesProcess
    {
        public static ErrorDel? errorMessage;

        public static void RegisterError(ErrorDel del)
        {
            errorMessage += del;
        }

        public static void UnregisterError(ErrorDel del)
        {
            try
            {
                errorMessage -= del;
            }
            catch
            {
                
            }
        }
    }
}
