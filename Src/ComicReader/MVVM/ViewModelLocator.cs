using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Skinnix.ComicReader.Services;
using Skinnix.ComicReader.Client.Services;
using Skinnix.ComicReader.Client.UI;

namespace Skinnix.ComicReader.Client.MVVM
{
	public class ViewModelLocator
	{
#if DEBUG
		private readonly DependencyObject dummy = new DependencyObject();
		private bool IsDesignMode => DesignerProperties.GetIsInDesignMode(dummy);
#else
		private const bool IsDesignMode = false;
#endif

		public IServiceProvider ServiceProvider { get; }

		#region ViewModels
		public IComicPageDisplayVM ComicPageDisplay => ServiceProvider.GetService<IComicPageDisplayVM>();
		#endregion

		public ViewModelLocator()
		{
			var services = new ServiceCollection();
			ConfigureServices(services);
			this.ServiceProvider = services.BuildServiceProvider();
		}

		private void ConfigureServices(IServiceCollection services)
		{
			//read configuration
			var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", IsDesignMode)
				.Build();

			//add configuration service
			services.AddSingleton<IClientConfiguration>(provider => new ClientConfiguration(config));

			//add connection service
			//services.AddSingleton<IHubConnectionService>(
			//	provider =>
			//	{
			//		var configuration = provider.GetService<IClientConfiguration>();

			//		return new HubConnectionService(configuration.ServerUrl, configuration.DisplayName);
			//	});

			//add view models
			foreach (var type in typeof(ViewModelLocator).Assembly.DefinedTypes)
			{
				if (type.IsInterface)
				{
					//view model
					var attribute = type.GetCustomAttribute<ViewModelAttribute>();
					if (attribute != null)
					{
						//singleton?
						if (attribute.Singleton)
							services.AddSingleton(type,
								IsDesignMode
								? attribute.DesignType ?? attribute.ImplementationType
								: attribute.ImplementationType);
						else
							services.AddTransient(type,
								IsDesignMode
								? attribute.DesignType ?? attribute.ImplementationType
								: attribute.ImplementationType);
					}
				}
			}

			//add start/stop
			services.AddStartStop();
		}

		public async Task Start()
		{
			//connect
			try
			{
				await ServiceProvider.Start();

				var mainWindow = new MainWindow();
				mainWindow.Show();
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
			{
				MessageBox.Show("Fehler beim Start: " + ex, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

				//stop application
				Application.Current.Shutdown();
				return;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}
	}
}
