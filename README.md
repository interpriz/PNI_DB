# PNI_DB
Application that interacts with a database for storing, processing and  analyzing the results of experimental studies of physical processes of power equipment

This application interacts with the database directly and calls procedures and functions in it to update, make changes, and also select data.

The main highlight of the project is that a database object can have an infinite number of parameters, which is implemented using
a vertical data storage structure in the database.
Also, the interface is completely dynamic (it has almost no fixed number of columns in the displayed tables).




Files xaml - files containing a description of the visual interface of the application window in the form of XAML code.

Files xaml.cs - files containing the logic of the C# application and associated with the xaml window.

Folder Scripts contains scripts for creating a database

Folders experiments, modeling, verification contains windows for 3 menu items.

Folder data contains entity classes from the database and the logic of their filling.

Folder taskParametrs contains windows for filling in the task parameters.
