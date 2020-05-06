using Skinnix.ComicReader.Client.MVVM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Skinnix.ComicReader.Client
{
    public partial class App : Application
	{
		private void OnStartup(object sender, StartupEventArgs e)
		{
			var viewModelLocator = (ViewModelLocator)Resources["ViewModelLocator"];
			viewModelLocator.Start().GetAwaiter().GetResult();
		}
	}
}
