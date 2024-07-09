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
            NewSetting();
        }

        public void NewSetting()
        {
            DbRepos = new DbReposSQLite("TrainingsDb.db");
            Settings = new Settings();
            Settings.ReadSettingsFromFile();
            scadaVConnection1 = new ScadaVConnection();
            scadaVConnection2 = new ScadaVConnection();
            scadaVConnection3 = new ScadaVConnection();
        }

        public async Task InitializeAsync()
        {
            DbRepos = new DbReposSQLite("TrainingsDb.db");
            Settings = new Settings();
            Settings.ReadSettingsFromFile();
            scadaVConnection1 = new ScadaVConnection();
            await scadaVConnection1.CreateArchiveHost(Settings.ArchiveIp);

            scadaVConnection2 = new ScadaVConnection();
            await scadaVConnection2.CreateArchiveHost(Settings.Archive2Ip);

            scadaVConnection3 = new ScadaVConnection();
            await scadaVConnection3.CreateArchiveHost(Settings.Archive3Ip);

            await scadaVConnection1.CreateServerHost(Settings.ArchiveIp);
            await scadaVConnection2.CreateServerHost(Settings.Archive2Ip);
            await scadaVConnection3.CreateServerHost(Settings.Archive3Ip);
        }
    }
}
