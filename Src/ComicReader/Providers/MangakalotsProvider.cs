using Skinnix.ComicReader.Client.Base.Comics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Skinnix.ComicReader.Client.Providers
{
	public class MangakalotsProvider : ComicProvider
	{
		private readonly Regex findChaptersRegex;

		private readonly string comicId;
		private readonly Uri comicUri;

		private readonly HttpClient httpClient = new HttpClient();

		public MangakalotsProvider(string comicId)
		{
			this.comicId = comicId;
			this.findChaptersRegex = new Regex($@"https?:\/\/mangakakalots.com\/chapter\/{comicId}\/\w+");
			this.comicUri = new Uri($"https://mangakakalots.com/manga/{comicId}");
		}

		protected override async Task<ComicInfo> InnerLoadInfo()
		{
			var comicHtml = await httpClient.GetStringAsync(comicUri);
			var images = findChaptersRegex.Matches(comicHtml).Select(match => new Uri(match.Value)).Distinct().Reverse().ToArray();

			return new MyComicInfo(images);
		}

		protected override async Task<ChapterProvider> InnerLoadChapter(int chapter)
		{
			var info = (MyComicInfo)await LoadInfo();
			return new MyChapterProvider(this, info.GetChapterUrl(chapter));
		}

		private class MyComicInfo : ComicInfo
		{
			private readonly Uri[] chapterUrls;

			public MyComicInfo(Uri[] chapterUrls)
				: base(chapterUrls.Length)
			{
				this.chapterUrls = chapterUrls;
			}

			public Uri GetChapterUrl(int index)
				=> chapterUrls[index];
		}

		private class MyChapterProvider : ChapterProvider
		{
			private static readonly Regex findImagesRegex = new Regex(@"https?:\/\/[^""]+\/mangakakalot\/[^""]+\/[^""]+\.\w+");

			private readonly Uri chapterUri;

			private new MangakalotsProvider ComicProvider => (MangakalotsProvider)base.ComicProvider;

			public MyChapterProvider(MangakalotsProvider comicProvider, Uri chapterUri)
				: base(comicProvider)
			{
				this.chapterUri = chapterUri;
			}

			protected override async Task<ChapterInfo> InnerLoadInfo()
			{
				var chapterHtml = await ComicProvider.httpClient.GetStringAsync(chapterUri);
				var images = findImagesRegex.Matches(chapterHtml).Select(match => new Uri(match.Value)).Distinct().ToArray();

				return new MyChapterInfo(images);
			}

			protected override async Task<PageProvider> InnerLoadPage(int page)
			{
				var info = (MyChapterInfo)await LoadInfo();
				return new MyPageProvider(this, info.GetImageUrl(page));
			}

			private class MyChapterInfo : ChapterInfo
			{
				private readonly Uri[] imageUrls;

				public MyChapterInfo(Uri[] imageUrls)
					: base(imageUrls.Length)
				{
					this.imageUrls = imageUrls;
				}

				public Uri GetImageUrl(int index)
					=> imageUrls[index];
			}
		}

		private class MyPageProvider : PageProvider
		{
			private readonly Uri imageUrl;

			private new MyChapterProvider ChapterProvider => (MyChapterProvider)base.ChapterProvider;

			public MyPageProvider(MyChapterProvider chapterProvider, Uri imageUrl)
				: base(chapterProvider)
			{
				this.imageUrl = imageUrl;
			}

			protected override Task<ImageSource> InnerLoadImage()
			{
				BitmapImage result = new BitmapImage();
				result.BeginInit();
				result.UriSource = imageUrl;
				result.EndInit();

				return Task.FromResult<ImageSource>(result!);
			}
		}
	}
}
