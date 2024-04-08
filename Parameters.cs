using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effective_TestTask
{
    public class Parameters
    {
        public string FileLog { get; set; }
        public string FileOutput { get; set; }
        public string AddressStart { get; set; }
        public int? AddressMask { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }

        public Parameters()
        {
            FileLog = @"IP_List.txt";
            FileOutput = @"Log.txt";
            TimeStart = DateTime.Parse("01.01.1999");
            TimeEnd = DateTime.Now;
        }
    }
}
