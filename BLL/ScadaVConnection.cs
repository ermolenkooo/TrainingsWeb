using Scada.Connect.Base;
using Scada.ConnectV;
using Scada.Interfaces.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Models;

namespace BLL
{
    public class ScadaVConnection
    {
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
            int a = 9;
        }

        public async Task CreateServerHost(string IP)
        {
            var serverGroup = ServerBuilder(IP).CreateGroup();
            serverConnection = await serverGroup.GetAccessibleAsync();
        }

        #region MinMaxFunctions
        public async Task<float> MaxOnIntervalFloat(int TagId, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(TagId, startTime, endTime);
            float max = Convert.ToSingle(values[0].Value);
            float cur;
            //выбор максимального значения из коллекции
            for (int i = 1; i < values.Count; i++)
            {
                cur = Convert.ToSingle(values[i].Value);
                if (cur > max) max = cur;
            }
            return max;
        }

        public async Task<int> MaxOnIntervalInt(int TagId, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(TagId, startTime, endTime);
            int max = Convert.ToInt32(values[0].Value);
            int cur;
            //выбор максимального значения из коллекции
            for (int i = 1; i < values.Count; i++)
            {
                cur = Convert.ToInt32(values[i].Value);
                if (cur > max) max = cur;
            }
            return max;
        }

        public async Task<float> MinOnIntervalFloat(int TagId, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(TagId, startTime, endTime);
            float min = Convert.ToSingle(values[0].Value);
            float cur;
            //выбор максимального значения из коллекции
            for (int i = 1; i < values.Count; i++)
            {
                cur = Convert.ToSingle(values[i].Value);
                if (cur < min) min = cur;
            }
            return min;
        }

        public async Task<int> MinOnIntervalInt(int TagId, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(TagId, startTime, endTime);
            int min = Convert.ToInt32(values[0].Value);
            int cur;
            //выбор максимального значения из коллекции
            for (int i = 1; i < values.Count; i++)
            {
                cur = Convert.ToInt32(values[i].Value);
                if (cur < min) min = cur;
            }
            return min;
        }
        #endregion

        #region TimeIntervalFunctions
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

