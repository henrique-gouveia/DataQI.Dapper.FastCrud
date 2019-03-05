namespace Net.Data.Dapper.FastCrud.Test.Resources
{
    public static class SqlResource
    {
        public static readonly string SELECT_LAST_INSERT_ROW_ID =
            "SELECT last_insert_rowid() [ID]";

        public static string PERSON_SELECT_ALL_SCRIPT =
            "SELECT "
          + "[PERSON_ID], "
          + "[FULL_NAME], "
          + "[EMAIL], "
          + "[TELEPHONE], "
          + "[DATE_BIRTH] "
          + "FROM PERSON ";

        public static readonly string PERSON_SELECT_ONE_SCRIPT =
            "SELECT "
          + "[PERSON_ID], "
          + "[FULL_NAME], "
          + "[EMAIL], "
          + "[TELEPHONE], "
          + "[DATE_BIRTH] "
          + "FROM PERSON "
          + "WHERE [PERSON_ID] = @PERSON_ID ";

        public static readonly string PERSON_CREATE_SCRIPT =
            "CREATE TABLE IF NOT EXISTS [PERSON] ("
          + "[PERSON_ID] INTEGER PRIMARY KEY AUTOINCREMENT, "
          + "[FULL_NAME] VARCHAR(60), "
          + "[EMAIL] VARCHAR(50), "
          + "[TELEPHONE] VARCHAR(15), "
          + "[DATE_BIRTH] DATE);";

        public static readonly string PERSON_INSERT_SCRIPT =
            "INSERT INTO [PERSON] ("
          + "[FULL_NAME], "
          + "[EMAIL], "
          + "[TELEPHONE], "
          + "[DATE_BIRTH]"
          + ") VALUES ("
          + "@FULL_NAME, "
          + "@EMAIL, "
          + "@TELEPHONE, "
          + "@DATE_BIRTH);";

        public static readonly string PERSON_DELETE_ALL_SCRIPT =
            "DELETE FROM [PERSON];";
    }
}