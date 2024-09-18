using Scada.Connect.Base;
using Scada.ConnectV;
using Scada.Interfaces.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Models;
using NLog;
using Alezy.Library.Core.Utils;

namespace BLL
{
    public class ScadaVConnection
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ScadaArchiveRemoteServiceBuilder ArchiveBuilder(string host1, string host1r = null, string host2 = null, string host2r = null)
        {
            var scp = new RemoteServiceConnectionProperties
            {
                Hosts = RemoteServiceConnectionProperties.CreateHosts(WellKnownPorts.ArchivePort, host1, host1r, host2, host2r)
            };

            var uc = RemoteServiceUserContext.AnonymousWithApiKey("vj6kzmT7jQ6eU5h81kKbfnDR8zYpxYYJUF1uPzHtrWC03TyGzf7DvmDekjqHms34RJuoX018xmx84I0Oc2VeUQ==|YLZt71BJbQHGxSiD3gXh+u7HCdVan2K/XxVJoWncubbAAjLeo5uOnKFmQ7n0LmQa/oUelD2UeKTLhft3ZaJ7/Rl4T1/mdVVz");
            return new ScadaArchiveRemoteServiceBuilder(scp, uc);
        }

        static ScadaServerRemoteServiceBuilder ServerBuilder(string host1, string host1r = null, string host2 = null, string host2r = null)
        {
            var scp = new ScadaServerConnectionProperties
            {
                Hosts = RemoteServiceConnectionProperties.CreateHosts(WellKnownPorts.ServerPort, host1, host1r, host2, host2r),
                SendStatisticsInterval = TimeSpan.FromSeconds(10)
            };
            var uc = RemoteServiceUserContext.AnonymousWithApiKey("vj6kzmT7jQ6eU5h81kKbfnDR8zYpxYYJUF1uPzHtrWC03TyGzf7DvmDekjqHms34RJuoX018xmx84I0Oc2VeUQ==|YLZt71BJbQHGxSiD3gXh+u7HCdVan2K/XxVJoWncubbAAjLeo5uOnKFmQ7n0LmQa/oUelD2UeKTLhft3ZaJ7/Rl4T1/mdVVz");
            return new ScadaServerRemoteServiceBuilder(scp, uc);
        }

        public async Task CreateArchiveHost(string IP)
        {
            Scada.ConnectV.ScadaArchiveHost? archiveHost = ScadaVConnection.ArchiveBuilder(IP).CreateHost();
            archiveConnection = await archiveHost.GetAccessibleAsync();
        }

        public async Task CreateServerHost(string IP)
        {
            var serverGroup = ServerBuilder(IP).CreateGroup();
            serverConnection = await serverGroup.GetAccessibleAsync();
        }

