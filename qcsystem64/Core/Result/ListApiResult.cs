using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace qcsystem64
{
    public class ListApiResult<T>
    {
        public int total { get; set; }
        public List<T> data { get; set; } 
    }
}
