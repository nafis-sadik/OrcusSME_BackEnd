﻿using DataLayer;
using Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DataLayer.Entities;
using DataLayer.MySql;
using Microsoft.EntityFrameworkCore;
using Repositories.Implementation;
using Services.Orcus.Abstraction;
using DataLayer.MSSQL;

namespace Services.Orcus.Implementation
{
    public class OutletManagerService : IOutletManagerService
    {
        private readonly IOutletManagerRepo _outletManagerRepo;
        private readonly ICrashLogRepo _crashLogRepo;

        public OutletManagerService()
        {
            OrcusSMEContext context = new OrcusSMEContext(new DbContextOptions<OrcusSMEContext>());
            _outletManagerRepo = new OutletManagerRepo(context);
            _crashLogRepo = new CrashLogRepo(context);
        }

        public List<Models.OutletModel> AddOutlet(Models.OutletModel outlet)
        {
            List<Models.OutletModel> response = new List<Models.OutletModel>();
            try
            {
                bool status = _outletManagerRepo.Add(new Outlet
                {
                    OutletAddresss = outlet.OutletAddresss,
                    OutletName = outlet.OutletName,
                    UserId = outlet.UserId,
                    Status = CommonConstants.StatusTypes.Active
                });

                if (status)
                    response = _outletManagerRepo.AsQueryable().Where(x => x.UserId == outlet.UserId && x.Status == CommonConstants.StatusTypes.Active).Select(x => new Models.OutletModel { OutletId = x.OutletId, OutletName = x.OutletName }).ToList();
                else
                    return null;
            }
            catch (Exception ex)
            {
                _outletManagerRepo.Rollback();

                if (ex.InnerException != null)
                    _crashLogRepo.Add(new Crashlog
                    {
                        ClassName = "OutletManagerService",
                        MethodName = "AddOutlet",
                        ErrorMessage = ex.Message,
                        ErrorInner =
                            (string.IsNullOrEmpty(ex.Message) || ex.Message == CommonConstants.MsgInInnerException
                                ? ex.InnerException.Message
                                : ex.Message),
                        Data = outlet.UserId,
                        TimeStamp = DateTime.Now
                    });
            }

            return response;
        }

        public List<Models.OutletModel> ArchiveOutlet(Models.OutletModel outlet)
        {
            List<Models.OutletModel> response = new List<Models.OutletModel>();
            Outlet oldData = _outletManagerRepo.AsQueryable().FirstOrDefault(x => x.OutletId == outlet.OutletId);
            try
            {
                if (oldData != null)
                {
                    oldData.Status = CommonConstants.StatusTypes.Archived;
                    _outletManagerRepo.Update(oldData);
                }

                response = _outletManagerRepo.AsQueryable().Where(x => x.UserId == outlet.UserId && x.Status == CommonConstants.StatusTypes.Active).Select(x => new Models.OutletModel { OutletId = x.OutletId, OutletName = x.OutletName }).ToList();
            }
            catch (Exception ex)
            {
                _outletManagerRepo.Rollback();
                
                if (ex.InnerException != null)
                    if (oldData != null)
                        _crashLogRepo.Add(new Crashlog
                        {
                            ClassName = "OutletManagerService",
                            MethodName = "ArchiveOutlet",
                            ErrorMessage = ex.Message,
                            ErrorInner =
                                (string.IsNullOrEmpty(ex.Message) || ex.Message == CommonConstants.MsgInInnerException
                                    ? ex.InnerException.Message
                                    : ex.Message),
                            Data = oldData.UserId,
                            TimeStamp = DateTime.Now
                        });
            }

            return response;
        }

        public Models.OutletModel GetOutlet(int outletId)
        {
            Models.OutletModel response;
            try
            {
                Outlet outlet = _outletManagerRepo.Get(outletId);
                response = new Models.OutletModel
                {
                    OutletId = outlet.OutletId,
                    OutletName = outlet.OutletName,
                    OutletAddresss = outlet.OutletAddresss,
                    UserId = outlet.UserId
                };
            }
            catch (Exception ex)
            {
                _outletManagerRepo.Rollback();

                int pk;
                if (_crashLogRepo.AsQueryable().Any())
                    pk = 0;
                else
                    pk = _crashLogRepo.AsQueryable().Max(x => x.CrashLogId) + 1;

                if (ex.InnerException != null)
                    _crashLogRepo.Add(new Crashlog
                    {
                        CrashLogId = pk,
                        ClassName = "OutletManagerService",
                        MethodName = "GetOutlet",
                        ErrorMessage = ex.Message,
                        ErrorInner =
                            (string.IsNullOrEmpty(ex.Message) || ex.Message == CommonConstants.MsgInInnerException
                                ? ex.InnerException.Message
                                : ex.Message),
                        Data = outletId.ToString(NumberFormatInfo.CurrentInfo),
                        TimeStamp = DateTime.Now
                    });
                response = null;
            }

            return response;
        }

