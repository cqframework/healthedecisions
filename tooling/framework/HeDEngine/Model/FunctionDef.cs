using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeD.Engine.Model
{
    public class FunctionDef : ExpressionDef
    {
        public IEnumerable<ParameterDef> Operands { get; set; }
    }
}
