using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Skinnix.ComicReader.Client.MVVM
{
	abstract class PropertyChangeBase : INotifyPropertyChanged, INotifyModified
	{
		public abstract event PropertyChangedEventHandler? PropertyChanged;
		public event ModifiedEventHandler? Modified;

		protected PropertyChangeBase()
		{
			foreach (var property in GetType().GetProperties())
			{
				if (!property.CanRead || property.GetIndexParameters().Length != 0)
					continue;

				//skip primitives and properties that definitely won't work
				if (property.PropertyType.IsValueType || property.PropertyType.IsSealed)
					if (!typeof(INotifyModified).IsAssignableFrom(property.PropertyType) && !typeof(INotifyPropertyChanged).IsAssignableFrom(property.PropertyType))
						continue;

				//add event handlers if applicable
				try
				{
					var value = property.GetValue(this);
					if (value is INotifyModified modify)
						modify.Modified += OnChildModified;
					else if (value is INotifyPropertyChanged change)
						change.PropertyChanged += OnChildPropertyChanged;
				}
				catch (Exception) { }
			}
		}

		protected abstract void RaisePropertyChanged(PropertyChangedEventArgs e);

		protected void RaiseModified(ModifiedEventArgs? e = null)
			=> Modified?.Invoke(this, e ?? new ModifiedEventArgs(this));

		protected void SetProperty<T>(
			ref T holder,
			T value,
			Action? beforeEventAction = null,
			ModifiedEventHandler? modifiedEventHandler = null,
			[CallerMemberName] string? callerName = null)
		{
			if (Equals(holder, value))
				return;

			if (holder is INotifyModified oldValue)
			{
				oldValue.Modified -= OnChildModified;
				if (modifiedEventHandler != null)
					oldValue.Modified -= modifiedEventHandler;
			}
			else if (holder is INotifyPropertyChanged oldValue1)
				oldValue1.PropertyChanged -= OnChildPropertyChanged;

			if (value is INotifyModified newValue)
			{
				newValue.Modified -= OnChildModified;
				newValue.Modified += OnChildModified;

				if (modifiedEventHandler != null)
					newValue.Modified += modifiedEventHandler;
			}
			else if (value is INotifyPropertyChanged newValue1)
			{
				newValue1.PropertyChanged -= OnChildPropertyChanged;
				newValue1.PropertyChanged += OnChildPropertyChanged;
			}

			holder = value;
			beforeEventAction?.Invoke();
			RaisePropertyChanged(new PropertyChangedEventArgs(callerName));
			RaiseModified();
		}

		private void OnChildModified(object? sender, ModifiedEventArgs e)
		{
			RaiseModified(e);
		}

		private void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RaiseModified(new ModifiedEventArgs(sender));
		}

		#region Register/Deregister
		[return: NotNullIfNotNull("value")]
		protected T? Register<T>(T? value)
			where T : class
		{
			if (value == null)
				return value!;

			if (value is INotifyPropertyChanged change)
			{
				change.PropertyChanged -= OnItemPropertyChanged;
				change.PropertyChanged += OnItemPropertyChanged;
			}

			if (value is INotifyModified modify)
			{
				modify.Modified -= OnItemModified;
				modify.Modified += OnItemModified;
			}

			return value;
		}

		[return: NotNullIfNotNull("value")]
		protected T Deregister<T>(T value)
			where T : class
		{
			if (value == null)
				return value!;

			if (value is INotifyPropertyChanged change)
				change.PropertyChanged -= OnItemPropertyChanged;

			if (value is INotifyModified modify)
				modify.Modified -= OnItemModified;

			return value;
		}

		private void OnItemModified(object? sender, ModifiedEventArgs e)
		{
			RaiseModified(e);
		}

		private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RaiseModified(new ModifiedEventArgs(sender));
		}
		#endregion
	}
}
