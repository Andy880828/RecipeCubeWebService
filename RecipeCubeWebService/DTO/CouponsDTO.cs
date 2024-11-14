﻿namespace RecipeCubeWebService.DTO
{
    public class couponsDTO
    {
        public int couponId { get; set; }

        public string couponName { get; set; }

        public int? couponStatus { get; set; }

        public decimal? discountValue { get; set; }

        public string discountType { get; set; }

        public int userConponId { get; set; }

        public string userId { get; set; }


        public int? usedStatus { get; set; }

        public DateTime acquireDate { get; set; }

        public int? minSpend { get; set; }
    }
}