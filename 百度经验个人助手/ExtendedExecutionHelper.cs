using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;


namespace 百度经验个人助手
{
    // NOT USED YET
    /// <summary>
    /// Helper class for using extended execution
    /// </summary>
    public sealed class ExtendedExecutionHelper
    {
        static ExtendedExecutionSession extendedExeSession = null;
        static int count;
        static readonly object countSyncRoot = new object();
        static readonly object sessionSyncRoot = new object();

        /// <summary>
        /// Get a deferral for an extended execution task.
        /// The extended execution session is disposed when all deferrals are completed.
        /// </summary>
        /// <returns>The deferral instance</returns>
        public static Deferral GetDeferral()
        {
            bool requestSession = false;
            lock (countSyncRoot)
            {
                ++count;
                requestSession = count == 1;
            }
            if (requestSession)
                RequestExtendedExecutionAsync();
            return new Deferral(TaskFinished);
        }

        private static void TaskFinished()
        {
            bool clearSession = false;
            lock (countSyncRoot)
            {
                --count;
                clearSession = count == 0;
            }
            if (clearSession)
                ClearExtendedExeSession();
        }

        private static Task RequestExtendedExecutionAsync()
        {
            return Task.Run(() =>
            {
                lock (sessionSyncRoot)
                {
                    if (extendedExeSession != null)
                    {
                        extendedExeSession.Dispose();
                        extendedExeSession = null;
                    }

                    var newSession = new ExtendedExecutionSession();
                    newSession.Reason = ExtendedExecutionReason.Unspecified;
                    newSession.Revoked += ExtendedExecutionRevoked;

                    var asyncTask = newSession.RequestExtensionAsync().AsTask();
                    asyncTask.Wait();
                    ExtendedExecutionResult result = asyncTask.Result;

                    switch (result)
                    {
                        case ExtendedExecutionResult.Allowed:
                            extendedExeSession = newSession;
                            break;
                        default:
                        case ExtendedExecutionResult.Denied:
                            newSession.Dispose();
                            break;
                    }
                }
            });
        }

        private static void ClearExtendedExeSession()
        {
            lock (sessionSyncRoot)
            {
                if (extendedExeSession != null)
                {
                    extendedExeSession.Dispose();
                    extendedExeSession = null;
                }
            }
        }

        private static void ExtendedExecutionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            lock (sessionSyncRoot)
            {
                if (extendedExeSession != null)
                {
                    extendedExeSession.Dispose();
                    extendedExeSession = null;
                }
            }
        }
    }
}
