using System;
namespace NppDB.MSSQL
{
    internal interface IMSSQLObject
    {
        string DBSchema { get; set; }
        string FullName { get; }
        string Title { get; set; }
    }
}
