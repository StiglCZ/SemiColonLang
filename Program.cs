using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;

namespace Complier{
    class Program {
        public string compiledCode;
        public string allLines;
        public bool debug = false;
        List<List<int>> localInts;

        public static void Main(string[] args) {
            Program program = new Program();
            if (args.Length > 0){
                program.allLines = file(args[0]);
                program.Compile();
            }
            else{
                program.Interactive();
            }
        }
        public void Compile(){
            formatCode();
            allLines= PrepareString(allLines);
            convertToC(localInts);
        }
        public void formatCode(){
            
            allLines = allLines.Replace("\n","");
            allLines = allLines.Replace("\t", "");
            allLines = allLines.Replace(" ", "");
        }
        public string PrepareString(string source){
            StringBuilder code = new StringBuilder(source);
            int opened = 0;
            bool already1 = false;
            List<List<int>> ints1 = new List<List<int>>();

            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '{')
                {
                    opened++;
                    already1 = true;
                }
                else if (code[i] == '}')
                {
                    opened--;
                }

                if (already1 && opened == 0)
                {
                    code[i + 1] = '|';
                    already1 = false;
                    opened = 0;
                }
            }
            List<string> command = code.ToString().Split('|').ToList();

            if (debug) { Console.Write("Values:"); }

                for (int i = 0; i < command.Count; i++)
                {
                    int x = 0;
                    List<int> ints = new List<int>();
                    for (int j = 0; j < command[i].Length; j++)
                    {
                        if (command[i][j] == ';')
                        {
                            for (int k = j; k < command[i].Length; k++)
                            {
                                x++;
                                if (command[i][k] != ';')
                                {
                                    if (x > 1)
                                    {
                                        ints.Add(x - 1);
                                    }
                                    j += x - 1;
                                    x = 0;
                                }
                            }
                        }
                    }
                    ints1.Add(ints);
                    if (debug){
                        Console.Write("\n" /*+ i.ToString()*/);
                        for (int j1 = 0; j1 < ints.Count; j1++){
                            Console.Write(ints[j1] + " ");
                        }
                    }
                }

