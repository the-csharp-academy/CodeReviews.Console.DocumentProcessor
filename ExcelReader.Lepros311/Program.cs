using ExcelReader.Controller;
using ExcelReader.Model;
using ExcelReader.View;
using OfficeOpenXml;

Console.Title = "Excel Reader";

ExcelPackage.License.SetNonCommercialPersonal("Andrew");

var dataRepository = new DataRepository(DatabaseUtility.GetConnectionString());
dataRepository.RecreateDatabase();

var userInterface = new UserInterface();
string filePath = userInterface.GetFilePath();

string extension = Path.GetExtension(filePath);
string fileNameWithExtension = Path.GetFileName(filePath);

try
{

    switch (extension)
    {
        case ".xlsx":
            var (excelHeaders, excelTableName) = dataRepository.ExtractHeadersFromExcel(filePath);
            var columnMapping = dataRepository.CreateTable(excelHeaders, excelTableName);
            var excelData = dataRepository.ReadExcelData(filePath);
            dataRepository.SeedData(fileNameWithExtension, excelTableName, excelData, columnMapping);
            bool hasIdColumn = dataRepository.CheckIfIdColumnExistsInExcel(filePath);
            Display.PrintAllData(excelTableName, hasIdColumn);
            bool externalOpen = userInterface.PromptForExternalOpen(extension);
            if (externalOpen)
                Display.OpenFileInExternalProgram(filePath);
            break;
        case ".csv":
            var (csvHeaders, csvTableName) = dataRepository.ExtractHeadersFromCsv(filePath);
            columnMapping = dataRepository.CreateTable(csvHeaders, csvTableName);
            var csvData = dataRepository.ReadCsvData(filePath);
            dataRepository.SeedData(fileNameWithExtension, csvTableName, csvData, columnMapping);
            hasIdColumn = dataRepository.CheckIfIdColumnExistsInCsv(filePath);
            Display.PrintAllData(csvTableName, hasIdColumn);
            externalOpen = userInterface.PromptForExternalOpen(extension);
            if (externalOpen)
                Display.OpenFileInExternalProgram(filePath);
            break;
        case ".pdf":
            var (pdfFields, pdfTableName) = dataRepository.ExtractFieldNamesFromPdf(filePath);
            columnMapping = dataRepository.CreateTable(pdfFields, pdfTableName);
            var pdfData = dataRepository.ReadPdfData(filePath);
            dataRepository.SeedData(fileNameWithExtension, pdfTableName, pdfData, columnMapping);
            Console.WriteLine("Gathering data for display...\n");
            Display.PrintAllData(pdfTableName, false);
            var editPdf = userInterface.PromptForPdfEdit();
            var dataController = new DataController(dataRepository);
            var data = dataRepository.GetAllData(pdfTableName);
            if (editPdf)
            {
                dataController.UpdatePdf(filePath, pdfTableName, data);
                Display.PrintAllData(pdfTableName, false);
            }
            externalOpen = userInterface.PromptForExternalOpen(extension);
            if (externalOpen)
                Display.OpenFileInExternalProgram(filePath);
            break;
        default:
            Console.WriteLine("Unsupported file type.");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}