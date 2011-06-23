using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using ReluctantDBA.SQLPatchGenerator;

namespace GenerateSQLPatch
{
  class Program
  {
    static void Main(string[] args)
    {
      Arguments CommandLine = new Arguments(args);
      bool DisplayHelp = false;
      if (CommandLine["?"] != null)
        DisplayHelp = true;
      string ErrorMessage = "";

      string sqlConnection = (CommandLine["connection"] == null ? CommandLine["c"] : CommandLine["connection"]);
      string schemaName = (CommandLine["schema"] == null ? CommandLine["s"] : CommandLine["schema"]);
      string objectName = (CommandLine["objectname"] == null ? CommandLine["o"] : CommandLine["objectname"]);
      bool updatePatch = (CommandLine["update"] != null);

      if (!DisplayHelp && sqlConnection == null)
      {
        ErrorMessage = ErrorMessage + "Connection is REQUIRED!\r\n";
      }

      if (!DisplayHelp && schemaName == null)
      {
        ErrorMessage = ErrorMessage + "Schema Name is REQUIRED!\r\n";
      }
      if (!DisplayHelp && objectName == null)
      {
        ErrorMessage = ErrorMessage + "Object Name is REQUIRED!\r\n";
      }



      if (DisplayHelp || ErrorMessage != string.Empty)
      {

        /// <summary>
        System.Version AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        Console.Clear();
        if (ErrorMessage != string.Empty)
          Console.WriteLine(ErrorMessage);
        Console.WriteLine("{4}\tVersion: {0}.{1}.{2}.{3}", AppVersion.Major.ToString(), AppVersion.Minor.ToString(),
         AppVersion.Build.ToString(), AppVersion.Revision.ToString(), System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name);
        SQLPatchGenerator spg = new SQLPatchGenerator();
        Console.WriteLine(spg.BuildInfo());
        Console.Write("\r\n\r\n{0}", System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name);

        Console.Write(" -? -c[onnection]:dbConnection -s[chema]:SchemaName -o[bjectname]:ObjectName ");
        Console.WriteLine("\t-update");
        Console.WriteLine("-? shows this message");
        Console.WriteLine("-c[onnection] the connection string used to connect to the server");
        Console.WriteLine("-s[chema] the schema name of the object");
        Console.WriteLine("-o[bjectname] name of the object to be created)");
        Console.WriteLine("-update Flag to indicate whether this is a new object or update of an existing");
      }
      else
      {
        SQLPatchGenerator spg = new SQLPatchGenerator(sqlConnection);
        string patchInfo = string.Empty;
        spg.generatePatchScript(schemaName, objectName, updatePatch, ref patchInfo);
        Console.WriteLine(patchInfo);
      }
    }
  }

  /// <summary>
  /// Arguments class- Parsed Arguments where valid argurments can being with
  /// -, / or -- followed by argument name.
  /// 
  /// When next token is another argument, argument is treated as a flag.
  /// Space, = or : terminate the token of the argument name definition. Anything after that
  /// up to the next argument identifier is defined as the value of that argument.
  /// 
  /// Values beginning with " run until the next " 
  /// 
  /// Valid argument examples include:
  /// -argument1 value1 --argument2 /argument3="This example contains spaces- though it could contain dashes-" /argument4 Hello /argument5:world
  /// These break down into:
  ///   argument1 = value1
  ///   argument2 exists
  ///   argument3 = This is a test
  ///   argument4 = Hello
  ///   argument5 = world
  /// </summary>
  public class Arguments
  {

    private StringDictionary pArguments;

    // Constructor
    public Arguments(string[] Args)
    {

      pArguments = new StringDictionary();
      Regex Splitter = new Regex(@"^-{1,2}|^/|=|:",
          RegexOptions.IgnoreCase | RegexOptions.Compiled);

      Regex matchRemoval = new Regex(@"^['""]?(.*?)['""]?$",
          RegexOptions.IgnoreCase | RegexOptions.Compiled);

      string Arguments = null;
      string[] argumentPieces;

      foreach (string individualArgument in Args)
      {
        // Look for new arguments and possible values
        argumentPieces = Splitter.Split(individualArgument, 3);

        switch (argumentPieces.Length)
        {
          // Either small value or space seperator
          case 1:
            if (Arguments != null)
            {
              if (!pArguments.ContainsKey(Arguments))
              {
                argumentPieces[0] =
                    matchRemoval.Replace(argumentPieces[0], "$1");

                pArguments.Add(Arguments.ToLower(), argumentPieces[0]);
              }
              Arguments = null;
            }
            // else Error: no parameter waiting for a value (skipped)
            break;

          // Only found a parameter token, no value
          case 2:
            // The last parameter is still waiting. 
            // With no value, set it to true.
            if (Arguments != null)
            {
              if (!pArguments.ContainsKey(Arguments))
                pArguments.Add(Arguments, "true");
            }
            Arguments = argumentPieces[1];
            break;

          // Parameter with enclosed value
          case 3:
            // The last parameter is still waiting. 
            // With no value, set it to true.
            if (Arguments != null)
            {
              if (!pArguments.ContainsKey(Arguments))
                pArguments.Add(Arguments, "true");
            }

            Arguments = argumentPieces[1];

            // Remove possible enclosing characters (",')
            if (!pArguments.ContainsKey(Arguments))
            {
              argumentPieces[2] = matchRemoval.Replace(argumentPieces[2], "$1");
              pArguments.Add(Arguments, argumentPieces[2]);
            }

            Arguments = null;
            break;
        }
      }
      // In case a parameter is still waiting
      if (Arguments != null)
      {
        if (!pArguments.ContainsKey(Arguments))
          pArguments.Add(Arguments, "true");
      }
    }

    // Retrieve a parameter value if it exists 
    // (overriding C# indexer property)
    public string this[string Param]
    {
      get
      {
        return (pArguments[Param]);
      }
    }

  }

}
