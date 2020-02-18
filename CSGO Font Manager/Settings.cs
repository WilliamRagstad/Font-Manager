using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGO_Font_Manager
{
    public class Settings
    {
        public string    CsgoPath { get; set; }
        public bool      ProTips { get; set; }  = true;
        public string    HideNewVersions { get; set; }
        public string    ActiveFont { get; set; }
    }
}