﻿using HabitTrackerLibrary;
using System.Globalization;

string divider = "----------------------------------\n";
bool closeApp = false;
while (!closeApp)
{
    MainMenu();
}

void MainMenu()
{
    List<string> habits = SqlCommands.GetTables();

    Console.WriteLine("What would you like to do?");
    Console.WriteLine("\t0: Exit");
    Console.WriteLine("\t1: Open Habit");
    Console.WriteLine("\t2: New Habit");

    string choice = Console.ReadLine()!;
    switch (choice)
    {
        case "0":
            Console.WriteLine($"{divider}Goodbye!\n{divider}");
            closeApp = true;
            Environment.Exit(0);
            break;
        case "1":
            OpenHabit(habits);
            break;
        case "2":
            CreateHabit();
            break;
        default:
            Console.WriteLine("\nInvalid Command. Please try again.\nHit Enter");
            Console.ReadLine();
            break;
    }
}

void DisplayHabits(List<string> habits)
{
    for (int i = 0; i < habits.Count - 1; i++)
    {
        Console.WriteLine($"{i}: {habits[i]}");
    }
}

void EditEntry(string habit)
{
    Console.Clear();
    DisplayEntries(GetEntries(habit));

    int entryId = GetNumberInput("Select the entry you wish to delete or \"X\" to return to Main Menu: ");

    try
    {
        if (SqlCommands.RecordExists(entryId, habit))
        {
            string date = GetDateInput();
            int quantity = GetNumberInput("\nEnter ounces (integer) or \"X\" to return to Main Menu: ");
            Habit editedHabit = new()
            {
                HabitName = habit,
                Id = entryId,
                Date = DateTime.ParseExact(date, "MM-dd-yy", new CultureInfo("en-US")),
                Quantity = quantity
            };

            SqlCommands.UpdateRecord(editedHabit);

            Console.WriteLine($"\nEntry {entryId} updated successfully.\nHit Enter...\n");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine($"\nEntry {entryId} does not exist.\nHit Enter...\n");
            Console.ReadLine();
            EditEntry(habit);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nAn unexpected error has occured\n{ex}\nHit Enter...\n");
        Console.ReadLine();
    }
}

void DeleteEntry(string habit)
{
    Console.Clear();
    DisplayEntries(GetEntries(habit));
    int entryId = GetNumberInput("Select the entry you wish to delete or \"X\" to return to Main Menu: ");

    try
    {
        int rowsDeleted = SqlCommands.DeleteRecord(entryId, habit);
        if (rowsDeleted == 0)
        {
            Console.WriteLine("\nEntry does not exist\nHit Enter...\n");
            Console.ReadLine();
            DeleteEntry(habit);
        }
        else
        {
            Console.WriteLine($"\nEntry {entryId} deleted successfully.\nHit Enter...\n ");
            Console.ReadLine();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"There was an error attempting to delete the entry.\n{ex}\nHit Enter...\n");
        Console.ReadLine();
    }
}

void DisplayEntries(List<Habit> entries)
{
    Console.Clear();

    if (entries.Count == 0)
    {
        Console.WriteLine("\nNo entries found.\nHit Enter...\n");
        Console.ReadLine();
    }
    else
    {
        Console.WriteLine(divider);
        foreach (var entry in entries)
        {
            Console.WriteLine($"{entry.Id}: {entry.Date:MMM dd, yyyy} - Qty: {entry.Quantity}");
        }
        Console.WriteLine();
        Console.WriteLine(divider);
    }
}

void AddNewEntry(string habitName)
{
    Console.Clear();

    string date = GetDateInput();
    int quantity = GetNumberInput("\nEnter amount (integer) or \"X\" to return to Main Menu: ");

    try
    {
        SqlCommands.InsertRecord(date, quantity, habitName);
        Console.WriteLine("\nEntry added\nHit Enter...\n");
        Console.ReadLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nEntry failed to insert\n{ex}\nHit Enter...\n");
        Console.ReadLine();
    }
}

int GetNumberInput(string prompt)
{
    Console.Write(prompt);
    string numberInput = Console.ReadLine()!;
    int output;

    if (numberInput.ToLower() == "x")
    {
        Console.Clear();
        MainMenu();
    }

    while (!int.TryParse(numberInput, out output) || output < 0)
    {
        Console.Write("Invalid number. Try again: ");
        numberInput = Console.ReadLine()!;
    }
    return output;
}

string GetDateInput()
{
    Console.Write("Enter the date (mm-dd-yy) or \"X\" to return to Main Menu: ");
    string dateInput = Console.ReadLine()!;

    if (dateInput.ToLower() == "x")
    {
        Console.Clear();
        MainMenu();
    }

    while (!DateTime.TryParseExact(dateInput, "MM-dd-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
    {
        Console.Write("Invalid format. Format: mm-dd-yy: ");
        dateInput = Console.ReadLine()!;
    }
    return dateInput;
}

void OpenHabit(List<string> habits)
{
    Console.WriteLine(divider);
    DisplayHabits(habits);
    Console.WriteLine();
    Console.WriteLine(divider);

    int habitChoice = GetNumberInput("Select a habit (or enter x to return to main menu): "); ;
    while (habitChoice < 0 || habitChoice > habits.Count - 2)
    {
        Console.WriteLine("Invalid selection, try again.");
        habitChoice = GetNumberInput("Select a habit (or enter x to return to main menu): ");
    }

    string habit = habits[habitChoice];
    bool returnToMain = false;
    while (!returnToMain)
    {
        Console.WriteLine();
        Console.WriteLine("What would you like to do?");
        Console.WriteLine("\t0: Return");
        Console.WriteLine("\t1: View entries");
        Console.WriteLine("\t2: Add new entry");
        Console.WriteLine("\t3: Delete entry");
        Console.WriteLine("\t4: Edit entry");
        Console.WriteLine(divider);

        string choice = Console.ReadLine()!;
        switch (choice)
        {
            case "0":
                returnToMain = true;
                break;
            case "1":
                DisplayEntries(GetEntries(habit));
                break;
            case "2":
                AddNewEntry(habit);
                break;
            case "3":
                DeleteEntry(habit);
                break;
            case "4":
                EditEntry(habit);
                break;
            default:
                Console.WriteLine("\nInvalid Command. Please try again.\nHit Enter");
                Console.ReadLine();
                break;
        }
    }
}

void CreateHabit()
{
    Console.Clear();
    Console.Write("Enter a habit name: ");
    string habitName = Console.ReadLine()!;

    SqlCommands.InitializeDB(DataConnection.ConnString, habitName);
}

List<Habit> GetEntries(string habitName)
{
    List<Habit> entries = new();
    try
    {
        entries = SqlCommands.GetAllRecords(habitName);
    }
    catch (Exception)
    {
        Console.WriteLine("\nError retrieving records\nHit Enter...\n");
        Console.ReadLine();
    }
    return entries;
}