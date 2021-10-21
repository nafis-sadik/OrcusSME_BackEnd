﻿using System;
using System.Collections.Generic;

#nullable disable

namespace DataLayer.Entities
{
    public partial class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public decimal? OutletId { get; set; }
        public virtual Outlet Outlet { get; set; }
    }
}
