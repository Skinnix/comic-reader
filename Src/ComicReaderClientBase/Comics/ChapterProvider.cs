using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Skinnix.ComicReader.Client.Base.Comics
{
	public abstract class ChapterProvider : AsyncProvider
	{
		private readonly ConcurrentDictionary<int, PageProvider> loadedPages = new ConcurrentDictionary<int, PageProvider>();
		private readonly ConcurrentDictionary<int, Task<PageProvider>> loadPageTasks = new ConcurrentDictionary<int, Task<PageProvider>>();

		private volatile ChapterInfo? info;
		private volatile Task<ChapterInfo>? loadInfoTask;

		public ComicProvider ComicProvider { get; }

		protected ChapterProvider(ComicProvider comicProvider)
		{
			this.ComicProvider = comicProvider;
		}

		public Task<ChapterInfo> LoadInfo()
			=> Load(info, loadInfoTask,
				value => info = value, () => loadInfoTask = null,
				InnerLoadInfo);

		public Task<PageProvider> LoadPage(int page)
			=> Load(loadedPages, loadPageTasks, InnerLoadPage, page);

		protected abstract Task<ChapterInfo> InnerLoadInfo();
		protected abstract Task<PageProvider> InnerLoadPage(int page);
	}
}
