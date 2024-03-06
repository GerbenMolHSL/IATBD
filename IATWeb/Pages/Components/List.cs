using System.Data;

namespace IATWeb.Pages.Components;

public static class List
{
    public static string Create(DataTable data, string editUrl, bool allowDelete, Dictionary<string, string> columnTranslations, params string[] showItems)
    {
        string list = "<table class=\"ui celled table\">";
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
                    list += $"<td data-label=\"{column.ColumnName}\">{row[column.ColumnName]}</td>";
                }
            }
            if (!string.IsNullOrEmpty(editUrl)) list += $"<td data-label=\"Edit\" class=\"center aligned\"><a href=\"{editUrl}?id={row["id"]}\"><i class=\"edit icon\"></i></a></td>";
            if (allowDelete && !string.IsNullOrEmpty(editUrl)) list += $"<td data-label=\"Delete\" class=\"center aligned\"><a onclick=\"handleDelete({row["id"]})\"><i class=\"trash icon red\"></i></a></td>";
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
            list += $"        window.location.href = '/{editUrl}?id=' + id;";
            list += "    }";
            list += "}";
            list += "</script>";
        }

        return list;
    }

    public static string CreateEdit(DataRow data, string entty, string pk, string createUpdateUrl, string backUrl, List<string> requiredFields, List<string> lockedFields, List<string> showFields, Dictionary<string, string> columnTranslations)
    {
        string edit = "";
        
        // Back button with icon
        edit += "<a href=\"" + backUrl + "\" class=\"ui labeled icon button\">";
        edit += "<i class=\"left arrow icon\"></i>";
        edit += "Terug";
        edit += "</a>";

        edit += $"<form id=\"{entty}-edit-{pk}\" class=\"ui form\" action=\"{createUpdateUrl}\" method=\"post\">";
        edit += $"<input data-required=\"true\" type=\"hidden\" name=\"{pk}\" value=\"{pk}\">";
        foreach (DataColumn column in data.Table.Columns)
        {
            if (showFields.Count == 0 || showFields.Contains(column.ColumnName))
            {
                edit += "<div class=\"field\">";
                string translatedColumn = columnTranslations.ContainsKey(column.ColumnName) ? columnTranslations[column.ColumnName] : column.ColumnName;
                edit += $"<label for=\"{column.ColumnName}\">{translatedColumn}</label>";
                bool required = requiredFields.Contains(column.ColumnName);
                if (lockedFields.Contains(column.ColumnName))
                {
                    edit += $"<input data-required=\"{required}\" type=\"text\" name=\"{column.ColumnName}\" value=\"{data[column.ColumnName]}\" disabled>";
                }
                else
                {
                    edit += $"<input data-required=\"{required}\" type=\"text\" name=\"{column.ColumnName}\" value=\"{data[column.ColumnName]}\">";
                }
                edit += "</div>";
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