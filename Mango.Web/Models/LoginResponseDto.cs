﻿namespace Mango.Web.Models.Dto
{
    public class LoginResponseDto
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }
    }
}
