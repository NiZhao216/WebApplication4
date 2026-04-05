namespace WebApplication4.Models
{
    public class User
    {
        // 原有字段
        public int id { get; set; }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        public string username { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? pr { get; set; }
        public string pwd { get; set; } = string.Empty;
        public string? age { get; set; }
        public string? sex { get; set; }
        public DateTime? by { get; set; }
        public string? address { get; set; }

        public string? avatar { get; set; }


    }
}