            //if (debug){
                ///Console.WriteLine(code.Replace("|", "\n"));
            //}
            localInts = ints1;
            return code.ToString(); 
        }
        
        
        public void convertToC(List<List<int>> ints){
            int[] jpoints = new int[255];
            compiledCode= "#include <stdio.h>\n#include <stdlib.h>\nint array[1000];\n";
            for (int i = 0; i < ints.Count; i++) {
                compiledCode += "int line" + i.ToString() + "();";
            }
            compiledCode += "\nint main(){\nline0();\nreturn 0;\n}";
            for (int i = 0; i < ints.Count; i++) {
                bool jumping = false ;
                compiledCode += "\nint line" + i.ToString() + "(){\n";
                    if (ints[i].Count > 0)
                    {
                        switch (ints[i][0])
                        {
                            case 1:
                                compiledCode += "array[" + ints[i][2] + "] =array[" + ints[i][1] + "]";
                                break;
                            case 2:
                                jumping = true;
                                compiledCode += "line" + ints[i][1] + "();\n";
                                break;
                            case 3:
                                switch (ints[i][2]){
                                case 1:
                                    compiledCode += "if(array[" + ints[i][1] + "] == array[" + ints[i][3]+"]){line " + ints[i][2].ToString()+"();}";
                                    break;
                                case 2:
                                    compiledCode += "if(array[" + ints[i][1] + "] != array[" + ints[i][3] + "]){line" + ints[i][2].ToString() + "();}";
                                    break;
                                case 3:
                                    compiledCode += "if(array[" + ints[i][1] + "] > array[" + ints[i][3] + "]){line" + ints[i][2].ToString() + "();}";
                                    break;
                                case 4:
                                    compiledCode += "if(array[" + ints[i][1] + "] < array[" + ints[i][3] + "]){line" + ints[i][2].ToString() + "();}";
                                    break;
                                case 5:
                                    compiledCode += "if(array[" + ints[i][1] + "] >= array[" + ints[i][3] + "]){line" + ints[i][2].ToString() + "();}";
                                    break;
                                case 6:
                                    compiledCode += "if(" + ints[i][1] + " <= array[" + ints[i][3] + "]){line" + ints[i][2].ToString() + "();}";
                                    break;
                                }

                                break;
                            case 4:
                                switch (ints[i][2]){
                                case 1:
                                    compiledCode += "array[" + ints[i][4] + "] = array[" + ints[i][1]  + "] + array[" + ints[i][3] + "];"; 
                                    break;
                                case 2:
                                    compiledCode += "array[" + ints[i][4] + "] = array[" + ints[i][1] + "] - array[" + ints[i][3] + "];";
                                    break;
                                case 3:
                                    compiledCode += "array[" + ints[i][4] + "] = array[" + ints[i][1] + "] * array[" + ints[i][3] + "];";
                                    break;
                                case 4:
                                    compiledCode += "array[" + ints[i][4] + "] = array[" + ints[i][1] + "] / array[" + ints[i][3] + "];";
                                    break;
                                default:
                                    compiledCode += "array[" + ints[i][4] + "] = array[" + ints[i][1] + "] + array[" + ints[i][3] + "];";
                                    break;
                                }
                            

                                //counting
                                break;
                            case 5:
                                //printing char
                                compiledCode += "printf(\"%c\",(char)array[" + ints[i][1] + "]);";
                                break;
                            case 6:
                                //exit
                                compiledCode += "exit(" + ints[i][1] + ");";
                                break;
                            case 7:
                                //input - read character
                            compiledCode += "char tmp;\nscanf(\"%c\",tmp);\nint tmp2 = (int)tmp; array[ " + ints[i][1] + "] = tmp2;";
                                break;
                            case 8:
                                //copy
                                compiledCode += "array[" + ints[i][2] + "] =" + ints[i][1] + ";";
                                break;
                            case 9:
                                //numeric input
                                if (ints[i][2] == 2){
                                    compiledCode += "int tmp;\nscanf(\"%d\",&tmp);\narray[" + ints[i][1] + "] = tmp;";
                                }
                                else if(ints[i][2] == 1){
                                    compiledCode += "printf(\"%d\",array[" + ints[i][1] + "]); ";
                                }
                                break;
                            case 10:
                                
                                break;

                        default:
                                compiledCode += "\n";
                                break;
                        }
                    }

                if (i < ints.Count - 1 && !jumping){
                    compiledCode += "\nline" + (i + 1).ToString() + "();\nreturn 0;\n}";
                }
                else if(!jumping){
                    compiledCode += "exit(0);\nreturn 0;\n}";
                }
                else{
                    compiledCode += "return 0;\n}";
                }

            }

            File.WriteAllText("compiled.c", compiledCode);
           // Console.WriteLine(compiledCode);
        }
        public static string file(string address){
            return File.ReadAllText(address);
        }
        public void Interactive(){
            int[] reg = new int[1000];
        shellinit:
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Semicolon Language Interactive [" + DateTime.Now.ToString().Split(' ')[1].Split(':')[0] +":" +DateTime.Now.ToString().Split(' ')[1].Split(':')[1] + "]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            while (true){
                int commandNumber = 0;
                Console.Write("$");
                string line = Console.ReadLine();
                if (line == "" || line == null){
                    goto end;
                }
                else if (line == "cls" || line == "clear"){
                    Console.Clear();
                    goto shellinit;
                }
                else if (line.Split(' ')[0] == "compile"){
                    allLines = file(line.Split(' ')[1]);
                    Compile();
                    Console.WriteLine("Done!");
                }
                else
                {
                    line = line.Replace("\n", "");
                    line = line.Replace("\t", "");
                    line = line.Replace(" ", "");
                    int x = 0;
                    List<int> ints = new List<int>();
                    for (int j = 0; j < line.Length; j++)
                    {
                        if (line[j] == ';')
                        {
                            for (int k = j; k < line.Length; k++)
                            {
                                x++;
                                if (line[k] != ';')
                                {
                                    if (x > 1)
                                    {
                                        ints.Add(x - 1);
                                    }
                                    j += x - 1;
                                    x = 0;
                                }
                            }
                        }
                    }
                    if (ints.Count > 0)
                    {
                        switch (ints[0])
                        {
                            case 1:
                                reg[ints[2]] = reg[ints[1]];
                                break;
                            case 2:
                                Console.WriteLine("In current moment, jumping is imposible in interactive mode");
                                break;
                            case 3:
                                Console.WriteLine("In current moment, jumping trough if is imposible in interactive mode");
                                break;
                            case 4:
                                if(ints[2] == 1){ reg[ints[4]] = ints[3] + ints[1]; }
                                else if (ints[2] == 2) { reg[ints[4]] = ints[3] - ints[1]; }
                                else if (ints[2] == 3) { reg[ints[4]] = ints[3] * ints[1]; }
                                else if (ints[2] == 4) { reg[ints[4]] = ints[3] / ints[1]; }
                                break;
                            case 5:
                                Console.Write(Convert.ToChar(reg[ints[1]]));
                                break;
                            case 6:
                                Environment.Exit(ints[1]);
                                break;
                            case 7:
                                char key = Console.ReadKey().KeyChar;
                                reg[ints[1]] = (int)key;
                                break;
                            case 8:
                                reg[ints[2]] = ints[1];
                                break;
                            case 9:
                                break;

                            default:
                                Console.WriteLine("Invalid command");
                                break;
                        }

                    }
                }
            end:
                commandNumber++;
            }
        }
    }
}
