using Skinnix.ComicReader.Client.MVVM;
using Skinnix.ComicReader.Client.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skinnix.ComicReader.Client
{
	[ViewModel(typeof(MainWindowVM))]
	public interface IMainWindowVM
	{
		
	}

	class MainWindowVM : IMainWindowVM
	{
		private readonly IHubConnectionService connection;

		public MainWindowVM(IHubConnectionService connection)
		{
			this.connection = connection;
		}
	}
}
