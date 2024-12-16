# Final Project: Class Grade Manager

* Author: Brian Wu & Mahala Solorzano
* Class: CS410 Section 001
* Semester: Fall 2024 

## Overview

This program connects to a hosted MySQL database in order to perform queries, update tables, and display data on a grade management database.
Commands are submitted through the command line and feedback on the success or failure of commands will be printed to the system. Users can add/display students to a specific class with the ability to grade assignments, get the grades of a student in every category for a specific class, or the grades of every student in the class with relevant info, and manage the various categories(Homework, Exams, etc.) and assignments.


## Reflection

Even though this started as a group project, we both wanted to challenge ourselves and write this program in our own ways. It was a challenge for sure, as I (Mahala Solorzano) hadn't connected directly to a database through Java before. Regardless, we both went at it and had some decent fun trying our hand at it. Eventually, though, we came to the consensus that Brian Wu's branch was much more developed than mine and thus more viable to be submitted. We both helped each other in writing our branches though, so there are no hard feelings since we were side by side. It helped that it was similar to a project that was done in a previous course, the foundation that it provided was invaluable.

Unfortunately, we didn't have enough time to 100% complete every aspect of the project. We didn't get to fully implement the gradebook method due to a lack of time. It's not that we don't know how to do it, it just couldn't be done. It sucks that this is what bottlenecks us in the end and not anything technical, but we could have also decided to start things earlier. Regardless, the experience of having to connect this new territory of Computer Science we've been learning about in class to the area we have had plenty of time in has been quite novel. There was a fair amount of challenge involved in making some of the methods, but most of that can be attributed to the query-making. Overall, we both had a good experience with this final project. 


## Compiling and Using

All commands will be executed through the command line alongside the relevant arguments. To see which commands are available, use the help command for
the specific list with their needed arguments. Before doing anything, be sure to run the following command to download all the necessary packages:
dotnet add package MySql.Data

After that, on the command line compile the code with the following while in the correct file:

dotnet build

and run it with:

dotnet run

## Results

As far as we are concerned, the program works the way we want it to. there may be some far-out edge-case bugs that we haven't caught, but
they don't seem to impact the general usage of the program. Unfortunately, not everything is fully incorporated, but about 90% of the total scope of the project has been achieved.

## Sources used

N/A besides textbooks and class-provided slides and documents.

----------
