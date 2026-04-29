using System;
using System.Collections.Generic;

namespace WarehouseApp.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Position { get; set; } = "";

        public ICollection<Waybill> Waybills { get; set; } = new List<Waybill>();
    }
}