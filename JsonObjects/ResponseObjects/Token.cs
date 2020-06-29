//TODO(REFACTOR)﻿
namespace Camellia_Management_System.JsonObjects
{


    /// @author Yevgeniy Cherdantsev
    /// @date 07.03.2020 16:37:58
    /// @version 1.0
    /// <summary>
    /// Token response while using sign
    /// </summary>
    public class TokenResponse
    {
        public string xml { get; set; } = "";
        public long timestamp { get; set; }
    }
}