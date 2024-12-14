using System.Text;
using System;

// Used for connecting to the MySQL database
using MySql.Data.MySqlClient;

// Used for the List
using System.Collections;

class CLIDatabase 
{

    private const String database = "CS310_DATABASE";
    private const String host = "CS310_HOST";
    private const String username = "CS310_USERNAME";
    private const String port = "CS310_PORT";
    private const String password = "CS310_PASSWORD";
    private readonly String[] names = ["database", "host", "username", "port", "password"];
    // private const int numOfEnvironmentVars = 5;

    static void Main() 
    {
        CLIDatabase database = new CLIDatabase();

        // Console.WriteLine($"Yo wassup gang.");
        List<String> envVars = new List<String>();
        StringBuilder connectionBuilder = new StringBuilder();
        String connectionString = "";

        try 
        {
            envVars = GrabEnvironment(CLIDatabase.database, CLIDatabase.host, CLIDatabase.username, CLIDatabase.port, CLIDatabase.password);
            connectionString = BuildConnection(connectionBuilder, envVars.ToArray(), database.names);

            // Connection only runs in this block
            using (MySqlConnection db = new MySqlConnection(connectionString))
            {
                db.Open();
            }
            
        } 
        catch (MySqlException e)
        {
            Console.WriteLine($"MySql Error: {e}");
        }
        catch (Exception e) 
        {
            Console.WriteLine($"Error: {e}");
        }

        // int index = 1;
        // foreach (String element in envVars) 
        // {
        //     Console.WriteLine($"Env {index}: {element}");
        //     index++;
        // }
    
        // Grabbing environment variables
        // String database, username, port, password;

        // Console.WriteLine(connectionString);

        // Console.WriteLine($"{database}");

        
    }

    //
    static List<String> GrabEnvironment(String database, String host, String username, String port, String password) 
    {
        String[] envVars = {database, host, username, port, password};
        List<String> data = new List<string>();

        for (int i = 0; i < envVars.Length; i++) 
        {
            if (!String.IsNullOrEmpty(envVars[i])) 
            {
                String value = Environment.GetEnvironmentVariable($"{envVars[i]}");
                Boolean dataIsNull = String.IsNullOrEmpty(value);

                if (dataIsNull) {
                    continue;
                } 
            
                data.Add(value);
            }
            else 
            {
                // Invalid arguments provided
                throw new Exception("Invalid environment variable names");
            }
            
        }

        // Missing data
        if (data.Count != envVars.Length)
        {
            throw new Exception("Missing environment data from provided arguments");
        }

        return data;
    }

    //
    static String BuildConnection(StringBuilder build, String[] data, String[] names) 
    {
        try 
        {
            for (int i = 0; i < names.Length; i++) 
            {
                build.Append($"{names[i]}={data[i]};");
            }
        }
        catch (IndexOutOfRangeException e)
        {
            throw new Exception("Data is missing indexed values");
        }

        String result = build.ToString();
        return result;
    }

}
 