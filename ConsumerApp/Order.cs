using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerApp
{
    public class Order
    {
        public int Id { get; set; }
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
