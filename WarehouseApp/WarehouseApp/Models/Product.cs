namespace WarehouseApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Article { get; set; }
        public int Quantity { get; set; }
        public int WaybillId { get; set; }

        public Waybill Waybill { get; set; }
    }
}