using System;
using System.Text;
using System.Security.Cryptography;
using ConsoleTables;
using System.Linq;

public class Rules
{
    private string[,] rulesTable;

    private int dimension;

    public int Dimension => dimension;

    public string[,] RulesTable => rulesTable;

    public string[] GetRow(int column)
    {
        string[] row = new string[dimension];
        for (int i = 0; i < dimension; i++)
            row[i] = rulesTable[column, i];
        return row;

    }

    public Rules(string[] moveNames)
    {
        dimension = moveNames.Length + 1;
        rulesTable = new string[dimension, dimension];

        for (int i = 1; i < dimension; i++)
        {
            rulesTable[0, i] = moveNames[i - 1];
            rulesTable[i, 0] = moveNames[i - 1];
        }
        CreateRules();
    }


    private void CreateRules()
    {
        rulesTable[0, 0] = "";

        for (int i = 1; i < dimension; i++)
            for (int j = 1; j < dimension; j++)
            {
                int posDiff = j - i;
                int bound = (dimension - 1) / 2;
                rulesTable[j, i] = "Win";
                if ((Math.Abs(posDiff) <= bound && posDiff > 0) 
                    || (Math.Abs(posDiff) > bound && posDiff < 0)) rulesTable[j, i] = "Lose";
                if (posDiff == 0) rulesTable[j, i] = "Draw";
            }
    }
}

public class Menu
{
    public static void ShowHelp(Rules Rules)
    {
        ConsoleTable table = new(Rules.GetRow(0));
        for (int i = 1; i < Rules.Dimension; i++)
            table.AddRow(Rules.GetRow(i));
        table.Write();
    }
}

public class Cryptographer
{
    RandomNumberGenerator random;

    byte[] key;

    public string Key => BitConverter.ToString(key).Replace("-", string.Empty);

    public Cryptographer()
    {
        random = RandomNumberGenerator.Create();
        key = RandomizeBytes(32);
    }

    public string ComputeHMacSha256(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        HMACSHA256 hmac = new HMACSHA256(key);
        hmac.ComputeHash(bytes);
        return BitConverter.ToString(hmac.Hash).Replace("-", string.Empty);
    }

    public byte[] RandomizeBytes(int size)
    {
        byte[] bytes = new byte[size];
        random.GetNonZeroBytes(bytes);
        return bytes;
    }

    public int RandomizeInt(int min, int max)
    {
        byte[] bytes = RandomizeBytes(sizeof(int));
        int val = BitConverter.ToInt32(bytes);
        return ((val - min) % (max - min + 1) + (max - min + 1)) % (max - min + 1) + min;
    }
}

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("At least three names required for game");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

        if (args.Length == 0)
        {
            Console.WriteLine("Odd number of moves is required for game");
            Console.ReadKey();
            System.Environment.Exit(2);
        }

        if (args.Distinct().Count() != args.Count())
        {
            Console.WriteLine("Unique names of moves are required for game");
            Console.ReadKey();
            System.Environment.Exit(3);
        }

        Rules Rules = new(args);
        Cryptographer Crypto = new();
        int compChoiceInt = Crypto.RandomizeInt(1, args.Length);

        Console.WriteLine("{0}: {1}", "HMAC", Crypto.ComputeHMacSha256(args[compChoiceInt - 1]));

        Console.WriteLine("Available moves:");
        for (int i = 0; i < args.Length; i++)
            Console.WriteLine("{0} - {1}", i + 1, args[i]);
        Console.WriteLine("0 - exit");
        Console.WriteLine("? - help");

        Console.Write("Enter your move:");
        string userChoiceString = Console.ReadLine();
        int userChoiceInt;

        switch(userChoiceString)
        {
            case "0":
                System.Environment.Exit(0);
                break;

            case "?":
                Menu.ShowHelp(Rules);
                break;

            default:
                if (!int.TryParse(userChoiceString, out userChoiceInt) 
                    || userChoiceInt < 0 || userChoiceInt > args.Length)
                {
                    Console.WriteLine("Incorrect input");
                    break;
                }    
                Console.WriteLine("{0}: {1}", "Your choice", args[userChoiceInt - 1]);
                Console.WriteLine("{0}: {1}", "Computer choice", args[compChoiceInt - 1]);
                Console.WriteLine("You " + Rules.RulesTable[compChoiceInt, userChoiceInt] + "!");
                Console.WriteLine("{0}: {1}", "Key", Crypto.Key);

                break;
        }
        Console.ReadKey();
    }
}
