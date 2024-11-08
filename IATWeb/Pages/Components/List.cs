﻿using System.Data;
using System.Globalization;

namespace IATWeb.Pages.Components;

public static class List
{
    public static string Create(DataTable data, string editUrl, string deleteUrl, bool allowDelete, bool allowNew, Dictionary<string, string> columnTranslations, Dictionary<string, Type> columnTypes, Dictionary<string, ForeignKeyObject> foreignKeys, string customButtons, Dictionary<string, string> customRefButtons, params string[] showItems)
    {
        string list = "";
        
        // Back button with icon
        if (allowNew)
        {
            list += "<a href=\"" + editUrl + "\" class=\"ui labeled icon button primary\">";
            list += "<i class=\"plus icon\"></i>";
            list += "New";
            list += "</a>";
        }
        
        if (!string.IsNullOrEmpty(customButtons))
        {
            list += "<br>";
            list += customButtons;
        }

        list += "<div style=\"max-width:calc(100vw - 4rem); overflow-x: auto; margin-top: 1rem;\">";
        list += "<table class=\"ui celled table\">";
        list += "<thead>";
        list += "<tr>";
        foreach (string column in showItems)
        {
            if(string.IsNullOrEmpty(column)) continue;
            string translatedColumn = columnTranslations.ContainsKey(column) ? columnTranslations[column] : column;
            list += $"<th>{translatedColumn}</th>";
        }
        
        if (!string.IsNullOrEmpty(editUrl)) list += "<th class=\"one wide\"></th>";
        if (allowDelete && !string.IsNullOrEmpty(editUrl)) list += "<th class=\"one wide\"></th>";
        
        if(customRefButtons != null && customRefButtons.Count > 0)
        {
            foreach (var customRefButton in customRefButtons)
            {
                list += $"<th class=\"one wide\"></th>";
            }
        }

        list += "</tr>";
        list += "</thead>";
        list += "<tbody>";
        foreach (DataRow row in data.Rows)
        {
            list += "<tr>";
            foreach (string column in showItems)
            {
                if(string.IsNullOrEmpty(column)) continue;
                string value = row[column].ToString();
                if (foreignKeys.ContainsKey(column))
                {
                    value = foreignKeys[column].GetLabel(value);
                }
                else if (columnTypes.ContainsKey(column))
                {
                    if(columnTypes[column].IsEnum)
                        value = Enum.GetName(columnTypes[column], row[column]);
                    else
                    {
                        switch (columnTypes[column])
                        {
                            case Type decimalType when decimalType == typeof(decimal):
                                decimal.TryParse(row[column]?.ToString(), out decimal result);
                                value = result.ToString("F2", CultureInfo.InvariantCulture);
                                break;
                            case Type passwordType when passwordType == typeof(Password):
                                value = "********";
                                break;
                        }
                    }
                }
                list += $"<td data-label=\"{column}\">{value}</td>";
            }
            if (!string.IsNullOrEmpty(editUrl)) list += $"<td data-label=\"Edit\" class=\"center aligned\"><a href=\"{editUrl}?id={row["id"]}\"><i class=\"edit icon\"></i></a></td>";
            if (allowDelete && !string.IsNullOrEmpty(deleteUrl)) list += $"<td data-label=\"Delete\" class=\"center aligned\"><a data-confirm=\"Are you sure?\" data-method=\"delete\" onclick=\"handleDelete({row["id"]})\"><i class=\"trash icon red\"></i></a></td>";
            
            if(customRefButtons != null && customRefButtons.Count > 0)
            {
                foreach (var customRefButton in customRefButtons)
                {
                    list += $"<td data-label=\"{customRefButton.Key}\" class=\"center aligned\"><a {(customRefButton.Value.Split("|").Length > 1 ? $"onclick=\"return confirm('{customRefButton.Value.Split("|").First()}')\"" : "")} href=\"{customRefButton.Value.Split("|").Last()}?id={row["id"]}\"><i class=\"{customRefButton.Key}\"></i></a></td>";
                }
            }
            
