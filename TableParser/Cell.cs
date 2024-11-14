using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableParser
{
    public class Cell
    {
        public double Value { get; set; }
        public string Expression { get; set; }
        public IList<string> Dependencies { get; set; }
        public IList<string> OwnDependents { get; set; }
        public Cell() 
        {
            Expression = "";
            Value = 0;
            Dependencies = new List<string>();
            OwnDependents = new List<string>();
        }
 /*       public Cell(string exp) 
        {
            Expression = exp;
            Value = Calculator.Evaluate(Expression);
            Dependencies = new List<string>();
            OwnDependents = new List<string>();
        }*/
    }
}
