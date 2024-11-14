using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TableParser
{
    public class TableIndex
    {
        public string CurrentCellName { get; set; }
        public Dictionary<string, Cell> TableIdentifier;
        public IList<string> EditedCells;

        public TableIndex()
        {
            TableIdentifier = new Dictionary<string, Cell>();
            EditedCells = new List<string>();
            CurrentCellName = " ";
        }

        public string EditCell(string cellName, string expression)
        {
            string result = "";
            var cell = TableIdentifier[cellName];
            if (cell == null)
            {
                return result;
            }
            var oldExpression = cell.Expression;
            double oldValue = cell.Value;

            var oldDependencies = new List<string>();

            foreach (var dependencyName in cell.Dependencies)
            {
                oldDependencies.Add(dependencyName);
            }
            TableIdentifier[cellName].Expression = expression;
            EditedCells.Clear();
            try
            {
                UpdateCellAndOwnDependents(cellName);
            }

            catch (KeyNotFoundException ex)
            {
                cell.Expression = oldExpression;
                cell.Value = oldValue;
                cell.Dependencies = oldDependencies;
                result = ex.Message;
                return result;
            }

            catch (Exception)
            {
                cell.Expression = oldExpression;
                cell.Value = oldValue;
                cell.Dependencies = oldDependencies;
                result = "Введено невалідний вираз. З граматикою можна ознайомитися в 'Довідці'.";
                return result;
            }
            return result;
        }

        public void UpdateCellAndOwnDependents(string cellName)
        {
            var cell = TableIdentifier[cellName];
            UpdateCell(cellName);

            for (int i = 0; i < cell.OwnDependents.Count; i++)
            {
                var observer = TableIdentifier[cell.OwnDependents[i]];
                if (observer.Dependencies.Contains(cellName))
                {
                    UpdateCellAndOwnDependents(cell.OwnDependents[i]);
                }
                else
                {
                    cell.OwnDependents.RemoveAt(i--);
                }
            }
        }

        private void UpdateCell(string cellName)
        {
            var cell = TableIdentifier[cellName];
            cell.Dependencies.Clear();
            CurrentCellName = cellName;
            cell.Value = Calculator.Evaluate(cell.Expression);
            if (!EditedCells.Contains(cellName))
            {
                EditedCells.Add(cellName);
            }
        }

        public bool HasCyclicDependency(string dependentCellName)
        {
            return (dependentCellName == CurrentCellName || TableIdentifier[dependentCellName].Dependencies.Any(HasCyclicDependency));
        }
    }
}