            list += "</tr>";
        }
        list += "</tbody>";
        list += "</table>";
        list += "</div>";
        
        //Add javascript for the delete functionality if it's enabled
        if (allowDelete && !string.IsNullOrEmpty(editUrl))
        {
            list += "<script>";
            list += "function handleDelete(id) {";
            list += "    if (confirm('Are you sure you want to delete this item?')) {";
            list += $"        fetch('{deleteUrl}'+'?id='+id, {{";
            list += "            method: 'DELETE'";
            list += "        })";
            list += "        .then(response => {";
            list += "            if (response.ok) {";
            list += "                return response.text();";
            list += "            } else {";
            list += "                throw new Error('Failed to delete and replace');";
            list += "            }";
            list += "        })";
            list += "        .then(html => {";
            list += "            document.open();";
            list += "            document.write(html);";
            list += "            document.close();";
            list += "        })";
            list += "        .catch(error => {";
            list += "            console.error('Error:', error);";
            list += "        });";
            list += "    }";
            list += "}";
            list += "</script>";
        }

        return list;
    }

    public static string CreateEdit(DataRow data, string entty, string pk, string createUpdateUrl, string backUrl, List<string> requiredFields, List<string> lockedFields, List<string> showFields, Dictionary<string, string> columnTranslations, Dictionary<string, Type> columnTypes = null, Dictionary<string, ForeignKeyObject> foreignKeys = null, KeyValuePair<string, string>[] errors = null)
    {
        string edit = "";
        try
        {
            // Back button with icon
            if (!string.IsNullOrEmpty(backUrl))
            {
                edit += "<a href=\"" + backUrl + "\" class=\"ui labeled icon button\">";
                edit += "<i class=\"left arrow icon\"></i>";
                edit += "Terug";
                edit += "</a>";
            }

            if (errors != null) edit += Errors.GenerateErrors(columnTranslations, errors);

            List<DataColumn> columns = SQL.GetColumns(entty);

            WebThread thread = ThreadConfig.GetWebThread();
            if (thread.HTTPContext.Request.Method == "POST")
            {
                // Create a DataTable with the desired structure
                DataTable dataTable = new DataTable();
                foreach (DataColumn column in columns)
                {
                    try
                    {
                        dataTable.Columns.Add(column);
                    }
                    catch (Exception e)
                    {
                        // Ignore, assume the column is virtual and does not exist in db. EG: passwordRepeat, password
                    }
                }

                // Add all showFields that dont exist these are virtual
                foreach (string showField in showFields)
                {
                    bool foundColumn = false;
                    foreach (DataColumn column in columns)
                    {
                        if (column.ColumnName.Equals(showField))
                        {
                            foundColumn = true;
                            break;
                        }
                    }

                    if (!foundColumn)
                    {
                        dataTable.Columns.Add(showField);
                    }
                }

                // Create a new DataRow and populate it with form data
                data = dataTable.NewRow();
                foreach (DataColumn column in columns)
                {
                    // Check if the form contains data for the current column
                    if (!string.IsNullOrEmpty(thread.HTTPContext.Request.Form[column.ColumnName].ToString()))
                    {
                        data[column] = thread.HTTPContext.Request.Form[column.ColumnName];
                    }
                }
            }

            string pkColumnn = SQL.GetPrimaryKeyColumn(entty);

            bool hasErrors = errors != null && errors.Length > 0;
            bool containsFiles = false;

            string extension = "";
            if (!string.IsNullOrEmpty(pk)) extension = $"?id={pk}";

            edit +=
                $"<form id=\"{entty}-edit-{pk}\" class=\"ui form {(hasErrors ? "error" : "")}\" action=\"{createUpdateUrl}{extension}\" method=\"post\">";
            if (!string.IsNullOrEmpty(pk))
                edit += $"<input data-required=\"true\" type=\"hidden\" name=\"{pkColumnn}\" value=\"{pk}\">";
            if (data != null)
            {
                foreach (string column in showFields)
                {
                    if (string.IsNullOrEmpty(column)) continue;
                    if (showFields.Count == 0 || showFields.Contains(column))
                    {
                        if(errors != null) edit +=
                            $"<div class=\"field {(Helpers.KeyValuePairContains(column, errors) ? "error" : "")}\">";
                        else edit += "<div class=\"field\">";
                        string translatedColumn =
                            columnTranslations.ContainsKey(column) ? columnTranslations[column] : column;
                        edit += $"<label for=\"{column}\">{translatedColumn}</label>";
                        bool required = requiredFields.Contains(column);
                        if (foreignKeys != null && foreignKeys.ContainsKey(column))
                        {
                            // Create a select from the foreign key
                            ForeignKeyObject foreignKey = foreignKeys[column];

                            edit +=
                                $"<select data-required=\"{required}\" name=\"{column}\" class=\"ui fluid search dropdown\">";
                            foreach (DataRow row in foreignKey.DataTable.Rows)
                            {
                                bool selected = row[foreignKey.PrimaryKeyTable].ToString()
                                    .Equals(data[column].ToString());
                                edit +=
                                    $"<option {(selected ? "selected" : "")} value=\"{row[foreignKey.PrimaryKeyTable]}\">{foreignKey.GetLabel(row[foreignKey.PrimaryKeyTable].ToString())}</option>";
                            }

                            edit += "</select>";
                        }
                        else if (columnTypes != null && columnTypes.ContainsKey(column))
                        {
                            if (columnTypes[column].IsEnum)
                            {
                                if (lockedFields.Contains(column))
                                {
                                    edit +=
                                        $"<select data-required=\"{required}\" name=\"{column}\" value=\"{data[column]}\" class=\"ui fluid search dropdown\" disabled>";
                                }
                                else
                                {
                                    edit +=
                                        $"<select data-required=\"{required}\" name=\"{column}\" value=\"{data[column]}\" class=\"ui fluid search dropdown\">";
                                }

                                // Make a select from provided enum each enum has a value so add the value and name to the select
                                foreach (var value in Enum.GetValues(columnTypes[column]))
                                {
                                    bool selected = ((int)value).ToString().Equals(data[column].ToString());
                                    edit +=
                                        $"<option {(lockedFields.Contains(column) ? "disabled" : "")} {(selected ? "selected" : "")} value=\"{(int)value}\">{value}</option>";
                                }

                                edit += "</select>";
                            }

                            switch (columnTypes[column])
                            {
                                case Type decimalType when decimalType == typeof(decimal):
                                    string value = data[column].ToString();
                                    value = value.Replace(".", ",");
                                    decimal.TryParse(value, out decimal result);
                                    // Logic specific to handling decimal columns
                                    edit +=
                                        $"<input {(lockedFields.Contains(column) ? "disabled" : "")} type=\"number\" name=\"{column}\" value=\"{result.ToString("F2", CultureInfo.InvariantCulture)}\" step=\"0.01\" min=\"0.00\" placeholder=\"0.00\">";
                                    break;
                                case Type dateTimeType when dateTimeType == typeof(DateTime):
                                    if (!string.IsNullOrEmpty(data[column]?.ToString()))
                                    {
                                        DateTime.TryParse(data[column]?.ToString(), out DateTime resultDate);
                                        edit +=
                                            $"<input {(lockedFields.Contains(column) ? "disabled" : "")} type=\"datetime-local\" name=\"{column}\" value=\"{resultDate.ToString("yyyy-MM-dd HH:mm:ss")}\">";
                                    }
                                    else
                                    {
                                        edit += $"<input {(lockedFields.Contains(column) ? "disabled" : "")} type=\"datetime-local\" name=\"{column}\">";
                                    }

                                    break;
                                case Type passwordType when passwordType == typeof(Password):
                                    edit += $"<input {(lockedFields.Contains(column) ? "disabled" : "")} type=\"password\" name=\"{column}\" value=\"{data[column]}\">";
                                    break;
                                case Type fileType when fileType == typeof(File):
                                    edit += FileHandler.GetFileUpload(column, data[column].ToString(), lockedFields.Contains(column));
                                    containsFiles = true;
                                    break;
                                case Type textAreaType when textAreaType == typeof(TextArea):
                                    edit +=
                                        $"<textarea {(lockedFields.Contains(column) ? "disabled" : "")} data-required=\"{required}\" name=\"{column}\">{data[column]}</textarea>";
                                    break;
                                case Type boolType when boolType == typeof(bool):
                                    edit +=
                                        $"<input {(lockedFields.Contains(column) ? "disabled" : "")} type=\"checkbox\" name=\"{column}\" value=\"true\" {(data[column].ToString().Equals("True") ? "checked" : "")}>";
                                    break;
                            }
                        }
                        else
                        {
                            if (lockedFields.Contains(column))
                            {
                                edit +=
                                    $"<input data-required=\"{required}\" type=\"text\" name=\"{column}\" value=\"{data[column]}\" disabled>";
                            }
                            else
                            {
                                edit +=
                                    $"<input data-required=\"{required}\" type=\"text\" name=\"{column}\" value=\"{data[column]}\">";
                            }
                        }

                        edit += "</div>";
                    }
                }
            }
            else
            {
                foreach (string column in showFields)
                {
                    if (string.IsNullOrEmpty(column)) continue;
                    if (showFields.Count == 0 || showFields.Contains(column))
                    {
                        if(errors != null) edit +=
                            $"<div class=\"field {(Helpers.KeyValuePairContains(column, errors) ? "error" : "")}\">";
                        else edit += "<div class=\"field\">";
                        string translatedColumn =
                            columnTranslations.ContainsKey(column) ? columnTranslations[column] : column;
                        edit += $"<label for=\"{column}\">{translatedColumn}</label>";
                        bool required = requiredFields.Contains(column);
                        if (foreignKeys != null && foreignKeys.ContainsKey(column))
                        {
                            // Create a select from the foreign key
                            ForeignKeyObject foreignKey = foreignKeys[column];

                            edit +=
                                $"<select data-required=\"{required}\" name=\"{column}\" class=\"ui fluid search dropdown\">";
                            foreach (DataRow row in foreignKey.DataTable.Rows)
                            {
                                edit +=
                                    $"<option value=\"{row[foreignKey.PrimaryKeyTable]}\">{foreignKey.GetLabel(row[foreignKey.PrimaryKeyTable].ToString())}</option>";
                            }

                            edit += "</select>";
                        }
                        else if (columnTypes != null && columnTypes.ContainsKey(column))
                        {
                            if (columnTypes[column].IsEnum)
                            {
                                edit +=
                                    $"<select data-required=\"{required}\" name=\"{column}\" class=\"ui fluid search dropdown\">";
                                // Make a select from provided enum each enum has a value so add the value and name to the select
                                foreach (var value in Enum.GetValues(columnTypes[column]))
                                {
                                    edit += $"<option value=\"{(int)value}\">{value}</option>";
                                }

                                edit += "</select>";
                            }

                            switch (columnTypes[column])
                            {
                                case Type decimalType when decimalType == typeof(decimal):
                                    // Logic specific to handling decimal columns
                                    edit +=
                                        $"<input data-required=\"{required}\" type=\"number\" name=\"{column}\" step=\"0.01\" min=\"0.00\" placeholder=\"0.00\">";
                                    break;
                                case Type dateTimeType when dateTimeType == typeof(DateTime):
                                    edit +=
                                        $"<input data-required=\"{required}\" type=\"datetime-local\" name=\"{column}\">";
                                    break;
                                case Type passwordType when passwordType == typeof(Password):
                                    edit += $"<input type=\"password\" name=\"{column}\"\">";
                                    break;
                                case Type fileType when fileType == typeof(File):
                                    edit += FileHandler.GetFileUpload(column, null, false);
                                    containsFiles = true;
                                    break;
                                case Type textAreaType when textAreaType == typeof(TextArea):
                                    edit += $"<textarea data-required=\"{required}\" name=\"{column}\"></textarea>";
                                    break;
                                case Type boolType when boolType == typeof(bool):
                                    edit += $"<input type=\"checkbox\" name=\"{column}\" value=\"true\">";
                                    break;
                            }
                        }
                        else
                        {
                            edit += $"<input data-required=\"{required}\" type=\"text\" name=\"{column}\">";
                        }

                        edit += "</div>";
                    }
                }
            }

            if (!string.IsNullOrEmpty(createUpdateUrl)) edit += "<button class=\"ui primary button\" type=\"submit\">Opslaan</button>";
            if (!string.IsNullOrEmpty(backUrl) && !string.IsNullOrEmpty(createUpdateUrl)) edit += $"<a class=\"ui negative button\" href={backUrl}>Annuleer</a>";
            edit += "</form>";

            if (containsFiles)
            {
                edit += FileHandler.generateViewFileDiv();
            }

            //Add javascript for the form validation
            edit += "<script>";
            edit += $"let form = document.getElementById('{entty}-edit-{pk}');";
            edit += "form.addEventListener('submit', function(event) {";
            edit += "    if (!validateForm(form)) {";
            edit += "        event.preventDefault();"; // Prevent form submission
            edit += "    }";
            edit += "});";
            edit += "</script>";
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        return edit;
    }
}

