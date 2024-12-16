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
                            if (!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }
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

                        case "add-student":
                            if (!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }      
                            if (!CreateStudent(db, arguments, selectedClassID))
                            {
                                Console.WriteLine("Something went wrong when creating a student.");                                
                            }                      
                            break;
                        case "show-students":
                            if (!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }
                            if (!ShowStudents(db, arguments, selectedClassID))
                            {
                                Console.WriteLine("This command only has 1 optional argument. Ex: show-students <name phrase>");
                            }
                            break;
                        case "grade":
                            if(!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }
                            if (!Grade(db, arguments, selectedClassID))
                            {
                                Console.WriteLine("Something went wrong when creating a grade.");                                
                            }
                            break;

                        case "student-grades":
                            if(!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }
                            if(!StudentGrade(db, arguments, selectedClassID))
                            {
                                Console.WriteLine("This command requires an argument. Ex: show-assignment [Username]");
                            }
                            break;
                        case "gradebook":
                            if(!classSelected)
                            {
                                Console.WriteLine("Class not selected.");
                                break;
                            }
                            GradeBook(db, selectedClassID);
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

                db.Close();

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

    // Creates a class
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

    // List all classes with students that are enrolled in them
    static Boolean ListClasses(MySqlConnection connection, List<String> arguments)
    {
        if (arguments.Count != 0)
        {
            return false;
        }

        String select = "SELECT class.*, COUNT(enrollment.class_ID) AS number_of_students " +
                        "FROM class " +
                        "LEFT JOIN enrollment ON class.class_ID = enrollment.class_ID " +
                        "GROUP BY class.class_ID";

        MySqlCommand query = new MySqlCommand(select, connection);

        using (MySqlDataReader lines = query.ExecuteReader())
        {

            Console.WriteLine("Class_ID | Course_Number | Term | Section_Number | Description | Number of Students Enrolled");
            Console.WriteLine(CLIDatabase.whitespaceBorder);
            while (lines.Read())
            {
                int courseID = lines.GetInt16("class_ID");
                String courseNumber = lines.GetString("course_number");
                String term = lines.GetString("term");
                int sectionNumber = lines.GetInt16("section_number");
                String description = lines.GetString("description");
                int numStudents = lines.GetInt16("number_of_students");

                Console.WriteLine($"{courseID} | {courseNumber} | {term} | {sectionNumber} | {description} | {numStudents}");
            }

            lines.Close();
        }

        return true;
    }

    // Selects a class with given arguments for specificity
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

            lines.Close();

        }

        // If nothing is found
        if (numLines != 1)
        {
            classID = -1;
        }

        return classID;
    }

    // Shows the current class
    static Boolean ShowClass(MySqlConnection connection, List<String> arguments, int classID)
    {
        // Only the command itself should be provided Ex: show-class
        if (arguments.Count != 0)
        {
            return false;
        }

        // Class is not selected
        // if (classID == -1)
        // {
        //     return false;
        // }

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

            lines.Close();
        }

        return true;
    }

    // Shows all avaliable categories for the selected classs
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

            lines.Close();
        }

        return true;
    }

    // Creates a category for the selected class
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

    // Shows all assignments in the selected class
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

            lines.Close();
        }

        return true;
    }

    // Creates an assignment for a selected class
    static Boolean CreateAssignment(MySqlConnection connection, List<String> arguments, int classID)
    {

        MySqlCommand query;
        int categoryID = -1;

        // command [Assignment_Name] [Category] [Description] [Points]
        if (arguments.Count != 4)
        {
            return false;
        }

        String categoryName = arguments.ElementAt(1);
        // Checking if category is valid for class
        String search = "SELECT category_ID FROM category WHERE class_ID = @class_ID AND name = @name";
        query = new MySqlCommand(search, connection); 

        query.Parameters.AddWithValue("@class_ID", classID);
        query.Parameters.AddWithValue("@name", categoryName);
        using (MySqlDataReader lines = query.ExecuteReader())
        {
            if (!lines.Read())
            {
                Console.WriteLine("Category does not exist for this class");
                return false;
            }
            else 
            {
                categoryID = lines.GetInt16("category_ID");
            }

            lines.Close();
        }

        String insertion = "INSERT INTO assignment (name, category_ID, description, point_value, class_ID) VALUES (@name, @category_ID, @description, @point_value, @class_ID)";
        query = new MySqlCommand(insertion, connection);

        query.Parameters.AddWithValue("@name", arguments.ElementAt(0));
        // query.Parameters.AddWithValue("@category_ID", Convert.ToInt16(arguments.ElementAt(1)));
        query.Parameters.AddWithValue("@category_ID", categoryID);
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

    // Creates, updates, and searches for a student to be assigned to the selected class
    static Boolean CreateStudent(MySqlConnection connection, List<String> arguments, int classID)
    {

        int studentID = -1, insertedRow = -1;
        String fullname = "";
        MySqlCommand query;

        // command [Username] [Student_ID] [Last] [First]
        if (arguments.Count != 4 && arguments.Count != 1)
        {
            return false;
        }

        // Adding new student
        if (arguments.Count == 4)
        {
            String insertion = "INSERT INTO student (username, student_ID, name) VALUES (@username, @student_ID, @name)";
            query = new MySqlCommand(insertion, connection);

            query.Parameters.AddWithValue("@username", arguments.ElementAt(0));

            studentID = Convert.ToInt16(arguments.ElementAt(1));
            query.Parameters.AddWithValue("@student_ID", studentID);

            // Combining first and last name
            fullname = String.Concat(arguments.ElementAt(3), " ", arguments.ElementAt(2));
            query.Parameters.AddWithValue("@name", fullname);

            try 
            {
                insertedRow = query.ExecuteNonQuery();

                // if (insertedRow == 1)
                // {
                //     Console.WriteLine($"Student ({arguments.ElementAt(0)}) : created for class {classID}");
                // }
            }
            catch (MySqlException e) 
            {
                Console.WriteLine("Student already exists, updating name.");

                String update = $"UPDATE student SET name = @name WHERE student_ID = @student_ID";
                query = new MySqlCommand(update, connection);

                query.Parameters.AddWithValue("@name", fullname);
                query.Parameters.AddWithValue("@student_ID", studentID);

                insertedRow = query.ExecuteNonQuery();
            }

            return AssignStudent(connection, studentID, classID);
        }

        // Looking for student
        if (arguments.Count == 1)
        {
            String select = $"SELECT * FROM student WHERE username = @username";
            query = new MySqlCommand(select, connection);

            query.Parameters.AddWithValue("@username", arguments.ElementAt(0));

            using (MySqlDataReader lines = query.ExecuteReader())
            {
                while (lines.Read())
                {
                    studentID = lines.GetInt16("student_ID");
                }

                lines.Close();
            }

            return AssignStudent(connection, studentID, classID);

        }

        // Should not be possible to get here
        return false;
    }

    // Assigns a student to a class
    static Boolean AssignStudent(MySqlConnection connection, int studentID, int classID)
    {
        // Student is not selected
        if (studentID == -1)
        {
            return false;
        }

        String insertion = "INSERT INTO enrollment (student_ID, class_ID) VALUES (@student_ID, @class_ID)";
        MySqlCommand query = new MySqlCommand(insertion, connection);

        query.Parameters.AddWithValue("@student_ID", studentID);
        query.Parameters.AddWithValue("@class_ID", classID);

        try
        {
            query.ExecuteNonQuery();
            Console.WriteLine($"Enrolled Student ({studentID}) for class {classID}!");
            return true;
        }
        catch (MySqlException e)
        {
            Console.WriteLine("Failed to enroll student.");
            return false;
        }
    }

    // Shows all the students that are in the selected class
    static Boolean ShowStudents(MySqlConnection connection, List<String> arguments, int classID)
    {

        String grabStudents = "";
        MySqlCommand query;

        if (arguments.Count != 0 && arguments.Count != 1)
        {
            return false;
        }

        if (arguments.Count == 0) {
            grabStudents =   "SELECT enrollment.student_ID, student.name, student.username " +
                                "FROM enrollment " +
                                "LEFT JOIN student " +
                                "ON enrollment.student_ID = student.student_ID " + 
                                "WHERE class_ID = @class_ID";
            query = new MySqlCommand(grabStudents, connection);

            query.Parameters.AddWithValue("@class_ID", classID);

        }
        else // Searching with regex
        {
            String name = arguments.ElementAt(0);
            Console.WriteLine(name);

            grabStudents =  "SELECT * " +  
                            "FROM enrollment " +
                            "JOIN student " + 
                            "ON student.student_ID = enrollment.student_ID " + 
                            $"WHERE enrollment.class_ID = @class_ID AND (username LIKE '%{name}%' OR name LIKE '%{name}%')";

            query = new MySqlCommand(grabStudents, connection);

            query.Parameters.AddWithValue("@class_ID", classID);

        }
        
        using (MySqlDataReader lines = query.ExecuteReader())
            {
                Console.WriteLine("Student_ID | Username | Name");
                Console.WriteLine(CLIDatabase.whitespaceBorder);
                while(lines.Read())
                {
                    int studentID = lines.GetInt16("student_ID");
                    String name = lines.GetString("name");
                    String username = lines.GetString("username");

                    Console.WriteLine($"{studentID} | {username} | {name} ");
                }

                lines.Close();
            }

        return true;      
    }

    // Grade students assignments with provided arguments
    static Boolean Grade(MySqlConnection connection, List<String> arguments, int classID)
    {

        MySqlCommand query;
        int studentID = -1, assignmentID = -1, maxGrade = -1, insertedRow = -1;
        Boolean gradeWarning = false;

        // command [Assignment_Name] [Username] [Grade]
        if (arguments.Count != 3)
        {
            Console.WriteLine("Invalid use of command.");
            return false;
        }

        String username = arguments.ElementAt(1);
        String assignmentName = arguments.ElementAt(0);
        int grade = Convert.ToInt16(arguments.ElementAt(2));

        String validateEnrollment = "SELECT enrollment.student_ID " + 
                                    "FROM enrollment " +
                                    "JOIN student " + 
                                    "ON student.student_ID = enrollment.student_ID " + 
                                    "WHERE class_ID = @class_ID AND username = @username";

        query = new MySqlCommand(validateEnrollment, connection);

        query.Parameters.AddWithValue("@class_ID", classID);
        query.Parameters.AddWithValue("@username", username);
        
        using (MySqlDataReader lines = query.ExecuteReader())
        {
            while (lines.Read())
            {
                studentID = lines.GetInt16("student_ID");
            }

            lines.Close();
        }

        if (studentID == -1) 
        {
            Console.WriteLine("Student is not in this class.");
            return false;
        }
        
        String validateAssignment = "SELECT * FROM assignment WHERE name = @name AND class_ID = @class_ID";
        query = new MySqlCommand(validateAssignment, connection);

        query.Parameters.AddWithValue("@name", assignmentName);
        query.Parameters.AddWithValue("@class_ID", classID);

        using (MySqlDataReader lines = query.ExecuteReader())
        {
            while (lines.Read()) 
            {
                assignmentID = lines.GetInt16("assignment_ID");
                maxGrade = lines.GetInt16("point_value");
            }
        }
        
        if (assignmentID == -1)
        {
            Console.WriteLine("Assignment does not exist for ths class.");
            return false;
        }

        String insertion = "INSERT INTO grade (student_ID, assignment_ID, value) VALUES (@student_ID, @assignment_ID, @value)";
        query = new MySqlCommand(insertion, connection);

        query.Parameters.AddWithValue("@student_ID", studentID);
        query.Parameters.AddWithValue("@assignment_ID", assignmentID);
        query.Parameters.AddWithValue("@value", grade);

        try
        {
            insertedRow = query.ExecuteNonQuery();
            gradeWarning = grade > maxGrade;
        }
        catch (MySqlException e)
        {
            Console.WriteLine("Grade already exists, updating it instead.");

            String update = "UPDATE grade SET value = @value WHERE student_ID = @student_ID AND assignment_ID = @assignment_ID";
            query = new MySqlCommand(update, connection);

            query.Parameters.AddWithValue("@value", grade);
            query.Parameters.AddWithValue("@student_ID", studentID);
            query.Parameters.AddWithValue("@assignment_ID", assignmentID);

            insertedRow = query.ExecuteNonQuery();
            gradeWarning = grade > maxGrade;
        }

        if (gradeWarning)
        {
            Console.WriteLine($"Warning: Grade inserted is higher than assignment max ({maxGrade})");
        }

        return insertedRow == 1;
    }

    // Grabs the student grade of the class and provides attempted and total scores
    static Boolean StudentGrade(MySqlConnection connection, List<String> arguments, int classID)
    {
        // command [Username]
        if (arguments.Count != 1)
        {
            return false;
        }

        String username = arguments.ElementAt(0);
        MySqlCommand query;

        String grabTotalGrades = "SELECT category.name AS Category, assignment.class_ID, (SUM( IFNULL(grade.value, 0) ) / SUM( IFNULL(assignment.point_value, 0) )) * 100 as grade_percentage FROM assignment LEFT JOIN category ON assignment.category_ID = category.category_ID LEFT JOIN class ON class.class_ID = assignment.class_ID JOIN enrollment ON class.class_ID = enrollment.class_ID LEFT JOIN student ON student.student_ID = enrollment.student_ID LEFT JOIN grade ON grade.assignment_ID = assignment.assignment_ID AND grade.student_ID = student.student_ID WHERE assignment.class_ID = @class_ID AND student.username = @username GROUP BY category.name ";
        String attemptedGrades = "SELECT category.name AS Category, assignment.class_ID, (SUM( IFNULL(grade.value, 0) ) / SUM( IF (grade.value IS NULL, 0, assignment.point_value) ) ) * 100 as grade_percentage FROM assignment LEFT JOIN category ON assignment.category_ID = category.category_ID LEFT JOIN class ON class.class_ID = assignment.class_ID JOIN enrollment ON class.class_ID = enrollment.class_ID LEFT JOIN student ON student.student_ID = enrollment.student_ID LEFT JOIN grade ON grade.assignment_ID = assignment.assignment_ID AND grade.student_ID = student.student_ID WHERE assignment.class_ID = @class_ID AND student.username = @username GROUP BY category.name ";
        
        query = new MySqlCommand(grabTotalGrades, connection);
        
        query.Parameters.AddWithValue("@class_ID", classID);
        query.Parameters.AddWithValue("@username", username);

        using (MySqlDataReader grades = query.ExecuteReader())
        {
            Console.WriteLine("Category | Current Grade (out of 100) ");
            Console.WriteLine(CLIDatabase.whitespaceBorder);

            while (grades.Read())
            {
                String category = grades.GetString("Category");
                decimal grade = grades.GetDecimal("grade_percentage");

                Console.WriteLine($"{category} | {grade}");
            }

            grades.Close();
        }

        // Console.WriteLine("Scaling Weights");
        (List<int> categoryIDs, List<decimal> weights) = ScaleWeights(connection, classID);
        
        if (!UpdateWeights(connection, weights, categoryIDs))
        {
            Console.WriteLine("Something went wrong updating weights");
            return false;
        }

        String finalGrade = "SELECT SUM(grade_percentage * weight) as total_grade " +
                            "FROM category " +
                            "JOIN (" +
                            grabTotalGrades +
                            ") as temp ON category.class_ID = temp.class_ID AND category.name = temp.Category";

        String attemptedTotalGrade = "SELECT SUM( IF(grade_percentage is NULL, 0, grade_percentage * weight) ) as total_grade " +
                                "FROM category " +
                                "JOIN (" +
                                attemptedGrades +
                                ") as temp ON category.class_ID = temp.class_ID AND category.name = temp.Category";

        // String attemptedTotalGrade = "SELECT SUM(grade_percentage_for_category * weight) as attempted_grade FROM category JOIN ( SELECT category.name as Category, assignment.class_ID, (SUM(IFNULL(grade.value, 0)) / SUM( IF (grade.value IS NULL, 0, assignment.point_value) ) ) * 100 as grade_percentage_for_category FROM assignment LEFT JOIN category ON assignment.category_ID = category.category_ID LEFT JOIN class ON class.class_ID = assignment.class_ID JOIN enrollment ON class.class_ID = enrollment.class_ID LEFT JOIN student ON student.student_ID = enrollment.student_ID LEFT JOIN grade ON grade.assignment_ID = assignment.assignment_ID AND grade.student_ID = student.student_ID WHERE assignment.class_ID = @class_ID AND student.username = @username GROUP BY category.name ) as temp ON category.class_ID = temp.class_ID AND category.name = temp.Category ";

        
        query = new MySqlCommand(finalGrade, connection);
        query.Parameters.AddWithValue("@class_ID", classID);
        query.Parameters.AddWithValue("@username", username);

        Object lines;
        decimal totalGrade, attemptedGrade;

        lines = query.ExecuteScalar();
        totalGrade = Convert.ToDecimal(lines);

        Console.WriteLine($"Total Grade: {totalGrade}");
        Console.WriteLine("----------------------------------------");

        query = new MySqlCommand(attemptedTotalGrade, connection);
        query.Parameters.AddWithValue("@class_ID", classID);
        query.Parameters.AddWithValue("@username", username);

        lines = query.ExecuteScalar();
        attemptedGrade = Convert.ToDecimal(lines);

        Console.WriteLine($"Attempted Grade: {attemptedGrade}");

        return true;
    }

    // Incomplete, unfortunately :'(
    static void GradeBook(MySqlConnection connection, int classID)
    {

    }

    // Scales the weights to be equal of a 100
    static (List<int>, List<decimal>) ScaleWeights(MySqlConnection connection, int classID)
    {
        List<int> categoryIDs = new List<int>();
        List<decimal> weights = new List<decimal>();

        String select = "SELECT * FROM category WHERE class_ID = @class_ID";
        MySqlCommand query = new MySqlCommand(select, connection);

        query.Parameters.AddWithValue("@class_ID", classID);

        using (MySqlDataReader lines = query.ExecuteReader()) 
        {
            while (lines.Read())
            {
                categoryIDs.Add(lines.GetInt16("category_ID"));
                weights.Add(lines.GetDecimal("weight"));
            }

            lines.Close();
        }

        decimal[] aggregate = weights.ToArray();
        decimal total = aggregate.Sum();

        // Console.WriteLine($"Total is {total}");

        List<decimal> newWeights = new List<decimal>();
        for (int i = 0; i < weights.Count; i++)
        {
            newWeights.Add(aggregate[i] / total);
        }

        // Modified weights
        return (categoryIDs, newWeights);
    }

    // Updates the weights to correctly reflect class
    static Boolean UpdateWeights(MySqlConnection connection, List<decimal> weights, List<int> category_IDs)
    {
        String update = "UPDATE category SET weight = @weight WHERE category_ID = @category_ID";
        MySqlCommand query;

        int insertedRow;

        try
        {
            for (int i = 0; i < weights.Count; i++)
            {
                // Console.WriteLine($"Updating {category_IDs.ElementAt(i)} : to {weights.ElementAt(i)} ");
                query = new MySqlCommand(update, connection);
                query.Parameters.AddWithValue("@weight", weights[i]);
                query.Parameters.AddWithValue("@category_ID", category_IDs[i]);

                insertedRow = query.ExecuteNonQuery();

                if (insertedRow != 1)
                {
                    return false;
                }
            }
        }
        catch (MySqlException e)
        {
            return false;
        }

        return true;
    }

    // Incomplete, should output help commands
    static void PrintHelp()
    {

    }

    // Grabs os environment vars for string building
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

    // Builds connection string to MYSQL db
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

    // Grabs arguments and cleans it up
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
 