# Excel Reader

The [Excel Reader](https://www.thecsharpacademy.com/project/20/excel-reader) project focuses on working with files. It gets a file path from the user for either a .xlsx, .csv, or .pdf file. Then, in a database created on startup, the data is saved into a table that is created dynamically using the headers or field names for the columns. I used Dapper and Microsoft SQL Server for the database. The data is displayed for the user in a table using Spectre.Console. The user can choose to open the file in the default program for that file type. And in the case of a PDF, the user can also update the field values for the PDF. I may expand this later to enable writing to the .xlsx and .csv files as well. This project is part of [The C# Academy](https://www.thecsharpacademy.com/) curriculum.

## Requirements

- [x] This is an application that will read data from an Excel spreadsheet into a database.
- [x] When the application starts, it should delete the database if it exists, create a new one, create all tables, read from Excel, and seed into the database.
- [x] You need to use EPPlus package.
- [x] You shouldn't read into Json first.
- [x] You can use SQLite or SQL Server (or MySQL if you're using a Mac).
- [x] Once the database is populated, you'll fetch data from it and show it in the console.
- [x] You don't need any user input.
- [x] You should print messages to the console letting the user know what the app is doing at that moment (e.g. reading from excel; creating tables, etc).
- [x] The application will be written for a known table, you don't need to make it dynamic.

## Challenges

- [x] Create a program that reads data from any Excel sheet, regardless of the number of columns or the content of the header.
- [x] Add the ability to read from other types of files (e.g., .csv, .pdf, .doc).
- [x] Let the user choose the file that will be read by inserting the file path.
- [x] Add functionality to write into files. 