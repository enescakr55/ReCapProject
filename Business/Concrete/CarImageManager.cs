﻿using Business.Abstract;
using Business.BusinessAspects;
using Business.Constants;
using Core.Results.Utilities;
using Core.Utilities.Business;
using Core.Utilities.Results.DataResults;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Concrete
{
    class CarImageManager : ICarImageService
    {
        ICarImageDal _carImageDal;
        public CarImageManager(ICarImageDal carImageDal)
        {
            _carImageDal = carImageDal;
        }
        public string DefaultCarImagePath = "/CarImages/default.jpg";
        //[SecuredOperation("admin")]
        public IResult Add(CarImage carImage,IFormFile resim)
        {
            var result = BusinessRules.Run(CarImageCountLessOrEqualThan5(carImage.CarId));
            if(result != null)
            {
                return result;
            }
            
            var Upload = FileOperations.UploadImage(resim);
            if (Upload.Success)
            {
                carImage.Date = DateTime.Now;
                carImage.ImagePath = Upload.Message;
                _carImageDal.Add(carImage);
                return new SuccessResult(Messages.Added);
            }
            return new ErrorResult("Dosya yüklenemedi");

        }
        //[SecuredOperation("admin")]
        public IResult Delete(CarImage carImage)
        {
            var carimginfo = _carImageDal.Get(p => p.Id == carImage.Id);
            if (FileOperations.DeleteImage(carimginfo.ImagePath).Success)
            {
                _carImageDal.Delete(carImage);
                return new SuccessResult(Messages.Deleted);
            }
            return new ErrorResult();

        }

        public IDataResult<List<CarImage>> GetAll()
        {
            return new SuccessDataResult<List<CarImage>>(_carImageDal.GetAll());
        }

        public IDataResult<List<CarImage>> GetByCarId(int id)
        {
            var CarImages = _carImageDal.GetAll(p => p.CarId == id);
            if(CarImages.Count == 0)
            {
                CarImage carImage = new CarImage { CarId = id, ImagePath = DefaultCarImagePath};
                CarImages.Add(carImage);
            }
            return new SuccessDataResult<List<CarImage>>(CarImages);
        }

        public IDataResult<CarImage> GetById(int id)
        {
            return new SuccessDataResult<CarImage>(_carImageDal.Get(p => p.Id == id));
        }
        [SecuredOperation("admin")]
        public IResult Update(CarImage carImage, IFormFile resim)
        {
            var Upload = FileOperations.UploadImage(resim);
            var CarImageInfo = _carImageDal.Get(p => p.Id == carImage.Id);
            if (Upload.Success)
            {
                FileOperations.DeleteImage(CarImageInfo.ImagePath);
                carImage.ImagePath = Upload.Message;
                carImage.Date = DateTime.Now;
            }
            _carImageDal.Update(carImage);
            return new SuccessResult(Messages.Updated);
        }
        public IResult CarImageCountLessOrEqualThan5(int carId)
        {
            List<CarImage> ResimListesi = _carImageDal.GetAll(c=>c.CarId == carId);
            int Resimsayisi = ResimListesi.Count;
            if (Resimsayisi >= 5)
            {
                return new ErrorResult(Messages.ImageCountError);
            }
            return new SuccessResult();
        }
    }
}
