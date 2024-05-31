using DAL.Interfaces;
using DAL.Repositories;
using DAL.Repositories.SQLite;
using Scada.Interfaces;
using System.Security.Cryptography.Xml;

namespace BLL
{
    public class MyOptions
    {
        public IDbRepos DbRepos {  get; set; }
        public Settings Settings { get; set; }
        public ScadaVConnection scadaVConnection1 { get; set; }
        public ScadaVConnection scadaVConnection2 { get; set; }
        public ScadaVConnection scadaVConnection3 { get; set; }

        public MyOptions() 
        {
            DbRepos = new DbReposSQLite("TrainingsDb.db");
            Settings = new Settings();
            Settings.ReadSettingsFromFile();
            scadaVConnection1 = new ScadaVConnection();
            scadaVConnection1.CreateArchiveHost(Settings.ArchiveIp);

            scadaVConnection2 = new ScadaVConnection();
            scadaVConnection2.CreateArchiveHost(Settings.Archive2Ip);

            scadaVConnection3 = new ScadaVConnection();
            scadaVConnection3.CreateArchiveHost(Settings.Archive3Ip);

            scadaVConnection1.CreateServerHost(Settings.ArchiveIp);
            scadaVConnection2.CreateServerHost(Settings.Archive2Ip);
            scadaVConnection3.CreateServerHost(Settings.Archive3Ip);
        }

        public void NewSetting()
        {
            Settings.ReadSettingsFromFile();
            scadaVConnection1 = new ScadaVConnection();
            scadaVConnection1.CreateArchiveHost(Settings.ArchiveIp);

            scadaVConnection2 = new ScadaVConnection();
            scadaVConnection2.CreateArchiveHost(Settings.Archive2Ip);

            scadaVConnection3 = new ScadaVConnection();
            scadaVConnection3.CreateArchiveHost(Settings.Archive3Ip);

            scadaVConnection1.CreateServerHost(Settings.ArchiveIp);
            scadaVConnection2.CreateServerHost(Settings.Archive2Ip);
            scadaVConnection3.CreateServerHost(Settings.Archive3Ip);
        }
    }
}
