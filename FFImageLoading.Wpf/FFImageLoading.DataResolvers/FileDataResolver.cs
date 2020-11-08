using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFImageLoading.Work;

namespace FFImageLoading.DataResolvers
{
	public class FileDataResolver : IDataResolver
	{
		public virtual async Task<DataResolverResult> Resolve(string identifier, TaskParameter parameters, CancellationToken token)
		{
			if (File.Exists(identifier))
			{
				ImageInformation imageInformation = new ImageInformation();
				imageInformation.SetPath(identifier);
				imageInformation.SetFilePath(identifier);
				token.ThrowIfCancellationRequested();
				return new DataResolverResult(await Task.Run(() => File.OpenRead(identifier)), LoadingResult.Disk, imageInformation);
			}
			throw new FileNotFoundException(identifier);
		}
	}
}
