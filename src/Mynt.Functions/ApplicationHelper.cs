using System;
using Mynt.Core.Interfaces;

namespace Mynt.Functions
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

        public static ITradingStrategy TryCreateTradingStrategy(string name)
        {
            try
            {
            var type = Type.GetType($"Mynt.Core.Strategies.{name}, Mynt.Core", true, true);
            return Activator.CreateInstance(type) as ITradingStrategy;

            }
            catch
            {
                return null;
            }
        }
    }
}
