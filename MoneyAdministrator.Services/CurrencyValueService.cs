﻿using MoneyAdministrator.DataAccess.Interfaces;
using MoneyAdministrator.DataAccess;
using MoneyAdministrator.Models;
using MoneyAdministrator.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyAdministrator.Services
{
    public class CurrencyValueService : IService<CurrencyValue>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CurrencyValueService(string databasePath)
        {
            _unitOfWork = new UnitOfWork(databasePath);
        }

        public CurrencyValueService(IUnitOfWork UnitOfWork)
        {
            _unitOfWork = UnitOfWork;
        }

        public List<CurrencyValue> GetAll()
        {
            return _unitOfWork.CurrencyValueRepository.GetAll().OrderByDescending(x => x.Date).ToList();
        }

        public CurrencyValue? GetByPeriod(DateTime period)
        {
            return _unitOfWork.CurrencyValueRepository.GetAll().Where(x => x.Date == period).FirstOrDefault();
        }

        public CurrencyValue Get(int id)
        {
            return _unitOfWork.CurrencyValueRepository.GetById(id);
        }

        public void Insert(CurrencyValue model)
        {
            //Valido el modelo
            Utilities.ModelValidator.Validate(model);

            //Compruebo si el objeto ya existe
            var item = _unitOfWork.CurrencyValueRepository.GetAll()
                .Where(x => x.Date.Year == model.Date.Year && x.Date.Month == model.Date.Month).FirstOrDefault();

            if (item != null)
            {
                //Si el objeto ya existe, añado el id en el modelo
                model.Id = item.Id;
            }
            else
            {
                //Agrego el modelo a la base de datos
                _unitOfWork.CurrencyValueRepository.Insert(model);
                _unitOfWork.Save();
            }
        }

        public void Update(CurrencyValue model)
        {
            //Valido el modelo
            Utilities.ModelValidator.Validate(model);

            var item = _unitOfWork.CurrencyValueRepository.GetById(model.Id);
            if (item != null)
            {
                _unitOfWork.CurrencyValueRepository.Update(model);
                _unitOfWork.Save();
            }
        }

        public void Delete(CurrencyValue model)
        {
            var item = _unitOfWork.CurrencyValueRepository.GetById(model.Id);
            if (item != null)
            {
                _unitOfWork.CurrencyValueRepository.Delete(item);
                _unitOfWork.Save();
            }
        }
    }
}
