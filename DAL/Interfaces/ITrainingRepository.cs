﻿using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ITrainingRepository
    {
        List<Training> GetList();
        Training GetItem(int id);
    }
}
