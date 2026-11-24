using System;
using System.Collections.Generic;
using System.Data;
using Avalonia.Controls;
using Avalonia.Data;

namespace StockTrack.Avalonia.Views;

public class DataGridRowModel
{
    public DataRow Row { get; }
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    public DataGridRowModel(DataRow row)
    {
        Row = row;
        foreach (DataColumn col in row.Table.Columns)
        {
            var val = row[col];
            if (val == DBNull.Value || val == null)
            {
                _values[col.ColumnName] = string.Empty;
            }
            else if (val is DateTime dt)
            {
                _values[col.ColumnName] = dt.ToString("yyyy-MM-dd HH:mm");
            }
            else if (val is decimal dec)
            {
                _values[col.ColumnName] = dec.ToString("N2");
            }
            else if (val is bool b)
            {
                _values[col.ColumnName] = b ? "Active" : "Inactive";
            }
            else
            {
                _values[col.ColumnName] = val.ToString() ?? string.Empty;
            }
        }
    }

    public string this[string key] => _values.TryGetValue(key, out var v) ? v : string.Empty;
}

public static class DataGridHelper
{
    public static void Populate(DataGrid grid, DataTable table)
    {
        grid.AutoGenerateColumns = false;
        grid.Columns.Clear();
        grid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
        grid.HeadersVisibility = DataGridHeadersVisibility.Column;

        foreach (DataColumn col in table.Columns)
        {
            if (col.ColumnName.Equals("PasswordHash", StringComparison.OrdinalIgnoreCase))
                continue;

            DataGridLength colWidth = col.ColumnName switch
            {
                "ProductName" => new DataGridLength(2, DataGridLengthUnitType.Star),
                "Reason" => new DataGridLength(2, DataGridLengthUnitType.Star),
                "Description" => new DataGridLength(2, DataGridLengthUnitType.Star),
                "SupplierName" => new DataGridLength(1, DataGridLengthUnitType.Star),
                "CategoryName" => new DataGridLength(1, DataGridLengthUnitType.Star),
                "FullName" => new DataGridLength(1, DataGridLengthUnitType.Star),
                _ => DataGridLength.Auto
            };

            grid.Columns.Add(new DataGridTextColumn
            {
                Header = FormatColumnHeader(col.ColumnName),
                Binding = new Binding($"[{col.ColumnName}]"),
                IsReadOnly = true,
                CanUserSort = true,
                Width = colWidth
            });
        }

        var rows = new List<DataGridRowModel>(table.Rows.Count);
        foreach (DataRow row in table.Rows)
        {
            rows.Add(new DataGridRowModel(row));
        }

        grid.ItemsSource = rows;
    }

    public static string FormatColumnHeader(string name)
    {
        return name switch
        {
            "ProductID" => "ID",
            "SKU" => "SKU Code",
            "ProductName" => "Product Name",
            "CategoryID" => "Cat ID",
            "CategoryName" => "Category",
            "SupplierID" => "Sup ID",
            "SupplierName" => "Supplier",
            "UnitPrice" => "Unit Price ($)",
            "Quantity" => "Quantity",
            "ReorderLevel" => "Reorder Level",
            "TransactionID" => "Trans ID",
            "TransactionType" => "Movement Type",
            "Reason" => "Reason / Notes",
            "CreatedAt" => "Created",
            "TransactionDate" => "Date & Time",
            "Timestamp" => "Date & Time",
            "UserID" => "User ID",
            "Username" => "Username",
            "FullName" => "Full Name",
            "Role" => "Role",
            "IsActive" => "Status",
            "Description" => "Description",
            "ContactPerson" => "Contact Person",
            "Email" => "Email",
            "Phone" => "Phone",
            "Address" => "Address",
            "LogID" => "Log ID",
            "Action" => "Action",
            "Details" => "Details",
            _ => name
        };
    }
}
