using System;
using System.Collections.Generic;
using System.Text;

namespace Skinnix.ComicReader
{
	public delegate void ModifiedEventHandler(object? sender, ModifiedEventArgs e);

	public interface INotifyModified
	{
		event ModifiedEventHandler? Modified;
	}
}
