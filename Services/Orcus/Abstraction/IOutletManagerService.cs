﻿using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Orcus.Abstraction
{
    public interface IOutletManagerService
    {
        public List<OutletModel> AddOutlet(OutletModel outlet);
        public List<OutletModel> UpdateOutlet(OutletModel outlet);
        public List<OutletModel> ArchiveOutlet(OutletModel outlet);
        public List<OutletModel> GetOutletsByUserId(string UserId);
        public OutletModel GetOutlet(int OutletId);
        public bool? OrderSite(int OutletId, out string response);
    }
}
