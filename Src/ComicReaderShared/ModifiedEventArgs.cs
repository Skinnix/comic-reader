using System;
using System.Collections.Generic;
using System.Text;

namespace Skinnix.ComicReader
{
	public class ModifiedEventArgs : EventArgs
	{
		public object Source { get; }

		public ModifiedEventArgs(object source)
		{
			this.Source = source;
		}
	}
}
