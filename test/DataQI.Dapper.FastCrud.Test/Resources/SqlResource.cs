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
            [FULL_NAME] VARCHAR(60), 
            [EMAIL] VARCHAR(50), 
            [TELEPHONE] VARCHAR(15), 
            [DATE_BIRTH] DATE);
            ";
    }
}