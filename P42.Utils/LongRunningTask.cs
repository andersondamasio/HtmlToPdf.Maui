using System;
using System.Threading.Tasks;

namespace P42.Utils
{
    public static class LongRunningTask
    {
        public static async Task Start(Action action)
            => await Start(action, (Action)null);

        public static async Task Start(Func<Task> action)
            => await Start(action, (Action)null);

        public static async Task<T> Start<T>(Func<T> action)
            => await Start(action, (Func<T>)null);

        public static async Task<T> Start<T>(Func<Task<T>> action)
            => await Start(action, (Func<T>)null);



        public static async Task<T> Start<T>(Func<T> func, Func<T> cancellationAction)
        {
            if (Environment.PlatformLongRunningTaskType is Type type)
            {
                var task = (ILongRunningTask)Activator.CreateInstance(type);
                return await task.RunAsync(func, cancellationAction);
            }
            System.Console.WriteLine("Enviroment not set.  Did you initiate P42.Utils.**platform**?");
            if (func != null)
                return func.Invoke();
            return default;
        }

        public static async Task<T> Start<T>(Func<Task<T>> func, Func<T> cancellationAction)
        {
            if (Environment.PlatformLongRunningTaskType is Type type)
            {
                var task = (ILongRunningTask)Activator.CreateInstance(type);
                return await task.RunAsync(func, cancellationAction);
            }
            System.Console.WriteLine("Enviroment not set.  Did you initiate P42.Utils.**platform**?");
            if (func != null)
                return await func.Invoke();
            return default;
        }

        public static async Task<T> Start<T>(Func<T> func, Func<Task<T>> cancellationAction)
        {
            if (Environment.PlatformLongRunningTaskType is Type type)
            {
                var task = (ILongRunningTask)Activator.CreateInstance(type);
                return await task.RunAsync(func, cancellationAction);
            }
            System.Console.WriteLine("Enviroment not set.  Did you initiate P42.Utils.**platform**?");
            if (func != null)
                return func.Invoke();
            return default;
        }

        public static async Task<T> Start<T>(Func<Task<T>> func, Func<Task<T>> cancellationAction)
        {
            if (Environment.PlatformLongRunningTaskType is Type type)
            {
                var task = (ILongRunningTask)Activator.CreateInstance(type);
                return await task.RunAsync(func, cancellationAction);
            }
            System.Console.WriteLine("Enviroment not set.  Did you initiate P42.Utils.**platform**?");
            if (func != null)
                return await func.Invoke();
            return default;
        }



        public static async Task Start(Action action, Action cancellationAction)
        {
            if (Environment.PlatformLongRunningTaskType is Type type)
            {
                var task = (ILongRunningTask)Activator.CreateInstance(type);
                await task.RunAsync(action, cancellationAction);
                return;
            }
            System.Console.WriteLine("Enviroment not set.  Did you initiate P42.Utils.**platform**?");
            action?.Invoke();
        }

        public static async Task Start(Action action, Func<Task> cancellationAction)
        {
            if (Environment.PlatformLongRunningTaskType is Type type)
            {
                var task = (ILongRunningTask)Activator.CreateInstance(type);
                await task.RunAsync(action, cancellationAction);
                return;
            }
            System.Console.WriteLine("Enviroment not set.  Did you initiate P42.Utils.**platform**?");
            action?.Invoke();
        }

        public static async Task Start(Func<Task> func, Action cancellationAction)
        {
            if (Environment.PlatformLongRunningTaskType is Type type)
            {
                var task = (ILongRunningTask)Activator.CreateInstance(type);
                await task.RunAsync(func, cancellationAction);
                return;
            }
            System.Console.WriteLine("Enviroment not set.  Did you initiate P42.Utils.**platform**?");
            if (func != null)
                await func.Invoke();
        }

        public static async Task Start(Func<Task> func, Func<Task> cancellationAction)
        {
            if (Environment.PlatformLongRunningTaskType is Type type)
            {
                var task = (ILongRunningTask)Activator.CreateInstance(type);
                await task.RunAsync(func, cancellationAction);
                return;
            }
            System.Console.WriteLine("Enviroment not set.  Did you initiate P42.Utils.**platform**?");
            if (func != null)
                await func.Invoke();
        }

    }
}
