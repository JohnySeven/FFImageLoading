using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FFImageLoading.Work;

namespace FFImageLoading.DataResolvers
{
	public class ResourceDataResolver : IDataResolver
	{
		private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

		private readonly Dictionary<string, string> _resourceNames;

		private readonly string[] _supportedExts = new string[3]
		{
			".png",
			".jpg",
			".gif"
		};

		private readonly Assembly _entryAssembly;

		public ResourceDataResolver()
		{
			_entryAssembly = Assembly.GetEntryAssembly();
			_resourceNames = (from e in _entryAssembly.GetManifestResourceNames()
				select new
				{
					key = e.ToLower(),
					value = e
				} into r
				where _supportedExts.Any((string e) => r.key.EndsWith(e))
				select r into e
				select new
				{
					key = GetResourceName(e.key),
					value = e.value
				}).ToDictionary(k => k.key, v => v.value);
		}

		private string GetResourceName(string name)
		{
			string[] array = name.Split('.');
			return array[array.Length - 2];
		}

		public virtual async Task<DataResolverResult> Resolve(string identifier, TaskParameter parameters, CancellationToken token)
		{
			ImageService.Instance.Config.Logger.Debug("Resolving resource " + identifier + ".");
			string name = Path.GetFileNameWithoutExtension(identifier.ToLower());
			if (_resourceNames.TryGetValue(name, out var resourceName))
			{
				ImageService.Instance.Config.Logger.Debug("Resource " + identifier + " resolved as " + resourceName + ".");
				Stream stream = await Task.Run(() => _entryAssembly.GetManifestResourceStream(resourceName));
				ImageInformation imageInformation = new ImageInformation();
				imageInformation.SetPath(identifier);
				return new DataResolverResult(stream, LoadingResult.EmbeddedResource, imageInformation);
			}
			ImageService.Instance.Config.Logger.Error("Resource " + identifier + " not resolved!");
			throw new FileNotFoundException(identifier);
		}
	}
}
