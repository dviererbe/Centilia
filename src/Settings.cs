using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Centilia
{
    class Settings
    {
        public bool HideControlls
        {
            get;
            set;
        }

        public string Homepage
        {
            get;
            set;
        }

        public bool AllowHTTP
        {
            get;
            set;
        }

        public bool IsWhitelistEnabled
        {
            get;
            set;
        }

        public IEnumerable<string> Whitelist
        {
            get;
            set;
        }

        public bool IsBlacklistEnabled
        {
            get;
            set;
        }

        public IEnumerable<string> Blacklist
        {
            get;
            set;
        }

        public string MasterPassword
        {
            get;
            set;
        }
    }
}
