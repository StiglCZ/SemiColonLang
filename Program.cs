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
            
            program.allLines = file(args[0]);
            program.Compile();
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

            if (debug)
            {
                Console.WriteLine(code.Replace("|", "\n"));
            }
            localInts = ints1;
            return code.ToString(); 
        }
        
        public void convertToC(List<List<int>> ints){
            compiledCode= "#include <stdio.h>\n#include <stdlib.h>\nint array[1000];\n";
            for (int i = 0; i < ints.Count; i++) {
                compiledCode += "int line" + i.ToString() + "();";
            }
            compiledCode += "\nint main(){\nline0();\nreturn 0;\n}";
            //foreach (List<int> ints1 in ints) {
            
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
                                    compiledCode += "if("+ ints[i][1] +" == "+ints[i][3]+"){line " + ints[i][2].ToString()+"();}";
                                    break;
                                case 2:
                                    compiledCode += "if(" + ints[i][1] + " != " + ints[i][3] + "){line " + ints[i][2].ToString() + "();}";
                                    break;
                                case 3:
                                    compiledCode += "if(" + ints[i][1] + " > " + ints[i][3] + "){line " + ints[i][2].ToString() + "();}";
                                    break;
                                case 4:
                                    compiledCode += "if(" + ints[i][1] + " < " + ints[i][3] + "){line " + ints[i][2].ToString() + "();}";
                                    break;
                                case 5:
                                    compiledCode += "if(" + ints[i][1] + " >= " + ints[i][3] + "){line " + ints[i][2].ToString() + "();}";
                                    break;
                                case 6:
                                    compiledCode += "if(" + ints[i][1] + " <= " + ints[i][3] + "){line " + ints[i][2].ToString() + "();}";
                                    break;
                                }

                                break;
                            case 4:
                                break;
                            case 5:
                                compiledCode += "printf(\"%c\",(char)array[" + ints[i][1] + "]);";
                                break;
                            case 6:
                                compiledCode += "exit(" + ints[i][1] + ");";
                                break;
                            case 7:
                                break;
                            case 8:
                                compiledCode += "array[" + ints[i][2] + "] =" + ints[i][1] + ";";
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

            File.WriteAllText("Compiled.c", compiledCode);
           // Console.WriteLine(compiledCode);
        }
        public static string file(string address){
            return File.ReadAllText(address);
        }
    }
}
