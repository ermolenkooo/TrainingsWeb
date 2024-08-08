using BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Operations
{
    public class ReportOperations
    {
        private string emergencyHeader = new string(' ', 34) + "Ведомость\n" + new string(' ', 21) + "анализа противоаварийной тренировки";
        private string startStopHeader = "Отчёт";
        private string evaluationCriteria = "Критерии оценки:";
        private string finalMark = "Итоговая оценка за тренировку:";
        private string dateAndTimeOfTraining = "Дата и время проведения тренировки:";

        public string CreateReport(int reportType, Report report)
        {
            string ans = "";
            if (reportType == 1)
            {
                //Составляем отчет для противоаварийной тренировки
                ans += emergencyHeader + "\n";
                ans += report.TrainingName + "\n";
                ans += report.FIO + " " + report.Position + "\n";
                for (int i = 0; i < report.CriteriasWithMarks.Length; i++)
                    ans += report.CriteriasWithMarks[i] + "\n";
                ans += finalMark + " " + report.EndMark + "\n";
                ans += dateAndTimeOfTraining + " " + report.Date.AddHours(3).ToString() + "\n";
                ans += "Подпись _____";
            }
            if (reportType == 2)
            {
                //Составляем отчет для ведомости пуска и останова
                ans += new string(' ', 37) + report.FIO + "\n" + new string(' ', 37) + report.Position + "\n";
                ans += new string(' ', 37) + dateAndTimeOfTraining + "\n" + new string(' ', 52) + report.Date.AddHours(3).ToString() + "\n";
                ans += startStopHeader + "\n";
                ans += report.TrainingName + "\n";
                ans += evaluationCriteria + "\n";
                for (int i = 0; i < report.CriteriasWithMarks.Length; i++)
                    ans += report.CriteriasWithMarks[i] + "\n";
                ans += finalMark + " " + report.EndMark + "\n";
            }
            return ans;
        }

        public void CreateDocument(string reportText, string path)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
            {
                using (StreamWriter sw = file.CreateText())
                {
                    sw.WriteLine(reportText);
                }
            }
        }
    }
}
