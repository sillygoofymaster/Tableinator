using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using static System.Net.Mime.MediaTypeNames;
using TableParser;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Calculator = TableParser.Calculator;
using Grid = Microsoft.Maui.Controls.Grid;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System.Xml.Linq;

namespace TableinatorMAUIApp
{
    public partial class MainPage : ContentPage
    {
        const int DefaultCountColumn = 5; // кількість стовпчиків (A to Z)
        const int DefaultCountRow = 5; // кількість рядків
        private IDictionary<string, IView> cellTable;
        private readonly FileManager fileManager;
        Microsoft.Maui.Controls.Entry lastFocusedEntry;
        bool unsavedChanges;

        public MainPage()
        {
            cellTable = new Dictionary<string, IView>();
            InitializeComponent();
            CreateGrid(DefaultCountColumn, DefaultCountRow);
            fileManager = new FileManager();
            unsavedChanges = false;
        }

        //створення таблиці
        private void CreateGrid(int colCount, int rowCount)
        {

            AddColumnsAndColumnLabels(colCount);
            AddRowsAndColumnLabels(colCount, rowCount);

        }

        private void DestroyGrid()
        {
            var countcol = grid.ColumnDefinitions.Count;
            var countrow = grid.RowDefinitions.Count;

            for (int col = 0; col < countcol; col++)
            {
                DeleteColumn();
            }

            for (int row = 0; row < countrow; row++)
            {
                DeleteRow();
            }
        }

        private void AddColumnsAndColumnLabels(int colCount)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            // Додати стовпці та підписи для стовпців
            for (int col = 0; col < colCount; col++)
            {
                AddColumn();
            }
        }

        private void AddColumn()
        {
            var freshColumn = grid.ColumnDefinitions.Count;
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var freshLabel = new Label
            {
                Text = GetColumnName(freshColumn),

                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };


            Grid.SetRow(freshLabel, 0);
            Grid.SetColumn(freshLabel, freshColumn);
            grid.Children.Add(freshLabel);
            cellTable[$"{GetColumnName(freshColumn)}0"] = freshLabel;
        }

