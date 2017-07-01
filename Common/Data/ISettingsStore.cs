using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mondo.Common
{
    /****************************************************************************/
	/****************************************************************************/
    public interface ISettingsStore
    {
        object GetSetting(string name);
        void   AddSetting(string name, object value, int expires = 1, bool httpOnly = true, bool secure = true, string domain = "");
        void   RemoveSetting(string name, string domain = "");
    }
}
