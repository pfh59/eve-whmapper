﻿using System;
namespace WHMapper.Models.DTO
{
    public class InitialApplicationState
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? XsrfToken { get; set; }
    }
}

