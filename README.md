# PNI_DB
Application that interacts with a database for storing, processing and  analyzing the results of experimental studies of physical processes of power equipment


Folder Scripts contains scripts for creating a database

Files xaml - files containing a description of the visual interface of the application window in the form of XAML code
Files xaml.cs - files containing the logic of the C# application and associated with the xaml window.

This application interacts with the database directly and calls procedures and functions in it to update, make changes, and also select data.

The main highlight of the project is that a database object can have an infinite number of parameters, which is implemented using
a vertical data storage structure in the database.
Also, the interface is completely dynamic (it has almost no fixed number of columns in the displayed tables).
