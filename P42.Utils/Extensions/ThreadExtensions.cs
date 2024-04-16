
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P42.Utils
{

    public static class ThreadExtensions
    {
        public static Action<Thread, Exception> DefaultExceptionHandler = P42.Serilog.QuickLog.QLogExtensions.LogException;


        [SuppressMessage("ReSharper", "VariableHidesOuterVariable", Justification = "Pass params explicitly to async local function or it will allocate to pass them")]
        public static void Forget(this Task task, Action<Thread, Exception> onException = null, [CallerMemberName] string callingMethodName = "")
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            // note: this code is inspired by a tweet from Ben Adams: https://twitter.com/ben_a_adams/status/1045060828700037125
            // Only care about tasks that may fault (not completed) or are faulted,
            // so fast-path for SuccessfullyCompleted and Canceled tasks.
            if (!task.IsCanceled && (!task.IsCompleted || task.IsFaulted))
            {
                // use "_" (Discard operation) to remove the warning IDE0058: Because this call is not awaited, execution of the
                // current method continues before the call is completed - https://docs.microsoft.com/en-us/dotnet/csharp/discards#a-standalone-discard
                _ = ForgetAwaited(task, onException, callingMethodName);
            }
        }

        // Allocate the async/await state machine only when needed for performance reasons.
        // More info about the state machine: https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/?WT.mc_id=DT-MVP-5003978
        // Pass params explicitly to async local function or it will allocate to pass them
        static async Task ForgetAwaited(Task task, Action<Thread, Exception> onException, string callingMethodName = "")
        {
            try
            {
                await task;
            }
            catch (TaskCanceledException)
            {
                // log a message if we were given a logger to use
                //Serilog.Log.Error(tce, $"Fire and forget task was canceled for calling method: {callingMethodName}");
                System.Diagnostics.Debug.WriteLine($"Fire and forget task was canceled for calling method: {callingMethodName}");
            }
            catch (Exception e)
            {
                // log a message if we were given a logger to use
                //Serilog.Log.Error(e, $"Fire and forget task failed for calling method: {callingMethodName}");
                //using (var p = Forms9Patch.Toast.Create("EXCEPTION : " + callingMethodName, e.Message)) { p.CancelOnPageOverlayTouch = p.CancelOnBackButtonClick = false; }

                System.Diagnostics.Debug.WriteLine($"Fire and forget task failed for calling method: {callingMethodName} [{e.Message}][{e.StackTrace}]");
                System.Console.WriteLine($"Fire and forget task failed for calling method: {callingMethodName} [{e.Message}][{e.StackTrace}]");

                onException = onException ?? DefaultExceptionHandler;
                onException.Invoke(Thread.CurrentThread, new FireAndForgetException(callingMethodName, e));
            }
        }

    }

    public class FireAndForgetException : Exception
    {
        /// <summary>
        /// Method that called the FireAndForget task
        /// </summary>
        public string CallingMethodName { get; private set; }

        public FireAndForgetException(string callingMethodName, Exception innerException) : base($"Fire and forget task, from calling method [{callingMethodName}], failed.", innerException)
        {
            CallingMethodName = callingMethodName;
        }
    }
}
