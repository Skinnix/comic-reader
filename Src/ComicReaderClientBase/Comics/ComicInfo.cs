using System;
using System.Collections.Generic;
using System.Text;

namespace Skinnix.ComicReader.Client.Base.Comics
{
	public class ComicInfo
	{
		public int ChapterCount { get; }

		public ComicInfo(int chapterCount)
		{
			this.ChapterCount = chapterCount;
		}
	}
}
