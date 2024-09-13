using System.Net.WebSockets;
using BLL;
using BLL.Models;
using BLL.Operations;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System;
using System.Reflection;
using DAL.Entities;
using NLog;

namespace React.Server
{
    public class MessageManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //необходимые подключения
        private IHubContext<MessageHub> _hubContext;
        private TrainingDbOperations _db;
        private ScadaVConnection scadaVConnection1;
        private ScadaVConnection scadaVConnection2;
        private ScadaVConnection scadaVConnection3;
        private TimerManager _timer;

        public int StatusTraining { get; set; } //текущий статус тренировки
        private TrainingModel _selectedTraining; //выбранная тренировка
        private int? _mark; //текущая оценка тренировки
        private string _endMark; //дополнительная оценка тренировки 
        private DateTime _endTime; //время завершения тренировки
        private List<string> _criteriasForReport1; //информация для отчёта №1
        private List<string> _criteriasForReport2; //информация для отчёта №2
        private List<AnalogSignalMark> marks = new List<AnalogSignalMark>(); 

        //сигналы и операции, из которых состоит тренировка
        private List<DiscretSignalModel> _discretSignals;
        private List<DoubleDiscretSignalModel> _doubleDiscretSignals;
        private List<DiscretFromAnalogSignalModel> _discretFromAnalogSignals;
        private List<GroupOfDiscretSignalsModel> _groupOfDiscretSignals;
        private List<OperationWithConditionModel> _operationsWithCondition;

        //для вывода логов
        private BlockingCollection<Log> q;
        private Thread thread;

        //для работы с задачами
        private List<Task> _tasks;
        private bool _isException;
        private CancellationTokenSource _cancellationTokenSource;

        //названия функций для отправления сообщений через SignalR
        private string startFunctionName = "Start";
        private string receiveFunctionName = "Receive";
        private string receive2FunctionName = "Receive2";
        private string receiveMarkFunctionName = "ReceiveMark";
        private string receiveStatusFunctionName = "ReceiveStatus";
        private string trainingIsEndFunctionName = "TrainingIsEnd";

        public MessageManager()
        {

        }

        //установка начальных настроек
        public void SetSettings(IHubContext<MessageHub> hubContext, MyOptions options, bool isRemoved)
        {
            StatusTraining = 0;
            _hubContext = hubContext;
            _db = new TrainingDbOperations(options);
            scadaVConnection1 = options.scadaVConnection1;
            scadaVConnection2 = options.scadaVConnection2;
            scadaVConnection3 = options.scadaVConnection3;

            _tasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
            _isException = false;

            q = new BlockingCollection<Log>();
            thread = new Thread(Consumer);
            thread.Start();

            //настройка для удалённого доступа
            if (isRemoved)
            {
                startFunctionName = "StartRemoved";
                receiveFunctionName = "ReceiveRemoved";
                receive2FunctionName = "Receive2Removed";
                receiveMarkFunctionName = "ReceiveMarkRemoved";
                receiveStatusFunctionName = "ReceiveStatusRemoved";
                trainingIsEndFunctionName = "TrainingIsEndRemoved";
            }
            //настройка для прямого доступа
            else
            {
                startFunctionName = "Start";
                receiveFunctionName = "Receive";
                receive2FunctionName = "Receive2";
                receiveMarkFunctionName = "ReceiveMark";
                receiveStatusFunctionName = "ReceiveStatus";
                trainingIsEndFunctionName = "TrainingIsEnd";
            }
        }

        //запуск тренировки
        public async Task StartConnection(int id)
        {
            await BeginScenary(id);
            //пока тренировка не завершена (без цикла соединение SignalR отключается до завершения)
            while (StatusTraining != 2)
                continue;
        }

        //проверка выбрана ли тренировка
        public bool CheckTraining(int id)
        {
            return _db.SelectTrainingById(id).Id != 0 ? true : false;
        }

