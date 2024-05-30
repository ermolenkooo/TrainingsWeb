using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Repositories.SQLite;
using BLL.Models;
using DAL.Entities;
using Microsoft.Data.Sqlite;

namespace BLL.Operations
{
    public class TrainingDbOperations
    {
        private IDbRepos _db;

        public TrainingDbOperations(MyOptions options)
        {
            _db = options.DbRepos;
        }

        #region Тренировки
        public List<TrainingModel> SelectAllTrainings()
        {
            return _db.Trainings.GetList().Select(t => new TrainingModel(t)).ToList();
        }

        public TrainingModel SelectTrainingById(int id)
        {
            return new TrainingModel(_db.Trainings.GetItem(id));
        }
        #endregion

        #region Операции
        public List<OperationModel> SelectOperationsWithTrainingId(int trainingId)
        {
            return _db.Operations.GetList(trainingId).Select(o => new OperationModel(o)).ToList();
        }
        #endregion

        #region Дискретные сигналы, полученные из аналоговых
        public List<DiscretFromAnalogSignalModel> SelectAllDiscretFromAnalogSignals(int trainingId, int isInGroup)
        {
            return _db.DiscretFromAnalogSignals.GetList(trainingId, isInGroup).Select(d => new DiscretFromAnalogSignalModel(d)).ToList();
        }

        public List<DiscretFromAnalogSignalModel> SelectSignalsStartSub(int id)
        {
            return _db.DiscretFromAnalogSignals.GetItemsStartSub(id).Select(d => new DiscretFromAnalogSignalModel(d)).ToList();
        }

        public List<DiscretFromAnalogSignalModel> SelectSignalsEndSub(int id)
        {
            return _db.DiscretFromAnalogSignals.GetItemsEndSub(id).Select(d => new DiscretFromAnalogSignalModel(d)).ToList();
        }
        #endregion

        #region Дискретные сигналы
        public List<DiscretSignalModel> SelectAllDiscretSignals(int trainingId)
        {
            return _db.DiscretSignals.GetList(trainingId).Select(d => new DiscretSignalModel(d)).ToList();
        }
        #endregion

        #region Двойные дискретные сигналы
        public List<DoubleDiscretSignalModel> SelectAllDoubleDiscretSignals(int trainingId)
        {
            return _db.DoubleDiscretSignals.GetList(trainingId).Select(d => new DoubleDiscretSignalModel(d)).ToList();
        }
        #endregion

        #region Группы дискретных сигналов
        public List<GroupOfDiscretSignalsModel> SelectGroupsByTrainingId(int trainingId)
        {
            var groupsOfDiscretSignals = _db.GroupsOfDiscretSignals.GetList(trainingId).Select(g => new GroupOfDiscretSignalsModel(g)).ToList();
            foreach (var group in groupsOfDiscretSignals)
            {
                group.StartLogicVariables = SelectLogicVariableStartSub(group.StartSubGroupId);
                group.EndLogicVariables = SelectLogicVariableEndSub(group.EndSubGroupId);
                group.StartSignals = SelectSignalsStartSub(group.StartSubGroupId);
                group.EndSignals = SelectSignalsEndSub(group.EndSubGroupId);
            }
            return groupsOfDiscretSignals;
        }
        #endregion

        #region Логические переменные
        public LogicVariableModel GetLogicVariableById(int id)
        {
            return new LogicVariableModel(_db.LogicVariables.GetItem(id));
        }

        public List<LogicVariableModel> SelectLogicVariableStartSub(int id)
        { 
            return _db.LogicVariables.GetItemsStartSub(id).Select(l => new LogicVariableModel(l)).ToList();
        }

        public List<LogicVariableModel> SelectLogicVariableEndSub(int id)
        {
            return _db.LogicVariables.GetItemsEndSub(id).Select(l => new LogicVariableModel(l)).ToList();
        }
        #endregion

        #region Операции с условием
        public List<OperationWithConditionModel> SelectOperationsWithConditionWithTrainingId(int trainingId)
        {
            return _db.OperationsWithCondition.GetList(trainingId).Select(o => new OperationWithConditionModel(o)).ToList();
        }
        #endregion

        #region Временные границы
        public TimeBordersModel SelectTimeBordersWithDiscretId(int signalId, int type)
        {
            return new TimeBordersModel(_db.TimeBorders.GetItem(signalId, type));
        }
        #endregion

        #region Аналоговые сигналы
        public List<AnalogSignalModel> SelectAllAnalogSignals(int trainingId, int type)
        {
            return _db.AnalogSignals.GetList(trainingId, type).Select(a => new AnalogSignalModel(a)).ToList();
        }
        #endregion

        #region Редактирование параметров функций аналоговых сигналов

        #region Диапазон

        public RangeModel SelectRange(int signalId)
        {
            return new RangeModel(_db.Ranges.GetItem(signalId));
        }

        #endregion

        #region Настраиваемый диапазон

        public AdjustableRangeModel SelectAdjustableRange(int signalId)
        {
            return new AdjustableRangeModel(_db.AdjustableRanges.GetItem(signalId));
        }

        #endregion

        #region Диапазон с параметрами

        public RangeWithParametersModel SelectRangeWithParameters(int signalId)
        {
            return new RangeWithParametersModel(_db.RangesWithParameters.GetItem(signalId));
        }

        #endregion

        #region Суммарное или неоднократное превышение уставки

        public ExceedingModel SelectExceeding(int signalId)
        {
            return new ExceedingModel(_db.Exceedings.GetItem(signalId));
        }

        #endregion

        #region Диапазон дополнительных критериев надёжности

        public DopRangeModel SelectDopRange(int signalId)
        {
            return new DopRangeModel(_db.DopRanges.GetItem(signalId));
        }

        #endregion

        #region Поддержание заданного уровня

        public MaintainingLevelModel SelectMaintainingLevel(int signalId)
        {
            return new MaintainingLevelModel(_db.MaintainingsLevel.GetItem(signalId));
        }

        #endregion

        #region Время пребывания в интервале

        public TimeInIntervalModel SelectTimeInInterval(int signalId)
        {
            return new TimeInIntervalModel(_db.TimeInIntervals.GetItem(signalId));
        }

        #endregion

        #region Наличие выхода за коридор

        public ExitToTheCorridorModel SelectExitToTheCorridor(int signalId)
        {
            return new ExitToTheCorridorModel(_db.ExitsToTheCorridor.GetItem(signalId));
        }

        #endregion

        #endregion
    }
}
