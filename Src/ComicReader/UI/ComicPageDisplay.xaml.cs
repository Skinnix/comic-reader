using Skinnix.ComicReader.Client.Base.Comics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Skinnix.ComicReader.Client.UI
{
	public partial class ComicPageDisplay : UserControl
	{
		public static readonly DependencyProperty PageProviderProperty =
			DependencyProperty.Register(nameof(PageProvider), typeof(PageProvider), typeof(ComicPageDisplay), new PropertyMetadata(null, (d, e) =>
			{
				((ComicPageDisplay)d).ViewModel.PageProvider = (PageProvider)e.NewValue;
			}));

		public PageProvider? PageProvider
		{
			get => (PageProvider?)GetValue(PageProviderProperty);
			set => SetValue(PageProviderProperty, value);
		}

		public IComicPageDisplayVM ViewModel => (IComicPageDisplayVM)base.DataContext;

		public ComicPageDisplay()
		{
			InitializeComponent();
		}
	}
}
