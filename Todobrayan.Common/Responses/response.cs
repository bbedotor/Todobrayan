using System;
using System.Collections.Generic;
using System.Text;

namespace Todobrayan.Common.Responses
{
    public class response
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public object Result { get; set; }
    }
}
