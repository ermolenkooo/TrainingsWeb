using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IDbRepos //интерфейс для взаимодействия с репозиториями
    {
        IAnalogSignalRepository AnalogSignals { get; }
        IDiscretFromAnalogSignalRepository DiscretFromAnalogSignals { get; }
        IDiscretSignalRepository DiscretSignals { get; }
        IDoubleDiscretSignalRepository DoubleDiscretSignals { get; }
        IGroupOfDiscretSignalsRepository GroupsOfDiscretSignals { get; }
        ILogicVariableRepository LogicVariables { get; }
        IOperationRepository Operations { get; }
        IOperationWithConditionRepository OperationsWithCondition { get; }
        ITimeBorderRepository TimeBorders { get; }
        ITrainingRepository Trainings { get; }
        IParameterRepository<Entities.Range> Ranges { get; }
        IParameterRepository<AdjustableRange> AdjustableRanges { get; }
        IParameterRepository<RangeWithParameters> RangesWithParameters { get; }
        IParameterRepository<Exceeding> Exceedings { get; }
        IParameterRepository<DopRange> DopRanges { get; }
        IParameterRepository<MaintainingLevel> MaintainingsLevel { get; }
        IParameterRepository<TimeInInterval> TimeInIntervals { get; }
        IParameterRepository<ExitToTheCorridor> ExitsToTheCorridor { get; }
    }
}
