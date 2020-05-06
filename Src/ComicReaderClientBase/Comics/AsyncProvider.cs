using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Skinnix.ComicReader.Client.Base.Comics
{
	public abstract class AsyncProvider
	{
		private readonly object loadLock = new object();

		protected Task<T> Load<T>(T? value, Task<T>? task, Action<T> setValue, Action resetTask, Func<Task<T>> loadValue)
			where T: class
		{
			lock (loadLock)
			{
				if (task != null)
					return task;

				if (value != null)
					return Task.FromResult(value);

				task = loadValue().ContinueWith(t =>
				{
					lock (loadLock)
					{
						resetTask();
						setValue(t.Result);

						return t.Result;
					}
				});

				return task;
			}
		}

		protected Task<T> Load<T, TKey>(ConcurrentDictionary<TKey, T> values, ConcurrentDictionary<TKey, Task<T>> tasks, Func<TKey, Task<T>> loadValue, TKey key)
			where T : notnull
			where TKey : notnull
		{
			lock (loadLock)
			{
				if (tasks.TryGetValue(key, out Task<T>? loadTask))
					return loadTask;

				if (values.TryGetValue(key, out T pageProvider))
					return Task.FromResult(pageProvider);

				loadTask = loadValue(key).ContinueWith(t =>
				{
					lock (loadLock)
					{
						tasks.TryRemove(key, out _);
						var pageProvider = t.Result;
						values.TryAdd(key, t.Result);

						return pageProvider;
					}
				});
				tasks.TryAdd(key, loadTask);

				return loadTask;
			}
		}
	}
}
