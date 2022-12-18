using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOTR_MODDING_TOOL.Classes
{
    public class DeckType
    {
        public int Index { get;}
        public string Name { get;}

        public DeckType(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }
}