        #region TimeIntervalFunction
        public async Task<TimeSpan> TimeInIntervalFloat(int TagId, float minBorder, float maxBorder, DateTime startTime, DateTime endTime)
        {
            try
            {
                List<TagValue> values = await ReadValuesFromArchive(TagId, startTime, endTime);
                List<TagValue> sortedValues = (from val in values orderby val.TimeStamp select val).ToList<TagValue>();
                float v;
                bool inside = false;
                DateTime open = new DateTime();
                DateTime close = new DateTime();
                TimeSpan ans = new TimeSpan();
                for (int i = 0; i < sortedValues.Count; i++)
                {
                    v = Convert.ToSingle(sortedValues[i].Value);
                    if (v >= minBorder && v <= maxBorder && !inside)
                    {
                        inside = true;
                        open = sortedValues[i].TimeStamp;
                    }
                    if ((inside && (v < minBorder || v > maxBorder)) || (i == values.Count - 1 && inside))
                    {
                        inside = false;
                        close = sortedValues[i].TimeStamp;
                        ans += close - open;
                    }
                }
                return ans;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Наличие выхода за коридор
        public async Task<bool> ValueOutOfBorders(int TagId, DateTime startTime, DateTime endTime, float leftBorder, float rightBorder)
        {
            try
            {
                List<TagValue> values = await ReadValuesFromArchive(TagId, startTime, endTime);
                for (int i = 0; i < values.Count; i++)
                {
                    if (Convert.ToSingle(values[i].Value) < leftBorder || Convert.ToSingle(values[i].Value) > rightBorder)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region F_WRITETEKVALUE
        public async Task<bool> WriteVariable(int tagId, float val, DateTime timeStamp)
        {
            List<TagValue> list = new List<TagValue>();
            list.Add(new TagValue(tagId, val, timeStamp.ToUniversalTime()));
            IReadOnlyCollection<TagValue> values = new TagValues(list);
            ArchiveDataPack dataPack = new ArchiveDataPack(1, values);
            bool access = archiveConnection.StateChecker.IsAccessible;
            if (!access)
            {
                throw new Exception("Нет связи с архивной станцией!");
            }
            var res = await archiveConnection.Service.WriteValues(dataPack);
            bool access1 = serverConnection.StateChecker.IsAccessible;
            if (!access1)
            {
                throw new Exception("Нет связи со шлюзом!");
            }
            var resServ = await serverConnection.Service.WriteValues(values);

            if (res.IsOk)
                Logger.Trace("Запись значения по тегу " + tagId.ToString() + " прошла успешно");
            else
                Logger.Trace("Запись значения по тегу " + tagId.ToString() + " прошла неудачно");

            return res.IsOk;
        }

        public async Task<bool> WriteVariable(int tagId, bool val, DateTime timeStamp)
        {
            List<TagValue> list = new List<TagValue>();
            list.Add(new TagValue(tagId, val, timeStamp.ToUniversalTime()));
            IReadOnlyCollection<TagValue> values = new TagValues(list);
            ArchiveDataPack dataPack = new ArchiveDataPack(1, values);
            bool access = archiveConnection.StateChecker.IsAccessible;
            if (!access)
            {
                throw new Exception("Нет связи с архивной станцией!");
            }
            var res = await archiveConnection.Service.WriteValues(dataPack);
            bool access1 = serverConnection.StateChecker.IsAccessible;
            if (!access1)
            {
                throw new Exception("Нет связи со шлюзом!");
            }
            var resServ = await serverConnection.Service.WriteValues(values);

            if (res.IsOk)
                Logger.Trace("Запись значения по тегу " + tagId.ToString() + " прошла успешно");
            else
                Logger.Trace("Запись значения по тегу " + tagId.ToString() + " прошла неудачно");

            return res.IsOk;
        }

        public async Task<bool> WriteVariable(int tagId, int val, DateTime timeStamp)
        {
            List<TagValue> list = new List<TagValue>();
            list.Add(new TagValue(tagId, val, timeStamp.ToUniversalTime()));
            IReadOnlyCollection<TagValue> values = new TagValues(list);
            ArchiveDataPack dataPack = new ArchiveDataPack(1, values);
            bool access = archiveConnection.StateChecker.IsAccessible;
            if (!access)
            {
                throw new Exception("Нет связи с архивной станцией!");
            }
            var resArch = await archiveConnection.Service.WriteValues(dataPack);
            bool access1 = serverConnection.StateChecker.IsAccessible;
            if (!access1)
            {
                throw new Exception("Нет связи со шлюзом!");
            }
            var resServ = await serverConnection.Service.WriteValues(values);

            if (resArch.IsOk)
                Logger.Trace("Запись значения по тегу " + tagId.ToString() + " прошла успешно");
            else
                Logger.Trace("Запись значения по тегу " + tagId.ToString() + " прошла неудачно");

            return resArch.IsOk;
        }
        #endregion

        public async Task<TagValue> ReadVariableFromServer(int tagId)
        {
            if (serverConnection == null)
            {
                throw new Exception("serverConnection имеет значение null!");
            }
            bool access = serverConnection.StateChecker.IsAccessible;
            if (!access)
            {
                throw new Exception("Нет связи со шлюзом!");
            }
            List<int> list = new List<int>();
            list.Add(tagId);
            IReadOnlyCollection<int> tagIds = list;
            var subscribeRes = await serverConnection.Service.SubscribeRead(tagIds);
            if (!subscribeRes.err.IsOk)
            {
                Logger.Trace("Ошибка подписки");
                while (!subscribeRes.err.IsOk)
                    subscribeRes = await serverConnection.Service.SubscribeRead(tagIds);
            }
            long groupId = subscribeRes.groupId;
            var res = await serverConnection.Service.ReadValues(groupId, true);
            Logger.Trace("Чтения из шлюза по тегу" + tagId.ToString() + " - " + res.err.ErrorMessage + " " + res.err.ErrorCategory.ToString());
            IReadOnlyCollection<TagValue> resultCollection = res.values;
            List<TagValue> values = resultCollection.ToList();
            await serverConnection.Service.UnsubscribeRead(groupId);
            if (values.Count != 0)
            {
                Logger.Trace("В результате чтения из шлюза по тегу " + tagId.ToString() + " вернулось значение " + values[values.Count - 1].Value.ToString());
                return values[values.Count - 1];
            }
            else
            {
                Logger.Trace("В результате чтения из шлюза по тегу " + tagId.ToString() + " вернулась пустая коллекция");
                throw new Exception("В результате чтения из шлюза по тегу " + tagId + " вернулась пустая коллекция");
            }
        }

        public async Task<bool> ReadDiscretFromServer(int tagId)
        {
            TagValue tagValue = await ReadVariableFromServer(tagId);
            return Convert.ToBoolean(tagValue.Value);
        }

        public async Task<bool> ReadDiscretFromServerForDiscretSignal(int tagId)
        {
            TagValue tagValue = await ReadVariableFromServer(tagId);
            return Convert.ToBoolean(tagValue.Value);
        }

        //public async Task<bool> ReadDiscretFromServerForDiscretSignal(int tagId)
        //{
        //    var values = new List<TagValue>();
        //    for (int i = 0; i < 3; i++)
        //    {
        //        //Thread.Sleep(1000 / 3);
        //        await Task.Delay(1000 / 3);
        //        values.Add(await ReadVariableFromServer(tagId));
        //    }
        //    //values.Add(await ReadVariableFromServer(tagId));
        //    //await Task.Delay(1000);
        //    //values.Add(await ReadVariableFromServer(tagId));
        //    //await Task.Delay(1000);
        //    //values.Add(await ReadVariableFromServer(tagId));
        //    return values.Where(v => Convert.ToBoolean(v.Value) == true).Count() >= 2 ? true : false;
        //}

        public async Task<List<TagValue>> ReadValuesFromArchive(int tagId, DateTime startTime, DateTime endTime)
        {
            ArchiveTagId _tagId = new ArchiveTagId(1, tagId);
            ArchiveReadingOptionsAggFunc options = new ArchiveReadingOptionsAggFunc();
            options.EndTimestamp = endTime.ToUniversalTime();
            bool access = archiveConnection.StateChecker.IsAccessible;
            if (!access)
            {
                throw new Exception("Нет связи с архивной станцией!");
            }
            var res = await archiveConnection.Service.ReadValues(_tagId, startTime.ToUniversalTime(), options);
            IReadOnlyCollection<TagValue> resultCollection = res.tagValues;
            if (resultCollection == null)
            {
                Logger.Trace("Нет данных по тегу " + tagId.ToString() + " в архиве");
                throw new Exception("Нет данных по тэгу " + tagId);
            }
            else
            {
                if (resultCollection.Count == 0)
                {
                    Logger.Trace("В результате чтения из архива по тэгу " + tagId.ToString() + " вернулась пустая коллекция");
                    throw new Exception("В результате чтения из архива по тэгу " + tagId + " вернулась пустая коллекция");
                }
            }
            List<TagValue> values = resultCollection.ToList();
            Logger.Trace("В результате чтения из архива по тэгу " + tagId.ToString() + " вернулась коллекция из " + values.Count.ToString() + " элементов");
            return values;
        }

        #region Основные критерии надёжности пуска / останова
        #region Перегрузки функции Диапазон
        public async Task<AnalogSignalMark> Diapazon(int tagId, DateTime startTime, DateTime endTime, float left, float right, bool useAbsValues)
        {
            try
            {
                List<TagValue> values = await ReadValuesFromArchive(tagId, startTime, endTime);
                float maxVal;
                if (useAbsValues)
                {
                    left = Math.Abs(left);
                    right = Math.Abs(right);
                    //Найти максимум
                    maxVal = Math.Abs(Convert.ToSingle(values[0].Value));
                    for (int i = 1; i < values.Count; i++)
                    {
                        float cur = Math.Abs(Convert.ToSingle(values[i].Value));
                        if (cur > maxVal) maxVal = cur;
                    }
                }
                else
                {
                    //Найти максимум
                    maxVal = Convert.ToSingle(values[0].Value);
                    for (int i = 1; i < values.Count; i++)
                    {
                        float cur = Convert.ToSingle(values[i].Value);
                        if (cur > maxVal) maxVal = cur;
                    }
                }
                //Сравнить максимум с границами
                if (maxVal >= right)
                {
                    return AnalogSignalMark.NeYd;
                }
                else if (maxVal <= left)
                {
                    return AnalogSignalMark.Otl;
                }
                else
                {
                    return AnalogSignalMark.Yd;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Перегрузки функции Настраиваемый Диапазон
        public async Task<AnalogSignalMark> CustomizableDiapazon(int tagId, DateTime startTime, DateTime endTime, float left, float right, int exitId)
        {
            try
            {
                //Ищем начало и конец временного промежутка, где exitId было true
                List<TagValue> boolValues = await ReadValuesFromArchive(exitId, startTime, endTime);
                DateTime start = DateTime.MinValue;
                DateTime end = DateTime.MinValue;
                for (int i = 0; i < boolValues.Count; i++)
                {
                    if (start == DateTime.MinValue)
                    {
                        if (Convert.ToBoolean(boolValues[i].Value))
                        {
                            start = boolValues[i].TimeStamp;
                        }
                    }
                    else
                    {
                        if (!Convert.ToBoolean(boolValues[i].Value))
                        {
                            end = boolValues[i].TimeStamp;
                            break;
                        }
                    }
                }
                //Анализируем значения на полученном промежутке
                List<TagValue> values = await ReadValuesFromArchive(tagId, start, end);
                float maxVal = Convert.ToSingle(values[0].Value);
                for (int i = 1; i < values.Count; i++)
                {
                    float cur = Convert.ToSingle(values[i].Value);
                    if (cur > maxVal) maxVal = cur;
                }
                //Сравнить максимум с границами
                if (maxVal >= right)
                {
                    return AnalogSignalMark.NeYd;
                }
                else if (maxVal <= left)
                {
                    return AnalogSignalMark.Otl;
                }
                else
                {
                    return AnalogSignalMark.Yd;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        public async Task<AnalogSignalMark> DiapazonWithParams(int tagId, DateTime startTime, DateTime endTime, float left, float right1, float right2, float signalValue1, float signalValue2, int signalId)
        {
            try
            {
                List<TagValue> values = await ReadValuesFromArchive(tagId, startTime, endTime);
                //Ищем глобальный максимум
                float maxVal = Convert.ToSingle(values[0].Value);
                for (int i = 1; i < values.Count; i++)
                {
                    float cur = Convert.ToSingle(values[i].Value);
                    if (cur > maxVal) maxVal = cur;
                }
                if (maxVal <= left) return AnalogSignalMark.Otl;
                else
                {
                    if (maxVal >= right2) return AnalogSignalMark.NeYd;
                    else
                    {
                        //разбиваем значения на два интервала по значению signalId
                        DateTime border = DateTime.MinValue;//граница, разделяющая временные интервалы
                        List<TagValue> paramValues = await ReadValuesFromArchive(signalId, startTime, endTime);
                        for (int i = 0; i < paramValues.Count; i++)
                        {
                            if (Convert.ToSingle(paramValues[i].Value) >= signalValue1)
                            {
                                border = paramValues[i].TimeStamp;
                                break;
                            }
                        }
                        for (int i = 0; i < values.Count; i++)
                        {
                            if (values[i].TimeStamp < border)
                            {
                                if (Convert.ToSingle(values[i].Value) >= right1) return AnalogSignalMark.NeYd;
                            }
                            else
                            {
                                if (Convert.ToSingle(values[i].Value) >= right1) return AnalogSignalMark.NeYd;
                            }
                        }
                        return AnalogSignalMark.Otl;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<AnalogSignalMark> TotalOrRepeatedExceeding(int tagId, DateTime startTime, DateTime endTime, float ystavka, TimeSpan summTime, float prev)
        {
            try
            {
                List<TagValue> values = await ReadValuesFromArchive(tagId, startTime, endTime);
                //Вычислить суммарное время превышения уставки
                DateTime startUp = DateTime.MinValue;
                DateTime endUp = DateTime.MinValue;
                TimeSpan summPrev = new TimeSpan();//суммарное время превышения уставки
                TimeSpan curPrev = new TimeSpan();//текущее обнаруженное время превышения уставки
                bool atTheTop = false;
                for (int i = 0; i < values.Count - 1; i++)
                {
                    if (!atTheTop)
                    {
                        if (Convert.ToSingle(values[i].Value) <= ystavka && Convert.ToSingle(values[i + 1].Value) >= ystavka)
                        {
                            //начинаем превышать уставку
                            atTheTop = true;
                            startUp = values[i + 1].TimeStamp;
                        }
                    }
                    else
                    {
                        if (Convert.ToSingle(values[i].Value) >= ystavka && Convert.ToSingle(values[i + 1].Value) <= ystavka)
                        {
                            //спускаемся ниже уставки
                            atTheTop = false;
                            endUp = values[i].TimeStamp;
                            curPrev = endUp - startUp;
                            summPrev += curPrev;
                        }
                    }
                }
                if (summPrev.TotalMinutes >= 20) return AnalogSignalMark.NeYd;
                //Проверить на неоднократное превышение уставки на величину prev
                startUp = DateTime.MinValue;
                endUp = DateTime.MinValue;
                summPrev = new TimeSpan();
                curPrev = new TimeSpan();
                atTheTop = false;
                int prevCount = 0;
                for (int i = 0; i < values.Count - 1; i++)
                {
                    if (!atTheTop)
                    {
                        if (Convert.ToSingle(values[i].Value) <= ystavka + prev && Convert.ToSingle(values[i + 1].Value) >= ystavka + prev)
                        {
                            //начинаем превышать уставку
                            atTheTop = true;
                            prevCount++;
                            if (prevCount > 1) return AnalogSignalMark.NeYd;
                        }
                    }
                    else
                    {
                        if (Convert.ToSingle(values[i].Value) >= ystavka && Convert.ToSingle(values[i + 1].Value) <= ystavka)
                        {
                            //спускаемся ниже уставки
                            atTheTop = false;
                        }
                    }
                }
                if (prevCount <= 1) return AnalogSignalMark.Otl;
                else return AnalogSignalMark.NeYd;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Дополнительные критерии надёжности пуска / останова
        public async Task<AnalogSignalMark> DopDiapazon(int tagId, DateTime startTime, DateTime endTime, float border1, float border2, float border3)
        {
            try
            {
                List<TagValue> values = await ReadValuesFromArchive(tagId, startTime, endTime);
                float maxVal = Convert.ToSingle(values[0].Value);
                for (int i = 1; i < values.Count; i++)
                {
                    float cur = Convert.ToSingle(values[i].Value);
                    if (cur > maxVal) maxVal = cur;
                }
                if (maxVal <= border1) return AnalogSignalMark.Otl;
                else
                {
                    if (maxVal > border3) return AnalogSignalMark.NeYd;
                    else
                    {
                        if (maxVal <= border2) return AnalogSignalMark.Xor;
                        else return AnalogSignalMark.Yd;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<AnalogSignalMark> DopAbsDiapazon(int tagId, DateTime startTime, DateTime endTime, float normalVal, float absValOtl, float absValNeYd)
        {
            try
            {
                List<TagValue> values = await ReadValuesFromArchive(tagId, startTime, endTime);
                bool otl = true;
                for (int i = 0; i < values.Count; i++)
                {
                    if (Math.Abs(Convert.ToSingle(values[i].Value) - normalVal) >= absValNeYd)
                        return AnalogSignalMark.NeYd;
                    if (Math.Abs(Convert.ToSingle(values[i].Value) - normalVal) > absValNeYd) otl = false;
                }
                if (otl) return AnalogSignalMark.Otl;
                else return AnalogSignalMark.Yd;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        private Scada.ConnectV.ScadaArchiveConnection? archiveConnection;
        private Scada.ConnectV.ScadaServerConnection? serverConnection;
    }
}
