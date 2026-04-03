namespace WebApplication4.Models
{
    public class User
    {
        // 原有字段
        public int id { get; set; }
        public string username { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? pr { get; set; }
        public string pwd { get; set; }
        public string? age { get; set; }
        public string? sex { get; set; }
        public DateTime? by { get; set; }
        public string? address { get; set; }

        public string? avatar { get; set; }


    }
}