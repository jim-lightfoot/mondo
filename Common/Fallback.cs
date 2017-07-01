using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mondo.Common
{
    /****************************************************************************/
    public static class Fallback
    {
        /****************************************************************************/
        public static T Run<L, T>(IEnumerable<L> paramList, Func<L, T> fn)
        {
            Exception exception = null;

            foreach(var item in paramList)
            {
                try
                {
                    return fn(item);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            throw exception;
        }
    }
}
