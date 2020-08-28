namespace DataQI.Dapper.FastCrud.Test.Resources
{
    public static class SqlResource
    {
        public static readonly string SELECT_LAST_INSERT_ROW_ID = @"
            SELECT last_insert_rowid() [ID]
            ";

        public static readonly string PERSON_CREATE_SCRIPT = @"
            CREATE TABLE IF NOT EXISTS [PERSON] (
            [PERSON_ID] INTEGER PRIMARY KEY AUTOINCREMENT,
            [ACTIVE] BOOLEAN,
            [DOCUMENT] VARCHAR(30),
            [FULL_NAME] VARCHAR(60),
            [PHONE_NUMBER] VARCHAR(15),
            [EMAIL] VARCHAR(50),
            [DATE_BIRTH] DATE,
            [DATE_REGISTER] DATE
            );";

        public static readonly string PRODUCT_CREATE_SCRIPT = @"
            CREATE TABLE IF NOT EXISTS [PRODUCT] (
            [PRODUCT_ID] INTEGER PRIMARY KEY AUTOINCREMENT,
            [ACTIVE] BOOLEAN,
            [EAN] VARCHAR(20),
            [REFERENCE] VARCHAR(30),
            [NAME] VARCHAR(120),
            [DEPARTMENT] VARCHAR(120),
            [PRICE] DECIMAL(9, 2),
            [LIST_PRICE] DECIMAL(9, 2),
            [KEYWORDS] VARCHAR(120),
            [STOCK] DECIMAL(9, 2),
            [DATE_REGISTER] DATE
            );";
    }
}