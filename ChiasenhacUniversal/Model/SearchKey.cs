using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.Model
{
    public class SearchKey
    {
        public string search_key { get; set; }

        //public SearchKey() { }

        public override string ToString()
        {
            return string.Format("{0}", search_key);
        }
    }
}
