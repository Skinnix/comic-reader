using Skinnix.ComicReader.Client.Base.Comics;
using Skinnix.ComicReader.Client.MVVM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Skinnix.ComicReader.Client.UI
{
	[ViewModel(typeof(ComicPageDisplayVM))]
	public interface IComicPageDisplayVM
	{
		PageProvider? PageProvider { get; set; }
	}

	class ComicPageDisplayVM : ViewModelBase, IComicPageDisplayVM
	{
		private PageProvider? pageProvider;
		public PageProvider? PageProvider
		{
			get => pageProvider;
			set
			{
				SetProperty(ref pageProvider, value, () =>
				{
					Image = null;
					pageProvider?.LoadImage().ContinueWith(t =>
					{
						if (pageProvider == value)
							Image = t.Result;
					});
				});
			}
		}

		private ImageSource? image;
		public ImageSource? Image
		{
			get => image;
			set => SetProperty(ref image, value);
		}
	}
}
