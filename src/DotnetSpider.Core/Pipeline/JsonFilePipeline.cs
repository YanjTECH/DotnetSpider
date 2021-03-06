using System.IO;
using System.Text;
using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json;
using NLog;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// Store results to files in JSON format.
	/// </summary>
	public class JsonFilePipeline : BasePipeline
	{
		private readonly string _intervalPath;

		public JsonFilePipeline()
		{
#if NET_CORE
			_intervalPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "data\\json" : "data/json";
#else
			_intervalPath = "data\\json";
#endif
		}

		public JsonFilePipeline(string path)
		{
			_intervalPath = path;
		}

		public string GetDataForlder()
		{
			return $"{BasePath}{Env.PathSeperator}{Spider.Identity}{Env.PathSeperator}";
		}

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

			string path;
			if (string.IsNullOrEmpty(_intervalPath))
			{
#if NET_CORE
				path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"\\{spider.Identity}\\data\\json" : $"/{spider.Identity}/data/json";
#else
				path = "\\{spider.Identity}\\data\\json";
#endif
			}
			else
			{
				path = _intervalPath;
			}
			SetPath(path);
		}

		public override void Process(params ResultItems[] resultItems)
		{
			try
			{
				foreach (var resultItem in resultItems)
				{
					string path = $"{BasePath}{Env.PathSeperator}{Spider.Identity}{Env.PathSeperator}{Encrypt.Md5Encrypt(resultItem.Request.Url.ToString())}.json";
					FileInfo file = PrepareFile(path);
					using (StreamWriter printWriter = new StreamWriter(file.OpenWrite(), Encoding.UTF8))
					{
						printWriter.WriteLine(JsonConvert.SerializeObject(resultItem.Results));
					}
				}
			}
			catch (IOException e)
			{
				Logger.AllLog(Spider.Identity, "Write data to json file failed.", LogLevel.Error, e);
				throw;
			}
		}
	}
}
