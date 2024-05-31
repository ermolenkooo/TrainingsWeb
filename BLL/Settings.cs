namespace BLL
{
    public class Settings
    {
        public string ArchiveIp { get; set; }
        public string Archive2Ip { get; set; }
        public string Archive3Ip { get; set; }

        public void SaveSettings(Settings settings)
        {
            ArchiveIp = settings.ArchiveIp;
            Archive2Ip = settings.Archive2Ip;
            Archive3Ip = settings.Archive3Ip;
            using (StreamWriter writer = new StreamWriter("Settings.txt", false))
            {
                writer.WriteLine(ArchiveIp);
                writer.WriteLine(Archive2Ip);
                writer.WriteLine(Archive3Ip);
            }
        }

        public void ReadSettingsFromFile()
        {
            //проверить наличие файла
            if (!File.Exists("Settings.txt"))
            {
                ArchiveIp = String.Empty;
                Archive2Ip = String.Empty;
                Archive3Ip = String.Empty;
            }
            else
            {
                var ans = File.ReadAllLines("Settings.txt").ToList();
                if (ans.Count == 3)
                {
                    ArchiveIp = ans[0];
                    Archive2Ip = ans[1];
                    Archive3Ip = ans[2];
                }
                else
                {
                    ArchiveIp = String.Empty;
                    Archive2Ip = String.Empty;
                    Archive3Ip = String.Empty;
                }
            }
        }
    }
}
