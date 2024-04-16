using System;
using System.Threading.Tasks;

namespace P42.Utils
{
    public interface ILongRunningTask
    {
        Task<T> RunAsync<T>(Func<T> function, Func<T> expirationHandler = null);

        Task<T> RunAsync<T>(Func<Task<T>> function, Func<T> expirationHandler = null);

        Task<T> RunAsync<T>(Func<T> function, Func<Task<T>> expirationHandler = null);

        Task<T> RunAsync<T>(Func<Task<T>> function, Func<Task<T>> expirationHandler = null);


        Task RunAsync(Action action, Action expirationHandler = null);

        Task RunAsync(Func<Task> action, Action expirationHandler = null);

        Task RunAsync(Action action, Func<Task> expirationHandler = null);

        Task RunAsync(Func<Task> action, Func<Task> expirationHandler = null);
    }
}
