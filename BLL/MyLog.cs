using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class MyLog
    {
        private static MyLog instance;
        private string path;

        private MyLog()
        {
            path = "Log.txt";
            if (!File.Exists(path))
            {
                File.Create(path);
            }
        }

        public static MyLog GetInstance()
        {
            if (instance == null)
                instance = new MyLog();
            return instance;
        }

        public async void Debug(string msg)
        {
            using (StreamWriter writer = new StreamWriter("Log.txt", true))
            {
                await writer.WriteLineAsync("Debug - " + DateTime.Now + " - " + msg);
                writer.Close();
            }
        }

        public async void Trace(string msg)
        {
            using (StreamWriter writer = new StreamWriter("Log.txt", true))
            {
                await writer.WriteLineAsync("Trace - " + DateTime.Now + " - " + msg);
                writer.Close();
            }
        }

        public async void Error(string msg)
        {
            using (StreamWriter writer = new StreamWriter("Log.txt", true))
            {
                await writer.WriteLineAsync("Error - " + DateTime.Now + " - " + msg);
                writer.Close();
            }
        }
    }
}
