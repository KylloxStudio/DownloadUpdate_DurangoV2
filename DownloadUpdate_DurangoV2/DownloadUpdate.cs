using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HtmlAgilityPack;

namespace DownloadUpdate
{
	class MainClass
	{
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr windowHandle, bool revert);

		[DllImport("user32.dll")]
		private static extern bool EnableMenuItem(IntPtr menuHandle, uint menuItemID, uint enabled);

		private const uint SC_CLOSE = 0xf060;
		private const uint MF_ENABLED = 0x00000000;
		private const uint MF_GRAYED = 0x00000001;

		static void Main(string[] args)
		{
			Console.Title = "DownloadUpdate_DurangoV2";
			IntPtr consoleWindowHandle = GetConsoleWindow();

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			HtmlWeb web = new HtmlWeb();
			HtmlDocument document = web.Load("https://github.com/KylloxStudio/Durango_V2/commits/main");
			HtmlNodeCollection aNodes = document.DocumentNode.SelectNodes("//a[@class='Link--primary text-bold js-navigation-open markdown-title']");

			string fileName = string.Empty;
			string pastFileName = string.Empty;
			string downloadedFilePath = string.Empty;
			bool foundUpdate = false;
			bool downloadFolder = false;
			bool isDownloading = false;
			int fileLength = 0;

			WebClient webClient = new WebClient();
			webClient.DownloadProgressChanged += (s, e) =>
			{
				if (isDownloading)
                {
					if (pastFileName != fileName)
					{
						pastFileName = fileName;
						Console.Write("\"{0}\": Downloading... ", fileName);
					}
				}
			};
			webClient.DownloadFileCompleted += (s, e) =>
			{
				fileLength = (int)new FileInfo(downloadedFilePath).Length;
				using (var progress = new ProgressBar())
				{
					for (int i = 0; i <= 100; i++)
					{
						progress.Report((double)i / 100);
						Thread.Sleep(fileLength / 1000000);
					}
				}
				Console.WriteLine("Completed.");
				Thread.Sleep(500);
				isDownloading = false;
			};

			Console.Title = "DownloadUpdate_DurangoV2 - Updating...";
			SetCloseButtonEnabled(consoleWindowHandle, false);

			for (int i = aNodes.Count - 1; i >= 0; i--)
			{
				if (aNodes[i].InnerText.Trim().ToLower().IndexOf("error") == -1 && aNodes[i].InnerText.Trim().ToLower().IndexOf("initial") == -1 && aNodes[i].InnerText.Trim().ToLower().IndexOf("first") == -1 && aNodes[i].InnerText.Trim().ToLower().IndexOf("readme") == -1)
				{
					HtmlDocument document2 = web.Load("https://github.com" + aNodes[i].GetAttributeValue("href", null));
					HtmlNodeCollection aNodes2 = document2.DocumentNode.SelectNodes("//a[@class='Link--primary']");
					foreach (HtmlNode node in aNodes2)
					{
						if (node.InnerText.IndexOf("DownloadUpdate") != -1)
						{
							foundUpdate = true;
							break;
						}
						if (node.InnerText.IndexOf("→") == -1)
						{
							string[] split = node.InnerText.Split(new char[]
							{
								'/'
							});

							string downloadPath = "https://github.com/KylloxStudio/Durango_V2/blob/main/" + node.InnerText + "?raw=true";
							string localPath = Directory.GetCurrentDirectory();
							for (int j = 0; j < split.Length; j++)
							{
								localPath += "\\" + split[j];
							}

							if (!Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(localPath).FullName).FullName).FullName).Exists)
							{
								Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(localPath).FullName).FullName).FullName).FullName);
								downloadFolder = true;
							}
							if (!Directory.GetParent(Directory.GetParent(Directory.GetParent(localPath).FullName).FullName).Exists)
							{
								Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(Directory.GetParent(localPath).FullName).FullName).FullName);
								downloadFolder = true;
							}
							if (!Directory.GetParent(Directory.GetParent(localPath).FullName).Exists)
							{
								Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(localPath).FullName).FullName);
								downloadFolder = true;
							}
							if (!Directory.GetParent(localPath).Exists)
							{
								Directory.CreateDirectory(Directory.GetParent(localPath).FullName);
								downloadFolder = true;
							}

							if (downloadFolder)
							{
								string path = "https://github.com/KylloxStudio/Durango_V2/blob/main";
								for (int j = 0; j < split.Length - 1; j++)
								{
									path += "/" + split[j];
								}
								HtmlDocument document3 = web.Load(path);
								if (web.StatusCode == HttpStatusCode.OK)
								{
									HtmlNodeCollection aNodes3 = document3.DocumentNode.SelectNodes("//a[@class='js-navigation-open Link--primary']");
									foreach (HtmlNode node4 in aNodes3)
									{
										if (node4.InnerText.ToLower().IndexOf("agree") == -1)
										{
											string[] split2 = node4.GetAttributeValue("href", null).Split(new string[]
											{
												"/KylloxStudio/Durango_V2/blob/main/"
											}, StringSplitOptions.None);

											string[] split3 = split2[1].Split(new char[]
											{
												'/'
											});

											string downloadPath2 = "https://github.com" + node4.GetAttributeValue("href", null) + "?raw=true";
											string localPath2 = Directory.GetCurrentDirectory();
											for (int j = 0; j < split3.Length; j++)
											{
												localPath2 += "\\" + split3[j];
											}
											while (webClient.IsBusy)
											{
											}
											while (isDownloading)
											{
											}
											downloadedFilePath = localPath2;
											webClient.DownloadFileAsync(new Uri(downloadPath2), localPath2);
											fileName = node4.InnerText;
											isDownloading = true;
										}
									}
									downloadFolder = false;
								}
							}
							else
							{
								try
								{
									HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadPath);
									HttpWebResponse response = (HttpWebResponse)request.GetResponse();
									if (response.StatusCode == HttpStatusCode.OK)
									{
										while (webClient.IsBusy)
										{
										}
										while (isDownloading)
                                        {
                                        }
										downloadedFilePath = localPath;
										webClient.DownloadFileAsync(new Uri(downloadPath), localPath);
										fileName = split[split.Length - 1];
										isDownloading = true;
									}
									response.Close();
								}
								catch (WebException)
								{
									break;
								}
								catch (Exception e)
								{
									Console.WriteLine(e.Message);
								}
							}
						}
					}
				}
			}

			Console.Title = "DownloadUpdate_DurangoV2";

			while (webClient.IsBusy)
			{
			}
			while (isDownloading)
			{
			}
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("All Files Update Completed.");

			bool isOpenedUrl = false;
			if (foundUpdate)
			{
				Console.WriteLine();
				Console.WriteLine("It found an update for the update automatic downloader. Do you want to go to the update page? (Y / N)\n(After manual download, and unzip into the [DurangoV2_data] folder.)");
				while (true)
				{
					string input = Console.ReadLine();
					if (input == "Y" || input == "y")
					{
						Process.Start("https://minhaskamal.github.io/DownGit/#/home?url=https://github.com/KylloxStudio/Durango_V2/tree/main/DurangoV2_Data/DownloadUpdate");
						isOpenedUrl = true;
						break;
					}
					else if (input == "N" || input == "n")
					{
						break;
					}
				}
			}

			foreach (Process process in Process.GetProcesses())
			{
				if (process.ProcessName.StartsWith("DurangoV2"))
				{
					process.Kill();
					break;
				}
			}
			if (!isOpenedUrl)
				Process.Start(Directory.GetCurrentDirectory() + "\\DurangoV2.exe");
		}

		private static void SetCloseButtonEnabled(IntPtr windowHandle, bool enabled)
		{
			IntPtr systemMenuHandle = GetSystemMenu(windowHandle, false);
			EnableMenuItem(systemMenuHandle, SC_CLOSE, MF_ENABLED | (enabled ? MF_ENABLED : MF_GRAYED));
		}
	}
}