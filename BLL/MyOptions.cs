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
        public ScadaVConnection scadaVConnection1 { get; set; }
        public ScadaVConnection scadaVConnection2 { get; set; }
        public ScadaVConnection scadaVConnection3 { get; set; }

        public MyOptions() 
        {
            DbRepos = new DbReposSQLite("TrainingsDb.db");
            scadaVConnection1 = new ScadaVConnection();
            scadaVConnection1.CreateArchiveHost("10.9.35.3");

            scadaVConnection2 = new ScadaVConnection();
            scadaVConnection2.CreateArchiveHost("10.9.35.2");

            scadaVConnection3 = new ScadaVConnection();
            scadaVConnection3.CreateArchiveHost("");

            scadaVConnection1.CreateServerHost("10.9.35.3");
            scadaVConnection2.CreateServerHost("10.9.35.2");
            scadaVConnection3.CreateServerHost("");
        }
    }
}
