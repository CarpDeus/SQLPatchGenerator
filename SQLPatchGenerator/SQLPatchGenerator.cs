using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;


namespace ReluctantDBA.SQLPatchGenerator
{
  public class SQLPatchGenerator
  {

    private string dbConnection = string.Empty;

    public SQLPatchGenerator()
    {
    }

    public SQLPatchGenerator(string connectionString)
    {
      dbConnection = connectionString;
    }

    /// <summary>
    /// Get the BuildInformation for revisions
    /// </summary>
    /// <returns></returns>
    public string BuildInfo()
    {
      System.Version AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      return (string.Format("{4}\tVersion: {0}.{1}.{2}.{3}", AppVersion.Major.ToString(), AppVersion.Minor.ToString(),
        AppVersion.Build.ToString(), AppVersion.Revision.ToString(), System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name));
    }

    public void generatePatchScript(string schemaName, string objectName, bool updatePatch, ref string patchScript)
    {
      string patchDate = DateTime.Now.ToString("o").Substring(0, 10);
      StringBuilder sb = new StringBuilder();
      if (!updatePatch)
      {
        sb.AppendFormat("IF NOT EXISTS(SELECT * FROM sys.all_objects WHERE name = '{0}' and schema_name(schema_id) = '{1}')\r\n", objectName, schemaName);
        sb.AppendFormat("BEGIN \r\ndeclare @SQLCommand nvarchar(max)\r\nSET @SQLCommand = '");
      }
      else
      {
        sb.AppendFormat("IF NOT EXISTS(SELECT * FROM sys.all_objects WHERE name = '{0}-{2}' and schema_name(schema_id) = '{1}')\r\n", objectName, schemaName, patchDate);
        sb.AppendFormat("BEGIN \r\ndeclare @SQLCommand nvarchar(max)\r\nSET @SQLCommand = '");
      }
      SqlCommand cmd = new SqlCommand(string.Format("sp_helptext '{0}.{1}'", schemaName, objectName), new SqlConnection(dbConnection));
      cmd.Connection.Open();
      SqlDataReader dr = cmd.ExecuteReader();
      while (dr.Read())
        sb.Append(dr[0].ToString().Replace("'", "''"));
      cmd.Connection.Close();
      sb.AppendLine("'");
      if (updatePatch)
      {
        sb.AppendFormat("IF EXISTS(SELECT * FROM sys.all_objects WHERE name = '{0}' and schema_name(schema_id) = '{1}')\r\n", objectName, schemaName);
        sb.AppendFormat("exec sp_rename '{0}.{1}', '{1}-{2}', 'object';\r\n", schemaName, objectName, patchDate);
      }
      sb.AppendLine("EXEC (@SQLCommand)");
      sb.AppendLine("END");
      sb.AppendLine("GO");
      patchScript = sb.ToString();
    }


  }
}
