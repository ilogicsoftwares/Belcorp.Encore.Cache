﻿using System;

namespace Belcorp.Encore.Cache.Entities
{
    public class ProductPrice
    {
        public int ProductPriceID { get; set; }
        public int ProductPriceTypeID { get; set; }
        public int ProductID { get; set; }
        public int CurrencyID { get; set; }
        public int CatalogID { get; set; }
        public decimal Price { get; set; }
        public int ModifiedByUserID { get; set; }
        public string ETLNaturalKey { get; set; }
        public string ETLHash { get; set; }
        public string ETLPhase { get; set; }
        public DateTime ETLDate { get; set; }
    }
}
