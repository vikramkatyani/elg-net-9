using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELG.Model.Learner
{
    public class Common
    {

    }

    public class ControllerResponse
    {
        public string Url { get; set; }
        public string Message { get; set; }
        public int Err { get; set; }
        public dynamic Status { get; set; }
    }
}
