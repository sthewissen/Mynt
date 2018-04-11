using System;
namespace Mynt.AzureFunctions
{
    public static class ApplicationHelper
    {
        private static bool IsStarted = false;
        private static object _syncLock = new object();
        ///<summary>
        /// Sets up the app before running any other code
        /// </summary>

        public static void Startup()
        {
            if (!IsStarted)
            {
                lock (_syncLock)
                {
                    if (!IsStarted)
                    {
                        AssemblyBindingRedirectHelper.ConfigureBindingRedirects();
                        IsStarted = true;
                    }
                }
            }
        }
    }
}
