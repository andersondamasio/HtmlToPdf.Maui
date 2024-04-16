using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace P42.Utils
{
    public static class DebugExtensions
    {
        static DateTime LastMessage = DateTime.MaxValue;

        public static Func<object, bool> ConditionFunc;

        public static bool IsMessagesEnabled = false;

        public static bool IsCensusEnabled = false;

        //static Dictionary<Guid, (string message, DateTime dateTime)> OpenTracks = new Dictionary<Guid, (string, DateTime)>();
        static ConcurrentDictionary<Guid, (string message, DateTime dateTime)> OpenTracks = new ConcurrentDictionary<Guid, (string, DateTime)>();

        static DebugExtensions()
        {
            System.Diagnostics.Debug.IndentSize = 4;
            P42.Utils.Timer.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                if (DateTime.Now - LastMessage > TimeSpan.FromSeconds(5))
                {
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine(" ~~~~~~~~~~~~~ OPEN TRACKS : " + OpenTracks.Count + " ~~~~~~~~~~~~~");
                    var openTracks = OpenTracks.Values.ToList();
                    openTracks = openTracks.OrderBy(pair => pair.dateTime).ToList();
                    foreach (var p in openTracks)
                        System.Diagnostics.Debug.WriteLine("\t [" + p.dateTime.ToString("HH':'mm':'ss'.'fffffffK") + "]" + p.message);
                    System.Diagnostics.Debug.WriteLine(" ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                    System.Diagnostics.Debug.WriteLine("");
                }
                return true;
            });
        }

        public static Guid? Message(bool enabled, string message, Guid? guid = null)
        {
            if (IsMessagesEnabled && enabled)
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                //var className = callingMethod.ReflectedType.Name;
                return Message(message, guid, method.ReflectedType + "." + method.Name);
            }
            return null;
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static Guid? Message(object obj, string message, Guid? guid = null)
        {
            if (IsMessagesEnabled && (ConditionFunc?.Invoke(obj) ?? false))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                //var className = callingMethod.ReflectedType.Name;
                return Message(message, guid, method.ReflectedType + "." + method.Name);
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static Guid? Message(string message, Guid? guid = null, string callingMethod = null)
        {
            if (!IsMessagesEnabled)
                return null;

            //if (message?.Contains("ENTER") ?? false)
            //    guid = Guid.NewGuid();

            //Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            //{
            if (string.IsNullOrWhiteSpace(callingMethod))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                callingMethod = method.ReflectedType + "." + method.Name;
            }
            System.Diagnostics.Debug.IndentSize = 4;
            if (message?.Contains("ENTER") ?? false)
            {
                guid = Guid.NewGuid();
                ((IDictionary<Guid, (string message, DateTime dateTime)>)OpenTracks).Add(guid.Value, (callingMethod + ": " + message, DateTime.Now));
                //OpenTracks.GetOrAdd(guid.Value, (callingMethod + ": " + message, DateTime.Now));
                if (System.Diagnostics.Debug.IndentLevel == 0)
                    System.Diagnostics.Debug.WriteLine("=========================================================");
                System.Diagnostics.Debug.IndentLevel = OpenTracks.Count;
            }
            System.Diagnostics.Debug.WriteLine(callingMethod + ": " + message);
            if (message?.Contains("EXIT") ?? false)
            {
                if (guid.HasValue)
                    ((IDictionary<Guid, (string message, DateTime dateTime)>)OpenTracks).Remove(guid.Value);
                System.Diagnostics.Debug.IndentLevel = OpenTracks.Count;
                if (System.Diagnostics.Debug.IndentLevel == 0)
                    System.Diagnostics.Debug.WriteLine("========================================================= OpenTracks.Count: " + OpenTracks.Count);
            }
            LastMessage = DateTime.Now;
            //});
            return guid;
        }


        static string NameOfCallingClass()
        {
            string fullName;
            Type declaringType;
            int skipFrames = 2;
            do
            {
                MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    return method.Name;
                }
                skipFrames++;
                fullName = declaringType.FullName;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return fullName;
        }


        public static System.Collections.Concurrent.ConcurrentDictionary<Type, long> Census = new System.Collections.Concurrent.ConcurrentDictionary<Type, long>();

        public static void AddToCensus(this object obj)
        {
            if (IsCensusEnabled)
            {
                var type = obj.GetType();
                if (Census.TryGetValue(type, out long count))
                    Census[type] = count + 1;
                else
                    Census[type] = 1;
            }
        }

        public static void RemoveFromCensus(this object obj)
        {
            if (IsCensusEnabled)
            {
                var type = obj.GetType();
                if (Census.TryGetValue(type, out long count))
                    Census[type] = count - 1;
                else
                    System.Diagnostics.Debug.WriteLine("Debug." + P42.Utils.ReflectionExtensions.CallerString() + ":" + "TYPE NOT FOUND!!!! [" + type + "]");
            }
        }

        public static long CensusActiveCount
        {
            get
            {
                var actives = Census.Values.ToArray();
                return actives.Sum();
            }
        }

        /// <summary>
        /// Enable ability to email user for help when unusual crash is encountered
        /// </summary>
        public static bool IsRequestUserHelpEnabled;

        /// <summary>
        /// Email address to which to send requests
        /// </summary>
        public static string RequestUserHelpEmailToAddress = null;

        /// <summary>
        /// Title of PermissionPopup presented to user where "YES" would send an email asking for help with bug
        /// </summary>
        public static string RequestUserHelpMissiveTitle = "I believe I need your help ...";

        /// <summary>
        /// Body of PermissionPopup presented to user where "YES" would send an email asking for help with bug
        /// </summary>
        public static string RequestUserHelpMissiveMessage = "I am the developer of this application (or at least the part the just didn't work).  Unfortunately you have managed to trigger a bug that, for the life of me, I cannot reproduce - and therefore fix!  Would you be willing to email me so I can learn more about what just happened?";

        public static Func<Exception, string, string, int, string, Task> RequestHelpDelegate;

        /// <summary>
        /// Used internally to communicate with user when perplexing exception is triggered;
        /// </summary>
        /// <param name="e">Exception thrown that prompted request for help</param>
        /// <param name="additionalInfo">Any additional info you would like shared with you </param>
        /// <param name="path">Path to file where this request was called</param>
        /// <param name="lineNumber">Linenumber of where this request was called</param>
        /// <param name="methodName">Name of method from which this request was called</param>
        /// <returns></returns>
        public static void RequestUserHelp(
            this Exception e,
            string additionalInfo = null,
            [System.Runtime.CompilerServices.CallerFilePath] string path = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = -1,
            [System.Runtime.CompilerServices.CallerMemberName] string methodName = null)
        {
            if (IsRequestUserHelpEnabled && RequestHelpDelegate is Func<Exception, string, string, int, string, Task> d)
            {
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(async () =>
                    await d(e, additionalInfo, path, lineNumber, methodName));
            }
        }

    }
}
