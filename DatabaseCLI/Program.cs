using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Data;

// Used for connecting to the MySQL database
using MySql.Data.MySqlClient;

// Used for the List
using System.Collections;

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

            int selectedClassID = -1;
            Boolean classSelected = false;

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

                    String commandLabel = arguments.ElementAt(0);
                    arguments.RemoveAt(0);
                    
                    switch (commandLabel)
                    {
                        case "new-class":
                            if (!CreateClass(db, arguments))
                            {
                                Console.WriteLine("Something went wrong when creating the class");
                            }
                            break;
                        case "select-class":
                            SelectClass(db, arguments, arguments.Count, ref selectedClassID);
                            if (selectedClassID == -1)
                            {
                                classSelected = false;
                                Console.WriteLine("Could not select class.");
                            }
                            else
                            {
                                classSelected = true;
                            }
                            break;
                        case "list-classes":
                            if (!ListClasses(db, arguments))
                            {
                                Console.WriteLine("This command does not require additional arguments. Ex: list-classes");
                            }
                            break;
                        case "show-class":
                            if (!ShowClass(db, arguments, selectedClassID))
                            {
                                Console.WriteLine("Could not output selected class");
                            }
                            break;
                        // Commands that require a selected class
                        case "show-categories":
                            if (!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }
                            if (!ShowCategories(db, arguments, selectedClassID))
                            {
                                Console.WriteLine("This command does not require additional arguments. Ex: show-categories");
                            }
                            break;
                        case "add-category":
                            if (!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }  
                            if (!CreateCategory(db, arguments, selectedClassID))
                            {
                                // Output assistance message to user 
                                Console.WriteLine("Something went wrong when creating a category");
                            }                          
                            break;
                        case "show-assignment":
                            if (!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }
                            if (!ShowAssignments(db, arguments, selectedClassID))
                            {
                                Console.WriteLine("This command does not require additional arguments. Ex: show-assignment");
                            }
                            break;                        
                        case "add-":
                            if (!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }
                            if (!CreateAssignment(db, arguments, selectedClassID))
                            {
                                Console.WriteLine("Something went wrong when creating an assignment.");
                            }
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

            Console.WriteLine("Class_ID | Course_Number | Term | Section_Number | Description");
            Console.WriteLine(CLIDatabase.whitespaceBorder);
            while (lines.Read())
            {
                int courseID = lines.GetInt16("class_ID");
                String courseNumber = lines.GetString("course_number");
                String term = lines.GetString("term");
                int sectionNumber = lines.GetInt16("section_number");
                String description = lines.GetString("description");
                
                Console.WriteLine($"{courseID} | {courseNumber} | {term} | {sectionNumber} | {description}");
            }
        }

        return true;
    }

    //
    static int SelectClass(MySqlConnection connection, List<String> arguments, int numberOfArgs, ref int classID)
    {

        String courseNumber = "", term = "";
        int sectionNumber = -1;

        // [courseNumber*] [term] [sectionNumber]
        if (numberOfArgs > 3)
            return -1;

        try 
        {
            courseNumber = arguments.ElementAt(0);
            term = arguments.ElementAt(1);
            sectionNumber = Convert.ToInt16(arguments.ElementAt(2));
        } 
        catch (ArgumentOutOfRangeException e)
        {
            // Continue execution
        }

        String search = "";
        MySqlCommand query;

        switch (numberOfArgs)
        {
            // Search by only course_number
            case 1:
                search = $"SELECT * FROM class WHERE '{courseNumber}' = course_number";
                break;
            // Search by course_number and term
            case 2:
                search = $"SELECT * FROM class WHERE '{courseNumber}' = course_number and '{term}' = term";
                break;
            // Search by course_number, term, and section_number
            case 3:
                search = $"SELECT * FROM class WHERE '{courseNumber}' = course_number and '{term}' = term and {sectionNumber} = section_number";
                break;
            default:
                return -1;
        }

        query = new MySqlCommand(search, connection);

        int numLines = 0;
        using (MySqlDataReader lines = query.ExecuteReader())
        {
            while(lines.Read())
            {
                numLines++;

                // Impossible to select from multiple records
                if (numLines > 1)
                {
                    classID = -1;
                    break;
                }

                classID = lines.GetInt16("class_ID");
            }
        }

        // If nothing is found
        if (numLines != 1)
        {
            classID = -1;
        }

        return classID;
    }

    //
    static Boolean ShowClass(MySqlConnection connection, List<String> arguments, int classID)
    {
        // Only the command itself should be provided Ex: show-class
        if (arguments.Count != 0)
        {
            return false;
        }

        // Class is not selected
        if (classID == -1)
        {
            return false;
        }

        String grabClass = $"SELECT * FROM class WHERE {classID} = class_ID";
        MySqlCommand query = new MySqlCommand(grabClass, connection);

        using (MySqlDataReader lines = query.ExecuteReader())
        {
            Console.WriteLine("Class_ID | Course_Number | Term | Section_Number | Description");
            Console.WriteLine(CLIDatabase.whitespaceBorder);
            while (lines.Read())
            {
                int courseID = lines.GetInt16("class_ID");
                String courseNumber = lines.GetString("course_number");
                String term = lines.GetString("term");
                int sectionNumber = lines.GetInt16("section_number");
                String description = lines.GetString("description");

                Console.WriteLine($"{courseID} | {courseNumber} | {term} | {sectionNumber} | {description}");
            }
        }

        return true;
    }

    //
    static Boolean ShowCategories(MySqlConnection connection, List<String> arguments, int classID)
    {
        if (arguments.Count != 0)
        {
            return false;
        }

        String select = $"SELECT * FROM category WHERE {classID} = class_ID";
        MySqlCommand query = new MySqlCommand(select, connection);

        using (MySqlDataReader lines = query.ExecuteReader())
        {
            Console.WriteLine("Category_ID | Name | Weight");
            Console.WriteLine(CLIDatabase.whitespaceBorder);
            while (lines.Read())
            {
                int categoryID = lines.GetInt16("category_ID");
                String name = lines.GetString("name");
                decimal weight = lines.GetDecimal("weight");
                Console.WriteLine($"{categoryID} | {name} | {weight}");
            }
        }

        return true;
    }

    //
    static Boolean CreateCategory(MySqlConnection connection, List<String> arguments, int classID)
    {
        // command [Name] [Weight]
        if (arguments.Count != 2)
        {
            return false;
        }

        String insertion = "INSERT INTO category (name, weight, class_ID) VALUES (@name, @weight, @class_ID)";
        MySqlCommand query = new MySqlCommand(insertion, connection);

        query.Parameters.AddWithValue("@name", arguments.ElementAt(0));
        query.Parameters.AddWithValue("@weight", Convert.ToDecimal(arguments.ElementAt(1)));
        query.Parameters.AddWithValue("@class_ID", classID);

        int insertedRow = query.ExecuteNonQuery();

        if (insertedRow == 1)
        {
            Console.WriteLine($"Category created for class {classID}!");
            return true;
        }
        else
        {
            return false;
        }
    }

    static Boolean ShowAssignments(MySqlConnection connection, List<String> arguments, int classID)
    {
        if (arguments.Count != 0)
        {
            return false;
        }

        String select = "SELECT *, category.name AS category_name " +
                        "FROM assignment " +
                        "LEFT JOIN category ON category.category_ID = assignment.category_ID " +
                        $"WHERE {classID} = assignment.class_ID";
        MySqlCommand query = new MySqlCommand(select, connection);

        using (MySqlDataReader lines = query.ExecuteReader())
        {
            Console.WriteLine("Assignment_ID | Name | Description | Point_Value | Category_Name");
            Console.WriteLine(CLIDatabase.whitespaceBorder);
            while (lines.Read())
            {
                int assignmentID = lines.GetInt16("assignment_ID");
                String name = lines.GetString("name");
                String description = lines.GetString("description");
                float pointValue = lines.GetFloat("point_value");
                // int categoryID = lines.GetInt16("category.name");

                String categoryName = lines.GetString("category_name");

                // Console.WriteLine($"{assignmentID} | {name} | {description} | {pointValue} | {categoryID}");
                Console.WriteLine($"{assignmentID} | {name} | {description} | {pointValue} | {categoryName}");

            }
        }

        return true;
    }

    //
    static Boolean CreateAssignment(MySqlConnection connection, List<String> arguments, int classID)
    {
        // command [Assignment_Name] [Category] [Description] [Points]
        if (arguments.Count != 4)
        {
            return false;
        }

        String insertion = "INSERT INTO assignment (name, category_ID, description, point_value, class_ID) VALUES (@name, @category_ID, @description, @point_value, @class_ID)";
        MySqlCommand query = new MySqlCommand(insertion, connection);

        query.Parameters.AddWithValue("@name", arguments.ElementAt(0));
        query.Parameters.AddWithValue("@category_ID", Convert.ToInt16(arguments.ElementAt(1)));
        query.Parameters.AddWithValue("@description", arguments.ElementAt(2));
        query.Parameters.AddWithValue("@point_value", Convert.ToDecimal(arguments.ElementAt(3)));
        query.Parameters.AddWithValue("@class_ID", classID);
        
        int insertedRow = query.ExecuteNonQuery();

        if (insertedRow == 1)
        {
            Console.WriteLine("Assignment Created!");
            return true;
        }
        else
        {
            return false;
        }
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
 