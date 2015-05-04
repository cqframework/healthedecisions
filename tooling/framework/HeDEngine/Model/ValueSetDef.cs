using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeD.Engine.Model
{
    public class ValueSetDef
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string Version { get; set; }

        public IEnumerable<String> CodeSystems { get; set; }
    }
}
