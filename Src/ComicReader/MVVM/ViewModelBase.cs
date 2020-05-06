using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Skinnix.ComicReader.Client.MVVM
{
	abstract class ViewModelBase : PropertyChangeBase
	{
		#region Change Events
		public override sealed event PropertyChangedEventHandler? PropertyChanged;

		protected override void RaisePropertyChanged(PropertyChangedEventArgs e)
		{
			//use dispatcher where applicable
			var handlers = PropertyChanged;
			if (handlers != null)
				foreach (PropertyChangedEventHandler handler in handlers.GetInvocationList())
					if (handler.Target is DispatcherObject dispatcherObject && dispatcherObject.Dispatcher != null)
						dispatcherObject.Dispatcher.BeginInvoke(() => handler(this, e));
					else
						handler(this, e);

			RaiseModified();
		}
		#endregion

		#region Window Finding
		private bool triedWindow;
		private Window? window;
		protected Window? Window
		{
			get
			{
				if (!triedWindow)
				{
					window = Application.Current.Windows.OfType<Window>().Where(w => ContainsThis(w)).FirstOrDefault();
					triedWindow = true;
				}

				return window;
			}
		}

		private bool ContainsThis(Window window)
		{
			foreach (var child in GetChildren(window))
				if (child.DataContext == this)
					return true;

			return false;
		}

		private IEnumerable<FrameworkElement> GetChildren(FrameworkElement element)
		{
			foreach (var child in GetDirectChildren(element))
			{
				yield return child;
				foreach (var subChild in GetChildren(child))
					yield return subChild;
			}
		}

		private IEnumerable<FrameworkElement> GetDirectChildren(FrameworkElement element)
		{
			var count = VisualTreeHelper.GetChildrenCount(element);
			for (var i = 0; i < count; i++)
				if (VisualTreeHelper.GetChild(element, i) is FrameworkElement fe)
					yield return fe;
		}
		#endregion
	}
}
