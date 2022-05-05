using AllanMilne.Ardkit;
using System;
using System.Collections.Generic;
using System.IO;

namespace CMP409_Compilers {

    class Program {
        static void Main() {
            List<ICompilerError> errors = new List<ICompilerError>();
            
            Console.WriteLine("Enter the directory that contains your PAL test programs:");
            String filepath = Console.ReadLine();
            Console.WriteLine("\n");
            DirectoryInfo directory = new DirectoryInfo(@filepath);
            FileInfo[] Files = directory.GetFiles("*.txt");

            foreach (FileInfo file in Files) {
                StreamReader source = new StreamReader(filepath + "/" + file.Name);
                Console.WriteLine(file.Name);
                PALParser parser = new PALParser();
                parser.Parse(source);

                foreach (CompilerError err in parser.Errors)
                    Console.WriteLine(err);
                Console.WriteLine("\n");
            }
            Console.ReadKey();
        }
    }
}
