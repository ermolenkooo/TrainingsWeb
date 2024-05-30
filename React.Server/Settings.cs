namespace React.Server
{
    public class Settings
    {
        private static Settings instance;

        private Settings()
        {
            ReadSettingsFromFile();
        }

        public static Settings GetInstance()
        {
            if (instance == null)
                instance = new Settings();
            return instance;
        }

        public string ScadaDbPath { get; private set; }
        public string ScadaDbIp { get; private set; }
        public string ArchiveIp { get; set; }
        public string ScadaDb2Path { get; set; }
        public string ScadaDb2Ip { get; set; }
        public string Archive2Ip { get; set; }
        public string ScadaDb3Path { get; set; }
        public string ScadaDb3Ip { get; set; }
        public string Archive3Ip { get; set; }
        public string TrainingDbPath { get; private set; }
        public string ReportDirectory { get; private set; }

        public void SaveSettings(string scadaDbPath, string scadaDbIp, string archiveIp, string scadaDb2Path, string scadaDb2Ip, string archive2Ip, string scadaDb3Path, string scadaDb3Ip, string archive3Ip, string trainingDbPath, string reportDirectory)
        {
            ScadaDbPath = scadaDbPath;
            ScadaDbIp = scadaDbIp;
            ArchiveIp = archiveIp;
            ScadaDb2Path = scadaDb2Path;
            ScadaDb2Ip = scadaDb2Ip;
            Archive2Ip = archive2Ip;
            ScadaDb3Path = scadaDb3Path;
            ScadaDb3Ip = scadaDb3Ip;
            Archive3Ip = archive3Ip;
            TrainingDbPath = trainingDbPath;
            ReportDirectory = reportDirectory;
            using (StreamWriter writer = new StreamWriter("Settings.txt", false))
            {
                writer.WriteLine(scadaDbPath);
                writer.WriteLine(scadaDbIp);
                writer.WriteLine(archiveIp);
                writer.WriteLine(scadaDb2Path);
                writer.WriteLine(scadaDb2Ip);
                writer.WriteLine(archive2Ip);
                writer.WriteLine(scadaDb3Path);
                writer.WriteLine(scadaDb3Ip);
                writer.WriteLine(archive3Ip);
                writer.WriteLine(trainingDbPath);
                writer.WriteLine(reportDirectory);
            }
        }

        public List<string> ReadSettingsFromFile()
        {
            List<string> ans = new List<string>();
            //проверить наличие файла
            if (!File.Exists("Settings.txt"))
            {
                ScadaDbPath = String.Empty;
                ScadaDbIp = String.Empty;
                ArchiveIp = String.Empty;
                ScadaDb2Path = String.Empty;
                ScadaDb2Ip = String.Empty;
                Archive2Ip = String.Empty;
                ScadaDb3Path = String.Empty;
                ScadaDb3Ip = String.Empty;
                Archive3Ip = String.Empty;
                TrainingDbPath = String.Empty;
                ReportDirectory = String.Empty;
                return ans;
            }
            else
            {
                ans = File.ReadAllLines("Settings.txt").ToList();
                if (ans.Count == 11)
                {
                    ScadaDbPath = ans[0];
                    ScadaDbIp = ans[1];
                    ArchiveIp = ans[2];
                    ScadaDb2Path = ans[3];
                    ScadaDb2Ip = ans[4];
                    Archive2Ip = ans[5];
                    ScadaDb3Path = ans[6];
                    ScadaDb3Ip = ans[7];
                    Archive3Ip = ans[8];
                    TrainingDbPath = ans[9];
                    ReportDirectory = ans[10];
                }
                else
                {
                    ScadaDbPath = String.Empty;
                    ScadaDbIp = String.Empty;
                    ArchiveIp = String.Empty;
                    ScadaDb2Path = String.Empty;
                    ScadaDb2Ip = String.Empty;
                    Archive2Ip = String.Empty;
                    ScadaDb3Path = String.Empty;
                    ScadaDb3Ip = String.Empty;
                    Archive3Ip = String.Empty;
                    TrainingDbPath = String.Empty;
                    ReportDirectory = String.Empty;
                }
                return ans;
            }
        }
    }
}
