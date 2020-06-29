using System;
//TODO(REFACTOR)
namespace EgovFoundersRequest.JsonObjects
{

/*!
@author Yevgeniy Cherdantsev
@date 22.01.2020 16:47:31
@version 1.0
@brief INPUT
    
@code
    
@endcode
    
    */

    public class Captcha
    {

        public Captcha(string captcha)
        {
            captchaCode = captcha;
        }
        public string captchaCode { get; set; } = "";
    }
}