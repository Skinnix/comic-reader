using Microsoft.AspNetCore.SignalR.Client;
using Skinnix.ComicReader.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Skinnix.ComicReader.Client.Services
{
	interface IHubConnectionService : IStartService, IStopService
	{

	}

	class HubConnectionService : IHubConnectionService
	{
		#region Connection
		private delegate Task AfterConnectActionDelegate();

		private static readonly TimeSpan reconnectionTimerInterval = new TimeSpan(0, 0, 5);

		private readonly object connectionLock = new object();
		private readonly HubConnection connection;
		private readonly Queue<AfterConnectActionDelegate> afterConnectActions = new Queue<AfterConnectActionDelegate>();

		private Task? connectTask, disconnectTask;

		public ServiceState State { get; private set; }

		public HubConnectionService(string url, string? displayName)
		{
			if (url.Contains('?') || url.Contains('#'))
				throw new ArgumentException("Die URL darf keine Parameter oder Anker haben");

			this.DisplayName = displayName ?? Guid.NewGuid().ToString();

			//create connection
			connection = new HubConnectionBuilder()
				.WithUrl(new Uri(url + "?displayName=" + Uri.EscapeUriString(DisplayName)))
				.WithAutomaticReconnect(new ReconnectPolicy())
				.Build();
		}

		private Task Connect()
		{
			lock (connectionLock)
			{
				//trying to disconnect?
				if (disconnectTask?.IsCompleted == true)
					throw new InvalidOperationException();
				disconnectTask = null;

				//connection task finished?
				if (connectTask?.IsCompleted == true)
					connectTask = null;

				//start or continue connecting
				if (connection.State == HubConnectionState.Disconnected)
					return connectTask ??= connection.StartAsync().ContinueWith(async _ =>
					{
						//only to be safe...
						var connectionId = connection.ConnectionId;
						if (connectionId == null)
							return;

						//clear task
						connectTask = null;

						//connection established again
						await AfterConnected(connectionId);
					});
				else
					return connectTask ?? Task.CompletedTask;
			}
		}

		private async Task AfterConnected(string connectionId)
		{
			while (afterConnectActions.TryDequeue(out AfterConnectActionDelegate? item))
				await item();
		}

		private Task Disconnect()
		{
			lock (connectionLock)
			{
				//trying to connect?
				if (connectTask?.IsCompleted == true)
					throw new InvalidOperationException();
				connectTask = null;

				//start or continue disconnecting
				if (connection.State == HubConnectionState.Disconnected)
					return disconnectTask ??= connection.StopAsync().ContinueWith(_ =>
					{
						//clear task
						disconnectTask = null;
					});
				else
					return disconnectTask ?? Task.CompletedTask;
			}
		}

		public async Task Start()
		{
			switch (State)
			{
				case ServiceState.Running:
					return;
				case ServiceState.Stopping:
					throw new InvalidOperationException();
				case ServiceState.Starting:
				case ServiceState.Idle:
					State = ServiceState.Starting;
					await Connect();
					State = ServiceState.Running;
					break;
			}
		}

		public async Task Stop()
		{
			switch (State)
			{
				case ServiceState.Idle:
					return;
				case ServiceState.Starting:
					throw new InvalidOperationException();
				case ServiceState.Stopping:
				case ServiceState.Running:
					State = ServiceState.Stopping;
					await Disconnect();
					State = ServiceState.Idle;
					break;
			}
		}

		private class ReconnectPolicy : IRetryPolicy
		{
			public TimeSpan? NextRetryDelay(RetryContext retryContext)
			{
				//just keep retrying
				return reconnectionTimerInterval;
			}
		}
		#endregion

		#region Display Properties
		public string DisplayName { get; private set; } = Guid.NewGuid().ToString();

		public Task SetDisplayName(string value)
		{
			DisplayName = value;

			switch (State)
			{
				case ServiceState.Running:
					return InnerSetDisplayName(value);
				case ServiceState.Starting:
					afterConnectActions.Enqueue(() => InnerSetDisplayName(value));
					break;
			}

			return Task.CompletedTask;
		}

		private async Task InnerSetDisplayName(string value)
		{
			await Connect();
			await connection.InvokeAsync("SetDisplayName", value);
		}
		#endregion
	}
}
