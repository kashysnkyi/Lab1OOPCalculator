using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;
using Grid = Microsoft.Maui.Controls.Grid;
using LabCalculator;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using MyExcelMAUIApp;


namespace MyExcelMAUIApp
{
    public partial class MainPage : ContentPage
    {
        CellManager cellManager = new CellManager();
        bool showFormulas = false;

        const int CountColumn = 10; // кількість стовпчиків (A to Z)
        int CountRow = 12; // кількість рядків
        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
        }
        //створення таблиці
        private void CreateGrid()
        {
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddColumnsAndColumnLabels();
            AddRowsAndCellEntries();
        }
        private void AddColumnsAndColumnLabels()
        {
            // Додати стовпці та підписи для стовпців
            for (int col = 0; col < CountColumn + 1; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                if (col > 0)
                {
                    var label = new Label
                    {
                        Text = GetColumnName(col),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, col);
                    grid.Children.Add(label);
                }
            }
        }
        private void AddRowsAndCellEntries()
        {
            // Додати рядки, підписи для рядків та комірки
            for (int row = 0; row < CountRow; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                // Додати підпис для номера рядка
                var label = new Label
                {
                    Text = (row + 1).ToString(),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                Grid.SetRow(label, row + 1);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);
                // Додати комірки (Entry) для вмісту
                for (int col = 0; col < CountColumn; col++)
                {
                    var entry = new Entry
                    {
                        Text = "",
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    entry.Unfocused += Entry_Unfocused; // обробник події Unfocused
                    Grid.SetRow(entry, row + 1);
                    Grid.SetColumn(entry, col + 1);
                    grid.Children.Add(entry);
                }
            }
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
        private void RefreshGridDisplay()
        {
            foreach (var child in grid.Children)
            {
                if (child is Entry entry)
                {
                    int row = Grid.GetRow(entry) - 1;
                    int col = Grid.GetColumn(entry) - 1;

                    string cellName = GetColumnName(col + 1) + (row + 1);

                    if (showFormulas)
                        entry.Text = cellManager.GetExpression(cellName); // показує формулу
                    else
                        entry.Text = cellManager.GetDisplayText(cellName); // показує значення
                }
            }
        }


        private void ToggleFormulasButton_Clicked(object sender, EventArgs e)
        {
            showFormulas = !showFormulas;
            RefreshGridDisplay();
        }

        private void Entry_Unfocused(object? sender, FocusEventArgs e)
        {
            var entry = (Entry)sender!;

            int row = Grid.GetRow(entry) - 1;
            int col = Grid.GetColumn(entry) - 1;

            string cellName = GetColumnName(col + 1) + (row + 1);
            string text = entry.Text?.Trim() ?? "";

            // Якщо поле стало порожнім → видаляємо клітинку і не показуємо 0
            if (string.IsNullOrEmpty(text))
            {
                cellManager.ClearCell(cellName); // <- додамо цю функцію нижче
                entry.Text = "";
                return;
            }

            cellManager.SetExpression(cellName, text);
            cellManager.RecalculateAll();
            RefreshGridDisplay();
        }

        private void RenumberRows()
        {
            int rowNumber = 1;

            foreach (var child in grid.Children)
            {
                if (child is Label lbl)
                {
                    var row = Microsoft.Maui.Controls.Grid.GetRow(lbl);
                    var col = Microsoft.Maui.Controls.Grid.GetColumn(lbl);

                    // нумеруємо лише ліву колонку (0) і не чіпаємо заголовок
                    if (col == 0 && row > 0)
                    {
                        lbl.Text = rowNumber.ToString();
                        rowNumber++;
                    }
                }
            }
        }


        private void UpdateAllCellDisplays()
        {
            foreach (var view in grid.Children)
            {
                if (view is Entry entry)
                {
                    int row = Grid.GetRow(entry) - 1;
                    int col = Grid.GetColumn(entry) - 1;

                    if (row < 0 || col < 0)
                        continue;

                    string cellName = $"{GetColumnName(col + 1)}{row + 1}";
                    entry.Text = cellManager.GetDisplayText(cellName);
                }
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "table.json");
            cellManager.SaveToFile(filePath);
            await DisplayAlert("Збережено", $"Файл:\n{filePath}", "OK");
        }

        private async void ReadButton_Clicked(object sender, EventArgs e)
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "table.json");
            cellManager.LoadFromFile(filePath);
            UpdateAllCellDisplays();
            await DisplayAlert("Завантажено", "Таблиця відновлена", "OK");
        }


        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти ? ", "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }
        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка", "Лабораторна робота 1. Варіант 1.\nВиконав студент групи К-24, Кашинський Владислав", "OK");
        }
        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            if (CountRow <= 1)
                return;

            int lastRowIndex = CountRow;

            var toRemove = new List<View>();
            foreach (var v in grid.Children)
            {
                if (v is View view && Microsoft.Maui.Controls.Grid.GetRow(view) == lastRowIndex)
                    toRemove.Add(view);
            }

            foreach (var v in toRemove)
                grid.Children.Remove(v);

            grid.RowDefinitions.RemoveAt(lastRowIndex);
            CountRow--;

            RenumberRows();
        }



        private void AddRowButton_Clicked(object sender, EventArgs e)
        {
            int newRow = CountRow + 1;

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new Label
            {
                Text = newRow.ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, newRow);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            for (int col = 0; col < CountColumn; col++)
            {
                var entry = new Entry
                {
                    Text = "",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                entry.Unfocused += Entry_Unfocused;
                Grid.SetRow(entry, newRow);
                Grid.SetColumn(entry, col + 1);
                grid.Children.Add(entry);
            }

            CountRow++;
        }
    }
}