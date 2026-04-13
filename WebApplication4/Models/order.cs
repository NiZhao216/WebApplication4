namespace WebApplication4.Models
{
    public class Orders
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int CatId { get; set; }
        public string CatName { get; set; }
        public string Phone { get; set; }
        public string ServiceType { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string Description { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; } // 待确认、已确认、已完成、已取消
        public DateTime CreateTime { get; set; }
    }


}
