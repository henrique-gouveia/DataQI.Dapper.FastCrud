namespace DataQI.Dapper.FastCrud.Test.Resources
{
    public static class SqlResource
    {
        public static readonly string SELECT_LAST_INSERT_ROW_ID = @"
            SELECT last_insert_rowid() [ID]";

        public static readonly string CUSTOMER_CREATE_SCRIPT = @"
            CREATE TABLE IF NOT EXISTS [CUSTOMER] (
                [CUSTOMER_ID] INTEGER PRIMARY KEY AUTOINCREMENT,
                [ACTIVE] BOOLEAN,
                [DOCUMENT] VARCHAR(30),
                [FULL_NAME] VARCHAR(60),
                [PHONE_NUMBER] VARCHAR(15),
                [EMAIL] VARCHAR(30),
                [DATE_BIRTH] DATE,
                [DATE_REGISTER] DATE
            );";

        public static readonly string DEPARTMENT_CREATE_SCRIT = @"
            CREATE TABLE IF NOT EXISTS [DEPARTMENT] (
                [DEPARTMENT_ID] INTEGER PRIMARY KEY AUTOINCREMENT,
                [NAME] VARCHAR(30)
            );";

        public static readonly string EMPLOYEE_CREATE_SCRIT = @"
            CREATE TABLE IF NOT EXISTS [EMPLOYEE] (
                [EMPLOYEE_ID] INTEGER PRIMARY KEY AUTOINCREMENT,
                [LAST_NAME] VARCHAR(30),
                [FIRST_NAME] VARCHAR(30),
                [TITLE] VARCHAR(30),
                [DEPARTMENT_ID] INTEGER NULL,
                [BIRTH_DATE] DATETIME,
                [HIRE_DATE] DATETIME,
                [PHONE_NUMBER] VARCHAR(15),
                [EMAIL] VARCHAR(30),
                FOREIGN KEY ([DEPARTMENT_ID]) REFERENCES [DEPARTMENT] ([DEPARTMENT_ID]) 
		        ON DELETE NO ACTION ON UPDATE NO ACTION
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