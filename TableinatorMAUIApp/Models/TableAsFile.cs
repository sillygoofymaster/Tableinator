using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TableinatorMAUIApp.Models
{
    public class TableAsFile
    {
        public int CountRow { get; set; }
        public int CountColumn { get; set; }
        public IDictionary<string, TableParser.Cell> CellTable { get; set; }

        public TableAsFile() { }

        public TableAsFile(int rows, int columns, IDictionary<string, TableParser.Cell> cellTable)
        {
            CountColumn = columns;
            CountRow = rows;
            CellTable = cellTable;
        }
    }
}