        //начало выполнения сценария
        private async Task BeginScenary(int id)
        {
            //await scadaVConnection1.ReadDiscretFromServer(729769);
            await _hubContext.Clients.All.SendAsync("ReceiveIdRemoved", id);

            //получаем выбранную тренировку по айди
            _selectedTraining = _db.SelectTrainingById(id);
            _mark = _selectedTraining.Mark;

            //информируем о начале тренировки 
            await _hubContext.Clients.All.SendAsync(startFunctionName);
            await _hubContext.Clients.All.SendAsync(receiveStatusFunctionName, "Начата");
            await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Начинаю запись сценария тренировки " + _selectedTraining.Name + " - " + DateTime.Now.ToString("G"));

            //записываем время начала выполнения тренировки
            _selectedTraining.StartDateTime = DateTime.Now;

            //считываем из базы данных информацию о выбранной тренировке
            await LoadDataAsync();

            //выполняем операции выбранной тренировки
            await PerformSelectedTrainingOperations();

            //запускаем таймер, который через каждую секунду будет вызывать функцию проверки всех сигналов
            _timer = new TimerManager();
            _timer.TimerTick += async () => await CheckAllAsync();
            await CheckAllAsync();
            _timer.Start();
        }

        //получение сигналов и операций, относящихся к выбранной тренировке
        private async Task LoadDataAsync()
        {
            _discretSignals = await Task.Run(() => _db.SelectAllDiscretSignals(_selectedTraining.Id));
            _doubleDiscretSignals = await Task.Run(() => _db.SelectAllDoubleDiscretSignals(_selectedTraining.Id));
            _discretFromAnalogSignals = await Task.Run(() => _db.SelectAllDiscretFromAnalogSignals(_selectedTraining.Id, 0));
            _operationsWithCondition = await Task.Run(() => _db.SelectOperationsWithConditionWithTrainingId(_selectedTraining.Id));
            _groupOfDiscretSignals = await Task.Run(() => _db.SelectGroupsByTrainingId(_selectedTraining.Id));
        }

