using System;

namespace WebApplication4.Models
{
    public class Cats
    {
        public int Id { get; set; }
        // 猫咪昵称
        public string CatName { get; set; } = string.Empty;
        // 性别
        public string Gender { get; set; } = string.Empty;
        // 年龄
        public string Age { get; set; } = string.Empty;
        // 品种
        public string Breed { get; set; } = string.Empty;
        // 体重
        public string Weight { get; set; } = string.Empty;
        // 生日
        public DateTime? Birthday { get; set; }
        // 毛色
        public string CoatColor { get; set; } = string.Empty;
        // 过敏史
        public string Allergy { get; set; } = string.Empty;
        // 既往病史
        public string MedicalHistory { get; set; } = string.Empty; 
        // 疫苗状态
        public string VaccineStatus { get; set; } = string.Empty;
        // 下次疫苗时间
        public DateTime? NextVaccineDate { get; set; }
        // 驱虫状态
        public string DewormStatus { get; set; } = string.Empty;
        // 下次驱虫时间
        public DateTime? NextDewormDate { get; set; }
        // 关联用户ID
        public string Username { get; set; } = string.Empty;
    }
}