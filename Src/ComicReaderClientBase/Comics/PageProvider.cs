using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Skinnix.ComicReader.Client.Base.Comics
{
	public abstract class PageProvider : AsyncProvider
	{
		private volatile ImageSource? image;
		private volatile Task<ImageSource>? loadImageTask;

		public ChapterProvider ChapterProvider { get; }

		protected PageProvider(ChapterProvider chapterProvider)
		{
			this.ChapterProvider = chapterProvider;
		}

		public Task<ImageSource> LoadImage()
			=> Load(image, loadImageTask,
				value => image = value, () => loadImageTask = null,
				InnerLoadImage);

		protected abstract Task<ImageSource> InnerLoadImage();
	}
}
