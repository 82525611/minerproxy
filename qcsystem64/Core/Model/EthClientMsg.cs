using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qcsystem64
{
    public class EthClientMsg
    {
        public int id { get; set; }
        public string method { get; set; }
        public string worker { get; set; }
        public string[] @params { get; set; }
        public string jsonrpc { get; set; }
        public JToken result { get; set; }
    }
}