        public async Task<TimeSpan> TimeInIntervalInt(int TagId, int minBorder, int maxBorder, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(TagId, startTime, endTime);
            List<TagValue> sortedValues = (from val in values orderby val.TimeStamp select val).ToList<TagValue>();
            int v;
            bool inside = false;
            DateTime open = new DateTime();
            DateTime close = new DateTime();
            TimeSpan ans = new TimeSpan();
            for (int i = 0; i < sortedValues.Count; i++)
            {
                v = Convert.ToInt32(sortedValues[i].Value);
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

        public async Task<TimeSpan> TimeInIntervalBool(int TagId, bool val, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(TagId, startTime, endTime);
            DateTime open = new DateTime();
            DateTime close = new DateTime();
            TimeSpan ans = new TimeSpan();
            bool v;
            bool inside = false;
            for (int i = 0; i < values.Count; i++)
            {
                v = Convert.ToBoolean(values[i].Value);
                if (v == val && !inside)
                {
                    inside = true;
                    open = values[i].TimeStamp;
                }
                if ((inside && (v != val)) || (i == values.Count - 1 && inside))
                {
                    inside = false;
                    close = values[i].TimeStamp;
                    ans += close - open;
                }
            }
            return ans;
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
            long groupId = subscribeRes.groupId;
            var res = await serverConnection.Service.ReadValues(groupId, true);
            IReadOnlyCollection<TagValue> resultCollection = res.values;
            List<TagValue> values = resultCollection.ToList();
            await serverConnection.Service.UnsubscribeRead(groupId);
            if (values.Count != 0)
                return values[values.Count - 1];
            else
                throw new Exception("В результате чтения из шлюза по тегу " + tagId + " вернулась пустая коллекция");
        }

        public async Task<bool> ReadDiscretFromServer(int tagId)
        {
            TagValue tagValue = await ReadVariableFromServer(tagId);
            return Convert.ToBoolean(tagValue.Value);
        }

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
                throw new Exception("Нет данных по тэгу " + tagId);
            }
            else
            {
                if (resultCollection.Count == 0)
                {
                    throw new Exception("В результате чтения из архива по тэгу " + tagId + " вернулась пустая коллекция");
                }
            }
            List<TagValue> values = resultCollection.ToList();
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
        public async Task<AnalogSignalMark> Diapazon(int tagId, DateTime startTime, DateTime endTime, int left, int right, bool useAbsValues)
        {
            List<TagValue> values = await ReadValuesFromArchive(tagId, startTime, endTime);
            int maxVal;
            if (useAbsValues)
            {
                left = Math.Abs(left);
                right = Math.Abs(right);
                //Найти максимум
                maxVal = Math.Abs(Convert.ToInt32(values[0].Value));
                for (int i = 1; i < values.Count; i++)
                {
                    int cur = Math.Abs(Convert.ToInt32(values[i].Value));
                    if (cur > maxVal) maxVal = cur;
                }
            }
            else
            {
                //Найти максимум
                maxVal = Convert.ToInt32(values[0].Value);
                for (int i = 1; i < values.Count; i++)
                {
                    int cur = Convert.ToInt32(values[i].Value);
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

        public async Task<AnalogSignalMark> CustomizableDiapazon(int tagId, DateTime startTime, DateTime endTime, int left, int right, int exitId)
        {
            //Ищем метку времени, когда exitId изменило значение с true на false
            List<TagValue> boolValues = await ReadValuesFromArchive(exitId, startTime, endTime);
            DateTime border = DateTime.MinValue;

            for (int i = 0; i < boolValues.Count; i++)
            {
                if (!Convert.ToBoolean(boolValues[i].Value)) border = boolValues[i].TimeStamp;
            }
            //Анализируем значения на полученном промежутке
            List<TagValue> values = await ReadValuesFromArchive(tagId, startTime, border);
            int maxVal = Convert.ToInt32(values[0].Value);
            for (int i = 1; i < values.Count; i++)
            {
                int cur = Convert.ToInt32(values[i].Value);
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

        #region F_LIMIT

        /// <summary>
        /// Проверяет, выходил ли параметр за коридор
        /// </summary>
        /// <param name="tag">tagId</param>
        /// <param name="down">нижняя граница коридора</param>
        /// <param name="up">верхняя граница коридора</param>
        /// <param name="startTime">время начала интервала</param>
        /// <param name="endTime">время конца интервала</param>
        /// <returns>true - параметр выходил за коридор, иначе - false</returns>
        public async Task<bool> FlimitReal(int tag, float down, float up, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(tag, startTime, endTime);
            for (int i = 0; i < values.Count; i++)
            {
                float val = Convert.ToSingle(values[i].Value);
                if (val <= down || val >= up) return true;
            }
            return false;
        }

        /// <summary>
        /// Проверяет, выходил ли параметр за коридор
        /// </summary>
        /// <param name="tag">tagId</param>
        /// <param name="down">нижняя граница коридора</param>
        /// <param name="up">верхняя граница коридора</param>
        /// <param name="startTime">время начала интервала</param>
        /// <param name="endTime">время конца интервала</param>
        /// <returns>true - параметр выходил за коридор, иначе - false</returns>
        public async Task<bool> FlimitInt(int tag, int down, int up, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(tag, startTime, endTime);
            for (int i = 0; i < values.Count; i++)
            {
                float val = Convert.ToSingle(values[i].Value);
                if (val <= down || val >= up) return true;
            }
            return false;
        }
        #endregion

        #region F_NUMB
        public async Task<int> NumberOfDeviationsFloat(int tag, float val, string sign, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(tag, startTime, endTime);
            int count = 0;
            if (sign == ">=")
            {
                bool inside = false;
                for (int i = 0; i < values.Count; i++)
                {
                    if (!inside && Convert.ToSingle(values[i].Value) >= val)
                    {
                        inside = true;
                    }
                    if (inside && (Convert.ToSingle(values[i].Value) <= val || i == values.Count - 1))
                    {
                        inside = false;
                        count++;
                    }
                }
            }
            else if (sign == "<=")
            {
                bool inside = false;
                for (int i = 0; i < values.Count; i++)
                {
                    if (!inside && Convert.ToSingle(values[i].Value) <= val)
                    {
                        inside = true;
                    }
                    if (inside && (Convert.ToSingle(values[i].Value) >= val || i == values.Count - 1))
                    {
                        inside = false;
                        count++;
                    }
                }
            }
            else
            {
                throw new Exception("Не коректный знак сравнения в методе NumberOfDeviations.");
            }
            return count;
        }

        public async Task<int> NumberOfDeviationsInt(int tag, int val, string sign, DateTime startTime, DateTime endTime)
        {
            List<TagValue> values = await ReadValuesFromArchive(tag, startTime, endTime);
            int count = 0;
            if (sign == ">=")
            {
                bool inside = false;
                for (int i = 0; i < values.Count; i++)
                {
                    if (!inside && Convert.ToInt32(values[i].Value) >= val)
                    {
                        inside = true;
                    }
                    if (inside && (Convert.ToInt32(values[i].Value) <= val || i == values.Count - 1))
                    {
                        inside = false;
                        count++;
                    }
                }
            }
            else if (sign == "<")
            {
                bool inside = false;
                for (int i = 0; i < values.Count; i++)
                {
                    if (!inside && Convert.ToInt32(values[i].Value) <= val)
                    {
                        inside = true;
                    }
                    if (inside && (Convert.ToInt32(values[i].Value) >= val || i == values.Count - 1))
                    {
                        inside = false;
                        count++;
                    }
                }
            }
            else
            {
                throw new Exception("Не коректный знак сравнения в методе NumberOfDeviations.");
            }
            return count;
        }
        #endregion

        private Scada.ConnectV.ScadaArchiveConnection? archiveConnection;
        private Scada.ConnectV.ScadaServerConnection? serverConnection;
    }
}
