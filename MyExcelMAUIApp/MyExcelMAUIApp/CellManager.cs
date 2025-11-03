using LabCalculator;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Numerics;

namespace MyExcelMAUIApp
{
    public class CellManager
    {
        private Dictionary<string, string> expressions = new();
        private Dictionary<string, BigInteger> values = new();
        public string GetDisplayText(string cellName)
        {
            if (!values.ContainsKey(cellName))
                return "";

            BigInteger val = values[cellName];

            // Якщо порожнє → не показуємо нічого
            if (val == BigInteger.Zero && GetExpression(cellName) == "")
                return "";

            return val.ToString();
        }

        public void SaveToFile(string filePath)
        {
            var json = JsonSerializer.Serialize(expressions);
            File.WriteAllText(filePath, json);
        }

        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            expressions = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();

            RecalculateAll();
        }

        public void SetExpression(string cellName, string expression)
        {
            expressions[cellName] = expression;

            RecalculateAll();
            RecalculateAll();
        }

        public void ClearCell(string cellName)
        {
            if (expressions.ContainsKey(cellName))
                expressions.Remove(cellName);
            if (values.ContainsKey(cellName))
                values.Remove(cellName);
        }

        public string GetExpression(string cellName)
        {
            return expressions.TryGetValue(cellName, out var expr) ? expr : "";
        }

        public BigInteger GetValue(string cellName)
        {
            return values.TryGetValue(cellName, out var val) ? val : BigInteger.Zero;
        }
        private bool IsLogical(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            string normalized = expression.ToLowerInvariant().Replace(" ", "");

            return normalized.Contains("=")
                || normalized.Contains("<")
                || normalized.Contains(">")
                || normalized.Contains("and")
                || normalized.Contains("or")
                || normalized.Contains("not")
                || normalized.Contains("true")
                || normalized.Contains("false");
        }


        public Dictionary<string, BigInteger> GetAllValues() => new(values);

        private void Recalculate(string cellName)
        {
            string expr = GetExpression(cellName);

            if (string.IsNullOrWhiteSpace(expr))
            {
                // Комірка порожня → стираємо значення
                values.Remove(cellName);
                return;
            }

            try
            {
                BigInteger result = Calculator.Evaluate(expr, name =>
                {
                    return values.TryGetValue(name, out var v) ? v : BigInteger.Zero;
                });

                values[cellName] = result;
            }
            catch
            {
                // Позначаємо невірний вираз спеціальним значенням
                values[cellName] = BigInteger.Zero;
            }
        }

        public void RecalculateAll()
        {
            foreach (var cell in expressions.Keys)
            {
                Recalculate(cell);
            }
        }

    }
}