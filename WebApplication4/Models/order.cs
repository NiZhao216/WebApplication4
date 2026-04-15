namespace WebApplication4.Models
{
    /// <summary>
    /// 订单类，用于存储和管理预约订单的相关信息
    /// </summary>
    public class Orders
    {
        /// <summary>
        /// 订单ID，唯一标识一个订单
        /// </summary>
        public int Orid { get; set; }
        /// <summary>
        /// 用户名，下单用户的名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 分类ID，用于标识订单所属的分类
        /// </summary>
        public int CatId { get; set; }
        /// <summary>
        /// 分类名称，订单所属分类的名称
        /// </summary>
        public string CatName { get; set; }
        /// <summary>
        /// 联系电话，用户预留的联系方式
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 服务类型，用户预约的服务类型
        /// </summary>
        public string ServiceType { get; set; }
        /// <summary>
        /// 预约时间，用户期望的服务时间
        /// </summary>
        public DateTime AppointmentTime { get; set; }
        /// <summary>
        /// 订单描述，用户对订单的额外说明
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 医生ID，负责该订单的医生标识
        /// </summary>
        public int DoctorId { get; set; }
        /// <summary>
        /// 医生姓名，负责该订单的医生姓名
        /// </summary>
        public string DoctorName { get; set; }
        /// <summary>
        /// 订单状态，包括：待确认、已确认、已完成、已取消
        /// </summary>
        public string Status { get; set; } // 待确认、已确认、已完成、已取消
        public DateTime CreateTime { get; set; }
    }


}
