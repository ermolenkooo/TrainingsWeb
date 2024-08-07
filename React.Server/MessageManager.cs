﻿using System.Net.WebSockets;
using System.Text;
using BLL;
using BLL.Models;
using BLL.Operations;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using Grpc.Core;
using Microsoft.Extensions.Options;
using DAL.Entities;
using System;

namespace React.Server
{
    public class MessageManager
    {
        private IHubContext<MessageHub> _hubContext;
        private TrainingDbOperations _db;
        private ScadaVConnection scadaVConnection1;
        private ScadaVConnection scadaVConnection2;
        private ScadaVConnection scadaVConnection3;

        public string StartFunctionName { get; set; } = "Start";
        public string ReceiveFunctionName { get; set; } = "Receive";
        public string Receive2FunctionName { get; set; } = "Receive2";
        public string ReceiveMarkFunctionName { get; set; } = "ReceiveMark";
        public string ReceiveStatusFunctionName { get; set; } = "ReceiveStatus";
        public string TrainingIsEndFunctionName { get; set; } = "TrainingIsEnd";

        public MessageManager()
        {

        }

        public void SetSettings(IHubContext<MessageHub> hubContext, MyOptions options, bool isRemoved)
        {
            StatusTraining = 0;
            _hubContext = hubContext;
            _db = new TrainingDbOperations(options);
            scadaVConnection1 = options.scadaVConnection1;
            scadaVConnection2 = options.scadaVConnection2;
            scadaVConnection3 = options.scadaVConnection3;

            if (isRemoved)
            {
                StartFunctionName = "StartRemoved";
                ReceiveFunctionName = "ReceiveRemoved";
                Receive2FunctionName = "Receive2Removed";
                ReceiveMarkFunctionName = "ReceiveMarkRemoved";
                ReceiveStatusFunctionName = "ReceiveStatusRemoved";
                TrainingIsEndFunctionName = "TrainingIsEndRemoved";
            }
            else
            {
                StartFunctionName = "Start";
                ReceiveFunctionName = "Receive";
                Receive2FunctionName = "Receive2";
                ReceiveMarkFunctionName = "ReceiveMark";
                ReceiveStatusFunctionName = "ReceiveStatus";
                TrainingIsEndFunctionName = "TrainingIsEnd";
            }
        }

        public async Task StartConnection(int id)
        {
            await BeginScenary(id);
            while (StatusTraining != 2)
                continue;
        }

        #region Выполнение сценария
        public int StatusTraining { get; set; } 
        private TrainingModel _selectedTraining;
        private int _count;
        private int? _mark;
        private string _endMark;
        private List<string> _criteriasForReport1;
        private List<string> _criteriasForReport2;

        private List<DiscretSignalModel> _discretSignals;
        private List<DoubleDiscretSignalModel> _doubleDiscretSignals;
        private List<DiscretFromAnalogSignalModel> _discretFromAnalogSignals;
        private List<GroupOfDiscretSignalsModel> _groupOfDiscretSignals;
        private List<OperationWithConditionModel> _operationsWithCondition;

        BlockingCollection<Log> q;
        Thread thread;

        public async Task BeginScenary(int id)
        {
            tasks = new List<Task>();
            isExepcion = false;

            await _hubContext.Clients.All.SendAsync(StartFunctionName);
            await _hubContext.Clients.All.SendAsync(ReceiveStatusFunctionName, "Начата");

            q = new BlockingCollection<Log>();
            thread = new Thread(Consumer);
            thread.Start();

            _count = 0;
            _selectedTraining = _db.SelectTrainingById(id);
            _mark = _selectedTraining.Mark;
            await PerformSelectedTrainingOperations();

            await LoadDataAsync();

            TimerManager.TimerTick += async () => await CheckAllAsync();
            TimerManager.Start();
        }

        public bool CheckTraining(int id)
        {
            return _db.SelectTrainingById(id).Id != 0 ? true : false;
        }

