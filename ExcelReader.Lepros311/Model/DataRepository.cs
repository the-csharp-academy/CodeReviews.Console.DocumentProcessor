using CsvHelper;
using Dapper;
using iText.Forms;
using iText.Kernel.Pdf;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Text;

namespace ExcelReader.Model;

public class DataRepository
{

    private readonly string _connectionString;
    private readonly string _targetDbName;

    public DataRepository(string connectionString)
    {
        _connectionString = connectionString;
        _targetDbName = "ExcelReaderDb";
    }

    public void RecreateDatabase()
    {
        Console.WriteLine("Creating database...");

        var masterConnectionString = $"{_connectionString};Database=master;";

        using (IDbConnection db = new SqlConnection(masterConnectionString))
        {
            db.Open();

            string dropDbSql = $@"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = '{_targetDbName}')
            BEGIN
                ALTER DATABASE [{_targetDbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_targetDbName}];
            END";
            db.Execute(dropDbSql);

            db.Execute($"CREATE DATABASE [{_targetDbName}]");
            Console.WriteLine($"Database {_targetDbName} created.\n");
        }
    }

    public (List<string> headers, string tableName) ExtractHeadersFromExcel(string filePath)
    {
        string tableName = Path.GetFileNameWithoutExtension(filePath);

        var headers = new List<string>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int colCount = worksheet.Dimension.Columns;

                for (int col = 1; col <= colCount; col++)
                {
                    headers.Add(worksheet.Cells[1, col].Text);
                }
            }
        }

        return (headers, tableName);
    }

    public (List<string> headers, string tableName) ExtractFieldNamesFromPdf(string filePath)
    {
        string tableName = Path.GetFileNameWithoutExtension(filePath);
        var fieldNames = new List<string>();

        using (PdfReader pdfReader = new PdfReader(filePath))
        using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
        {
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDocument, true);
            var fields = form.GetAllFormFields();

            foreach (var field in fields)
            {
                fieldNames.Add(field.Key);
            }
        }

        return (fieldNames, tableName);
    }

    public (List<string> headers, string tableName) ExtractHeadersFromCsv(string filePath)
    {
        string tableName = Path.GetFileNameWithoutExtension(filePath);

        var headers = new List<string>();

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            headers = csv.Context.Reader.HeaderRecord.ToList();
        }

        return (headers, tableName);
    }

    public Dictionary<string, string> CreateTable(List<string> headers, string tableName)
    {
        Console.WriteLine($"Creating [{tableName}] table...");

        var columnMapping = new Dictionary<string, string>();

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var createTableQuery = $"CREATE TABLE [{tableName}] (";
                var existingColumnNames = new HashSet<string>();

                string idColumnName = headers.FirstOrDefault(header => string.Equals(header, "id", StringComparison.OrdinalIgnoreCase));

                if (idColumnName != null)
                {
                    createTableQuery += "[Id] INT PRIMARY KEY, ";
                    columnMapping[idColumnName] = "Id";
                }
                else
                {
                    createTableQuery += "Id INT IDENTITY(1,1) PRIMARY KEY, ";
                    columnMapping["Id"] = "Id";
                }

                foreach (var header in headers)
                {
                    if (!string.Equals(header, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        string sanitizedHeader = SanitizeColumnName(header, existingColumnNames);
                        createTableQuery += $"[{sanitizedHeader}] NVARCHAR(MAX), ";
                        columnMapping[header] = sanitizedHeader;
                    }
                }

                createTableQuery = createTableQuery.TrimEnd(',', ' ') + ");";

                using (var command = new SqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Table [{tableName}] created successfully.\n");
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL error occurred while creating table [{tableName}]: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating table [{tableName}]: {ex.Message}");
        }

        return columnMapping;
    }

    public List<Dictionary<string, object>> ReadExcelData(string filePath)
    {
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            var data = new List<Dictionary<string, object>>();
            var (headers, _) = ExtractHeadersFromExcel(filePath);

            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = new Dictionary<string, object>();
                for (int col = 1; col <= colCount; col++)
                {
                    string header = headers[col - 1];
                    object value = worksheet.Cells[row, col].Text;
                    rowData[header] = value;
                }
                data.Add(rowData);
            }
            return data;
        }
    }

    public List<Dictionary<string, object>> ReadPdfData(string filePath)
    {
        var data = new List<Dictionary<string, object>>();

        using (PdfReader pdfReader = new PdfReader(filePath))
        using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
        {
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDocument, true);
            var fields = form.GetAllFormFields();

            var rowData = new Dictionary<string, object>();

            foreach (var field in fields)
            {
                string fieldName = field.Key;
                object fieldValue = field.Value.GetValue();

                if (fieldValue is PdfString pdfString)
                {
                    rowData[fieldName] = pdfString.GetValue();
                }
                else
                {
                    rowData[fieldName] = fieldValue;
                }
            }

            data.Add(rowData);
        }

        return data;
    }

    public List<Dictionary<string, object>> ReadCsvData(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<dynamic>().ToList();
            var data = new List<Dictionary<string, object>>();

            foreach (var record in records)
            {
                var rowData = new Dictionary<string, object>();
                foreach (var kvp in (IDictionary<string, object>)record)
                {
                    rowData[kvp.Key] = kvp.Value;
                }
                data.Add(rowData);
            }
            return data;
        }
    }

    public void SeedData(string fileName, string tableName, List<Dictionary<string, object>> data, Dictionary<string, string> columnMapping)
    {
        Console.WriteLine($"Populating data from [{fileName}] to [{tableName}] table...");

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var row in data)
                {
                    var parameters = new Dictionary<string, object>();

                    if (row.Keys.Any(key => string.Equals(key, "id", StringComparison.OrdinalIgnoreCase)))
                    {
                        parameters["Id"] = row.First(kvp => string.Equals(kvp.Key, "id", StringComparison.OrdinalIgnoreCase)).Value;
                    }

                    foreach (var kvp in row)
                    {
                        if (columnMapping.TryGetValue(kvp.Key, out string cleanParameterName))
                        {
                            parameters[cleanParameterName] = kvp.Value ?? DBNull.Value;
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Key '{kvp.Key}' not found in column mapping.");
                        }
                    }

                    var insertColumns = new List<string>();
                    var insertValues = new List<string>();

                    foreach (var kvp in columnMapping)
                    {
                        if (row.ContainsKey(kvp.Key))
                        {
                            insertColumns.Add($"[{kvp.Value}]");
                            insertValues.Add($"@{kvp.Value}");
                        }
                    }

                    if (!row.Keys.Any(key => string.Equals(key, "id", StringComparison.OrdinalIgnoreCase)) && columnMapping.ContainsKey("Id"))
                    {
                        insertColumns.Remove($"[Id]");
                        insertValues.Remove($"@Id");
                    }

                    string insertQuery = $"INSERT INTO [{tableName}] ({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)})";

                    using (var command = new SqlCommand(insertQuery, connection))
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        command.ExecuteNonQuery();
                    }
                }
            }

            Console.WriteLine($"Data populated to [{tableName}] table.\n");
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL error occurred while populating data to [{tableName}]: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while populating data to [{tableName}]: {ex.Message}");
        }
    }

    private string SanitizeColumnName(string columnName, HashSet<string> existingColumnNames)
    {
        var sanitized = new StringBuilder();

        foreach (char c in columnName)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                sanitized.Append(c);
            }
            else if (c == ' ')
            {
                sanitized.Append('_');
            }
            else
            {
                continue;
            }
        }

        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
        {
            sanitized.Insert(0, '_');
        }

        if (sanitized.Length > 128)
        {
            sanitized.Length = 128;
        }

        var reservedKeywords = new HashSet<string>
        {
            "SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "WHERE", "TABLE", "COLUMN", "DATABASE"
        };

        string sanitizedStringColName = sanitized.ToString();
        if (reservedKeywords.Contains(sanitizedStringColName.ToUpper()))
        {
            sanitizedStringColName = "_" + sanitizedStringColName;
        }

        if (string.IsNullOrEmpty(sanitizedStringColName))
        {
            sanitizedStringColName = "Unknown";
        }

        string uniqueColumnName = sanitizedStringColName;
        int counter = 1;
        while (existingColumnNames.Contains(uniqueColumnName))
        {
            uniqueColumnName = $"{sanitizedStringColName}_{counter++}";
        }

        existingColumnNames.Add(uniqueColumnName);

        return uniqueColumnName;
    }


    public List<Dictionary<string, object>> GetAllData(string tableName)
    {
        var data = new List<Dictionary<string, object>>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = $"SELECT * FROM [{tableName}] ORDER BY Id ASC";

            using (var command = new SqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[columnName] = value;
                    }

                    data.Add(row);
                }
            }
        }

        return data;
    }

    public bool CheckIfIdColumnExistsInExcel(string filePath)
    {
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];

            var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];

            foreach (var cell in headerRow)
            {
                if (cell.Text.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool CheckIfIdColumnExistsInCsv(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            var headerRecord = csv.Context.Reader.HeaderRecord;

            foreach (var header in headerRecord)
            {
                if (header.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void UpdatePdfData(string filePath, Dictionary<string, object> updatedValues)
    {
        string tempFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_temp.pdf");

        using (PdfReader pdfReader = new PdfReader(filePath))
        using (PdfWriter pdfWriter = new PdfWriter(tempFilePath))
        using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
        {
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDocument, true);
            var fields = form.GetAllFormFields();

            var sanitizedFields = fields.ToDictionary(field => SanitizeColumnName(field.Key, new HashSet<string>()), field => field.Value);

            foreach (var updatedValue in updatedValues)
            {
                string sanitizedFieldName = SanitizeColumnName(updatedValue.Key, new HashSet<string>());

                if (sanitizedFields.ContainsKey(sanitizedFieldName))
                {
                    var field = sanitizedFields[sanitizedFieldName];
                    field.SetValue(updatedValue.Value.ToString());
                }
                else
                {
                    Console.WriteLine($"Warning: Field '{sanitizedFieldName}' not found in PDF form.");
                }
            }
        }

        File.Delete(filePath);
        File.Move(tempFilePath, filePath);
    }

    public void UpdateDb(Dictionary<string, object> updatedKvp, string tableName)
    {
        using (var connection = new SqlConnection(DatabaseUtility.GetConnectionString()))
        {
            connection.Open();

            var column = updatedKvp.Keys.FirstOrDefault();
            var newValue = updatedKvp[column];

            string updateQuery = $"UPDATE [{tableName}] SET [{column}] = @newValue WHERE Id = 1";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = updateQuery;
                command.Parameters.AddWithValue("@newValue", newValue);

                command.ExecuteNonQuery();
            }
        }
    }
}