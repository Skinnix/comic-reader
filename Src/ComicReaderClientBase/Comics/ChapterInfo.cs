using System;
using System.Collections.Generic;
using System.Text;

namespace Skinnix.ComicReader.Client.Base.Comics
{
	public class ChapterInfo
	{
		public int PageCount { get; }

		public ChapterInfo(int pageCount)
		{
			this.PageCount = pageCount;
		}
	}
}
