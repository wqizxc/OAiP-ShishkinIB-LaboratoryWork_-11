using System;
using System.Collections.Generic;

namespace WarehouseApp.Models
{
    public class Waybill
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } 
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}