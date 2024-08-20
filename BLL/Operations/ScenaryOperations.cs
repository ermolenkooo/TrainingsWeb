using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using Grpc.Core;
using NLog.Fluent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Operations
{
    public class ScenaryOperations
    {
        private TrainingDbOperations _db;
        private ScadaVConnection scadaVConnection1;
        private ScadaVConnection scadaVConnection2;
        private ScadaVConnection scadaVConnection3;

        public ScenaryOperations(MyOptions options)
        {
            _db = new TrainingDbOperations(options);
            scadaVConnection1 = options.scadaVConnection1;
            scadaVConnection2 = options.scadaVConnection2;
            scadaVConnection3 = options.scadaVConnection3;
        }

        private TrainingModel _selectedTraining;
        private int _count;
        private int? _mark;
        private string _endMark;
        private string _logText = string.Empty;
        private string _logText2 = string.Empty;
        private List<string> _criteriasForReport1;
        private List<string> _criteriasForReport2;

        private List<DiscretSignalModel> _discretSignals;
        private List<DoubleDiscretSignalModel> _doubleDiscretSignals;
        private List<DiscretFromAnalogSignalModel> _discretFromAnalogSignals;
        private List<GroupOfDiscretSignalsModel> _groupOfDiscretSignals;
        private List<OperationWithConditionModel> _operationsWithCondition;

        BlockingCollection<Models.Log> q;
        Thread thread;

        public async Task BeginScenary(int id)
        {
            _count = 0;
            _selectedTraining = _db.SelectTrainingById(id);
            await PerformSelectedTrainingOperations();

            await LoadDataAsync();

            q = new BlockingCollection<Models.Log>();
            thread = new Thread(Consumer);
            thread.Start();

            //TimerManager.TimerTick += async () => await CheckAllAsync();
            //TimerManager.Start();
        }

        private void Consumer()
        {
            foreach (var s in q.GetConsumingEnumerable())
            {
                using (StreamWriter writer = new StreamWriter("Log.txt", true))
                {
                    writer.WriteLine(s.Type + " - " + DateTime.Now + " - " + s.Message);
                    writer.Close();
                }
            }
        }

        private string getWord(int? number)
        {
            switch (number)
            {
                case 0: return "баллов";
                case 1: return "балл";
                case 2: return "балла";
                case 3: return "балла";
                case 4: return "балла";
                case 5: return "баллов";
                default: return "";
            }
        }

        private async Task LoadDataAsync()
        {
            _discretSignals = await Task.Run(() => _db.SelectAllDiscretSignals(_selectedTraining.Id));
            _doubleDiscretSignals = await Task.Run(() => _db.SelectAllDoubleDiscretSignals(_selectedTraining.Id));
            _discretFromAnalogSignals = await Task.Run(() => _db.SelectAllDiscretFromAnalogSignals(_selectedTraining.Id, 0));
            _operationsWithCondition = await Task.Run(() => _db.SelectOperationsWithConditionWithTrainingId(_selectedTraining.Id));
            _groupOfDiscretSignals = await Task.Run(() => _db.SelectGroupsByTrainingId(_selectedTraining.Id));
        }

        private async Task PerformSelectedTrainingOperations()
        {
            MyLog.GetInstance().Trace("Начинаю запись сценария тренировки " + _selectedTraining.Name);
            //в окне тоже выводить это

            _selectedTraining.StartDateTime = DateTime.Now;

            var operations = _db.SelectOperationsWithTrainingId(_selectedTraining.Id);

            Parallel.For(0, operations.Count(), (i) => PerformOperationAsync(operations[i]));
        }
        
        private async Task PerformOperationAsync(OperationModel operation)
        {
            //Возможные варианты записываемых значений
            bool valBool;
            float valFloat;
            int valInt;
            ScadaVConnection scadaVConnection = new ScadaVConnection();
            switch (operation.BaseNum)
            {
                case 1:
                    scadaVConnection = scadaVConnection1;
                    break;
                case 2:
                    scadaVConnection = scadaVConnection2;
                    break;
                case 3:
                    scadaVConnection = scadaVConnection3;
                    break;
            }

            if (operation.TimePause.Ticks > 0)
                await Task.Delay(operation.TimePause); //выдержка времени перед операцией

            //Результат записи
            bool result;
            //определяем тип записываемого значения
            if (operation.ValueToWrite.ToLower() == "true")
            {
                valBool = true;
                try
                {
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valBool, DateTime.Now);
                    MyLog.GetInstance().Trace("Запись значения типа BOOL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + ".");
                }
                catch (Exception ex)
                {
                    MyLog.GetInstance().Error(ex.Message);
                }
            }
            else if (operation.ValueToWrite.ToLower() == "false")
            {
                valBool = false;
                try
                {
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valBool, DateTime.Now);
                    MyLog.GetInstance().Trace("Запись значения типа BOOL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + ".");
                }
                catch (Exception ex)
                {
                    MyLog.GetInstance().Error(ex.Message);
                }
            }
            else if (operation.ValueToWrite.Contains('.'))
            {
                valFloat = Convert.ToSingle(operation.ValueToWrite);
                try
                {
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valFloat, DateTime.Now);
                    MyLog.GetInstance().Trace("Запись значения типа REAL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + ".");
                }
                catch (Exception ex)
                {
                    MyLog.GetInstance().Error(ex.Message);
                }
            }
            else
            {
                valInt = Convert.ToInt32(operation.ValueToWrite);
                try
                {
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valInt, DateTime.Now);
                    MyLog.GetInstance().Trace("Запись значения типа INT по тэгу " + +operation.ExitId + ". Прошла ли запись удачно - " + result + ".");
                }
                catch (Exception ex)
                {
                    MyLog.GetInstance().Error(ex.Message);
                }
            }
        }

        public async Task CheckAllAsync()
        {
            _count++;
            //одиночные дискретные сигналы
            for (int i = 0; i < _discretSignals.Count; i++)
                await CheckDiscretSignalAsync(i);

            //двойные дискретные сигналы
            for (int i = 0; i < _doubleDiscretSignals.Count; i++)
                await CheckDoubleDiscretSignalAsync(i);

            //дискретные сигналы, полученные из аналоговых
            for (int i = 0; i < _discretFromAnalogSignals.Count; i++)
                await CheckDiscretFromAnalogSignalAsync(i);

            //группы дискретных сигналов
            for (int i = 0; i < _groupOfDiscretSignals.Count; i++)
                await CheckGroupOfDiscretSignalAsync(i);

            //операции с условием
            for (int i = 0; i < _operationsWithCondition.Count; i++)
                await CheckOperationWithConditionAsync(i);

            _count--;
        }

        private async Task CheckDiscretSignalAsync(int i, bool isFinal = false)
        {
            if (!_discretSignals[i].IsChecked)
            {
                var lv = _db.GetLogicVariableById(_discretSignals[i].LogicVariableId);
                bool value = false;
                try
                {
                    if (!isFinal)
                    {
                        switch (lv.BaseNum)
                        {
                            case 1:
                                value = await scadaVConnection1.ReadDiscretFromServer(lv.ExitId);
                                break;
                            case 2:
                                value = await scadaVConnection2.ReadDiscretFromServer(lv.ExitId);
                                break;
                            case 3:
                                value = await scadaVConnection3.ReadDiscretFromServer(lv.ExitId);
                                break;
                        }
                        _discretSignals[i].DeltaT = _discretSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));
                    }
                    else
                        value = true;

                    if (value)
                    {
                        var borders = _db.SelectTimeBordersWithDiscretId(_discretSignals[i].Id, 0);

                        if (_discretSignals[i].DeltaT.Ticks < borders.T1)
                        {
                            _mark -= borders.Score1;
                            string str = _discretSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + "\n";
                            _logText += str;
                        }

                        if (borders.T2 != null && borders.Score2 != null)
                            if (_discretSignals[i].DeltaT.Ticks >= borders.T1 && _discretSignals[i].DeltaT.Ticks < borders.T2)
                            {
                                _mark -= borders.Score2;
                                string str = _discretSignals[i].Name + " - " + borders.Score2 + " " + getWord(borders.Score2) + "\n";
                                _logText += str;
                            }

                        if (borders.T2 != null && borders.T3 != null && borders.Score3 != null)
                            if (_discretSignals[i].DeltaT.Ticks >= borders.T2 && _discretSignals[i].DeltaT.Ticks < borders.T3)
                            {
                                _mark -= borders.Score3;
                                string str = _discretSignals[i].Name + " - " + borders.Score3 + " " + getWord(borders.Score3) + "\n";
                                _logText += str;
                            }

                        if (borders.T3 != null && borders.T4 != null && borders.Score4 != null)
                            if (_discretSignals[i].DeltaT.Ticks >= borders.T3 && _discretSignals[i].DeltaT.Ticks < borders.T4)
                            {
                                _mark -= borders.Score4;
                                string str = _discretSignals[i].Name + " - " + borders.Score4 + " " + getWord(borders.Score4) + "\n";
                                _logText += str;
                            }

                        if (borders.T4 != null && borders.Score5 != null)
                            if (_discretSignals[i].DeltaT.Ticks >= borders.T4)
                            {
                                _mark -= borders.Score5;
                                string str = _discretSignals[i].Name + " - " + borders.Score5 + " " + getWord(borders.Score5) + "\n";
                                _logText += str;
                            }

                        _discretSignals[i].IsChecked = true;
                        q.Add(new Models.Log { Type = "Trace", Message = "TagId = " + lv.ExitId.ToString() });
                        q.Add(new Models.Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                    }
                    else
                        _discretSignals[i].DeltaT = _discretSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));
                }
                catch (Exception ex)
                {
                    q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                }
            }
        }

        private async Task CheckDoubleDiscretSignalAsync(int i, bool isFinal = false)
        {
            if (!_doubleDiscretSignals[i].IsChecked)
            {
                if (_doubleDiscretSignals[i].StartDate != null || isFinal)
                {
                    try
                    {
                        var lv = _db.GetLogicVariableById(_doubleDiscretSignals[i].LogicVariableId2);
                        bool value = false;

                        if (!isFinal)
                        {
                            switch (lv.BaseNum)
                            {
                                case 1:
                                    value = await scadaVConnection1.ReadDiscretFromServer(lv.ExitId);
                                    break;
                                case 2:
                                    value = await scadaVConnection2.ReadDiscretFromServer(lv.ExitId);
                                    break;
                                case 3:
                                    value = await scadaVConnection3.ReadDiscretFromServer(lv.ExitId);
                                    break;
                            }
                            _doubleDiscretSignals[i].DeltaT = _doubleDiscretSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));
                        }
                        else
                            value = true;

                        if (value)
                        {
                            var borders = _db.SelectTimeBordersWithDiscretId(_doubleDiscretSignals[i].Id, 3);

                            if (_doubleDiscretSignals[i].DeltaT.Ticks < borders.T1)
                            {
                                _mark -= borders.Score1;
                                string str = _doubleDiscretSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + "\n";
                                _logText += str;
                            }

                            if (borders.T2 != null && borders.Score2 != null)
                                if (_doubleDiscretSignals[i].DeltaT.Ticks >= borders.T1 && _doubleDiscretSignals[i].DeltaT.Ticks < borders.T2)
                                {
                                    _mark -= borders.Score2;
                                    string str = _doubleDiscretSignals[i].Name + " - " + borders.Score2 + " " + getWord(borders.Score2) + "\n";
                                    _logText += str;
                                }

                            if (borders.T2 != null && borders.T3 != null && borders.Score3 != null)
                                if (_doubleDiscretSignals[i].DeltaT.Ticks >= borders.T2 && _doubleDiscretSignals[i].DeltaT.Ticks < borders.T3)
                                {
                                    _mark -= borders.Score3;
                                    string str = _doubleDiscretSignals[i].Name + " - " + borders.Score3 + " " + getWord(borders.Score3) + "\n";
                                    _logText += str;
                                }

                            if (borders.T3 != null && borders.T4 != null && borders.Score4 != null)
                                if (_doubleDiscretSignals[i].DeltaT.Ticks >= borders.T3 && _doubleDiscretSignals[i].DeltaT.Ticks < borders.T4)
                                {
                                    _mark -= borders.Score4;
                                    string str = _doubleDiscretSignals[i].Name + " - " + borders.Score4 + " " + getWord(borders.Score4) + "\n";
                                    _logText += str;
                                }

                            if (borders.T4 != null && borders.Score5 != null)
                                if (_doubleDiscretSignals[i].DeltaT.Ticks >= borders.T4)
                                {
                                    _mark -= borders.Score5;
                                    string str = _doubleDiscretSignals[i].Name + " - " + borders.Score5 + " " + getWord(borders.Score5) + "\n";
                                    _logText += str;
                                }

                            _doubleDiscretSignals[i].IsChecked = true;
                            q.Add(new Models.Log { Type = "Trace", Message = "TagId = " + lv.ExitId.ToString() });
                            q.Add(new Models.Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                        }
                    }
                    catch (Exception ex)
                    {
                        q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                    }
                }
                else
                {
                    var lv = _db.GetLogicVariableById(_doubleDiscretSignals[i].LogicVariableId1);
                    bool value = false;
                    try
                    {
                        switch (lv.BaseNum)
                        {
                            case 1:
                                value = await scadaVConnection1.ReadDiscretFromServer(lv.ExitId);
                                break;
                            case 2:
                                value = await scadaVConnection2.ReadDiscretFromServer(lv.ExitId);
                                break;
                            case 3:
                                value = await scadaVConnection3.ReadDiscretFromServer(lv.ExitId);
                                break;
                        }
                        if (value)
                            _doubleDiscretSignals[i].StartDate = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                    }
                }
            }
        }

        private async Task CheckDiscretFromAnalogSignalAsync(int i, bool isFinal = false)
        {
            if (!_discretFromAnalogSignals[i].IsChecked)
            {
                try
                {
                    bool value = false;

                    if (!isFinal)
                    {
                        ScadaVConnection scadaVConnection = new ScadaVConnection();
                        switch (_discretFromAnalogSignals[i].BaseNum)
                        {
                            case 1:
                                scadaVConnection = scadaVConnection1;
                                break;
                            case 2:
                                scadaVConnection = scadaVConnection2;
                                break;
                            case 3:
                                scadaVConnection = scadaVConnection3;
                                break;
                        }
                        var res = await scadaVConnection.ReadVariableFromServer(_discretFromAnalogSignals[i].ExitId);

                        if (_discretFromAnalogSignals[i].Const.ToString().Contains('.'))
                        {
                            if (Convert.ToInt32(res.Value) == _discretFromAnalogSignals[i].Const)
                                value = true;
                        }
                        else
                        {
                            if (Convert.ToSingle(res.Value) == _discretFromAnalogSignals[i].Const)
                                value = true;
                        }
                        _discretFromAnalogSignals[i].DeltaT = _discretFromAnalogSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));
                    }
                    else
                        value = true;

                    if (value)
                    {
                        var borders = _db.SelectTimeBordersWithDiscretId(_discretFromAnalogSignals[i].Id, 1);

                        if (_discretFromAnalogSignals[i].DeltaT.Ticks < borders.T1)
                        {
                            _mark -= borders.Score1;
                            string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + "\n";
                            _logText += str;
                        }

                        if (borders.T2 != null && borders.Score2 != null)
                            if (_discretFromAnalogSignals[i].DeltaT.Ticks >= borders.T1 && _discretFromAnalogSignals[i].DeltaT.Ticks < borders.T2)
                            {
                                _mark -= borders.Score2;
                                string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score2 + " " + getWord(borders.Score2) + "\n";
                                _logText += str;
                            }

                        if (borders.T2 != null && borders.T3 != null && borders.Score3 != null)
                            if (_discretFromAnalogSignals[i].DeltaT.Ticks >= borders.T2 && _discretFromAnalogSignals[i].DeltaT.Ticks < borders.T3)
                            {
                                _mark -= borders.Score3;
                                string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score3 + " " + getWord(borders.Score3) + "\n";
                                _logText += str;
                            }

                        if (borders.T3 != null && borders.T4 != null && borders.Score4 != null)
                            if (_discretFromAnalogSignals[i].DeltaT.Ticks >= borders.T3 && _discretFromAnalogSignals[i].DeltaT.Ticks < borders.T4)
                            {
                                _mark -= borders.Score4;
                                string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score4 + " " + getWord(borders.Score4) + "\n";
                                _logText += str;
                            }

                        if (borders.T4 != null && borders.Score5 != null)
                            if (_discretFromAnalogSignals[i].DeltaT.Ticks >= borders.T4)
                            {
                                _mark -= borders.Score5;
                                string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score5 + " " + getWord(borders.Score5) + "\n";
                                _logText += str;
                            }

                        _discretFromAnalogSignals[i].IsChecked = true;
                        q.Add(new Models.Log { Type = "Trace", Message = "TagId = " + _discretFromAnalogSignals[i].ExitId.ToString() });
                        q.Add(new Models.Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                    }
                    else
                        _discretFromAnalogSignals[i].DeltaT = _discretFromAnalogSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));
                }
                catch (Exception ex)
                {
                    q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                }
            }
        }

        private async Task CheckGroupOfDiscretSignalAsync(int i, bool isFinal = false)
        {
            if (!_groupOfDiscretSignals[i].IsChecked)
            {
                if (_groupOfDiscretSignals[i].StartDate != null || isFinal)
                {
                    if (!isFinal)
                    {
                        for (int j = 0; j < _groupOfDiscretSignals[i].EndLogicVariables.Count; j++)
                        {
                            ScadaVConnection scadaVConnection = new ScadaVConnection();
                            switch (_groupOfDiscretSignals[i].EndLogicVariables[j].BaseNum)
                            {
                                case 1:
                                    scadaVConnection = scadaVConnection1;
                                    break;
                                case 2:
                                    scadaVConnection = scadaVConnection2;
                                    break;
                                case 3:
                                    scadaVConnection = scadaVConnection3;
                                    break;
                            }

                            try
                            {
                                if (!_groupOfDiscretSignals[i].EndLogicVariables[j].IsChecked)
                                {
                                    bool value = await scadaVConnection.ReadDiscretFromServer(_groupOfDiscretSignals[i].EndLogicVariables[j].ExitId);
                                    if (value)
                                        _groupOfDiscretSignals[i].EndLogicVariables[j].IsChecked = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                            }
                        }

                        for (int j = 0; j < _groupOfDiscretSignals[i].EndSignals.Count; j++)
                        {
                            if (!_groupOfDiscretSignals[i].EndSignals[j].IsChecked)
                            {
                                try
                                {
                                    ScadaVConnection scadaVConnection = new ScadaVConnection();
                                    switch (_groupOfDiscretSignals[i].EndSignals[j].BaseNum)
                                    {
                                        case 1:
                                            scadaVConnection = scadaVConnection1;
                                            break;
                                        case 2:
                                            scadaVConnection = scadaVConnection2;
                                            break;
                                        case 3:
                                            scadaVConnection = scadaVConnection3;
                                            break;
                                    }

                                    var res = await scadaVConnection.ReadVariableFromServer(_groupOfDiscretSignals[i].EndSignals[j].ExitId);
                                    bool value = false;

                                    if (_groupOfDiscretSignals[i].EndSignals[j].Const.ToString().Contains('.'))
                                    {
                                        if (Convert.ToInt32(res.Value) == _groupOfDiscretSignals[i].EndSignals[j].Const)
                                            value = true;
                                    }
                                    else
                                    {
                                        if (Convert.ToSingle(res.Value) == _groupOfDiscretSignals[i].EndSignals[j].Const)
                                            value = true;
                                    }

                                    if (value)
                                        _groupOfDiscretSignals[i].EndSignals[j].IsChecked = true;
                                }
                                catch (Exception ex)
                                {
                                    q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                                }
                            }
                        }
                    }

                    if (_groupOfDiscretSignals[i].EndLogicVariables.Where(x => x.IsChecked).Count() == _groupOfDiscretSignals[i].EndLogicVariables.Count
                        && _groupOfDiscretSignals[i].EndSignals.Where(x => x.IsChecked).Count() == _groupOfDiscretSignals[i].EndSignals.Count || isFinal)
                    {
                        if (!isFinal)
                            _groupOfDiscretSignals[i].DeltaT = _groupOfDiscretSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));

                        var borders = _db.SelectTimeBordersWithDiscretId(_groupOfDiscretSignals[i].Id, 2);

                        if (_groupOfDiscretSignals[i].DeltaT.Ticks < borders.T1)
                        {
                            _mark -= borders.Score1;
                            string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + "\n";
                            _logText += str;
                        }

                        if (borders.T2 != null && borders.Score2 != null)
                            if (_groupOfDiscretSignals[i].DeltaT.Ticks >= borders.T1 && _groupOfDiscretSignals[i].DeltaT.Ticks < borders.T2)
                            {
                                _mark -= borders.Score2;
                                string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score2 + " " + getWord(borders.Score2) + "\n";
                                _logText += str;
                            }

                        if (borders.T2 != null && borders.T3 != null && borders.Score3 != null)
                            if (_groupOfDiscretSignals[i].DeltaT.Ticks >= borders.T2 && _groupOfDiscretSignals[i].DeltaT.Ticks < borders.T3)
                            {
                                _mark -= borders.Score3;
                                string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score3 + " " + getWord(borders.Score3) + "\n";
                                _logText += str;
                            }

                        if (borders.T3 != null && borders.T4 != null && borders.Score4 != null)
                            if (_groupOfDiscretSignals[i].DeltaT.Ticks >= borders.T3 && _groupOfDiscretSignals[i].DeltaT.Ticks < borders.T4)
                            {
                                _mark -= borders.Score4;
                                string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score4 + " " + getWord(borders.Score4) + "\n";
                                _logText += str;
                            }

                        if (borders.T4 != null && borders.Score5 != null)
                            if (_groupOfDiscretSignals[i].DeltaT.Ticks >= borders.T4)
                            {
                                _mark -= borders.Score5;
                                string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score5 + " " + getWord(borders.Score5) + "\n";
                                _logText += str;
                            }

                        _groupOfDiscretSignals[i].IsChecked = true;
                        q.Add(new Models.Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                    }
                    else
                        _groupOfDiscretSignals[i].DeltaT = _groupOfDiscretSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));
                }
                else
                {
                    for (int j = 0; j < _groupOfDiscretSignals[i].StartLogicVariables.Count; j++)
                    {
                        if (!_groupOfDiscretSignals[i].StartLogicVariables[j].IsChecked)
                        {
                            ScadaVConnection scadaVConnection = new ScadaVConnection();
                            switch (_groupOfDiscretSignals[i].StartLogicVariables[j].BaseNum)
                            {
                                case 1:
                                    scadaVConnection = scadaVConnection1;
                                    break;
                                case 2:
                                    scadaVConnection = scadaVConnection2;
                                    break;
                                case 3:
                                    scadaVConnection = scadaVConnection3;
                                    break;
                            }

                            try
                            {
                                bool value = await scadaVConnection.ReadDiscretFromServer(_groupOfDiscretSignals[i].StartLogicVariables[j].ExitId);
                                if (value)
                                    _groupOfDiscretSignals[i].StartLogicVariables[j].IsChecked = true;
                            }
                            catch (Exception ex)
                            {
                                q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                            }
                        }
                    }

                    for (int j = 0; j < _groupOfDiscretSignals[i].StartSignals.Count; j++)
                    {
                        if (!_groupOfDiscretSignals[i].StartSignals[j].IsChecked)
                        {
                            try
                            {
                                ScadaVConnection scadaVConnection = new ScadaVConnection();
                                switch (_groupOfDiscretSignals[i].StartSignals[j].BaseNum)
                                {
                                    case 1:
                                        scadaVConnection = scadaVConnection1;
                                        break;
                                    case 2:
                                        scadaVConnection = scadaVConnection2;
                                        break;
                                    case 3:
                                        scadaVConnection = scadaVConnection3;
                                        break;
                                }

                                var res = await scadaVConnection.ReadVariableFromServer(_groupOfDiscretSignals[i].StartSignals[j].ExitId);
                                bool value = false;

                                if (_groupOfDiscretSignals[i].StartSignals[j].Const.ToString().Contains('.'))
                                {
                                    if (Convert.ToInt32(res.Value) == _groupOfDiscretSignals[i].StartSignals[j].Const)
                                        value = true;
                                }
                                else
                                {
                                    if (Convert.ToSingle(res.Value) == _groupOfDiscretSignals[i].StartSignals[j].Const)
                                        value = true;
                                }

                                if (value)
                                    _groupOfDiscretSignals[i].StartSignals[j].IsChecked = true;
                            }
                            catch (Exception ex)
                            {
                                q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                            }
                        }
                    }

                    if (_groupOfDiscretSignals[i].StartLogicVariables.Where(x => x.IsChecked).Count() == _groupOfDiscretSignals[i].StartLogicVariables.Count
                        && _groupOfDiscretSignals[i].StartSignals.Where(x => x.IsChecked).Count() == _groupOfDiscretSignals[i].StartSignals.Count)
                    {
                        _groupOfDiscretSignals[i].StartDate = DateTime.Now;
                    }
                }
            }
        }

        private async Task CheckOperationWithConditionAsync(int i, bool isFinal = false)
        {
            if (!_operationsWithCondition[i].IsChecked)
            {
                try
                {
                    ScadaVConnection scadaV2Connection = new ScadaVConnection();
                    switch (_operationsWithCondition[i].Base2Num)
                    {
                        case 1:
                            scadaV2Connection = scadaVConnection1;
                            break;
                        case 2:
                            scadaV2Connection = scadaVConnection2;
                            break;
                        case 3:
                            scadaV2Connection = scadaVConnection3;
                            break;
                    }

                    ScadaVConnection scadaVConnection = new ScadaVConnection();
                    switch (_operationsWithCondition[i].Base1Num)
                    {
                        case 1:
                            scadaVConnection = scadaVConnection1;
                            break;
                        case 2:
                            scadaVConnection = scadaVConnection2;
                            break;
                        case 3:
                            scadaVConnection = scadaVConnection3;
                            break;
                    }
                    bool value = await scadaV2Connection.ReadDiscretFromServer(_operationsWithCondition[i].ConditionExitId);
                    if (value)
                    {
                        if (_operationsWithCondition[i].TimePause.Ticks > 0)
                            Thread.Sleep(_operationsWithCondition[i].TimePause); //выдержка времени перед операцией

                        //Возможные варианты записываемых значений
                        bool valBool;
                        float valFloat;
                        int valInt;

                        //Результат записи
                        bool result = false;

                        //выполнение операции
                        //определяем тип записываемого значения
                        if (_operationsWithCondition[i].ValueToWrite.ToLower() == "true")
                        {
                            valBool = true;
                            try
                            {
                                result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valBool, DateTime.Now);
                                q.Add(new Models.Log { Type = "Trace", Message = "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                            }
                            catch (Exception ex)
                            {
                                q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                            }
                        }
                        else if (_operationsWithCondition[i].ValueToWrite.ToLower() == "false")
                        {
                            valBool = false;
                            try
                            {
                                result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valBool, DateTime.Now);
                                q.Add(new Models.Log { Type = "Trace", Message = "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                            }
                            catch (Exception ex)
                            {
                                q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                            }
                        }
                        else if (_operationsWithCondition[i].ValueToWrite.Contains('.'))
                        {
                            valFloat = Convert.ToSingle(_operationsWithCondition[i].ValueToWrite);
                            try
                            {
                                result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valFloat, DateTime.Now);
                                q.Add(new Models.Log { Type = "Trace", Message = "Запись значения типа REAL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                            }
                            catch (Exception ex)
                            {
                                q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                            }
                        }
                        else
                        {
                            valInt = Convert.ToInt32(_operationsWithCondition[i].ValueToWrite);
                            try
                            {
                                result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valInt, DateTime.Now);
                                q.Add(new Models.Log { Type = "Trace", Message = "Запись значения типа INT по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                            }
                            catch (Exception ex)
                            {
                                q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                            }
                        }

                        _operationsWithCondition[i].IsChecked = true;
                    }
                }
                catch (Exception ex)
                {
                    q.Add(new Models.Log { Type = "Error", Message = ex.Message });
                }
            }
        }

        public async void HandlerEndTrainingAsync()
        {
            if (_selectedTraining != null)
            {
                //TimerManager.Stop();
                DateTime endTime = DateTime.Now;
                string str;

                //одиночные дискретные сигналы
                for (int i = 0; i < _discretSignals.Count; i++)
                    await CheckDiscretSignalAsync(i, true);

                //двойные дискретные сигналы
                for (int i = 0; i < _doubleDiscretSignals.Count; i++)
                    await CheckDoubleDiscretSignalAsync(i, true);

                //дискретные сигналы, полученные из аналоговых
                for (int i = 0; i < _discretFromAnalogSignals.Count; i++)
                    await CheckDiscretFromAnalogSignalAsync(i, true);

                //группы дискретных сигналов
                for (int i = 0; i < _groupOfDiscretSignals.Count; i++)
                    await CheckGroupOfDiscretSignalAsync(i, true);

                List<AnalogSignalMark> marks = new List<AnalogSignalMark>();

                _criteriasForReport1 = new List<string>();
                _criteriasForReport2 = new List<string>();

                //основные критерии надёжности пуска и останова
                List<AnalogSignalModel> analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 0);
                for (int i = 0; i < analogSignals.Count; i++)
                {
                    ScadaVConnection scadaVConnection = new ScadaVConnection();
                    switch (analogSignals[i].BaseNum)
                    {
                        case 1:
                            scadaVConnection = scadaVConnection1;
                            break;
                        case 2:
                            scadaVConnection = scadaVConnection2;
                            break;
                        case 3:
                            scadaVConnection = scadaVConnection3;
                            break;
                    }

                    if (analogSignals[i].Func == "Диапазон")
                    {
                        var f = _db.SelectRange(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.Diapazon(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Left, (float)f.Right, f.AbsValues == 1 ? true : false));
                    }
                    else if (analogSignals[i].Func == "Настраиваемый диапазон")
                    {
                        var f = _db.SelectAdjustableRange(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.CustomizableDiapazon(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Left, (float)f.Right, f.ExitId));
                    }
                    else if (analogSignals[i].Func == "Диапазон с параметрами")
                    {
                        var f = _db.SelectRangeWithParameters(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.DiapazonWithParams(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Left, (float)f.Right1, (float)f.Right2, (float)f.ParamVal1, (float)f.ParamVal2, f.ExitId));
                    }
                    else if (analogSignals[i].Func == "Суммарное или неоднократное превышение уставки")
                    {
                        var f = _db.SelectExceeding(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.TotalOrRepeatedExceeding(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Ustavka, new TimeSpan(f.SummTime), (float)f.Prev));
                    }
                    str = "Аналоговый сигнал " + analogSignals[i].Name + " оценён на " + marks.Last().ToString() + "\n";
                    _criteriasForReport2.Add(str);
                    _logText2 += str;
                }

                //дополнительные критерии надёжности пуска и останова
                analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 1);
                for (int i = 0; i < analogSignals.Count; i++)
                {
                    ScadaVConnection scadaVConnection = new ScadaVConnection();
                    switch (analogSignals[i].BaseNum)
                    {
                        case 1:
                            scadaVConnection = scadaVConnection1;
                            break;
                        case 2:
                            scadaVConnection = scadaVConnection2;
                            break;
                        case 3:
                            scadaVConnection = scadaVConnection3;
                            break;
                    }

                    if (analogSignals[i].Func == "Диапазон")
                    {
                        var f = _db.SelectDopRange(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.DopDiapazon(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.OtlBorder, (float)f.XorBorder, (float)f.NeydBorder));
                    }
                    else if (analogSignals[i].Func == "Поддержание заданного уровня")
                    {
                        var f = _db.SelectMaintainingLevel(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.DopAbsDiapazon(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Ustavka, (float)f.OtlBorder, (float)f.NeydBorder));
                    }
                    str = "Аналоговый сигнал " + analogSignals[i].Name + " оценён на " + marks.Last().ToString() + "\n";
                    _criteriasForReport2.Add(str);
                    _logText2 += str;
                }

                //основные критерии оценки качества пуска и останова
                analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 2);
                for (int i = 0; i < analogSignals.Count; i++)
                {
                    ScadaVConnection scadaVConnection = new ScadaVConnection();
                    switch (analogSignals[i].BaseNum)
                    {
                        case 1:
                            scadaVConnection = scadaVConnection1;
                            break;
                        case 2:
                            scadaVConnection = scadaVConnection2;
                            break;
                        case 3:
                            scadaVConnection = scadaVConnection3;
                            break;
                    }

                    if (analogSignals[i].Func == "Диапазон")
                    {
                        var f = _db.SelectRange(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.Diapazon(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Left, (float)f.Right, f.AbsValues == 1 ? true : false));
                    }
                    else if (analogSignals[i].Func == "Настраиваемый диапазон")
                    {
                        var f = _db.SelectAdjustableRange(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.CustomizableDiapazon(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Left, (float)f.Right, f.ExitId));
                    }
                    else if (analogSignals[i].Func == "Диапазон с параметрами")
                    {
                        var f = _db.SelectRangeWithParameters(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.DiapazonWithParams(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Left, (float)f.Right1, (float)f.Right2, (float)f.ParamVal1, (float)f.ParamVal2, f.ExitId));
                    }
                    else if (analogSignals[i].Func == "Суммарное или неоднократное превышение уставки")
                    {
                        var f = _db.SelectExceeding(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.TotalOrRepeatedExceeding(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Ustavka, new TimeSpan(f.SummTime), (float)f.Prev));
                    }
                    str = "Аналоговый сигнал " + analogSignals[i].Name + " оценён на " + marks.Last().ToString() + "\n";
                    _criteriasForReport2.Add(str);
                    _logText2 += str;
                }

                //дополнительные критерии оценки качества пуска и останова
                analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 3);
                for (int i = 0; i < analogSignals.Count; i++)
                {
                    ScadaVConnection scadaVConnection = new ScadaVConnection();
                    switch (analogSignals[i].BaseNum)
                    {
                        case 1:
                            scadaVConnection = scadaVConnection1;
                            break;
                        case 2:
                            scadaVConnection = scadaVConnection2;
                            break;
                        case 3:
                            scadaVConnection = scadaVConnection3;
                            break;
                    }

                    if (analogSignals[i].Func == "Диапазон")
                    {
                        var f = _db.SelectDopRange(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.DopDiapazon(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.OtlBorder, (float)f.XorBorder, (float)f.NeydBorder));
                    }
                    else if (analogSignals[i].Func == "Поддержание заданного уровня")
                    {
                        var f = _db.SelectMaintainingLevel(analogSignals[i].Id);
                        marks.Add(await scadaVConnection.DopAbsDiapazon(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Ustavka, (float)f.OtlBorder, (float)f.NeydBorder));
                    }
                    str = "Аналоговый сигнал " + analogSignals[i].Name + " оценён на " + marks.Last().ToString() + "\n";
                    _criteriasForReport2.Add(str);
                    _logText2 += str;
                }

                if (marks.Count != 0)
                {
                    int neyd = marks.Where(x => x == AnalogSignalMark.NeYd).Count();
                    int yd = marks.Where(x => x == AnalogSignalMark.Yd).Count();
                    int xor = marks.Where(x => x == AnalogSignalMark.Xor).Count();
                    int otl = marks.Where(x => x == AnalogSignalMark.Otl).Count();

                    string mark = "удовлетворительно";

                    if (neyd * 100 / marks.Count >= 5)
                        mark = "неудовлетворительно";
                    else if (yd * 100 / marks.Count < 20 && neyd == 0)
                        mark = "хорошо";
                    else if (xor * 100 / marks.Count < 10 && yd == 0 && neyd == 0)
                        mark = "отлично";

                    _endMark = mark;
                    str = "Итоговая оценка - " + mark + "\n";
                    _logText2 += str;
                }

                //аналоговые сигналы для противоаварийных тренировок
                analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 4);
                for (int i = 0; i < analogSignals.Count; i++)
                {
                    ScadaVConnection scadaVConnection = new ScadaVConnection();
                    switch (analogSignals[i].BaseNum)
                    {
                        case 1:
                            scadaVConnection = scadaVConnection1;
                            break;
                        case 2:
                            scadaVConnection = scadaVConnection2;
                            break;
                        case 3:
                            scadaVConnection = scadaVConnection3;
                            break;
                    }

                    if (analogSignals[i].Func == "Время пребывания в интервале")
                    {
                        var f = _db.SelectTimeInInterval(analogSignals[i].Id);
                        var time = await scadaVConnection.TimeInIntervalFloat(analogSignals[i].ExitId, (float)f.Bottom, (float)f.Top, (DateTime)_selectedTraining.StartDateTime, endTime);
                        bool flag = false;
                        if (f.Sign == ">" && time > f.Ustavka)
                            flag = true;
                        if (f.Sign == ">=" && time >= f.Ustavka)
                            flag = true;
                        if (f.Sign == "<" && time < f.Ustavka)
                            flag = true;
                        if (f.Sign == "<=" && time <= f.Ustavka)
                            flag = true;
                        if (f.Sign == "=" && time == f.Ustavka)
                            flag = true;
                        if (f.Sign == "!=" && time != f.Ustavka)
                            flag = true;

                        if (!flag)
                        {
                            _mark -= f.Score;
                            str = analogSignals[i].Name + " - " + f.Score + " " + getWord(f.Score) + "\n";
                            _criteriasForReport1.Add(str);
                            _logText2 += str;
                        }
                    }
                    else if (analogSignals[i].Func == "Наличие выхода за коридор")
                    {
                        var f = _db.SelectExitToTheCorridor(analogSignals[i].Id);
                        var res = await scadaVConnection.ValueOutOfBorders(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Bottom, (float)f.Top);

                        if (!res)
                        {
                            _mark -= f.Score;
                            str = analogSignals[i].Name + " - " + f.Score + " " + getWord(f.Score) + "\n";
                            _criteriasForReport1.Add(str);
                            _logText2 += str;
                        }
                    }
                }
            }

            while (true)
                if (_count == 0)
                    break;

            q.CompleteAdding();
            thread.Join();
        }
    }
}
