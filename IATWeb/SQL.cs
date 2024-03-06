using System.Data;
using System.Text;

namespace IATWeb;

using System.Data.SqlClient;

public static class SQL
{
    private static string _connectionString = "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=IATBD;Data Source=GerbenWerk";

    private static SqlConnection _sqlConnection = new SqlConnection(_connectionString);

    public static DataTable DoSearch(string table, string selectGroup, params object[] whereClause)
    {
        if (_sqlConnection.State != ConnectionState.Open) _sqlConnection.Open();

        DataTable dataTable = new DataTable();

        try
        {
            // Construct the WHERE clause dynamically
            StringBuilder whereBuilder = new StringBuilder();
            if (whereClause != null && whereClause.Length > 0)
            {
                whereBuilder.Append(" WHERE ");
                for (int i = 0; i < whereClause.Length; i += 2) // Increment by 2 to handle pairs of columnName, value
                {
                    string columnName = whereClause[i].ToString();
                    if(!columnName.StartsWith("[")) columnName = "[" + columnName;
                    if(!columnName.EndsWith("]")) columnName = columnName + "]";
                    string value = whereClause[i + 1].ToString();
                    whereBuilder.Append($"{columnName} = @param{i / 2}"); // Use index / 2 to map to parameter index
                    if (i < whereClause.Length - 2)
                    {
                        whereBuilder.Append(" AND ");
                    }
                }
            }

            // split the selectgroup and check if [ and ] are used but still check for characters like *
            string[] selectGroupSplit = selectGroup.Split(',');
            for (int i = 0; i < selectGroupSplit.Length; i++)
            {
                if (!selectGroupSplit[i].StartsWith("[") && !selectGroupSplit[i].EndsWith("]") && !selectGroupSplit[i].Contains("*"))
                {
                    selectGroupSplit[i] = "[" + selectGroupSplit[i] + "]";
                }
            }
            selectGroup = string.Join(",", selectGroupSplit);
            
            // Construct the SQL query
            string query = $"SELECT {selectGroup} FROM {table}{whereBuilder.ToString()}";

            using (SqlCommand sqlCommand = new SqlCommand(query, _sqlConnection))
            {
                // Add parameters for WHERE clause
                for (int i = 0; i < whereClause.Length; i += 2)
                {
                    sqlCommand.Parameters.AddWithValue("@param" + (i / 2), whereClause[i + 1].ToString()); // Add parameter value
                }

                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    dataTable.Load(reader);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error executing SQL query: " + ex.Message);
            ExceptionHandler.HandleError(ex);
            return null;
        }
        finally
        {
            _sqlConnection.Close();
        }

        return dataTable;
    }

    public static DataRow Get(string table, string selectGroup, params object[] whereClause)
    {
        DataTable dataTable = DoSearch(table, selectGroup, whereClause);
        if (dataTable.Rows.Count > 0)
        {
            return dataTable.Rows[0];
        }
        return null;
    }
    
    public static bool Exists(string table, params object[] whereClause)
    {
        DataTable dataTable = DoSearch(table, "*", whereClause);
        return dataTable.Rows.Count > 0;
    }

    public static void Insert(string table, params object[] values)
    {
        if (_sqlConnection.State != ConnectionState.Open) _sqlConnection.Open();

        try
        {
            // Construct the SQL query
            StringBuilder valueBuilder = new StringBuilder();
            valueBuilder.Append("(");
            for (int i = 0; i < values.Length; i += 2) // Increment by 2 to handle pairs of columnName, value
            {
                valueBuilder.Append("@param" + (i / 2));
                if (i < values.Length - 2)
                {
                    valueBuilder.Append(", ");
                }
            }
            valueBuilder.Append(")");

            string query = $"INSERT INTO {table} VALUES {valueBuilder.ToString()}";

            using (SqlCommand sqlCommand = new SqlCommand(query, _sqlConnection))
            {
                // Add parameters for values
                for (int i = 0; i < values.Length; i += 2)
                {
                    sqlCommand.Parameters.AddWithValue("@param" + (i / 2), values[i + 1]); // Add parameter value
                }

                sqlCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error executing SQL query: " + ex.Message);
            ExceptionHandler.HandleError(ex);
        }
        finally
        {
            _sqlConnection.Close();
        }
    }
}