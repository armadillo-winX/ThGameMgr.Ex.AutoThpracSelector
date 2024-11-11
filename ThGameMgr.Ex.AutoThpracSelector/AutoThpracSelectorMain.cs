global using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ThGameMgr.Ex.Plugin;

namespace ThGameMgr.Ex.AutoThpracSelector
{
    public class AutoThpracSelectorMain : StartGamePluginBase
    {
        public override string Name => "AutoThpracSelector";

        public override string Version => "1.0.0";

        public override string Developer => "珠音茉白/東方管制塔開発部";

        public override string Description => "自動で最新バージョンの thprac を選択して、選択した thprac を適用してゲームを起動します。";

        public override string CommandName => "thprac を自動適用してゲーム起動";

        public override Process Main(string gameId, string gameFilePath)
        {
            Process gameProcess = StartGameProcessWithApplyingThprac(gameFilePath);
            return gameProcess;
        }

        private Process StartGameProcessWithApplyingThprac(string gameFilePath)
        {
            List<string> thpracFiles = GetThpracFiles(gameFilePath);
            string latestThpracFilePath = GetLatestThpracFile(thpracFiles);

            string gameDirectory = Path.GetDirectoryName(gameFilePath);

            if (File.Exists(gameFilePath) && File.Exists(latestThpracFilePath))
            {
                ProcessStartInfo gameProcessStartInfo = new()
                {
                    FileName = latestThpracFilePath,
                    WorkingDirectory = gameDirectory,
                    UseShellExecute = true
                };

                _ = Process.Start(gameProcessStartInfo);


                int i = 0;
                while (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gameFilePath)).Length == 0)
                {
                    Thread.Sleep(100);
                    if (i == 10)
                    {
                        throw new ProcessNotFoundException("ゲームプロセスの検出に失敗しました。");
                    }
                }

                Process gameProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gameFilePath))[0];

                gameProcess.WaitForInputIdle();

                return gameProcess;
            }
            else if (!File.Exists(latestThpracFilePath))
            {
                throw new FileNotFoundException($"thprac が見つかりませんでした。");
            }
            else
            {
                throw new FileNotFoundException(
                    $"ゲーム実行ファイル{Path.GetFileName(gameFilePath)}が見つかりませんでした。"
                );
            }
        }

        private List<string> GetThpracFiles(string gameFilePath)
        {
            string gameDirectory = Path.GetDirectoryName(gameFilePath);
            if (!string.IsNullOrEmpty(gameDirectory))
            {
                List<string> thpracFiles
                    = [.. Directory.GetFiles(gameDirectory, "thprac*.exe", SearchOption.TopDirectoryOnly)];

                return thpracFiles;
            }
            else
            {
                throw new DirectoryNotFoundException("ゲームがインストールされているフォルダが見つかりませんでした。");
            }
        }

        private string GetLatestThpracFile(List<string> thpracFiles)
        {
            Dictionary<string, string> thpracVersionDictionary = [];
            List<string> versionList = [];
            foreach (string thpracFile in thpracFiles)
            {
                string version = GetProductVersion(thpracFile);
                thpracVersionDictionary.Add(thpracFile, version);
                versionList.Add(version);
            }

            versionList.Sort();
            string latestVersion = versionList[versionList.Count - 1];

            string latestThpracFile
                = thpracVersionDictionary.FirstOrDefault(x => x.Value.Equals(latestVersion)).Key;
            return latestThpracFile;
        }

        private string GetProductVersion(string filePath)
        {
            try
            {
                string productVersion = FileVersionInfo.GetVersionInfo(filePath).ProductVersion;
                if (!string.IsNullOrEmpty(productVersion))
                {
                    return productVersion;
                }
                else
                {
                    return "0.0.0.0";
                }
            }
            catch (Exception)
            {
                return "0.0.0.0";
            }
        }
    }
}