public class ForeignKeyObject
{
    public DataTable DataTable { get; set; }
    public string PrimaryKeyTable { get; set; }
    public string LabelGroup { get; set; }
    
    public ForeignKeyObject(DataTable dataTable, string primaryKeyTable, string labelGroup)
    {
        DataTable = dataTable;
        PrimaryKeyTable = primaryKeyTable;
        LabelGroup = labelGroup;
    }

    public string GetLabel(string pk)
    {
        if (string.IsNullOrEmpty(pk)) return "";
        try
        {
            DataRow row = DataTable.Select($"{PrimaryKeyTable} = '{pk}'")[0];
            List<string> labelSplit = LabelGroup.Split(',').ToList();
            string label = "";
            foreach (string labelPart in labelSplit)
            {
                string part = labelPart.Trim();
                label += row[part] + " ";
            }
            return label.Trim();
        }
        catch(Exception e){}

        return pk;
    }
}
// <table class="ui celled table">
//     <thead>
//     <tr><th>Name</th>
//     <th>Age</th>
//     <th>Job</th>
//     </tr></thead>
//     <tbody>
//     <tr>
//     <td data-label="Name">James</td>
//     <td data-label="Age">24</td>
//     <td data-label="Job">Engineer</td>
//     </tr>
//     <tr>
//     <td data-label="Name">Jill</td>
//     <td data-label="Age">26</td>
//     <td data-label="Job">Engineer</td>
//     </tr>
//     <tr>
//     <td data-label="Name">Elyse</td>
//     <td data-label="Age">24</td>
//     <td data-label="Job">Designer</td>
//     </tr>
//     </tbody>
//     </table>


public class TextArea
{
}