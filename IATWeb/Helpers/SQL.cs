using System.Data;
using System.Text;
using IATWeb.Authenticators;
using IATWeb.Pages;

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
                    string value = whereClause[i + 1] == null ? "" : whereClause[i + 1].ToString();
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
                    sqlCommand.Parameters.AddWithValue("@param" + (i / 2), whereClause[i + 1] == null ? "" : whereClause[i + 1].ToString()); // Add parameter value
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
            // Consstruct the columns
            StringBuilder columnBuilder = new StringBuilder();
            columnBuilder.Append("(");
            for (int i = 0; i < values.Length; i += 2) // Increment by 2 to handle pairs of columnName, value
            {
                string column = values[i].ToString();
                if(!column.StartsWith("[")) column = "[" + column;
                if(!column.EndsWith("]")) column = column + "]";
                columnBuilder.Append(column);
                if (i < values.Length - 2)
                {
                    columnBuilder.Append(", ");
                }
            }
            columnBuilder.Append(")");

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

            string query = $"INSERT INTO {table} {columnBuilder.ToString()} VALUES {valueBuilder.ToString()}";

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
    
    public static void Update(string table, object[] values, string whereColumn, string whereValue)
    {
        if (_sqlConnection.State != ConnectionState.Open) _sqlConnection.Open();

        try
        {
            // Construct the SET clause dynamically
            StringBuilder setBuilder = new StringBuilder();
            for (int i = 0; i < values.Length; i += 2) // Increment by 2 to handle pairs of columnName, value
            {
                string columnName = values[i].ToString();
                if(!columnName.StartsWith("[")) columnName = "[" + columnName;
                if(!columnName.EndsWith("]")) columnName = columnName + "]";
                setBuilder.Append($"{columnName} = @param{i / 2}"); // Use index / 2 to map to parameter index
                if (i < values.Length - 2)
                {
                    setBuilder.Append(", ");
                }
            }

            // Construct the SQL query
            string query = $"UPDATE {table} SET {setBuilder.ToString()} WHERE [{whereColumn}] = @whereValue";

            using (SqlCommand sqlCommand = new SqlCommand(query, _sqlConnection))
            {
                // Add parameters for SET clause
                for (int i = 0; i < values.Length; i += 2)
                {
                    sqlCommand.Parameters.AddWithValue("@param" + (i / 2), values[i + 1]); // Add parameter value
                }
                sqlCommand.Parameters.AddWithValue("@whereValue", whereValue); // Add parameter value for WHERE clause

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

    public static string GetPrimaryKeyColumn(string table)
    {
        // Get the column name of the primary key
        if (_sqlConnection.State != ConnectionState.Open) _sqlConnection.Open();
        
        string primaryKeyColumn = "";
        
        try
        {
            string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{table}' AND CONSTRAINT_NAME LIKE '%PK_%'";
            using (SqlCommand sqlCommand = new SqlCommand(query, _sqlConnection))
            {
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        primaryKeyColumn = reader.GetString(0);
                    }
                }
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
        
        return primaryKeyColumn;
    }
    
    public static bool InsertOrUpdateForm(string table, Authenticator authenticator, out KeyValuePair<string,string>[] errors, params string[] columns)
    {
        WebThread thread = ThreadConfig.GetWebThread();
        List<KeyValuePair<string,string>> errorList = new();
        errors = errorList.ToArray();
        // Check if request is POST
        if (thread.HTTPContext.Request.Method == "POST")
        {
            // Check if all columns are present in the form
            bool allColumnsPresent = true;
            foreach (string column in columns)
            {
                if (string.IsNullOrEmpty(thread.HTTPContext.Request.Form[column]))
                {
                    allColumnsPresent = false;
                    errorList.Add(new KeyValuePair<string, string>(column, $"{column} is een verplicht veld!"));
                }
            }
            
            errors = errorList.ToArray();
            
            if (allColumnsPresent)
            {
                if (!authenticator.AuthenticatePost())
                {
                    return false;
                }

                string primaryKeyValue = thread.HTTPContext.Request.Form[authenticator.column];

                // Insert the form data into the database
                List<object> values = new();
                foreach (string column in columns)
                {
                    values.Add(column);
                    if (DateTime.TryParse(thread.HTTPContext.Request.Form[column].ToString(), out DateTime date))
                    {
                        values.Add(date);
                    }
                    else
                    {
                        values.Add(thread.HTTPContext.Request.Form[column].ToString());
                    }
                }

                // Check if the record already exists
                if (Exists(table, authenticator.column, primaryKeyValue))
                {
                    Update(table, values.ToArray(), authenticator.column, primaryKeyValue);
                }
                else
                {
                    Insert(table, values.ToArray());
                }
            }
            else return false;
        }
        
        return true;
    }

    public static bool Delete(string table, Authenticator authenticator, params object[] whereClause)
    {
        // Delete function with where clause builder surrounded by try/catch
        
        if (!authenticator.AuthenticateDelete())
        {
            return false;
        }
        
        if (_sqlConnection.State != ConnectionState.Open) _sqlConnection.Open();
        
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
            
            // Construct the SQL query
            string query = $"DELETE FROM {table}{whereBuilder.ToString()}";

            using (SqlCommand sqlCommand = new SqlCommand(query, _sqlConnection))
            {
                // Add parameters for WHERE clause
                for (int i = 0; i < whereClause.Length; i += 2)
                {
                    sqlCommand.Parameters.AddWithValue("@param" + (i / 2), whereClause[i + 1].ToString()); // Add parameter value
                }

                sqlCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error executing SQL query: " + ex.Message);
            ExceptionHandler.HandleError(ex);
            return false;
        }
        finally
        {
            _sqlConnection.Close();
        }
        
        return true;
    }

    public static string GetNewID(string table)
    {
        // Get next id for a table
        if (_sqlConnection.State != ConnectionState.Open) _sqlConnection.Open();
        
        string newID = "";
        
        try
        {
            string query = $"SELECT IDENT_CURRENT('{table}') + 1";
            using (SqlCommand sqlCommand = new SqlCommand(query, _sqlConnection))
            {
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        newID = reader.GetDecimal(0).ToString();
                    }
                }
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
        
        return newID;
    }

    public static List<DataColumn> GetColumns(string table)
    {
        // Get the columns of a table
        if (_sqlConnection.State != ConnectionState.Open) _sqlConnection.Open();
        
        List<DataColumn> columns = new();
        
        try
        {
            string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}'";
            using (SqlCommand sqlCommand = new SqlCommand(query, _sqlConnection))
            {
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(new DataColumn(reader.GetString(0)));
                    }
                }
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
        
        return columns;
    }
}