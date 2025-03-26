using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class Mode
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public int? MaxBottleNumber { get; set; }
        public int? MaxUsedTips { get; set; }

        public ICollection<Step>? Steps { get; set; }
    }
}
