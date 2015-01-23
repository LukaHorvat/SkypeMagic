using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeMagic
{
    public class ManualAttribute : Attribute
    {
        public string Help { get; set; }
        public ManualAttribute(string help)
        {
            Help = help;
        }
    }
}
