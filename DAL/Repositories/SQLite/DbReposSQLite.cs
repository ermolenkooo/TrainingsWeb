using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.SQLite
{
    public class DbReposSQLite : IDbRepos
    {
        private SqliteConnection _connection;
        private AnalogSignalRepositorySQLite _analogSignalRepository;
        private DiscretFromAnalogSignalRepositorySQLite _discretFromAnalogSignalRepository;
        private DiscretSignalRepositorySQLite _discretSignalRepository;
        private DoubleDiscretSignalRepositorySQLite _doubleDiscretSignalRepository;
        private GroupOfDiscretSignalsRepositorySQLite _groupOfDiscretSignalsRepository;
        private LogicVariableRepositorySQLite _logicVariableRepository;
        private OperationRepositorySQLite _operationRepository;
        private OperationWithConditionRepositorySQLite _operationWithConditionRepository;
        private TimeBorderRepositorySQLite _timeBorderRepository;
        private TrainingRepositorySQLite _trainingRepository;
        private AdjustableRangeRepositorySQLite _adjustableRangeRepository;
        private DopRangeRepositorySQLite _dopRangeRepository;
        private ExceedingRepositorySQLite _exceedingRepository;
        private ExitToTheCorridorRepositorySQLite _exitToTheCorridorRepository;
        private MaintainingLevelRepositorySQLite _maintainingLevelRepository;
        private RangeRepositorySQLite _rangeRepository;
        private RangeWithParametersRepositorySQLite _rangeWithParametersRepository;
        private TimeInIntervalRepositorySQLite _timeInIntervalRepository;

        public DbReposSQLite(string path)
        {
            _connection = new SqliteConnection("Data Source=" + path);
            _connection.Open();
        }

        public ITrainingRepository Trainings
        {
            get
            {
                _trainingRepository ??= new TrainingRepositorySQLite(_connection);
                return _trainingRepository;
            }
        }

        public IOperationRepository Operations
        {
            get
            {
                _operationRepository ??= new OperationRepositorySQLite(_connection);
                return _operationRepository;
            }
        }

        public IDiscretFromAnalogSignalRepository DiscretFromAnalogSignals
        {
            get
            {
                _discretFromAnalogSignalRepository ??= new DiscretFromAnalogSignalRepositorySQLite(_connection);
                return _discretFromAnalogSignalRepository;
            }
        }

        public IDiscretSignalRepository DiscretSignals
        {
            get
            {
                _discretSignalRepository ??= new DiscretSignalRepositorySQLite(_connection);
                return _discretSignalRepository;
            }
        }

        public IDoubleDiscretSignalRepository DoubleDiscretSignals
        {
            get
            {
                _doubleDiscretSignalRepository ??= new DoubleDiscretSignalRepositorySQLite(_connection);
                return _doubleDiscretSignalRepository;
            }
        }

        public IGroupOfDiscretSignalsRepository GroupsOfDiscretSignals 
        {
            get
            {
                _groupOfDiscretSignalsRepository ??= new GroupOfDiscretSignalsRepositorySQLite(_connection);
                return _groupOfDiscretSignalsRepository;
            }
        }

        public IOperationWithConditionRepository OperationsWithCondition
        {
            get
            {
                _operationWithConditionRepository ??= new OperationWithConditionRepositorySQLite(_connection);
                return _operationWithConditionRepository;
            }
        }

        public ILogicVariableRepository LogicVariables
        {
            get
            {
                _logicVariableRepository ??= new LogicVariableRepositorySQLite(_connection);
                return _logicVariableRepository;
            }
        }

        public ITimeBorderRepository TimeBorders
        {
            get
            {
                _timeBorderRepository ??= new TimeBorderRepositorySQLite(_connection);
                return _timeBorderRepository;
            }
        }

        public IAnalogSignalRepository AnalogSignals
        {
            get
            {
                _analogSignalRepository ??= new AnalogSignalRepositorySQLite(_connection);
                return _analogSignalRepository;
            }
        }

        public IParameterRepository<Entities.Range> Ranges
        {
            get
            {
                _rangeRepository ??= new RangeRepositorySQLite(_connection);
                return _rangeRepository;
            }
        }

        public IParameterRepository<AdjustableRange> AdjustableRanges
        {
            get
            {
                _adjustableRangeRepository ??= new AdjustableRangeRepositorySQLite(_connection);
                return _adjustableRangeRepository;
            }
        }

        public IParameterRepository<RangeWithParameters> RangesWithParameters
        {
            get
            {
                _rangeWithParametersRepository ??= new RangeWithParametersRepositorySQLite(_connection);
                return _rangeWithParametersRepository; 
            }
        }

        public IParameterRepository<Exceeding> Exceedings
        {
            get
            {
                _exceedingRepository ??= new ExceedingRepositorySQLite(_connection);
                return _exceedingRepository;
            }
        }

        public IParameterRepository<DopRange> DopRanges
        {
            get
            {
                _dopRangeRepository ??=new DopRangeRepositorySQLite(_connection);
                return _dopRangeRepository;
            }
        }

        public IParameterRepository<MaintainingLevel> MaintainingsLevel
        {
            get
            {
                _maintainingLevelRepository ??= new MaintainingLevelRepositorySQLite(_connection);
                return _maintainingLevelRepository;
            }
        }

        public IParameterRepository<TimeInInterval> TimeInIntervals
        {
            get
            {
                _timeInIntervalRepository ??= new TimeInIntervalRepositorySQLite(_connection);
                return _timeInIntervalRepository;
            }
        }

        public IParameterRepository<ExitToTheCorridor> ExitsToTheCorridor
        {
            get
            {
                _exitToTheCorridorRepository ??= new ExitToTheCorridorRepositorySQLite(_connection);
                return _exitToTheCorridorRepository;
            }
        }
    }
}