        private void AddRowsAndColumnLabels(int colCount, int rowCount)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            // Додати рядки, підписи для рядків та комірки
            for (int row = 0; row < rowCount; row++)
            {
                AddRow();


                // Додати комірки (Entry) для вмісту
                for (int col = 0; col < colCount; col++)
                {
                    AddCell(row + 1, col + 1);

                }
            }
        }

        private void AddRow()
        {
            var freshRow = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition());

            // Додати підпис для номера рядка
            var label = new Label
            {
                Text = (freshRow).ToString(),

                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center

            };
            Grid.SetRow(label, freshRow);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);
            cellTable[$"0{(freshRow).ToString()}"] = label;
        }

        private void AddCell(int row, int col)
        {
            var entry = new Microsoft.Maui.Controls.Entry
            {
                Text = "",

                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center

            };

            entry.Unfocused += Entry_Unfocused; // обробник події Unfocused
            entry.Focused += Entry_Focused;

            Grid.SetRow(entry, row);
            Grid.SetColumn(entry, col);
            grid.Children.Add(entry);

            cellTable[$"{GetColumnName(col)}{(row).ToString()}"] = entry;
            Calculator.TableIndex.TableIdentifier.Add(GetColumnName(col) + row.ToString(), new TableParser.Cell());
        }

        private string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }

        private void DeleteRow()
        {
            string affectedList = "";
            var lastRowIndex = grid.RowDefinitions.Count - 1;
             grid.RowDefinitions.RemoveAt(lastRowIndex);
             if (cellTable.ContainsKey($"0{lastRowIndex.ToString()}")) grid.Children.Remove(cellTable[$"0{lastRowIndex.ToString()}"]); // Remove label

            var countcol = grid.ColumnDefinitions.Count;
            var countrow = grid.RowDefinitions.Count;
            for (int col = 1; col < countcol; col++)
            {
                string cellIndex = GetColumnName(col) + lastRowIndex.ToString();
                if (Calculator.TableIndex.TableIdentifier.ContainsKey(cellIndex)) 
                    affectedList = RemoveCell(cellIndex, affectedList); // Remove entry
            }

            if (affectedList.Length != 0)
            {
                affectedList = affectedList.Remove(affectedList.Length - 2);
                DisplayAlert("Увага", "Видалення рядка призвело до порушення залежностей у клітинках: " + affectedList + ".", "ОК");
            }
        }

        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            if (unsavedChanges == false) unsavedChanges = true;

            if (grid.RowDefinitions.Count > 2)
            {
                DeleteRow();

            }
            else DisplayAlert("Увага", "Вилучено всі можливі рядки.", "ОК");
        }

        private string RemoveCell(string cellIndex, string affectedList)
        {
            foreach (var name in Calculator.TableIndex.TableIdentifier[cellIndex].OwnDependents)
            {
                if (!Calculator.TableIndex.TableIdentifier.ContainsKey(name)) continue;

                if (Calculator.TableIndex.TableIdentifier[name].Expression.Contains(cellIndex))
                {
                    var res = Calculator.TableIndex.TableIdentifier[name].Expression.Replace(cellIndex, "0");
                    Calculator.TableIndex.EditCell(name, res);

                    if (!affectedList.Contains(name)) affectedList += (name + ", ");
                }

                var cell = Calculator.TableIndex.TableIdentifier[name];
                if (cell.Dependencies.Contains(cellIndex))
                {
                    cell.Dependencies.Remove(cellIndex);
                }
                UpdateCellAndOwnDependents(name);
            }

            foreach (var name in Calculator.TableIndex.TableIdentifier[cellIndex].Dependencies)
            {
                var cell = Calculator.TableIndex.TableIdentifier[name];
                if (cell.OwnDependents.Contains(cellIndex))
                {
                    cell.OwnDependents.Remove(cellIndex);
                    UpdateCellAndOwnDependents(name);
                }
            }

            Calculator.TableIndex.TableIdentifier[cellIndex].Dependencies.Clear();
            Calculator.TableIndex.TableIdentifier[cellIndex].OwnDependents.Clear();

            Calculator.TableIndex.TableIdentifier.Remove(cellIndex);
            grid.Children.Remove(cellTable[cellIndex]);
            cellTable.Remove(cellIndex);

            return affectedList;
        }

        private void UpdateCellAndOwnDependents(string cellIndex)
        {
            UpdateCell(cellIndex);

            foreach (var name in Calculator.TableIndex.TableIdentifier[cellIndex].OwnDependents)
            {
                UpdateCellAndOwnDependents(name);
            }
        }

        private void DeleteColumn()
        {
            string affectedList = "";
                var lastColumnIndex = grid.ColumnDefinitions.Count - 1;
                grid.ColumnDefinitions.RemoveAt(lastColumnIndex);
                if (cellTable.ContainsKey($"{GetColumnName(lastColumnIndex)}0")) {
                grid.Children.Remove(cellTable[$"{GetColumnName(lastColumnIndex)}0"]); } // Remove label 
                var countcol = grid.ColumnDefinitions.Count;
                var countrow = grid.RowDefinitions.Count;
                for (int row = 1; row < countrow; row++)
                {
                    string cellIndex = GetColumnName(lastColumnIndex) + row.ToString();
                    if (Calculator.TableIndex.TableIdentifier.ContainsKey(cellIndex))
                    affectedList = RemoveCell(cellIndex, affectedList); // Remove entry
                }

            if (affectedList.Length != 0)
            {
                affectedList = affectedList.Remove(affectedList.Length - 2);
                DisplayAlert("Увага", "Видалення стовпчика призвело до порушення залежностей у клітинках: " + affectedList + ".", "ОК");
            }
        }

        private void DeleteColumnButton_Clicked(object sender, EventArgs e)
        {
            if (unsavedChanges == false) unsavedChanges = true;

            if (grid.ColumnDefinitions.Count > 2) DeleteColumn();
            else DisplayAlert("Увага", "Вилучено всі можливі стовпчики.", "ОК");
        }

        private void AddRowButton_Clicked(object sender, EventArgs e)
        {
            if (unsavedChanges == false) unsavedChanges = true;

            var freshRow = grid.RowDefinitions.Count;
            AddRow();

            for (int col = 1; col < grid.ColumnDefinitions.Count; col++)
            {
                AddCell(freshRow, col);
            }
        }

        private void AddColumnButton_Clicked(object sender, EventArgs e)
        {
            if (unsavedChanges == false) unsavedChanges = true;

            var freshColumn = grid.ColumnDefinitions.Count;
            AddColumn();
            if (freshColumn == 0) return;


            for (var row = 1; row < grid.RowDefinitions.Count; row++)
            {
                AddCell(row, freshColumn);
            }
        }


        // викликається, коли користувач вийде зі зміненої клітинки (втратить фокус)
        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = new Microsoft.Maui.Controls.Entry();
            if (sender == textInput) entry = lastFocusedEntry;
            else entry = (Microsoft.Maui.Controls.Entry)sender;
            var row = Grid.GetRow(entry);
            var col = Grid.GetColumn(entry);
            var content = entry.Text;
            string cellIndex = GetColumnName(col) + row.ToString();
            if (entry.Text == "" || !Calculator.TableIndex.TableIdentifier.ContainsKey(cellIndex))
            {
                return;
            }

            var possibleErrorMessage = Calculator.TableIndex.EditCell(cellIndex, entry.Text);
            if (!string.IsNullOrEmpty(possibleErrorMessage))
            {
                entry.Text = "#ERROR";
                DisplayAlert("Помилка введення", possibleErrorMessage, "OK");
                return;
               
            }

            UpdateCellAndOwnDependents(cellIndex);
        }

        private void TextInput_Focused(object sender, FocusEventArgs e)
        {
            var entry = lastFocusedEntry;
            var row = Grid.GetRow(entry);
            var col = Grid.GetColumn(entry);
            string cellIndex = GetColumnName(col) + row.ToString();
            Dispatcher.Dispatch(() =>
            {
                if (!Calculator.TableIndex.TableIdentifier.ContainsKey(cellIndex))
                {
                    textInput.Unfocus();
                    DisplayAlert("Помилка введення", $"Клітинку {cellIndex} було видалено користувачем.", "ОК");
                    return; 
                }

                if (entry.Text != Calculator.TableIndex.TableIdentifier[cellIndex].Expression)
                    textInput.Text = Calculator.TableIndex.TableIdentifier[cellIndex].Expression;

                textInput.CursorPosition = 0;
                textInput.SelectionLength = entry.Text == null ? 0 : entry.Text.Length;
            });
        }

        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            if (!textInput.IsEnabled)
            {
                textInput.IsEnabled = true;
            }

            if (lastFocusedEntry != null) lastFocusedEntry.RemoveBinding(Microsoft.Maui.Controls.Entry.TextProperty);
            var entry = (Microsoft.Maui.Controls.Entry)sender;
            var row = Grid.GetRow(entry);
            var col = Grid.GetColumn(entry);
            string cellIndex = GetColumnName(col) + row.ToString();
            Dispatcher.Dispatch(() =>
            {
                entry.SetBinding(Microsoft.Maui.Controls.Entry.TextProperty, "Text", BindingMode.TwoWay);
                entry.BindingContext = textInput;
                lastFocusedEntry = entry;

                if (entry.Text != Calculator.TableIndex.TableIdentifier[cellIndex].Expression)
                entry.Text = Calculator.TableIndex.TableIdentifier[cellIndex].Expression;
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text == null ? 0 : entry.Text.Length;
            });
        }

        private void UpdateCell(string cellName)
        {
            if (Calculator.TableIndex.TableIdentifier.ContainsKey(cellName))
            {
                var cell = (Microsoft.Maui.Controls.Entry)cellTable[cellName];
                cell.Text = Calculator.TableIndex.TableIdentifier[cellName].Value.ToString();
            }
        }

        private TableinatorMAUIApp.Models.TableAsFile ToFile()
        {
            var table = new Dictionary<string, TableParser.Cell>();
            var representation = new TableinatorMAUIApp.Models.TableAsFile(grid.RowDefinitions.Count - 1, grid.ColumnDefinitions.Count - 1, table);
            for (int row = 1; row <= representation.CountRow; row++)
            {
                for (int column = 1; column <= representation.CountColumn; column++)
                {
                    var name = GetColumnName(column) + row.ToString();
                    var cell = Calculator.TableIndex.TableIdentifier[name];
                    if (cell.Expression != "")
                    {
                        table[name] = cell;
                    }
                }
            }

            return representation;
        }

        private async void Save()
        {
            try
            {
                await fileManager.SaveAs(ToFile());
                unsavedChanges = false; ;
                return;
            }
            catch (Exception)
            {
                await DisplayAlert("Поммлка зберігання", "Таблицю не було збережено.", "OK");
            }
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            Save();
        }

        private async void LoadButton_Clicked(object sender, EventArgs e)
        {
            if (unsavedChanges)
            {
                bool answer1 = await DisplayAlert("Підтвердження", "Незбережені зміни. Бажаєте зберегти?",
                "Так", "Ні");
                if (answer1)
                {
                    Save();
                    unsavedChanges = false;
                }
            }

            var representation = new Models.TableAsFile();
            try
            {
                representation = await fileManager.Load();
                if (representation == null)
                {
                    await DisplayAlert("Помилка завантаження", "Таблицю не було завантажено", "OK");
                    return;
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Помилка завантаження", "Таблицю не було завантажено", "OK");
                return;
            }

            DestroyGrid();
            CreateGrid(representation.CountRow, representation.CountColumn);

            foreach (var pair in representation.CellTable)
            {
                var cell = (Microsoft.Maui.Controls.Entry)cellTable[pair.Key];
                Calculator.TableIndex.TableIdentifier[pair.Key] = pair.Value;
                cell.Text = pair.Value.Value.ToString();
            }

            unsavedChanges = false;
        }

        private async void NewTable_Clicked(object sender, EventArgs e)
        {
            if (unsavedChanges)
            {
                bool answer1 = await DisplayAlert("Підтвердження", "Незбережені зміни. Бажаєте зберегти?",
                "Так", "Ні");
                if (answer1)
                {
                    Save();
                    unsavedChanges = false;
                }
            }

            bool answer = await DisplayAlert("Увага", "Ви точно бажаєте скинути таблицю?", "Так", "Ні");
            if (!answer) return;

            DestroyGrid();
            CreateGrid(DefaultCountColumn, DefaultCountRow);
            unsavedChanges = false;
        }

        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            if (unsavedChanges) {
                bool answer1 = await DisplayAlert("Підтвердження", "Незбережені зміни. Бажаєте зберегти?",
                "Так", "Ні");
                if (answer1) { 
                    Save();
                    unsavedChanges = false;
                }
            }
            bool answer2 = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти?",
            "Так", "Ні");
            if (answer2)
            {
                System.Environment.Exit(0);
            }
        }

        private async void OnWork_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Про роботу", "Лабораторна робота 1 (варіант 6) студентки Демянчук Дарини.",
            "OK");
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            string message = "1) бінарні a+b, a-b, a*b, a/b;\n" +
                             "2) mod(a, b), div(a, b);\n" +
                             "3) a^b;\n" +
                             "4) max(a, b), min(a, b).";
            await DisplayAlert("Довідка", "Підтримувані операції, де a та b — дійсні числа або вказівки на комірки (e.g. A1):\n" + message,
            "OK");
        }
    }
}
