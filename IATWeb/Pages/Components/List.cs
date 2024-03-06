﻿using System.Data;
using System.Globalization;

namespace IATWeb.Pages.Components;

public static class List
{
    public static string Create(DataTable data, string editUrl, string deleteUrl, bool allowDelete, bool allowNew, Dictionary<string, string> columnTranslations, Dictionary<string, Type> columnTypes, Dictionary<string, ForeignKeyObject> foreignKeys, params string[] showItems)
    {
        string list = "";
        
        // Back button with icon
        list += "<a href=\"" + editUrl + "\" class=\"ui labeled icon button primary\">";
        list += "<i class=\"plus icon\"></i>";
        list += "New";
        list += "</a>";
        
        list += "<table class=\"ui celled table\">";
        list += "<thead>";
        list += "<tr>";
        foreach (DataColumn column in data.Columns)
        {
            if (showItems.Length == 0 || showItems.Contains(column.ColumnName))
            {
                string translatedColumn = columnTranslations.ContainsKey(column.ColumnName) ? columnTranslations[column.ColumnName] : column.ColumnName;
                list += $"<th>{translatedColumn}</th>";
            }
        }
        
        if (!string.IsNullOrEmpty(editUrl)) list += "<th class=\"one wide\"></th>";
        if (allowDelete && !string.IsNullOrEmpty(editUrl)) list += "<th class=\"one wide\"></th>";

        list += "</tr>";
        list += "</thead>";
        list += "<tbody>";
        foreach (DataRow row in data.Rows)
        {
            list += "<tr>";
            foreach (DataColumn column in data.Columns)
            {
                if (showItems.Length == 0 || showItems.Contains(column.ColumnName))
                {
                    string value = row[column.ColumnName].ToString();
                    if (foreignKeys.ContainsKey(column.ColumnName))
                    {
                        value = foreignKeys[column.ColumnName].GetLabel(value);
                    }
                    else if (columnTypes.ContainsKey(column.ColumnName))
                    {
                        if(columnTypes[column.ColumnName].IsEnum)
                            value = Enum.GetName(columnTypes[column.ColumnName], row[column.ColumnName]);
                        else
                        {
                            switch (columnTypes[column.ColumnName])
                            {
                                case Type decimalType when decimalType == typeof(decimal):
                                    decimal.TryParse(row[column.ColumnName]?.ToString(), out decimal result);
                                    value = result.ToString("F2", CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                    }
                    list += $"<td data-label=\"{column.ColumnName}\">{value}</td>";
                }
            }
            if (!string.IsNullOrEmpty(editUrl)) list += $"<td data-label=\"Edit\" class=\"center aligned\"><a href=\"{editUrl}?id={row["id"]}\"><i class=\"edit icon\"></i></a></td>";
            if (allowDelete && !string.IsNullOrEmpty(deleteUrl)) list += $"<td data-label=\"Delete\" class=\"center aligned\"><a data-confirm=\"Are you sure?\" data-method=\"delete\" onclick=\"handleDelete({row["id"]})\"><i class=\"trash icon red\"></i></a></td>";
            list += "</tr>";
        }
        list += "</tbody>";
        list += "</table>";
        
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
        
        // Back button with icon
        edit += "<a href=\"" + backUrl + "\" class=\"ui labeled icon button\">";
        edit += "<i class=\"left arrow icon\"></i>";
        edit += "Terug";
        edit += "</a>";

        edit += Errors.GenerateErrors(columnTranslations, errors);
        
        List<DataColumn> columns = SQL.GetColumns(entty);
        
        WebThread thread = ThreadConfig.GetWebThread();
        if (thread.HTTPContext.Request.Method == "POST")
        {
            // Create a DataTable with the desired structure
            DataTable dataTable = new DataTable();
            foreach (DataColumn column in columns)
            {
                dataTable.Columns.Add(column);
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
        
        edit += $"<form id=\"{entty}-edit-{pk}\" class=\"ui form {(hasErrors ? "error" : "")}\" action=\"{createUpdateUrl}\" method=\"post\">";
        if(!string.IsNullOrEmpty(pk)) edit += $"<input data-required=\"true\" type=\"hidden\" name=\"{pkColumnn}\" value=\"{pk}\">";
        if (data != null)
        {
            foreach (DataColumn column in data.Table.Columns)
            {
                if (showFields.Count == 0 || showFields.Contains(column.ColumnName))
                {
                    edit += $"<div class=\"field {(Helpers.KeyValuePairContains(column.ColumnName, errors) ? "error" : "")}\">";
                    string translatedColumn = columnTranslations.ContainsKey(column.ColumnName) ? columnTranslations[column.ColumnName] : column.ColumnName;
                    edit += $"<label for=\"{column.ColumnName}\">{translatedColumn}</label>";
                    bool required = requiredFields.Contains(column.ColumnName);
                    if (foreignKeys != null && foreignKeys.ContainsKey(column.ColumnName))
                    {
                        // Create a select from the foreign key
                        ForeignKeyObject foreignKey = foreignKeys[column.ColumnName];
                        
                        edit += $"<select data-required=\"{required}\" name=\"{column.ColumnName}\" class=\"ui fluid search dropdown\">";
                        foreach (DataRow row in foreignKey.DataTable.Rows)
                        {
                            bool selected = row[foreignKey.PrimaryKeyTable].ToString().Equals(data[column.ColumnName].ToString());
                            edit += $"<option {(selected ? "selected" : "")} value=\"{row[foreignKey.PrimaryKeyTable]}\">{foreignKey.GetLabel(row[foreignKey.PrimaryKeyTable].ToString())}</option>";
                        }
                        edit += "</select>";
                    }
                    else if(columnTypes != null && columnTypes.ContainsKey(column.ColumnName))
                    {
                        if (columnTypes[column.ColumnName].IsEnum)
                        {
                            if (lockedFields.Contains(column.ColumnName))
                            {
                                edit += $"<select data-required=\"{required}\" name=\"{column.ColumnName}\" value=\"{data[column.ColumnName]}\" class=\"ui fluid search dropdown\" disabled>";
                            }
                            else
                            {
                                edit += $"<select data-required=\"{required}\" name=\"{column.ColumnName}\" value=\"{data[column.ColumnName]}\" class=\"ui fluid search dropdown\">";
                            }
                            // Make a select from provided enum each enum has a value so add the value and name to the select
                            foreach (var value in Enum.GetValues(columnTypes[column.ColumnName]))
                            {
                                bool selected = ((int)value).ToString().Equals(data[column.ColumnName].ToString());
                                edit += $"<option {(selected ? "selected" : "")} value=\"{(int)value}\">{value}</option>";
                            }
                            edit += "</select>";   
                        }

                        switch (columnTypes[column.ColumnName])
                        {
                            case Type decimalType when decimalType == typeof(decimal):
                                decimal.TryParse(data[column.ColumnName]?.ToString(), out decimal result);
                                // Logic specific to handling decimal columns
                                edit += $"<input type=\"number\" name=\"{column.ColumnName}\" value=\"{result.ToString("F2", CultureInfo.InvariantCulture)}\" step=\"0.01\" min=\"0.00\" placeholder=\"0.00\">";
                                break;
                            case Type dateTimeType when dateTimeType == typeof(DateTime):
                                if (!string.IsNullOrEmpty(data[column.ColumnName]?.ToString()))
                                {
                                    DateTime.TryParse(data[column.ColumnName]?.ToString(), out DateTime resultDate);
                                    edit += $"<input type=\"datetime-local\" name=\"{column.ColumnName}\" value=\"{resultDate.ToString("yyyy-MM-dd HH:mm:ss")}\">";
                                }
                                else
                                {
                                    edit += $"<input type=\"datetime-local\" name=\"{column.ColumnName}\">";
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (lockedFields.Contains(column.ColumnName))
                        {
                            edit += $"<input data-required=\"{required}\" type=\"text\" name=\"{column.ColumnName}\" value=\"{data[column.ColumnName]}\" disabled>";
                        }
                        else
                        {
                            edit += $"<input data-required=\"{required}\" type=\"text\" name=\"{column.ColumnName}\" value=\"{data[column.ColumnName]}\">";
                        }
                    }
                    edit += "</div>";
                }
            }
        }
        else
        {
            foreach (DataColumn column in columns)
            {
                if (showFields.Count == 0 || showFields.Contains(column.ColumnName))
                {
                    edit += $"<div class=\"field {(Helpers.KeyValuePairContains(column.ColumnName, errors) ? "error" : "")}\">";
                    string translatedColumn = columnTranslations.ContainsKey(column.ColumnName) ? columnTranslations[column.ColumnName] : column.ColumnName;
                    edit += $"<label for=\"{column.ColumnName}\">{translatedColumn}</label>";
                    bool required = requiredFields.Contains(column.ColumnName);
                    if (foreignKeys != null && foreignKeys.ContainsKey(column.ColumnName))
                    {
                        // Create a select from the foreign key
                        ForeignKeyObject foreignKey = foreignKeys[column.ColumnName];
                        
                        edit += $"<select data-required=\"{required}\" name=\"{column.ColumnName}\" class=\"ui fluid search dropdown\">";
                        foreach (DataRow row in foreignKey.DataTable.Rows)
                        {
                            edit += $"<option value=\"{row[foreignKey.PrimaryKeyTable]}\">{foreignKey.GetLabel(row[foreignKey.PrimaryKeyTable].ToString())}</option>";
                        }
                        edit += "</select>";
                    }
                    else
                    if(columnTypes != null && columnTypes.ContainsKey(column.ColumnName))
                    {
                        if (columnTypes[column.ColumnName].IsEnum)
                        {
                            edit +=
                                $"<select data-required=\"{required}\" name=\"{column.ColumnName}\" class=\"ui fluid search dropdown\">";
                            // Make a select from provided enum each enum has a value so add the value and name to the select
                            foreach (var value in Enum.GetValues(columnTypes[column.ColumnName]))
                            {
                                edit += $"<option value=\"{(int)value}\">{value}</option>";
                            }

                            edit += "</select>";
                        }
                        
                        switch (columnTypes[column.ColumnName])
                        {
                            case Type decimalType when decimalType == typeof(decimal):
                                // Logic specific to handling decimal columns
                                edit += $"<input data-required=\"{required}\" type=\"number\" name=\"{column.ColumnName}\" step=\"0.01\" min=\"0.00\" placeholder=\"0.00\">";
                                break;
                            case Type dateTimeType when dateTimeType == typeof(DateTime):
                                edit += $"<input data-required=\"{required}\" type=\"datetime-local\" name=\"{column.ColumnName}\">";
                                break;
                        }
                    }
                    else
                    {
                        edit += $"<input data-required=\"{required}\" type=\"text\" name=\"{column.ColumnName}\">";
                    }
                    edit += "</div>";
                }
            }
        }

        edit += "<button class=\"ui primary button\" type=\"submit\">Opslaan</button>";
        edit += $"<a class=\"ui negative button\" href={backUrl}>Annuleer</a>";
        edit += "</form>";
        
        //Add javascript for the form validation
        edit += "<script>";
        edit += $"let form = document.getElementById('{entty}-edit-{pk}');";
        edit += "form.addEventListener('submit', function(event) {";
        edit += "    if (!validateForm(form)) {";
        edit += "        event.preventDefault();"; // Prevent form submission
        edit += "    }";
        edit += "});";
        edit += "</script>";

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
        DataRow row = DataTable.Select($"{PrimaryKeyTable} = {pk}")[0];
        List<string> labelSplit = LabelGroup.Split(',').ToList();
        string label = "";
        foreach (string labelPart in labelSplit)
        {
            string part = labelPart.Trim();
            label += row[part] + " ";
        }
        return label.Trim();
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