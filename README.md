# Excel Reader Project by C# Academy

## Project Overview:

This project involves reading data from an .xlsx or .csv file, storing the content in a database, and exporting the data back to an .xlsx or .csv file.

Project link: [Excel Reader](https://www.thecsharpacademy.com/project/20/excel-reader)

## Project Requirements

- This is an application that will read data from an Excel spreadsheet into a database
- When the application starts, it should delete the database if it exists, create a new one, create all tables, read from Excel, seed into the database.
- You need to use EPPlus package
- You shouldn't read into Json first.
- You can use SQLite or SQL Server (or MySQL if you're using a Mac)
- Once the database is populated, you'll fetch data from it and show it in the console.
- You don't need any user input
- You should print messages to the console letting the user know what the app is doing at that moment (i.e. reading from excel; creating tables, etc)
- The application will be written for a known table, you don't need to make it dynamic.
- When submitting the project for review, you need to include an xls file that can be read by your application.

## Additional challenges:

- If you want to expand on this project, try to create a program that reads data from any excel sheet, regardless of the number of columns or the content of the header.
- Add the ability to read from other types of files, i.e. csv, pdf, doc
- Let the user choose the file that will be read, by inserting the path.
- Add a functionality to write into files, you can also use EPPlus for that.

## Project approach:
I started by creating a blank project to test basic Excel file reading. From the beginning, I knew I wanted to tackle most of the project’s challenges, so I began brainstorming ideas for dynamic data reading.
The core model idea came together fairly quickly, but I spent a lot of time figuring out how to populate a database with dynamic models using EF Core. Ultimately, I couldn’t find a clean way to do this without relying on raw SQL. As a result, I switched to Dapper, which gave me much more flexibility and success in creating dynamic tables and inserting dynamic records.
After getting Excel reading in place, I moved on to implementing CSV file reading. I discovered the CsvHelper package, which had great documentation and made the implementation process quick. However, I did need to make some adjustments in the repository, including fixing SQL formatting and adding parameter support—something I hadn't implemented for ExcelReader.
Once both the Excel and CSV readers were working, I proceeded to implement file writing functionality.

## Lessons Learned
- Learned how to use EPP and CSV reader packages and refreshed knowledge about StreamReaders and Writers

## Main resources used:
- [EPP package documentation](https://epplussoftware.com/) 
- [CSV reader documentation](https://joshclose.github.io/CsvHelper/)
- [Dapper documentation](https://www.learndapper.com/)

## Packages Used
| Package | Version |
|---------|---------|
| CsvHelper | 33.1.0 |
| Dapper | 2.1.66 |
| EPPlus | 8.0.5 |
| Microsoft.Data.SqlClient | 5.1.6 |
| Microsoft.Extensions.DependencyInjection | 9.0.5 |
| Spectre.Console | 0.50.0 |