        public List<Models.OutletModel> GetOutletsByUserId(string userId)
        {
            List<Models.OutletModel> response = new List<Models.OutletModel>();
            try
            {
                response = _outletManagerRepo.AsQueryable()
                    .Where(x => x.UserId == userId && x.Status == CommonConstants.StatusTypes.Active)
                    .Select(x => new Models.OutletModel { OutletId = x.OutletId, OutletName = x.OutletName })
                    .ToList();
            }
            catch (Exception ex)
            {
                _outletManagerRepo.Rollback();

                int pk;
                if (_crashLogRepo.AsQueryable().Any())
                    pk = 0;
                else
                    pk = _crashLogRepo.AsQueryable().Max(x => x.CrashLogId) + 1;

                if (ex.InnerException != null)
                    _crashLogRepo.Add(new Crashlog
                    {
                        CrashLogId = pk,
                        ClassName = "OutletManagerService",
                        MethodName = "GetOutletsByUserId",
                        ErrorMessage = ex.Message,
                        ErrorInner =
                            (string.IsNullOrEmpty(ex.Message) || ex.Message == CommonConstants.MsgInInnerException
                                ? ex.InnerException.Message
                                : ex.Message),
                        Data = userId,
                        TimeStamp = DateTime.Now
                    });
            }
            return response;
        }

        public bool? OrderSite(int outletId, out string response)
        {
            try
            {
                Outlet outletData = _outletManagerRepo.Get(outletId);

                if (!string.IsNullOrEmpty(outletData.SiteUrl))
                {
                    response = "You already have a site for this outlet</br>Visit : " + outletData.SiteUrl;
                    return false;
                }

                outletData.RequestSite = 1; // 1 in mssql means true in mysql and 0 means false
                _outletManagerRepo.Update(outletData);
                response = "Site order placed successfully";
                return true;
            }
            catch (Exception ex)
            {
                _outletManagerRepo.Rollback();

                int pk;
                if (_crashLogRepo.AsQueryable().Any())
                    pk = 0;
                else
                    pk = _crashLogRepo.AsQueryable().Max(x => x.CrashLogId) + 1;

                if (ex.InnerException != null)
                    _crashLogRepo.Add(new Crashlog
                    {
                        CrashLogId = pk,
                        ClassName = "OutletManagerService",
                        MethodName = "OrderSite",
                        ErrorMessage = ex.Message,
                        ErrorInner = 
                            (string.IsNullOrEmpty(ex.Message) || ex.Message == CommonConstants.MsgInInnerException
                                ? ex.InnerException.Message
                                : ex.Message),
                        Data = null,
                        TimeStamp = DateTime.Now
                    });
                response = "An internal error has occured";
                return null;
            }
        }

        public List<Models.OutletModel> UpdateOutlet(Models.OutletModel outlet)
        {
            List<Models.OutletModel> response = new List<Models.OutletModel>();
            try
            {
                Outlet oldData = _outletManagerRepo.AsQueryable().FirstOrDefault(x => x.OutletId == outlet.OutletId);

                if (oldData != null)
                {
                    oldData.OutletAddresss = outlet.OutletAddresss;
                    oldData.OutletName = outlet.OutletName;
                    oldData.UserId = outlet.UserId;

                    _outletManagerRepo.Update(oldData);
                }

                response = _outletManagerRepo.AsQueryable().Where(x => x.UserId == outlet.UserId && x.Status == CommonConstants.StatusTypes.Active).Select(x => new Models.OutletModel { OutletId = x.OutletId, OutletName = x.OutletName }).ToList();
            }
            catch (Exception ex)
            {
                _outletManagerRepo.Rollback();

                int pk;
                if (_crashLogRepo.AsQueryable().Any())
                    pk = 0;
                else
                    pk = _crashLogRepo.AsQueryable().Max(x => x.CrashLogId) + 1;

                if (ex.InnerException != null)
                    _crashLogRepo.Add(new Crashlog
                    {
                        CrashLogId = pk,
                        ClassName = "OutletManagerService",
                        MethodName = "UpdateOutlet",
                        ErrorMessage = ex.Message,
                        ErrorInner =
                            (string.IsNullOrEmpty(ex.Message) || ex.Message == CommonConstants.MsgInInnerException
                                ? ex.InnerException.Message
                                : ex.Message),
                        Data = outlet.UserId,
                        TimeStamp = DateTime.Now
                    });
            }

            return response;
        }
    }
}
