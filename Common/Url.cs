using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mondo.Common
{
    public class Url
    {
        /****************************************************************************/
        /****************************************************************************/
        public static string Combine(params string[] vals)
        {
            StringList list = new StringList();

            foreach(string val in vals)
                if(!string.IsNullOrWhiteSpace(val))
                    list.Add(val.EnsureNotEndsWith("/").EnsureNotStartsWith("/"));

            return list.Pack("/");
        }
    }
}
