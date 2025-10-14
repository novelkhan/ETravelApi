using System.Collections.Generic;
using System;

namespace ETravelApi.DTOs.Admin
{
    public class MemberViewDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }  // New field
        public bool IsLocked { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}