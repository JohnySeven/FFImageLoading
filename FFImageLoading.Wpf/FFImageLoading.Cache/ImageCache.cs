using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using FFImageLoading.Helpers;
using FFImageLoading.Work;
using FFImageLoading.Wpf;

namespace FFImageLoading.Cache
{
	internal class ImageCache : IImageCache, IMemoryCache<BitmapSource>
	{
		private static IImageCache _instance;

		private readonly WriteableBitmapLRUCache _reusableBitmaps;

		private readonly IMiniLogger _logger;

		public static IImageCache Instance => _instance ?? (_instance = new ImageCache(ImageService.Instance.Config.MaxMemoryCacheSize, ImageService.Instance.Config.Logger));

		private ImageCache(int maxCacheSize, IMiniLogger logger)
		{
			_logger = logger;
			if (maxCacheSize == 0)
			{
				maxCacheSize = 256000000;
				_logger?.Debug($"Memory cache size: {maxCacheSize} bytes");
			}
			_reusableBitmaps = new WriteableBitmapLRUCache(maxCacheSize);
		}

		public void Add(string key, ImageInformation imageInformation, BitmapSource bitmap)
		{
			if (!string.IsNullOrWhiteSpace(key) && bitmap != null)
			{
				_reusableBitmaps.TryAdd(key, new Tuple<BitmapSource, ImageInformation>(bitmap, imageInformation));
			}
		}

		public ImageInformation GetInfo(string key)
		{
			if (_reusableBitmaps.TryGetValue(key, out var value))
			{
				return value.Item2;
			}
			return null;
		}

		public Tuple<BitmapSource, ImageInformation> Get(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return null;
			}
			if (_reusableBitmaps.TryGetValue(key, out var value) && value.Item1 != null)
			{
				return new Tuple<BitmapSource, ImageInformation>(value.Item1, value.Item2);
			}
			return null;
		}

		public void Clear()
		{
			_reusableBitmaps.Clear();
			GC.Collect();
		}

		public void Remove(string key)
		{
			if (!string.IsNullOrWhiteSpace(key))
			{
				_logger.Debug($"Called remove from memory cache for '{key}'");
				_reusableBitmaps.Remove(key);
			}
		}

		public void RemoveSimilar(string baseKey)
		{
			if (string.IsNullOrWhiteSpace(baseKey))
			{
				return;
			}
			string pattern = baseKey + ";";
			List<string> list = _reusableBitmaps.Keys.Where((string i) => i.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)).ToList();
			foreach (string item in list)
			{
				Remove(item);
			}
		}
	}
}
