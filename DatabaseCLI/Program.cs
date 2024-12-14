// Used for connecting to the MySQL database
using MySql.Data.MySqlClient;
using System;

// Used for the List
using System.Collections;

class CLIDatabase 
{

    private const String database = "CS310_DATABASE";
    private const String username = "CS310_USERNAME";
    private const String port = "CS310_PORT";
    private const String password = "CS310_PASSWORD";
    // private const int numOfEnvironmentVars = 4;

    static void Main() 
    {
        // Console.WriteLine($"Yo wassup gang.");
        List<String> envVars = new List<string>();

        try {
            envVars = grabEnvironment(CLIDatabase.database, CLIDatabase.username, CLIDatabase.port, CLIDatabase.password);
        } catch (Exception e) {
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

        // Console.WriteLine($"{database}");
    }

    //
    static List<String> grabEnvironment(String database, String username, String port, String password) 
    {
        String[] envVars = [database, username, port, password];
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

}
 