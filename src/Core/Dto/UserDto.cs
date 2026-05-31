using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? MidName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}
