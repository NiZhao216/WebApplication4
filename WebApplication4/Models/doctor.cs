using System;

/// <summary>
/// 医生信息实体
/// </summary>
namespace WebApplication4.Models
{
    public class Doctors
{
    /// <summary>
    /// 医生ID
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// 医生姓名
    /// </summary>
    public string DoctorName { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public string Gender { get; set; }

    /// <summary>
    /// 所属科室
    /// </summary>
    public string Department { get; set; }

    /// <summary>
    /// 职称
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 执业证号
    /// </summary>
    public string LicenseNo { get; set; }

    /// <summary>
    /// 头像图片地址
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 个人简介
    /// </summary>
    public string Introduction { get; set; }

    /// <summary>
    /// 擅长领域
    /// </summary>
    public string Specialty { get; set; }

    /// <summary>
    /// 个人座右铭/签名
    /// </summary>
    public string Motto { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 入职日期
    /// </summary>
    public DateTime? HireDate { get; set; }

    /// <summary>
    /// 状态：在职、离职、休假
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}
}