        private async Task PerformSelectedTrainingOperations()
        {
            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Начинаю запись сценария тренировки " + _selectedTraining.Name + " - " + DateTime.Now.ToString("G"));

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
                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Запись значения типа BOOL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);
                }
            }
            else if (operation.ValueToWrite.ToLower() == "false")
            {
                valBool = false;
                try
                {
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valBool, DateTime.Now);
                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Запись значения типа BOOL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);
                }
            }
            else if (operation.ValueToWrite.Contains('.') || operation.ValueToWrite.Contains(','))
            {
                operation.ValueToWrite = operation.ValueToWrite.Replace('.', ',');
                valFloat = float.Parse(operation.ValueToWrite);
                try
                {
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valFloat, DateTime.Now);
                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Запись значения типа REAL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);
                }
            }
            else
            {
                valInt = Convert.ToInt32(operation.ValueToWrite);
                try
                {
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valInt, DateTime.Now);
                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Запись значения типа INT по тэгу " + +operation.ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);
                }
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

        List<Task> tasks;
        bool isExepcion;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public async Task CheckAllAsync()
        {
            //_count++;
            //одиночные дискретные сигналы
            for (int i = 0; i < _discretSignals.Count; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        await CheckDiscretSignalAsync(index);
                    }
                    catch (TaskCanceledException)
                    {
                        q.Add(new Log { Type = "Info", Message = $"Задача {index} отменена." });
                    }
                    catch (Exception ex)
                    {
                        //_count--;
                        if (!isExepcion)
                        {
                            isExepcion = true;
                            TimerManager.Stop();
                            _cancellationTokenSource.Cancel();

                            q.Add(new Log { Type = "Error", Message = ex.Message });
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);

                            await Task.WhenAll(tasks);
                            await TrainingEnd(false);
                            //while (true)
                            //    if (_count == 0)
                            //    {
                            //        await TrainingEnd(false);
                            //        break;
                            //    }
                        }
                    }
                }, _cancellationTokenSource.Token));
            }
            //tasks.Add(CheckDiscretSignalAsync(i));
            //await CheckDiscretSignalAsync(i);

            //двойные дискретные сигналы
            for (int i = 0; i < _doubleDiscretSignals.Count; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        await CheckDoubleDiscretSignalAsync(index);
                    }
                    catch (TaskCanceledException)
                    {
                        q.Add(new Log { Type = "Info", Message = $"Задача {index} отменена." });
                    }
                    catch (Exception ex)
                    {
                        //_count--;
                        if (!isExepcion)
                        {
                            isExepcion = true;
                            TimerManager.Stop();
                            _cancellationTokenSource.Cancel();

                            q.Add(new Log { Type = "Error", Message = ex.Message });
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);

                            await Task.WhenAll(tasks);
                            await TrainingEnd(false);
                            //while (true)
                            //    if (_count == 0)
                            //    {
                            //        await TrainingEnd(false);
                            //        break;
                            //    }
                        }
                    }
                }));
            }
            //await CheckDoubleDiscretSignalAsync(i);

            //дискретные сигналы, полученные из аналоговых
            for (int i = 0; i < _discretFromAnalogSignals.Count; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        await CheckDiscretFromAnalogSignalAsync(index);
                    }
                    catch (TaskCanceledException)
                    {
                        q.Add(new Log { Type = "Info", Message = $"Задача {index} отменена." });
                    }
                    catch (Exception ex)
                    {
                        //_count--;
                        if (!isExepcion)
                        {
                            isExepcion = true;
                            TimerManager.Stop();
                            _cancellationTokenSource.Cancel();

                            q.Add(new Log { Type = "Error", Message = ex.Message });
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);

                            await Task.WhenAll(tasks);
                            await TrainingEnd(false);
                            //while (true)
                            //    if (_count == 0)
                            //    {
                            //        await TrainingEnd(false);
                            //        break;
                            //    }
                        }
                    }
                }));
            }
            //await CheckDiscretFromAnalogSignalAsync(i);

            //группы дискретных сигналов
            for (int i = 0; i < _groupOfDiscretSignals.Count; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        await CheckGroupOfDiscretSignalAsync(index);
                    }
                    catch (TaskCanceledException)
                    {
                        q.Add(new Log { Type = "Info", Message = $"Задача {index} отменена." });
                    }
                    catch (Exception ex)
                    {
                        //_count--;
                        if (!isExepcion)
                        {
                            isExepcion = true;
                            TimerManager.Stop();
                            _cancellationTokenSource.Cancel();

                            q.Add(new Log { Type = "Error", Message = ex.Message });
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);

                            await Task.WhenAll(tasks);
                            await TrainingEnd(false);
                            //while (true)
                            //    if (_count == 0)
                            //    {
                            //        await TrainingEnd(false);
                            //        break;
                            //    }
                        }
                    }
                }));
            }
            //await CheckGroupOfDiscretSignalAsync(i);

            //операции с условием
            for (int i = 0; i < _operationsWithCondition.Count; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        await CheckOperationWithConditionAsync(index);
                    }
                    catch (TaskCanceledException)
                    {
                        q.Add(new Log { Type = "Info", Message = $"Задача {index} отменена." });
                    }
                    catch (Exception ex)
                    {
                        //_count--;
                        if (!isExepcion)
                        {
                            isExepcion = true;
                            TimerManager.Stop();
                            _cancellationTokenSource.Cancel();

                            q.Add(new Log { Type = "Error", Message = ex.Message });
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);

                            await Task.WhenAll(tasks);
                            await TrainingEnd(false);
                            //while (true)
                            //    if (_count == 0)
                            //    {
                            //        await TrainingEnd(false);
                            //        break;
                            //    }
                        }
                    }
                }));
            }
            //await CheckOperationWithConditionAsync(i);

            //_count--;
        }

        private async Task CheckDiscretSignalAsync(int i)
        {
            //throw new Exception("dhfffffffffdjk");
            if (!_discretSignals[i].IsChecked)
            {
                _count++;
                var lv = _db.GetLogicVariableById(_discretSignals[i].LogicVariableId);
                bool value = false;
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

                if (value)
                {
                    var borders = _db.SelectTimeBordersWithDiscretId(_discretSignals[i].Id, 0);

                    if (_discretSignals[i].DeltaT.Ticks < borders.T1 && !_discretSignals[i].Tags[0])
                    {
                        _mark -= borders.Score1;
                        _discretSignals[i].Tags[0] = true;
                        if (borders.Score1 != 0) 
                        {
                            string str = _discretSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + " - " + DateTime.Now.ToString("T");
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                            await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка");
                        }
                    }

                    if (borders.T2 != null && borders.Score2 != null)
                        if (_discretSignals[i].DeltaT.Ticks >= borders.T1 && _discretSignals[i].DeltaT.Ticks < borders.T2 && !_discretSignals[i].Tags[1])
                        {
                            _mark -= borders.Score2;
                            _discretSignals[i].Tags[1] = true;
                            if (borders.Score2 != 0)
                            {
                                string str = _discretSignals[i].Name + " - " + borders.Score2 + " " + getWord(borders.Score2) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (borders.T2 != null && borders.T3 != null && borders.Score3 != null)
                        if (_discretSignals[i].DeltaT.Ticks >= borders.T2 && _discretSignals[i].DeltaT.Ticks < borders.T3 && !_discretSignals[i].Tags[2])
                        {
                            _mark -= borders.Score3;
                            _discretSignals[i].Tags[2] = true;
                            if (borders.Score3 != 0)
                            {
                                string str = _discretSignals[i].Name + " - " + borders.Score3 + " " + getWord(borders.Score3) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (borders.T3 != null && borders.T4 != null && borders.Score4 != null)
                        if (_discretSignals[i].DeltaT.Ticks >= borders.T3 && _discretSignals[i].DeltaT.Ticks < borders.T4 && !_discretSignals[i].Tags[3])
                        {
                            _mark -= borders.Score4;
                            _discretSignals[i].Tags[3] = true;
                            if (borders.Score4 != 0)
                            {
                                string str = _discretSignals[i].Name + " - " + borders.Score4 + " " + getWord(borders.Score4) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (borders.T4 != null && borders.Score5 != null)
                        if (_discretSignals[i].DeltaT.Ticks >= borders.T4 && !_discretSignals[i].Tags[4])
                        {
                            _mark -= borders.Score5;
                            _discretSignals[i].Tags[4] = true;
                            if (borders.Score5 != 0)
                            {
                                string str = _discretSignals[i].Name + " - " + borders.Score5 + " " + getWord(borders.Score5) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (_discretSignals[i].Tags.All(value => value))
                        _discretSignals[i].IsChecked = true;
                    q.Add(new Log { Type = "Trace", Message = "TagId = " + lv.ExitId.ToString() });
                    q.Add(new Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                }
                else
                    _discretSignals[i].DeltaT = _discretSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));
                _count--;
            }
        }

        private async Task CheckDoubleDiscretSignalAsync(int i)
        {
            if (!_doubleDiscretSignals[i].IsChecked)
            {
                //_count++;
                if (_doubleDiscretSignals[i].StartDate != null)
                {
                    var lv = _db.GetLogicVariableById(_doubleDiscretSignals[i].LogicVariableId2);
                    bool value = false;

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

                    if (value)
                    {
                        var borders = _db.SelectTimeBordersWithDiscretId(_doubleDiscretSignals[i].Id, 3);

                        if (_doubleDiscretSignals[i].DeltaT.Ticks < borders.T1 && !_doubleDiscretSignals[i].Tags[0])
                        {
                            _mark -= borders.Score1;
                            _doubleDiscretSignals[i].Tags[0] = true;
                            if (borders.Score1 != 0)
                            {
                                string str = _doubleDiscretSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                        if (borders.T2 != null && borders.Score2 != null)
                            if (_doubleDiscretSignals[i].DeltaT.Ticks >= borders.T1 && _doubleDiscretSignals[i].DeltaT.Ticks < borders.T2 && !_doubleDiscretSignals[i].Tags[1])
                            {
                                _mark -= borders.Score2;
                                _doubleDiscretSignals[i].Tags[1] = true;
                                if (borders.Score2 != 0)
                                {
                                    string str = _doubleDiscretSignals[i].Name + " - " + borders.Score2 + " " + getWord(borders.Score2) + " - " + DateTime.Now.ToString("T");
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (borders.T2 != null && borders.T3 != null && borders.Score3 != null)
                            if (_doubleDiscretSignals[i].DeltaT.Ticks >= borders.T2 && _doubleDiscretSignals[i].DeltaT.Ticks < borders.T3 && !_doubleDiscretSignals[i].Tags[2])
                            {
                                _mark -= borders.Score3;
                                _doubleDiscretSignals[i].Tags[2] = true;
                                if (borders.Score3 != 0)
                                {
                                    string str = _doubleDiscretSignals[i].Name + " - " + borders.Score3 + " " + getWord(borders.Score3) + " - " + DateTime.Now.ToString("T");
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (borders.T3 != null && borders.T4 != null && borders.Score4 != null)
                            if (_doubleDiscretSignals[i].DeltaT.Ticks >= borders.T3 && _doubleDiscretSignals[i].DeltaT.Ticks < borders.T4 && !_doubleDiscretSignals[i].Tags[3])
                            {
                                _mark -= borders.Score4;
                                _doubleDiscretSignals[i].Tags[3] = true;
                                if (borders.Score4 != 0)
                                {
                                    string str = _doubleDiscretSignals[i].Name + " - " + borders.Score4 + " " + getWord(borders.Score4) + " - " + DateTime.Now.ToString("T");
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (borders.T4 != null && borders.Score5 != null)
                            if (_doubleDiscretSignals[i].DeltaT.Ticks >= borders.T4 && !_doubleDiscretSignals[i].Tags[4])
                            {
                                _mark -= borders.Score5;
                                _doubleDiscretSignals[i].Tags[4] = true;
                                if (borders.Score5 != 0)
                                {
                                    string str = _doubleDiscretSignals[i].Name + " - " + borders.Score5 + " " + getWord(borders.Score5) + " - " + DateTime.Now.ToString("T");
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (_doubleDiscretSignals[i].Tags.All(value => value))
                            _doubleDiscretSignals[i].IsChecked = true;
                        q.Add(new Log { Type = "Trace", Message = "TagId = " + lv.ExitId.ToString() });
                        q.Add(new Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                    }
                }
                else
                {
                    var lv = _db.GetLogicVariableById(_doubleDiscretSignals[i].LogicVariableId1);
                    bool value = false;
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
                //_count--;
            }
        }

        private async Task CheckDiscretFromAnalogSignalAsync(int i)
        {
            if (!_discretFromAnalogSignals[i].IsChecked)
            {
                //_count++;
                bool value = false;

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

                if (value)
                {
                    var borders = _db.SelectTimeBordersWithDiscretId(_discretFromAnalogSignals[i].Id, 1);

                    if (_discretFromAnalogSignals[i].DeltaT.Ticks < borders.T1 && !_discretFromAnalogSignals[i].Tags[0])
                    {
                        _mark -= borders.Score1;
                        _discretFromAnalogSignals[i].Tags[0] = true;
                        if (borders.Score1 != 0)
                        {
                            string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + " - " + DateTime.Now.ToString("T");
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                            await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                            await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                        }
                    }

                    if (borders.T2 != null && borders.Score2 != null)
                        if (_discretFromAnalogSignals[i].DeltaT.Ticks >= borders.T1 && _discretFromAnalogSignals[i].DeltaT.Ticks < borders.T2 && !_discretFromAnalogSignals[i].Tags[1])
                        {
                            _mark -= borders.Score2;
                            _discretFromAnalogSignals[i].Tags[1] = true;
                            if (borders.Score2 != 0)
                            {
                                string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score2 + " " + getWord(borders.Score2) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (borders.T2 != null && borders.T3 != null && borders.Score3 != null)
                        if (_discretFromAnalogSignals[i].DeltaT.Ticks >= borders.T2 && _discretFromAnalogSignals[i].DeltaT.Ticks < borders.T3 && !_discretFromAnalogSignals[i].Tags[2])
                        {
                            _mark -= borders.Score3;
                            _discretFromAnalogSignals[i].Tags[2] = true;
                            if (borders.Score3 != 0)
                            {
                                string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score3 + " " + getWord(borders.Score3) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (borders.T3 != null && borders.T4 != null && borders.Score4 != null)
                        if (_discretFromAnalogSignals[i].DeltaT.Ticks >= borders.T3 && _discretFromAnalogSignals[i].DeltaT.Ticks < borders.T4 && !_discretFromAnalogSignals[i].Tags[3])
                        {
                            _mark -= borders.Score4;
                            _discretFromAnalogSignals[i].Tags[3] = true;
                            if (borders.Score4 != 0)
                            {
                                string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score4 + " " + getWord(borders.Score4) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (borders.T4 != null && borders.Score5 != null)
                        if (_discretFromAnalogSignals[i].DeltaT.Ticks >= borders.T4 && !_discretFromAnalogSignals[i].Tags[4])
                        {
                            _mark -= borders.Score5;
                            _discretFromAnalogSignals[i].Tags[4] = true;
                            if (borders.Score5 != 0)
                            {
                                string str = _discretFromAnalogSignals[i].Name + " - " + borders.Score5 + " " + getWord(borders.Score5) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (_discretFromAnalogSignals[i].Tags.All(value => value))
                        _discretFromAnalogSignals[i].IsChecked = true;
                    q.Add(new Log { Type = "Trace", Message = "TagId = " + _discretFromAnalogSignals[i].ExitId.ToString() });
                    //await SendMessageAsync("TagId = " + _discretFromAnalogSignals[i].ExitId.ToString(), webSocket);
                    q.Add(new Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                }
                else
                    _discretFromAnalogSignals[i].DeltaT = _discretFromAnalogSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));
                //_count--;
            }
        }

        private async Task CheckGroupOfDiscretSignalAsync(int i)
        {
            if (!_groupOfDiscretSignals[i].IsChecked)
            {
                _count++;
                if (_groupOfDiscretSignals[i].StartDate != null)
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

                        _groupOfDiscretSignals[i].EndLogicVariables[j].IsChecked = await scadaVConnection.ReadDiscretFromServer(_groupOfDiscretSignals[i].EndLogicVariables[j].ExitId);
                    }

                    for (int j = 0; j < _groupOfDiscretSignals[i].EndSignals.Count; j++)
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
                            value = Convert.ToInt32(res.Value) == _groupOfDiscretSignals[i].EndSignals[j].Const;
                        else
                            value = Convert.ToSingle(res.Value) == _groupOfDiscretSignals[i].EndSignals[j].Const;

                        _groupOfDiscretSignals[i].EndSignals[j].IsChecked = value;
                    }

                    if (_groupOfDiscretSignals[i].EndLogicVariables.Where(x => x.IsChecked).Count() == _groupOfDiscretSignals[i].EndLogicVariables.Count
                        && _groupOfDiscretSignals[i].EndSignals.Where(x => x.IsChecked).Count() == _groupOfDiscretSignals[i].EndSignals.Count)
                    {
                        _groupOfDiscretSignals[i].DeltaT = _groupOfDiscretSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));

                        var borders = _db.SelectTimeBordersWithDiscretId(_groupOfDiscretSignals[i].Id, 2);

                        if (_groupOfDiscretSignals[i].DeltaT.Ticks < borders.T1 && !_groupOfDiscretSignals[i].Tags[0])
                        {
                            _mark -= borders.Score1;
                            _groupOfDiscretSignals[i].Tags[0] = true;
                            if (borders.Score1 != 0)
                            {
                                string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                        if (borders.T2 != null && borders.Score2 != null)
                            if (_groupOfDiscretSignals[i].DeltaT.Ticks >= borders.T1 && _groupOfDiscretSignals[i].DeltaT.Ticks < borders.T2 && !_groupOfDiscretSignals[i].Tags[1])
                            {
                                _mark -= borders.Score2;
                                _groupOfDiscretSignals[i].Tags[1] = true;
                                if (borders.Score2 != 0)
                                {
                                    string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score2 + " " + getWord(borders.Score2) + " - " + DateTime.Now.ToString("T");
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (borders.T2 != null && borders.T3 != null && borders.Score3 != null)
                            if (_groupOfDiscretSignals[i].DeltaT.Ticks >= borders.T2 && _groupOfDiscretSignals[i].DeltaT.Ticks < borders.T3 && !_groupOfDiscretSignals[i].Tags[2])
                            {
                                _mark -= borders.Score3;
                                _groupOfDiscretSignals[i].Tags[2] = true;
                                if (borders.Score3 != 0)
                                {
                                    string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score3 + " " + getWord(borders.Score3) + " - " + DateTime.Now.ToString("T");
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (borders.T3 != null && borders.T4 != null && borders.Score4 != null)
                            if (_groupOfDiscretSignals[i].DeltaT.Ticks >= borders.T3 && _groupOfDiscretSignals[i].DeltaT.Ticks < borders.T4 && !_groupOfDiscretSignals[i].Tags[3])
                            {
                                _mark -= borders.Score4;
                                _groupOfDiscretSignals[i].Tags[3] = true;
                                if (borders.Score4 != 0)
                                {
                                    string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score4 + " " + getWord(borders.Score4) + " - " + DateTime.Now.ToString("T");
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (borders.T4 != null && borders.Score5 != null)
                            if (_groupOfDiscretSignals[i].DeltaT.Ticks >= borders.T4 && !_groupOfDiscretSignals[i].Tags[4])
                            {
                                _mark -= borders.Score5;
                                _groupOfDiscretSignals[i].Tags[4] = true;
                                if (borders.Score5 != 0)
                                {
                                    string str = _groupOfDiscretSignals[i].Name + " - " + borders.Score5 + " " + getWord(borders.Score5) + " - " + DateTime.Now.ToString("T");
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (_groupOfDiscretSignals[i].Tags.All(value => value))
                            _groupOfDiscretSignals[i].IsChecked = true;
                        q.Add(new Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
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

                            bool value = await scadaVConnection.ReadDiscretFromServer(_groupOfDiscretSignals[i].StartLogicVariables[j].ExitId);
                            if (value)
                                _groupOfDiscretSignals[i].StartLogicVariables[j].IsChecked = true;
                        }
                    }

                    for (int j = 0; j < _groupOfDiscretSignals[i].StartSignals.Count; j++)
                    {
                        if (!_groupOfDiscretSignals[i].StartSignals[j].IsChecked)
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
                    }

                    if (_groupOfDiscretSignals[i].StartLogicVariables.Where(x => x.IsChecked).Count() == _groupOfDiscretSignals[i].StartLogicVariables.Count
                        && _groupOfDiscretSignals[i].StartSignals.Where(x => x.IsChecked).Count() == _groupOfDiscretSignals[i].StartSignals.Count)
                    {
                        _groupOfDiscretSignals[i].StartDate = DateTime.Now;
                    }
                }
                _count--;
            }
        }

        private async Task CheckOperationWithConditionAsync(int i)
        {
            if (!_operationsWithCondition[i].IsChecked)
            {
                //_count++;
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
                    _operationsWithCondition[i].IsChecked = true;

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
                        result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valBool, DateTime.Now);
                        q.Add(new Log { Type = "Trace", Message = "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                        await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                    }
                    else if (_operationsWithCondition[i].ValueToWrite.ToLower() == "false")
                    {
                        valBool = false;
                        result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valBool, DateTime.Now);
                        q.Add(new Log { Type = "Trace", Message = "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                        await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                    }
                    else if (_operationsWithCondition[i].ValueToWrite.Contains('.') || _operationsWithCondition[i].ValueToWrite.Contains(','))
                    {
                        _operationsWithCondition[i].ValueToWrite = _operationsWithCondition[i].ValueToWrite.Replace('.', ',');
                        valFloat = float.Parse(_operationsWithCondition[i].ValueToWrite);
                        result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valFloat, DateTime.Now);
                        q.Add(new Log { Type = "Trace", Message = "Запись значения типа REAL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                        await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Запись значения типа REAL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                    }
                    else
                    {
                        valInt = Convert.ToInt32(_operationsWithCondition[i].ValueToWrite);
                        result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valInt, DateTime.Now);
                        q.Add(new Log { Type = "Trace", Message = "Запись значения типа INT по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                        await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Запись значения типа INT по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                    }
                }
                //_count--;
            }
        }

        public async void HandlerEndTrainingAsync()
        {
            try
            {
                //await _hubContext.Clients.All.SendAsync(TrainingIsEndFunctionName);
                StatusTraining = 1;
                if (_selectedTraining != null)
                {
                    TimerManager.Stop();
                    DateTime endTime = DateTime.Now;
                    //await _hubContext.Clients.All.SendAsync("IsOver");
                    await Task.WhenAll(tasks);
                    //while (_count != 0)
                    //    continue;

                    string str;

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
                        str = "Аналоговый сигнал " + analogSignals[i].Name + " оценён на " + marks.Last().ToString() + " - " + DateTime.Now.ToString("T");
                        _criteriasForReport2.Add(str);
                        await _hubContext.Clients.All.SendAsync("ReceiveCriterias2", str);
                        await _hubContext.Clients.All.SendAsync(Receive2FunctionName, str);
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
                        str = "Аналоговый сигнал " + analogSignals[i].Name + " оценён на " + marks.Last().ToString() + " - " + DateTime.Now.ToString("T");
                        _criteriasForReport2.Add(str);
                        await _hubContext.Clients.All.SendAsync("ReceiveCriterias2", str);
                        await _hubContext.Clients.All.SendAsync(Receive2FunctionName, str);
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
                        str = "Аналоговый сигнал " + analogSignals[i].Name + " оценён на " + marks.Last().ToString() + " - " + DateTime.Now.ToString("T");
                        _criteriasForReport2.Add(str);
                        await _hubContext.Clients.All.SendAsync("ReceiveCriterias2", str);
                        await _hubContext.Clients.All.SendAsync(Receive2FunctionName, str);
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
                        str = "Аналоговый сигнал " + analogSignals[i].Name + " оценён на " + marks.Last().ToString() + " - " + DateTime.Now.ToString("T");
                        _criteriasForReport2.Add(str);
                        await _hubContext.Clients.All.SendAsync("ReceiveCriterias2", str);
                        await _hubContext.Clients.All.SendAsync(Receive2FunctionName, str);
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
                        await _hubContext.Clients.All.SendAsync(Receive2FunctionName, str);
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
                                str = analogSignals[i].Name + " - " + f.Score + " " + getWord(f.Score) + " - " + DateTime.Now.ToString("T");
                                //_criteriasForReport1.Add(str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                            }
                        }
                        else if (analogSignals[i].Func == "Наличие выхода за коридор")
                        {
                            var f = _db.SelectExitToTheCorridor(analogSignals[i].Id);
                            var res = await scadaVConnection.ValueOutOfBorders(analogSignals[i].ExitId, (DateTime)_selectedTraining.StartDateTime, endTime, (float)f.Bottom, (float)f.Top);

                            if (!res)
                            {
                                _mark -= f.Score;
                                str = analogSignals[i].Name + " - " + f.Score + " " + getWord(f.Score) + " - " + DateTime.Now.ToString("T");
                                //_criteriasForReport1.Add(str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, str);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                q.Add(new Log { Type = "Error", Message = ex.Message });
                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);

                await Task.WhenAll(tasks);
                await TrainingEnd(false);

                while (true)
                    if (_count == 0)
                    {
                        await TrainingEnd(false);
                        break;
                    }

                //if (ex.Message.Contains("вернулась пустая коллекция"))
                //    await _hubContext.Clients.All.SendAsync(ReceiveStatusFunctionName, "Тренировка не завершена, оценка не сформирована!");

                //await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, ex.Message);
                //q.Add(new Log { Type = "Error", Message = ex.Message });

                //while (true)
                //    if (_count == 0)
                //    {
                //        StatusTraining = 2;
                //        await _hubContext.Clients.All.SendAsync(TrainingIsEndFunctionName);
                //        break;
                //    }
                return;
            }

            await Task.WhenAll(tasks);
            await TrainingEnd(true);

            //while (true)
            //    if (_count == 0)
            //    {
            //        await TrainingEnd(true);
            //        break;
            //    }
        }

        private async Task TrainingEnd(bool success)
         {
            if (success)
            {
                await _hubContext.Clients.All.SendAsync(ReceiveStatusFunctionName, "Завершена");
                await _hubContext.Clients.All.SendAsync(ReceiveFunctionName, "Итоговая оценка: " + _mark.ToString() + " - " + DateTime.Now.ToString("T"));
                await _hubContext.Clients.All.SendAsync(ReceiveMarkFunctionName, _mark.ToString());
                await _hubContext.Clients.All.SendAsync("IsOver");
            }
            else
                await _hubContext.Clients.All.SendAsync(ReceiveStatusFunctionName, "Тренировка не завершена, оценка не сформирована!");

            StatusTraining = 2;
            q.CompleteAdding();
            thread.Join();
            await _hubContext.Clients.All.SendAsync(TrainingIsEndFunctionName);
        }
    }
    #endregion
}
