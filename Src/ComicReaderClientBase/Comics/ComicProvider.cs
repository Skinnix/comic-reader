using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Skinnix.ComicReader.Client.Base.Comics
{
	public abstract class ComicProvider : AsyncProvider
	{
		private readonly ConcurrentDictionary<int, ChapterProvider> loadedChapters = new ConcurrentDictionary<int, ChapterProvider>();
		private readonly ConcurrentDictionary<int, Task<ChapterProvider>> loadChapterTasks = new ConcurrentDictionary<int, Task<ChapterProvider>>();

		private volatile ComicInfo? info;
		private volatile Task<ComicInfo>? loadInfoTask;

		public Task<ComicInfo> LoadInfo()
			=> Load(info, loadInfoTask,
				value => info = value, () => loadInfoTask = null,
				InnerLoadInfo);

		public Task<ChapterProvider> LoadChapter(int chapter)
			=> Load(loadedChapters, loadChapterTasks, InnerLoadChapter, chapter);

		protected abstract Task<ComicInfo> InnerLoadInfo();
		protected abstract Task<ChapterProvider> InnerLoadChapter(int chapter);
	}
}
