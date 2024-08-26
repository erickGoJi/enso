using System;
using System.Collections.Generic;

namespace Enso.dal.DBContext
{
    public partial class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public Guid? Createby { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid? Updateby { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}