using System;
using System.Threading.Tasks;

namespace P42.Utils
{
    public static class Threading
    {
		public static void AppleMainThread(Action action)
		{
			if (action is null)
				return;
			if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
				Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(action);
			else
				action.Invoke();
		}

		public static Task<T> InvokeInBackgroundAsync<T> (Func<Task<T>> funcTask)
        {
			if (!Xamarin.Essentials.MainThread.IsMainThread)
			{
				return funcTask();
			}
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			Task.Run(async delegate {
				try
				{
					T result = await funcTask().ConfigureAwait(continueOnCapturedContext: false);
					tcs.SetResult(result);
				}
				catch (Exception exception)
				{
					tcs.SetException(exception);
				}
			});
			return tcs.Task;
		}

		public static Task InvokeInBackgroundAsync(Func<Task> funcTask)
		{
			if (!Xamarin.Essentials.MainThread.IsMainThread)
			{
				return funcTask();
			}
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
			Task.Run(async delegate {
				try
				{
					await funcTask().ConfigureAwait(continueOnCapturedContext: false);
					tcs.SetResult(null);
				}
				catch (Exception exception)
				{
					tcs.SetException(exception);
				}
			});
			return tcs.Task;
		}

		public static Task<T> InvokeInBackgroundAsync<T>(Func<T> func)
		{
			if (!Xamarin.Essentials.MainThread.IsMainThread)
			{
				return Task.FromResult(func());
			}
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			Task.Run(delegate {
				try
				{
					T result = func();
					tcs.TrySetResult(result);
				}
				catch (Exception exception)
				{
					tcs.TrySetException(exception);
				}
			});
			return tcs.Task;
		}

		public static Task InvokeInBackgroundAsync(Action action)
		{
			if (!Xamarin.Essentials.MainThread.IsMainThread)
			{
				action();
				return Task.CompletedTask;
			}
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Task.Run(delegate {
				try
				{
					action();
					tcs.TrySetResult(result: true);
				}
				catch (Exception exception)
				{
					tcs.TrySetException(exception);
				}
			});
			return tcs.Task;
		}

	}
}
