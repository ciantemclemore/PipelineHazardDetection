using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Online_Final_Computer_Architecture
{
    class Program
    {
        private const string key = "run";

        static void Main(string[] args)
        {
            var configuration = GetConfigurationData(@"Database\Configuration.json");
            var mipsCompiler = new MipsCompiler(configuration);

            bool runProg = true;

            while (runProg)
            {
                int userSelection = DisplayMenu();

                switch (userSelection)
                {
                    case 1:
                        //Gather the user's commands
                        GetUserInputInstructions(configuration, mipsCompiler);
                        var result = mipsCompiler.ExecuteCommands();
                        PrintResults(result);
                        //PrintState(configuration);
                        //PrintResults(results);
                        break;
                    case 2:
                        runProg = false;
                        break;
                    default:
                        Console.WriteLine("Invalid selection. Try Again:");
                        break;
                }
            }
        }

        private static int DisplayMenu()
        {
            Console.WriteLine("1. Test Pipeline");
            Console.WriteLine("2. Exit");

            int selectionValue;
            var isValid = Int32.TryParse(Console.ReadLine(), out selectionValue);

            if (isValid)
            {
                return selectionValue;
            }

            //Just return an invalid selction to display menu again
            Console.WriteLine();
            return -1;
        }


        static void GetUserInputInstructions(Configuration configuration, MipsCompiler mipsCompiler) 
        {
            Console.WriteLine("Enter instructions: (Press enter after entering each instruction)");
            Console.WriteLine("Enter 'run' command once finished to compile");
            int instructionCount = 0;

            while (true) 
            {
                if (instructionCount == 4) 
                {
                    break;
                }

                string input = Console.ReadLine();
                //var commandValidation = mipsCompiler.IsValidCommand(input); 


                if (input.Equals(key, StringComparison.OrdinalIgnoreCase)) 
                {
                    break;
                }

                mipsCompiler.QueueCommand(input);
                instructionCount++;
                //if (commandValidation.IsValid)
                //{
                //    mipsCompiler.QueueCommand(input);
                //    instructionCount++;
                //}
                //else 
                //{
                //    Console.WriteLine(commandValidation.Message);
                //}
            }
            Console.WriteLine();
        }

        private static void PrintResults(Solution result) 
        {
            //Header
            Console.WriteLine("{0, -21} {1,-16} {2,-18}", "Instruction", "Hazard", "Registers");

            foreach (var hazard in result.PotentialHazards) 
            {
                if(!hazard.Name.Equals("None"))
                    Console.WriteLine("{0,-21} {1,-16} {2, -18}", hazard.Instruction, hazard.Name, hazard.Message);
            }
            Console.WriteLine();

            Console.WriteLine("Without Forwarding Unit");
            foreach (var pipe in result.Pipelines[0]) 
            {
                foreach (var sequence in pipe)
                {
                    Console.Write($"{sequence} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("With Forwarding Unit");
            foreach (var pipe in result.Pipelines[1])
            {
                foreach (var sequence in pipe)
                {
                    Console.Write($"{sequence} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static Configuration GetConfigurationData(string path)
        {
            var jsonData = string.Empty;
            var configuration = new Configuration();
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    jsonData = sr.ReadToEnd();
                }
                configuration = JsonSerializer.Deserialize<Configuration>(jsonData);

            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }
            return configuration;
        }
    }
}
