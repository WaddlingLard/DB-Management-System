using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Data;

// Used for connecting to the MySQL database
using MySql.Data.MySqlClient;

// Used for the List
using System.Collections;
using MySqlX.XDevAPI;

class CLIDatabase 
{

    private const String database = "CS410_DATABASE";
    private const String host = "CS410_HOST";
    private const String username = "CS410_USERNAME";
    private const String port = "CS410_PORT";
    private const String password = "CS410_PASSWORD";
    private const String whitespaceBorder = "--------------------------------------------------";
    private readonly String[] names = ["database", "host", "username", "port", "password"];
    

    static void Main() 
    {
        CLIDatabase database = new CLIDatabase();

        List<String> envVars = new List<String>();
        StringBuilder connectionBuilder = new StringBuilder();
        String connectionString = "";
        Boolean terminate = false;

        try 
        {
            envVars = GrabEnvironment(CLIDatabase.database, CLIDatabase.host, CLIDatabase.username, CLIDatabase.port, CLIDatabase.password);
            connectionString = BuildConnection(connectionBuilder, envVars.ToArray(), database.names);

            // Connection only runs in this block
            using (MySqlConnection db = new MySqlConnection(connectionString))
            {
                db.Open();
                // Console.WriteLine("Connected to Database!");
                Console.WriteLine("Welcome to the School Management System!");

                // CLI Interface
                while (!terminate)
                {
                    Console.Write("Command: ");
                    String command = Console.ReadLine();
                    // String[] arguments = ParseArguments(command);
                    List<String> arguments = ParseArguments(command);

                    // Command must be populated
                    if (String.IsNullOrEmpty(command))
                    {
                        continue;
                    }
                    
                    switch (arguments.ElementAt(0))
                    {
                        case "new-class":
                            arguments.RemoveAt(0);
                            if (!CreateClass(db, arguments))
                            {
                                Console.WriteLine("Something went wrong when creating the class");
                            }
                            break;
                        case "select-class":
                            // arguments.RemoveAt(0);
                            // SelectClass(db, arguments, arguments.Count);
                            break;
                        case "list-classes":
                            arguments.RemoveAt(0);
                            if (!ListClasses(db, arguments))
                            {
                                Console.WriteLine("This command does not require additional arguments. Ex: list-classes");
                            }
                            break;
                        case "show-class":
                            // ShowClass(db, arguments);
                            break;
                        case "help":
                            // PrintHelp();
                            // List all commands
                            break;
                        case "exit":
                            Console.WriteLine("Bye!");
                            terminate = true;
                            break;
                        default:
                            Console.WriteLine("Unrecognized Command.");
                            break;
                    }

                }
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
        
    }

    // NEED TO ADD # of students funtionality
    static Boolean CreateClass(MySqlConnection connection, List<String> arguments)
    {   
        // command [Class Name] [Term] [Section] [Description]
        if (arguments.Count != 4)
        {
            return false;
        }

        String insertion = "INSERT INTO class (course_number, term, section_number, description) VALUES (@course_number, @term, @section_number, @description)";
        MySqlCommand query = new MySqlCommand(insertion, connection);

        query.Parameters.AddWithValue("@course_number", arguments.ElementAt(0));
        query.Parameters.AddWithValue("@term", arguments.ElementAt(1));
        query.Parameters.AddWithValue("@section_number", Convert.ToInt16(arguments.ElementAt(2)));
        query.Parameters.AddWithValue("@description", arguments.ElementAt(3));

        int insertedRow = query.ExecuteNonQuery();

        if (insertedRow == 1)
        {
            Console.WriteLine("Class Created!");
            return true;
        }
        else 
        {
            return false;
        }
    }

    //
    static Boolean ListClasses(MySqlConnection connection, List<String> arguments)
    {
        if (arguments.Count != 0)
        {
            return false;
        }

        String select = "SELECT * FROM class";
        MySqlCommand query = new MySqlCommand(select, connection);

        using (MySqlDataReader lines = query.ExecuteReader())
        {

            Console.WriteLine("Course_Number | Term | Section_Number | Description");
            Console.WriteLine(CLIDatabase.whitespaceBorder);
            while (lines.Read())
            {
                String courseNumber = lines.GetString("course_number");
                String term = lines.GetString("term");
                int sectionNumber = lines.GetInt16("section_number");
                String description = lines.GetString("description");
                
                Console.WriteLine($"{courseNumber} | {term} | {sectionNumber} | {description}");
            }
        }

        return true;
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

                // Console.WriteLine(value);
            
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
            // Console.WriteLine($"{data.Count} : {envVars.Length}");
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

    //
    static List<String> ParseArguments(String arguments)
    {
        List<String> strings = new List<String>();

        // Using Regex to split up arguments (Quotes and spaces)
        String pattern = @"(\""[^\""]*\"")|(\S+)";
        MatchCollection segment = Regex.Matches(arguments, pattern);

        // Add to list for each argument
        foreach(Match phrase in segment)
        {
            if (!String.IsNullOrWhiteSpace(phrase.Value))
            {
                strings.Add(phrase.Value.Trim('"'));
            }
        }

        // Outputting Arguments
        // Console.Write("Arguments:");
        // foreach (String word in strings)
        // {
        //     Console.Write(word + ", ");
        // }
        // Console.WriteLine();

        return strings;
    }

}
 