        //выполнение операций выбранной тренировки
        private async Task PerformSelectedTrainingOperations()
        {
            //получаем все операции, относящиеся к данной тренировке, и запускаем их в параллельном цикле
            var operations = _db.SelectOperationsWithTrainingId(_selectedTraining.Id);
            for (int i = 0; i < operations.Count; i++)
            {
                int index = i;
                //создаём новую задачу
                var task = new Task(async () =>
                {
                    try
                    {
                        //проверяем не отменена ли задача
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        //вызываем функцию проверки дискретного сигнала
                        await PerformOperationAsync(operations[index]);
                        //снова проверяем на отмену
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex)
                    {
                        //если в ходе выполнения возникла ошибка
                        if (!_isException)
                        {
                            _isException = true;
                            //останавливаем таймер
                            _timer?.Stop();
                            //отменяем задачи
                            _cancellationTokenSource.Cancel();

                            //отправляем сообщение об ошибке
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);

                            //ожидаем завершения всех задач
                            foreach (var t in _tasks)
                                if (t.Id != Task.CurrentId && !t.IsCanceled)
                                    await t;
                            //вызываем функцию окончания тренировки
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                //запускаем задачу
                task.Start();
                _tasks.Add(task);
            }
        }

        //выполнение одной операции
        private async Task PerformOperationAsync(OperationModel operation)
        {
            //возможные варианты записываемых значений
            bool valBool;
            float valFloat;
            int valInt;
            
            //определение нужного подключения
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

            //выдержка времени перед операцией
            if (operation.TimePause.Ticks > 0)
            {
                var time = operation.TimePause;
                while (time != TimeSpan.Zero)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    Thread.Sleep(1000);
                    time = time.Subtract(new TimeSpan(0, 0, 1));
                }
            }

            //результат записи
            bool result;
            //определяем тип записываемого значения
            //bool
            if (operation.ValueToWrite.ToLower() == "true")
            {
                valBool = true;
                try
                {
                    //записываем значение и информируем о его успехе
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valBool, DateTime.Now);
                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Запись значения типа BOOL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);
                }
            }
            else if (operation.ValueToWrite.ToLower() == "false")
            {
                valBool = false;
                try
                {
                    //записываем значение и информируем о его успехе
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valBool, DateTime.Now);
                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Запись значения типа BOOL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);
                }
            }
            //float
            else if (operation.ValueToWrite.Contains('.') || operation.ValueToWrite.Contains(','))
            {
                //приводим значение к нужному типу
                operation.ValueToWrite = operation.ValueToWrite.Replace('.', ',');
                valFloat = float.Parse(operation.ValueToWrite);
                try
                {
                    //записываем значение и информируем о его успехе
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valFloat, DateTime.Now);
                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Запись значения типа REAL по тэгу " + operation.ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);
                }
            }
            //int
            else
            {
                valInt = Convert.ToInt32(operation.ValueToWrite);
                try
                {
                    //записываем значение и информируем о его успехе
                    result = await scadaVConnection.WriteVariable(operation.ExitId, valInt, DateTime.Now);
                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Запись значения типа INT по тэгу " + +operation.ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                }
                catch (Exception ex)
                {
                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);
                }
            }
        }

        public async Task CheckAllAsync()
        {
            //одиночные дискретные сигналы
            for (int i = 0; i < _discretSignals.Count; i++)
            {
                int index = i;
                //создаём новую задачу
                var task = new Task(async () =>
                {
                    try
                    {
                        //проверяем не отменена ли задача
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        //вызываем функцию проверки дискретного сигнала
                        await CheckDiscretSignalAsync(index);
                        //снова проверяем на отмену
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex)
                    {
                        //если в ходе выполнения возникла ошибка
                        if (!_isException)
                        {
                            _isException = true;
                            //останавливаем таймер
                            _timer.Stop();
                            //отменяем задачи
                            _cancellationTokenSource.Cancel();

                            //отправляем сообщение об ошибке
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);

                            //ожидаем завершения всех задач
                            foreach (var t in _tasks)
                                if (t.Id != Task.CurrentId && !t.IsCanceled)
                                    try 
                                    { 
                                        await t; 
                                    }
                                    catch
                                    { }
                            //вызываем функцию окончания тренировки
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                //запускаем задачу
                if (!task.IsCompleted && !task.IsCanceled)
                {
                    task.Start();
                    _tasks.Add(task);
                }
            }

            //двойные дискретные сигналы
            for (int i = 0; i < _doubleDiscretSignals.Count; i++)
            {
                int index = i;
                //создаём новую задачу
                var task = new Task(async () =>
                {
                    try
                    {
                        //проверяем не отменена ли задача
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        //вызываем функцию проверки двойного дискретного сигнала
                        await CheckDoubleDiscretSignalAsync(index);
                        //снова проверяем на отмену
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex)
                    {
                        //если в ходе выполнения возникла ошибка
                        if (!_isException)
                        {
                            _isException = true;
                            //останавливаем таймер
                            _timer.Stop();
                            //отменяем задачи
                            _cancellationTokenSource.Cancel();

                            //отправляем сообщение об ошибке
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);

                            //ожидаем завершения всех задач
                            foreach (var t in _tasks)
                                if (t.Id != Task.CurrentId && !t.IsCanceled)
                                    try
                                    {
                                        await t;
                                    }
                                    catch
                                    { }
                            //вызываем функцию окончания тренировки
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                //запускаем задачу
                if (!task.IsCompleted && !task.IsCanceled)
                {
                    task.Start();
                    _tasks.Add(task);
                }
            }

            //дискретные сигналы, полученные из аналоговых
            for (int i = 0; i < _discretFromAnalogSignals.Count; i++)
            {
                int index = i;
                //создаём новую задачу
                var task = new Task(async () =>
                {
                    try
                    {
                        //проверяем не отменена ли задача
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        //вызываем функцию проверки дискретного сигнала, полученного из аналогового
                        await CheckDiscretFromAnalogSignalAsync(index);
                        //снова проверяем на отмену
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex)
                    {
                        //если в ходе выполнения возникла ошибка
                        if (!_isException)
                        {
                            _isException = true;
                            //останавливаем таймер
                            _timer.Stop();
                            //отменяем задачи
                            _cancellationTokenSource.Cancel();

                            //отправляем сообщение об ошибке
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);

                            //ожидаем завершения всех задач
                            foreach (var t in _tasks)
                                if (t.Id != Task.CurrentId && !t.IsCanceled)
                                    try
                                    {
                                        await t;
                                    }
                                    catch
                                    { }
                            //вызываем функцию окончания тренировки
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                //запускаем задачу
                if (!task.IsCompleted && !task.IsCanceled)
                {
                    task.Start();
                    _tasks.Add(task);
                }
            }

            //группы дискретных сигналов
            for (int i = 0; i < _groupOfDiscretSignals.Count; i++)
            {
                int index = i;
                //создаём новую задачу
                var task = new Task(async () =>
                {
                    try
                    {
                        //проверяем не отменена ли задача
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        //вызываем функцию проверки группы дискретных сигналов
                        await CheckGroupOfDiscretSignalAsync(index);
                        //снова проверяем на отмену
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex)
                    {
                        //если в ходе выполнения возникла ошибка
                        if (!_isException)
                        {
                            _isException = true;
                            //останавливаем таймер
                            _timer.Stop();
                            //отменяем задачи
                            _cancellationTokenSource.Cancel();

                            //отправляем сообщение об ошибке
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);

                            //ожидаем завершения всех задач
                            foreach (var t in _tasks)
                                if (t.Id != Task.CurrentId && !t.IsCanceled)
                                    try
                                    {
                                        await t;
                                    }
                                    catch
                                    { }
                            //вызываем функцию окончания тренировки
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                //запускаем задачу
                if (!task.IsCompleted && !task.IsCanceled)
                {
                    task.Start();
                    _tasks.Add(task);
                }
            }

            //операции с условием
            for (int i = 0; i < _operationsWithCondition.Count; i++)
            {
                int index = i;
                //создаём новую задачу
                var task = new Task(async () =>
                {
                    try
                    {
                        //проверяем не отменена ли задача
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        //вызываем функцию проверки операции с условием
                        await CheckOperationWithConditionAsync(index);
                        //снова проверяем на отмену
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex)
                    {
                        //если в ходе выполнения возникла ошибка
                        if (!_isException)
                        {
                            _isException = true;
                            //останавливаем таймер
                            _timer.Stop();
                            //отменяем задачи
                            _cancellationTokenSource.Cancel();

                            //отправляем сообщение об ошибке
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, ex.Message);

                            //ожидаем завершения всех задач
                            foreach (var t in _tasks)
                                if (t.Id != Task.CurrentId && !t.IsCanceled)
                                    try
                                    {
                                        await t;
                                    }
                                    catch
                                    { }
                            //вызываем функцию окончания тренировки
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                //запускаем задачу
                if (!task.IsCompleted && !task.IsCanceled)
                {
                    task.Start();
                    _tasks.Add(task);
                }
            }
        }

        private async Task CheckDiscretSignalAsync(int i)
        {
            if (!_discretSignals[i].IsChecked)
            {
                //получаем логическую переменную
                var lv = _db.GetLogicVariableById(_discretSignals[i].LogicVariableId);
                bool value = false;
                //считываем значение с сервера
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
                //прибавляем одну секунду
                _discretSignals[i].DeltaT = _discretSignals[i].DeltaT.Add(new TimeSpan(0, 0, 1));

                //если считанное значение истинно
                if (value)
                {
                    //получаем временные границы
                    var borders = _db.SelectTimeBordersWithDiscretId(_discretSignals[i].Id, 0);

                    //вычитаем баллы в зависимости от диапазона, в котором находится текущее значение времени
                    if (_discretSignals[i].DeltaT.Ticks < borders.T1 && !_discretSignals[i].Tags[0])
                    {
                        _mark -= borders.Score1; 
                        //помечаем диапазон отмеченным
                        _discretSignals[i].Tags[0] = true;
                        if (borders.Score1 != 0) 
                        {
                            string str = _discretSignals[i].Name + " - " + borders.Score1 + " " + getWord(borders.Score1) + " - " + DateTime.Now.ToString("T");
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                            await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка");
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    //если все диапазоны отмечены, помечаем сигнал отмеченным
                    if (_discretSignals[i].Tags.All(value => value))
                        _discretSignals[i].IsChecked = true;
                    //q.TryAdd(new Log { Type = "Trace", Message = "TagId = " + lv.ExitId.ToString() });
                    //q.TryAdd(new Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                }
            }
        }

        //проверка двойных дискретных сигналов
        private async Task CheckDoubleDiscretSignalAsync(int i)
        {
            if (!_doubleDiscretSignals[i].IsChecked)
            {
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (_doubleDiscretSignals[i].Tags.All(value => value))
                            _doubleDiscretSignals[i].IsChecked = true;
                        //q.TryAdd(new Log { Type = "Trace", Message = "TagId = " + lv.ExitId.ToString() });
                        //q.TryAdd(new Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
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
            }
        }

        //проверка дискретных сигналов, полученных из аналоговых
        private async Task CheckDiscretFromAnalogSignalAsync(int i)
        {
            if (!_discretFromAnalogSignals[i].IsChecked)
            {
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
                    if (Convert.ToSingle(res.Value) == _discretFromAnalogSignals[i].Const)
                        value = true;
                }
                else
                {
                    if (Convert.ToInt32(res.Value) == _discretFromAnalogSignals[i].Const)
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
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                            await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                            await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
                            }
                        }

                    if (_discretFromAnalogSignals[i].Tags.All(value => value))
                        _discretFromAnalogSignals[i].IsChecked = true;
                    //q.TryAdd(new Log { Type = "Trace", Message = "TagId = " + _discretFromAnalogSignals[i].ExitId.ToString() });
                    //q.TryAdd(new Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
                }
            }
        }

        //проверка групп дискретных сигналов
        private async Task CheckGroupOfDiscretSignalAsync(int i)
        {
            if (!_groupOfDiscretSignals[i].IsChecked)
            {
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
                            value = Convert.ToSingle(res.Value) == _groupOfDiscretSignals[i].EndSignals[j].Const;
                        else
                            value = Convert.ToInt32(res.Value) == _groupOfDiscretSignals[i].EndSignals[j].Const;

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
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
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
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                    await _hubContext.Clients.All.SendAsync(receiveFunctionName, _mark.ToString() + " - текущая оценка.");
                                }
                            }

                        if (_groupOfDiscretSignals[i].Tags.All(value => value))
                            _groupOfDiscretSignals[i].IsChecked = true;
                        //q.TryAdd(new Log { Type = "Trace", Message = _mark.ToString() + " - текущая оценка." });
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
                                if (Convert.ToSingle(res.Value) == _groupOfDiscretSignals[i].StartSignals[j].Const)
                                    value = true;
                            }
                            else
                            {
                                if (Convert.ToInt32(res.Value) == _groupOfDiscretSignals[i].StartSignals[j].Const)
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
            }
        }

        //проверка операций с условием
        private async Task CheckOperationWithConditionAsync(int i)
        {
            if (!_operationsWithCondition[i].IsChecked)
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
                    _operationsWithCondition[i].IsChecked = true;

                    if (_operationsWithCondition[i].TimePause.Ticks > 0)
                    {
                        var time = _operationsWithCondition[i].TimePause;
                        while (time !=  TimeSpan.Zero)
                        {
                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            Thread.Sleep(1000);
                            time = time.Subtract(new TimeSpan(0, 0, 1));
                        }
                    }

                    if (_operationsWithCondition[i].TimePause.Ticks > 0)
                        Thread.Sleep(_operationsWithCondition[i].TimePause); //выдержка времени перед операцией

                    //Возможные варианты записываемых значений
                    bool valBool;
                    float valFloat;
                    int valInt;

                    //Результат записи
                    bool result;

                    //выполнение операции
                    //определяем тип записываемого значения
                    if (_operationsWithCondition[i].ValueToWrite.ToLower() == "true")
                    {
                        valBool = true;
                        result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valBool, DateTime.Now);
                        //q.TryAdd(new Log { Type = "Trace", Message = "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                        await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                    }
                    else if (_operationsWithCondition[i].ValueToWrite.ToLower() == "false")
                    {
                        valBool = false;
                        result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valBool, DateTime.Now);
                        //q.TryAdd(new Log { Type = "Trace", Message = "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                        await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Запись значения типа BOOL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                    }
                    else if (_operationsWithCondition[i].ValueToWrite.Contains('.') || _operationsWithCondition[i].ValueToWrite.Contains(','))
                    {
                        _operationsWithCondition[i].ValueToWrite = _operationsWithCondition[i].ValueToWrite.Replace('.', ',');
                        valFloat = float.Parse(_operationsWithCondition[i].ValueToWrite);
                        result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valFloat, DateTime.Now);
                        //q.TryAdd(new Log { Type = "Trace", Message = "Запись значения типа REAL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                        await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Запись значения типа REAL по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                    }
                    else
                    {
                        valInt = Convert.ToInt32(_operationsWithCondition[i].ValueToWrite);
                        result = await scadaVConnection.WriteVariable(_operationsWithCondition[i].ExitId, valInt, DateTime.Now);
                        //q.TryAdd(new Log { Type = "Trace", Message = "Запись значения типа INT по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + "." });
                        await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Запись значения типа INT по тэгу " + _operationsWithCondition[i].ExitId + ". Прошла ли запись удачно - " + result + " - " + DateTime.Now.ToString("T"));
                    }
                }
            }
        }

        //критерии надёжности пуска и останова 
        private async Task CheckCriteriasForReliabilityOfStartAndStop()
        {
            string str;

            //основные критерии надёжности пуска и останова
            List<AnalogSignalModel> analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 0);
            for (int i = 0; i < analogSignals.Count; i++)
            {
                int index = i;
                var task = new Task(async () =>
                {
                    try
                    {
                        ScadaVConnection scadaVConnection = new ScadaVConnection();
                        switch (analogSignals[index].BaseNum)
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

                        if (analogSignals[index].Func == "Диапазон")
                        {
                            var f = _db.SelectRange(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.Diapazon(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Left, (float)f.Right, f.AbsValues == 1 ? true : false));
                        }
                        else if (analogSignals[index].Func == "Настраиваемый диапазон")
                        {
                            var f = _db.SelectAdjustableRange(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.CustomizableDiapazon(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Left, (float)f.Right, f.ExitId));
                        }
                        else if (analogSignals[index].Func == "Диапазон с параметрами")
                        {
                            var f = _db.SelectRangeWithParameters(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.DiapazonWithParams(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Left, (float)f.Right1, (float)f.Right2, (float)f.ParamVal1, (float)f.ParamVal2, f.ExitId));
                        }
                        else if (analogSignals[index].Func == "Суммарное или неоднократное превышение уставки")
                        {
                            var f = _db.SelectExceeding(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.TotalOrRepeatedExceeding(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Ustavka, new TimeSpan(f.SummTime), (float)f.Prev));
                        }
                        str = "Аналоговый сигнал " + analogSignals[index].Name + " оценён на " + marks.Last().ToString() + " - " + DateTime.Now.ToString("T");
                        _criteriasForReport2.Add(str);
                        await _hubContext.Clients.All.SendAsync("ReceiveCriterias2", str);
                        await _hubContext.Clients.All.SendAsync(receive2FunctionName, str);
                    }
                    catch (Exception ex)
                    {
                        if (!_isException)
                        {
                            _isException = true;
                            _cancellationTokenSource.Cancel();

                            await _hubContext.Clients.All.SendAsync(receive2FunctionName, ex.Message);

                            foreach (var t in _tasks)
                                if (t.Id != Task.CompletedTask.Id)
                                    await t;
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                task.Start();
                _tasks.Add(task);
            }

            //дополнительные критерии надёжности пуска и останова
            analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 1);
            for (int i = 0; i < analogSignals.Count; i++)
            {
                int index = i;
                var task = new Task(async () =>
                {
                    try
                    {
                        ScadaVConnection scadaVConnection = new ScadaVConnection();
                        switch (analogSignals[index].BaseNum)
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

                        if (analogSignals[index].Func == "Диапазон")
                        {
                            var f = _db.SelectDopRange(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.DopDiapazon(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.OtlBorder, (float)f.XorBorder, (float)f.NeydBorder));
                        }
                        else if (analogSignals[index].Func == "Поддержание заданного уровня")
                        {
                            var f = _db.SelectMaintainingLevel(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.DopAbsDiapazon(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Ustavka, (float)f.OtlBorder, (float)f.NeydBorder));
                        }
                        str = "Аналоговый сигнал " + analogSignals[index].Name + " оценён на " + marks.Last().ToString() + " - " + DateTime.Now.ToString("T");
                        _criteriasForReport2.Add(str);
                        await _hubContext.Clients.All.SendAsync("ReceiveCriterias2", str);
                        await _hubContext.Clients.All.SendAsync(receive2FunctionName, str);
                    }
                    catch (Exception ex)
                    {
                        if (!_isException)
                        {
                            _isException = true;
                            _cancellationTokenSource.Cancel();

                            await _hubContext.Clients.All.SendAsync(receive2FunctionName, ex.Message);

                            foreach (var t in _tasks)
                                if (t.Id != Task.CompletedTask.Id)
                                    await t;
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                task.Start();
                _tasks.Add(task);
            }
        }

        //критерии оценки пуска и останова
        private async Task CriterisForEvaluatingTheQualityOfStartAndStop()
        {
            string str;

            //основные критерии оценки качества пуска и останова
            List<AnalogSignalModel> analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 2);
            for (int i = 0; i < analogSignals.Count; i++)
            {
                int index = i;
                var task = new Task(async () =>
                {
                    try
                    {
                        ScadaVConnection scadaVConnection = new ScadaVConnection();
                        switch (analogSignals[index].BaseNum)
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

                        if (analogSignals[index].Func == "Диапазон")
                        {
                            var f = _db.SelectRange(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.Diapazon(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Left, (float)f.Right, f.AbsValues == 1 ? true : false));
                        }
                        else if (analogSignals[index].Func == "Настраиваемый диапазон")
                        {
                            var f = _db.SelectAdjustableRange(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.CustomizableDiapazon(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Left, (float)f.Right, f.ExitId));
                        }
                        else if (analogSignals[index].Func == "Диапазон с параметрами")
                        {
                            var f = _db.SelectRangeWithParameters(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.DiapazonWithParams(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Left, (float)f.Right1, (float)f.Right2, (float)f.ParamVal1, (float)f.ParamVal2, f.ExitId));
                        }
                        else if (analogSignals[index].Func == "Суммарное или неоднократное превышение уставки")
                        {
                            var f = _db.SelectExceeding(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.TotalOrRepeatedExceeding(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Ustavka, new TimeSpan(f.SummTime), (float)f.Prev));
                        }
                        str = "Аналоговый сигнал " + analogSignals[index].Name + " оценён на " + marks.Last().ToString() + " - " + DateTime.Now.ToString("T");
                        _criteriasForReport2.Add(str);
                        await _hubContext.Clients.All.SendAsync("ReceiveCriterias2", str);
                        await _hubContext.Clients.All.SendAsync(receive2FunctionName, str);
                    }
                    catch (Exception ex)
                    {
                        if (!_isException)
                        {
                            _isException = true;
                            _cancellationTokenSource.Cancel();

                            await _hubContext.Clients.All.SendAsync(receive2FunctionName, ex.Message);

                            foreach (var t in _tasks)
                                if (t.Id != Task.CompletedTask.Id)
                                    await t;
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                task.Start();
                _tasks.Add(task);
            }

            //дополнительные критерии оценки качества пуска и останова
            analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 3);
            for (int i = 0; i < analogSignals.Count; i++)
            {
                int index = i;
                var task = new Task(async () =>
                {
                    try
                    {
                        ScadaVConnection scadaVConnection = new ScadaVConnection();
                        switch (analogSignals[index].BaseNum)
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

                        if (analogSignals[index].Func == "Диапазон")
                        {
                            var f = _db.SelectDopRange(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.DopDiapazon(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.OtlBorder, (float)f.XorBorder, (float)f.NeydBorder));
                        }
                        else if (analogSignals[index].Func == "Поддержание заданного уровня")
                        {
                            var f = _db.SelectMaintainingLevel(analogSignals[index].Id);
                            marks.Add(await scadaVConnection.DopAbsDiapazon(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Ustavka, (float)f.OtlBorder, (float)f.NeydBorder));
                        }
                        str = "Аналоговый сигнал " + analogSignals[index].Name + " оценён на " + marks.Last().ToString() + " - " + DateTime.Now.ToString("T");
                        _criteriasForReport2.Add(str);
                        await _hubContext.Clients.All.SendAsync("ReceiveCriterias2", str);
                        await _hubContext.Clients.All.SendAsync(receive2FunctionName, str);
                    }
                    catch (Exception ex)
                    {
                        if (!_isException)
                        {
                            _isException = true;
                            _cancellationTokenSource.Cancel();

                            await _hubContext.Clients.All.SendAsync(receive2FunctionName, ex.Message);

                            foreach (var t in _tasks)
                                if (t.Id != Task.CompletedTask.Id)
                                    await t;
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                task.Start();
                _tasks.Add(task);
            }
        }

        //аналоговые сигналы для противоаварийных тренировок
        private async Task CheckAnalogSignalsForEmergencyTraining()
        {
            string str;

            List<AnalogSignalModel> analogSignals = _db.SelectAllAnalogSignals(_selectedTraining.Id, 4);
            for (int i = 0; i < analogSignals.Count; i++)
            {
                int index = i;
                var task = new Task(async () =>
                {
                    try
                    {
                        ScadaVConnection scadaVConnection = new ScadaVConnection();
                        switch (analogSignals[index].BaseNum)
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

                        if (analogSignals[index].Func == "Время пребывания в интервале")
                        {
                            var f = _db.SelectTimeInInterval(analogSignals[index].Id);
                            var time = await scadaVConnection.TimeInIntervalFloat(analogSignals[index].ExitId, (float)f.Bottom, (float)f.Top, (DateTime)_selectedTraining.StartDateTime, _endTime);
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
                                str = analogSignals[index].Name + " - " + f.Score + " " + getWord(f.Score) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                            }
                        }
                        else if (analogSignals[index].Func == "Наличие выхода за коридор")
                        {
                            var f = _db.SelectExitToTheCorridor(analogSignals[index].Id);
                            var res = await scadaVConnection.ValueOutOfBorders(analogSignals[index].ExitId, (DateTime)_selectedTraining.StartDateTime, _endTime, (float)f.Bottom, (float)f.Top);

                            if (!res)
                            {
                                _mark -= f.Score;
                                str = analogSignals[index].Name + " - " + f.Score + " " + getWord(f.Score) + " - " + DateTime.Now.ToString("T");
                                await _hubContext.Clients.All.SendAsync("ReceiveCriterias1", str);
                                await _hubContext.Clients.All.SendAsync(receiveFunctionName, str);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!_isException)
                        {
                            _isException = true;
                            _cancellationTokenSource.Cancel();

                            await _hubContext.Clients.All.SendAsync(receive2FunctionName, ex.Message);

                            foreach (var t in _tasks)
                                if (t.Id != Task.CompletedTask.Id)
                                    await t;
                            await TrainingEnd(false);
                        }
                    }
                }, _cancellationTokenSource.Token);
                task.Start();
                _tasks.Add(task);
            }
        }

        //обработка завершения тренировки
        public async void HandlerEndTrainingAsync()
        {
            if (_selectedTraining != null)
            {
                StatusTraining = 1;
                _timer.Stop();
                _endTime = DateTime.Now;

                //ожидаем завершения всех задач
                foreach (var t in _tasks)
                    if (t.Id != Task.CurrentId && !t.IsCanceled)
                        await t;

                string str;

                await CheckAllAsync();

                await CheckCriteriasForReliabilityOfStartAndStop();

                await CriterisForEvaluatingTheQualityOfStartAndStop();

                //определяем оценку пуска и останова
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
                    await _hubContext.Clients.All.SendAsync(receive2FunctionName, str);
                }

                await CheckAnalogSignalsForEmergencyTraining();

                //ожидаем завершения всех задач
                foreach (var t in _tasks)
                    if (t.Id != Task.CurrentId && !t.IsCanceled)
                        await t;
                await TrainingEnd(true);
            }
        }

        //завершение тренировки
        private async Task TrainingEnd(bool success)
         {
            //если всё прошло успешно
            if (success)
            {
                await _hubContext.Clients.All.SendAsync(receiveStatusFunctionName, "Завершена");
                await _hubContext.Clients.All.SendAsync(receiveFunctionName, "Итоговая оценка: " + _mark.ToString() + " - " + DateTime.Now.ToString("T"));
                await _hubContext.Clients.All.SendAsync(receiveMarkFunctionName, _mark.ToString());
                await _hubContext.Clients.All.SendAsync("IsOver");
            }
            else
                await _hubContext.Clients.All.SendAsync(receiveStatusFunctionName, "Тренировка не завершена, оценка не сформирована!");

            StatusTraining = 2;
            q.CompleteAdding();
            thread.Join();
            await _hubContext.Clients.All.SendAsync(trainingIsEndFunctionName);
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

        //получаем нужную форма слова "балл"
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
    }
}
