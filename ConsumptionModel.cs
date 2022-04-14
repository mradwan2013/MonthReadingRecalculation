using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonthReadingRecalculation
{
   public class ConsumptionModel
    {
        public int tarrifId;

        public DateTime tarrifStartDate;

        public DataTable Tarrifa;

        public decimal[,] PriceResult;

        public DataTable Stairs;

        public decimal Fixfee;

        public decimal WaterPrice;

        public decimal ServiceBoxWithTax;

        public decimal SewagePrice;

        public decimal TotalPrice;
    }
}
