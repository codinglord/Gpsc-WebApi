﻿using GpscWebApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace GpscWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    public class PowerPlantController : ApiController
    {
        GpscEntities _Db;
        GpscEntities Db
        {
            get
            {
                if (_Db == null) _Db = new GpscEntities();
                return _Db;
            }
            set => _Db = value;
        }

        [HttpPost]
        
        public ResultModel<List<CountryModel>> GetAllCountry([FromBody] JObject Body)
        {
            //if (!CheckAuthorize(Body["UserCode"].ToString()))
            //{
            //    return new ResultModel<List<CountryModel>>()
            //    {
            //        ResultCode = HttpStatusCode.Unauthorized.GetHashCode(),
            //        Message = "Unauthorize."
            //    };
            //}

            DbSet<Country> CountryEntity = Db.Countries;
            List<CountryModel> Countries = CountryEntity.Select(c => new CountryModel()
            {
                CountryId = c.Id,
                CountryName = c.Country_Name,
                Location = new LocationModel()
                {
                    Lat = c.Country_Latitude,
                    Lng = c.Country_Longitude
                }
            }).ToList();

            return new ResultModel<List<CountryModel>>()
            {
                ResultCode = HttpStatusCode.OK.GetHashCode(),
                Message = "",
                Result = Countries
            };
        }

        [HttpPost]
        public ResultModel<List<PlantModel>> GetPlantByCountry([FromBody] JObject Body)
        {
            //if (!CheckAuthorize(Body["UserCode"].ToString()))
            //{
            //    return new ResultModel<List<PlantModel>>()
            //    {
            //        ResultCode = HttpStatusCode.Unauthorized.GetHashCode(),
            //        Message = "Unauthorize."
            //    };
            //}
            int CountryId = (int)Body["CountryId"];

            List<Plant> Plants = Db.Countries.FirstOrDefault(c => c.Id == CountryId).Plants.OrderBy(p => p.Order).ToList();
            List<PlantModel> PlantModels = Plants.Select(p => new PlantModel()
            {
                PlantId = p.ID,
                PlantName = p.Company.Company_Name,
                PlantInfo = new CompanyModel()
                {
                    CompanyId = p.Company.ID,
                    CompanyName = p.Company.Company_Name,
                    CompanyLogo = p.Company.Company_Logo_Path,
                    Capacity = p.Company.Capacity,
                    COD = p.Company.COD,
                    PPA = p.Company.PPA,
                    IsEnabled = p.Company.IsEnabled
                },
                Customer = new CustomerModel()
                {
                    CustomerId = p.Customer.Id,
                    CustomerName = p.Customer.Customer_Name
                },
                Location = new LocationModel()
                {
                    Lat = p.Location_Latitude,
                    Lng = p.Location_Longitude
                },
                PlantType = p.PlantType.PlantType_Type,
                PowerGen = p.Power_Gen,
                ElectricGen = p.Electricity_Gen,
                SharedHolder = new SharedHolderModel()
                {
                    SharedHolderId = p.SharedHolder.Id,
                    SharedHolderName = p.SharedHolder.SharedHolder_Name,
                    GpscShared = p.SharedHolder.Gpsc_Share
                },
                SharedHolderPercentage = p.SharedHolder_Percentage,
                Order = p.Order
            }).ToList();

            return new ResultModel<List<PlantModel>>()
            {
                ResultCode = HttpStatusCode.OK.GetHashCode(),
                Message = "",
                Result = PlantModels
            };
        }

        [HttpPost]
        public ResultModel<PlantModel> GetPlantInfo([FromBody] JObject Body)
        {
            //if (!CheckAuthorize(Body["UserCode"].ToString()))
            //{
            //    return new ResultModel<PlantModel>()
            //    {
            //        ResultCode = HttpStatusCode.Unauthorized.GetHashCode(),
            //        Message = "Unauthorize."
            //    };
            //}
            int PlantId = (int)Body["PlantId"];

            Plant Plant = Db.Plants.FirstOrDefault(p => p.ID == PlantId);
            PlantModel PlantInfo = new PlantModel()
            {
                PlantId = Plant.ID,
                PlantName = Plant.Company.Company_Name,
                PlantInfo = new CompanyModel()
                {
                    CompanyId = Plant.Company.ID,
                    CompanyName = Plant.Company.Company_Name,
                    CompanyLogo = Plant.Company.Company_Logo_Path,
                    Capacity = Plant.Company.Capacity,
                    COD = Plant.Company.COD,
                    PPA = Plant.Company.PPA
                },
                Customer = new CustomerModel()
                {
                    CustomerId = Plant.Customer.Id,
                    CustomerName = Plant.Customer.Customer_Name
                },
                Location = new LocationModel()
                {
                    Lat = Plant.Location_Latitude,
                    Lng = Plant.Location_Longitude
                },
                PlantType = Plant.PlantType.PlantType_Type,
                PowerGen = Plant.Power_Gen,
                ElectricGen = Plant.Electricity_Gen,
                SharedHolder = new SharedHolderModel()
                {
                    SharedHolderId = Plant.SharedHolder.Id,
                    SharedHolderName = Plant.SharedHolder.SharedHolder_Name,
                    GpscShared = Plant.SharedHolder.Gpsc_Share
                },
                SharedHolderPercentage = Plant.SharedHolder_Percentage
            };
            return new ResultModel<PlantModel>()
            {
                ResultCode = HttpStatusCode.OK.GetHashCode(),
                Message = "",
                Result = PlantInfo
            };
        }

        [HttpPost]
        public ResultModel<List<EnergyGenModel>> GetHourlyEnergyGen([FromBody] JObject Body)
        {
            //if (!CheckAuthorize(Body["UserCode"].ToString()))
            //{
            //    return new ResultModel<List<EnergyGenModel>>()
            //    {
            //        ResultCode = HttpStatusCode.Unauthorized.GetHashCode(),
            //        Message = "Unauthorize."
            //    };
            //}
            int CompanyId = (int)Body["CompanyId"];
            DateTime StartDate = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0);
            DateTime EndDate = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, DateTime.Today.Day, 23, 59, 59);
            List<EnergyGenModel> Models = new List<EnergyGenModel>();

            var hourly = Db.PlantEnergyGenHourlyViews.Where(a => a.Time_Stamp.Value >= StartDate && a.Time_Stamp.Value <= EndDate && a.PlantId.Equals(CompanyId)).OrderBy(a => a.Time_Stamp).Select(a => new
            {
                Index = a.Row,
                EnergyValue = a.AverageEnergyGenValue.Value,
                TimeStamp = a.Time_Stamp.Value
            });
            foreach (var record in hourly)
            {
                Models.Add(new EnergyGenModel()
                {
                    EnergyValue = (double)record.EnergyValue,
                    Target = -1,
                    TimeStamp = record.TimeStamp
                });
            }

            ResultModel<List<EnergyGenModel>> Result = new ResultModel<List<EnergyGenModel>>()
            {
                ResultCode = HttpStatusCode.OK.GetHashCode(),
                Message = "",
                Result = Models
            };

            return Result;
        }

        [HttpPost]
        public ResultModel<List<EnergyGenModel>> GetDailyEnergyGen([FromBody] JObject Body)
        {
            //if (!CheckAuthorize(Body["UserCode"].ToString()))
            //{
            //    return new ResultModel<List<EnergyGenModel>>()
            //    {
            //        ResultCode = HttpStatusCode.Unauthorized.GetHashCode(),
            //        Message = "Unauthorize."
            //    };
            //}
            int CompanyId = (int)Body["CompanyId"];
            DateTime StartDate = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, 1);
            DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
            List<EnergyGenModel> Models = new List<EnergyGenModel>();

            var hourly = Db.PlantEnergyGenDailyViews.Where(a => a.Time_Stamp.Value >= StartDate && a.Time_Stamp.Value <= EndDate && a.PlantId.Equals(CompanyId)).OrderBy(a => a.Time_Stamp).Select(a => new
            {
                Index = a.Row,
                EnergyValue = a.AverageEnergyGenValue.Value,
                TimeStamp = a.Time_Stamp.Value
            });
            foreach (var record in hourly)
            {
                Models.Add(new EnergyGenModel()
                {
                    EnergyValue = (double)record.EnergyValue,
                    Target = -1,
                    TimeStamp = record.TimeStamp
                });
            }

            ResultModel<List<EnergyGenModel>> Result = new ResultModel<List<EnergyGenModel>>()
            {
                ResultCode = HttpStatusCode.OK.GetHashCode(),
                Message = "",
                Result = Models
            };

            return Result;
        }

        [HttpPost]
        public ResultModel<List<EnergyGenModel>> GetMonthlyEnergyGen([FromBody] JObject Body)
        {
            //if (!CheckAuthorize(Body["UserCode"].ToString()))
            //{
            //    return new ResultModel<List<EnergyGenModel>>()
            //    {
            //        ResultCode = HttpStatusCode.Unauthorized.GetHashCode(),
            //        Message = "Unauthorize."
            //    };
            //}
            int CompanyId = (int)Body["CompanyId"];

            DateTime StartDate = new DateTime(DateTime.Today.Year - 1, 1, 1);
            DateTime EndDate = StartDate.AddYears(1).AddMonths(-1);
            List<EnergyGenModel> Models = new List<EnergyGenModel>();

            var hourly = Db.PlantEnergyGenMonthlyViews.Where(a => a.Time_Stamp.Value >= StartDate && a.Time_Stamp.Value <= EndDate && a.PlantId.Equals(CompanyId)).OrderBy(a => a.Time_Stamp).Select(a => new
            {
                Index = a.Row,
                EnergyValue = a.AverageEnergyGenValue.Value,
                TimeStamp = a.Time_Stamp.Value
            });
            foreach (var record in hourly)
            {
                string YearMonth = $"{record.TimeStamp.Year}-{record.TimeStamp.Month.ToString().PadLeft(2, '0')}";
                var Target = Db.EnergyGenTargets.Where(t => t.YearMonth.Equals(YearMonth)).ToList().FirstOrDefault().TargetValue;
                Models.Add(new EnergyGenModel()
                {
                    EnergyValue = (double)record.EnergyValue,
                    Target = (double)Target,
                    TimeStamp = record.TimeStamp
                });
            }

            ResultModel<List<EnergyGenModel>> Result = new ResultModel<List<EnergyGenModel>>()
            {
                ResultCode = HttpStatusCode.OK.GetHashCode(),
                Message = "",
                Result = Models
            };

            return Result;
        }

        [HttpPost]
        public ResultModel<List<EnergyGenModel>> GetYearlyEnergyGen([FromBody] JObject Body)
        {
            //if (!CheckAuthorize(Body["UserCode"].ToString()))
            //{
            //    return new ResultModel<List<EnergyGenModel>>()
            //    {
            //        ResultCode = HttpStatusCode.Unauthorized.GetHashCode(),
            //        Message = "Unauthorize."
            //    };
            //}
            int CompanyId = (int)Body["CompanyId"];
            //DateTime StartDate = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0);
            //DateTime EndDate = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, DateTime.Today.Day, 23, 59, 59);
            List<EnergyGenModel> Models = new List<EnergyGenModel>();

            var hourly = Db.PlantEnergyGenYearlyViews.Where(a => a.PlantId.Equals(CompanyId)).OrderBy(a => a.Time_Stamp).Select(a => new
            {
                Index = a.Row,
                EnergyValue = a.AverageEnergyGenValue.Value,
                TimeStamp = a.Time_Stamp.Value
            });
            foreach (var record in hourly)
            {
                string Year = record.TimeStamp.Year.ToString();
                var Target = Db.EnergyGenTargets.Where(t => t.YearMonth.StartsWith(Year)).Average(c => c.TargetValue);
                Models.Add(new EnergyGenModel()
                {
                    EnergyValue = (double)record.EnergyValue,
                    Target = (double)Target,
                    TimeStamp = record.TimeStamp
                });
            }

            ResultModel<List<EnergyGenModel>> Result = new ResultModel<List<EnergyGenModel>>()
            {
                ResultCode = HttpStatusCode.OK.GetHashCode(),
                Message = "",
                Result = Models
            };

            return Result;
        }

        [HttpGet]
        public List<string> FlushVersion()
        {

            var path = Environment.GetEnvironmentVariable("PATH");
            List<string> Versions = new List<string>();
            // get all
            var enumerator = Environment.GetEnvironmentVariables().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Versions.Add($"{enumerator.Key,5}:{enumerator.Value,100}");
            }
            return Versions;
        }

        //private bool CheckAuthorize(string UserCode) => UserCode.Equals("UserCode123456");
    }

}
