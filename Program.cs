using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class Program
{
    //target hash
    static string targetHash = "b706835de79a2b4e80506f582af3676a"; //Hash for "999"
    //static string targetHash = "a9b7ba70783b617e9998dc4dd82eb3c5"; //Hash for "1000"

    //list of possible strings
    static List<string> possibleStrings = GeneratePossibleStrings();
    //list of worker threads
    static List<Thread> workerThreads = new List<Thread>();
    static bool passwordFound = false;
    static string foundPassword = "";
    static object _lock = new object();

    static void Main(string[] args)
    {
        Console.WriteLine("Starting worker threads...");

        //start worker threads
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            Thread workerThread = new Thread(HandleWorkerNode);
            workerThreads.Add(workerThread);
            workerThread.Start();
        }

        Console.WriteLine("Waiting for all threads to complete...");

        //wait for all threads to complete
        foreach (Thread workerThread in workerThreads)
        {
            workerThread.Join();
        }

        if (passwordFound)
        {
            Console.WriteLine("Password found: " + foundPassword);
        }
        else
        {
            Console.WriteLine("Password not found");
        }
        Console.ReadKey();
    }

    static List<string> GeneratePossibleStrings()
    {
        //generate all possible strings
        //this should be replaced with a more efficient method 
        //for generating possible strings
        List<string> possibleStrings = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                for (int k = 0; k < 10; k++)
                {
                    possibleStrings.Add(i.ToString() + j.ToString() + k.ToString());
                }
            }
        }
        return possibleStrings;
    }

    static void HandleWorkerNode()
    {
        //process the task
        using (MD5 md5Hash = MD5.Create())
        {
            while (!passwordFound)
            {
                string possibleString;
                lock (_lock)
                {
                    if (possibleStrings.Count == 0) break;
                    possibleString = possibleStrings[0];
                    possibleStrings.RemoveAt(0);
                }
                Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " is working on: " + possibleString);
                if (VerifyMd5Hash(md5Hash, possibleString, targetHash))
                {
                    lock (_lock)
                    {
                        if (!passwordFound)
                        {
                            passwordFound = true;
                            foundPassword = possibleString;
                            Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " found the password: " + foundPassword);
                        }
                    }
                    break;
                }
            }
        }

    }
    static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
    {
        //hash the input
        string hashOfInput = GetMd5Hash(md5Hash, input);

        //compare the hash of the input to the target hash
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        return 0 == comparer.Compare(hashOfInput, hash);
    }

    static string GetMd5Hash(MD5 md5Hash, string input)
    {
        //convert the input string to a byte array and compute the hash
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        //create a new StringBuilder to collect the bytes
        //and create a string
        StringBuilder sBuilder = new StringBuilder();

        //loop through each byte of the hashed data
        //and format each one as a hexadecimal string
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
}
