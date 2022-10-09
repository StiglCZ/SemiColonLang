using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
namespace Complier{
    class Program {
        public string compiledCode = "";
        public string allLines = "";
        public bool debug = false;
        List<List<int>> localInts;
        public bool inFunc = false;
        public string output = "compiled.c";

        public bool jumping = false;
        public static void Main(string[] args) {
            string pathToConfigs = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"/scl/";
            Program program = new Program();
            string complier = "";
            string complierArgs = "";
            string execution1 = ""; 
            bool delCCode = false;
            
            string path = "";
            for(int i = 0; i < args.Length; i++){
                if (args[i] == "-o"|| args[i] == "--out"){
                    i++;
                    program.output = args[i];       
                }else if(args[i] == "-i"|| args[i] == "--input"){
                    i++;
                    path = args[i];
                }else if (args[i] == "-g" || args[i] == "--complier"){
                    i++;
                    complier = args[i];
                }else if (args[i] == "-gp"){
                    i++;
                    complierArgs = args[i].Replace("_"," ");
                }else if(args[i] == "-d" || args[i] == "-delete"){
                    delCCode = true;
                }
                else if (args[i] == "-x" || args[i] == "-exec"){
                    i++;
                    execution1 = args[i];
                }
                else if (args[i] == "-c" ){
                    if (args.Length - i -1 > 0)
                    {
                        if (args[i + 1] == "new")
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine("Semicolon Lang Configuration utility");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.Write("Program sl filename:");
                            string Input = Console.ReadLine() + "";
                            Console.Write("Output filename:");
                            string Output = Console.ReadLine() + "";
                            Console.Write("C complier name:");
                            string Complier = Console.ReadLine() + "";
                            Console.Write("Additional compiler arguments:");
                            string ComplierArgs = Console.ReadLine() + "";
                            Console.Write("Configuration file path:");
                            string ConfOut = Console.ReadLine() + "";
                            Console.Write("Path to execute after compilation(empty=dont run):");
                            string execution = Console.ReadLine() + "";

                        a:
                            Console.Write("Remove c file after compiling[y/n]?");
                            char del = Console.ReadKey().KeyChar;
                            if (del == 'y'){
                                delCCode = true;
                            }
                            else if (del == 'n'){
                                Console.Write("\n");
                                delCCode = false;
                            }
                            else{
                                goto a;
                            }
                            string all = Input + "|" + Output + "|" + Complier + "|" + ComplierArgs + "|" + execution + "|" + delCCode.ToString();
                            if (!Directory.Exists(pathToConfigs)){
                                Directory.CreateDirectory(pathToConfigs);
                            }

                             File.WriteAllText(pathToConfigs + ConfOut, all);
                        }
                        else if (args[i + 1] == "default"){
                            path = "code.sl";
                            program.output = "compiled.c";
                            program.allLines = file(path);
                            program.Compile();
                            var proc = Process.Start("gcc", "compiled.c -o compiled.exe");
                            proc.WaitForExit();
                            File.Delete("compiled.c");
                            proc.WaitForExit();
                            Process.Start("compiled.exe");
                        }
                        else if (File.Exists(pathToConfigs + args[i + 1])){
                            string[] strings = File.ReadAllText(pathToConfigs+ args[i + 1]).Split('|');
                            path = strings[0];
                            program.output = strings[1];
                            program.allLines = file(path);
                            program.Compile();
                            var proc = Process.Start(strings[2], strings[1] + " " + strings[3]);
                            if (strings[4] != "" || strings[5].ToLower() == "true")
                            {
                                proc.WaitForExit();
                            }
                            if (strings[5].ToLower() == "true"){
                                File.Delete(strings[1]);
                            }
                            if (strings[4] != ""){
                                Process.Start(strings[4]);
                            }
                        }
                        else{
                            Console.WriteLine("Fatal error: No valid config file found");
                        }
                    }
                    else{
                        Console.Write("Fatal error: No config files");
                    }
                    Environment.Exit(0);
                }
                else if (File.Exists(args[i])){
                    path = args[i];
                }
            }
            if (args.Length > 0 && path != ""){
                program.allLines = file(path);
                program.Compile();
                if(complier.Length > 0){
                    var process = Process.Start(complier,program.output +" " + complierArgs);
                    if(delCCode || execution1 != ""){
                        process.WaitForExit();
                    }
                    if (delCCode){
                        File.Delete(program.output);
                    }
                    if(execution1 != ""){
                        Process.Start(execution1);
                    }
                    
                    
                }
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

            for (int i = 0; i < code.Length; i++){
                if (code[i] == '{'){
                    opened++;
                    already1 = true;
                }
                else if (code[i] == '}'){
                    opened--;
                }
                if (already1 && opened == 0){
                    code[i + 1] = '|';
                    already1 = false;
                    opened = 0;
                }
            }
            List<string> command = code.ToString().Split('|').ToList();
            if (debug) { Console.Write("Values:"); }
                for (int i = 0; i < command.Count; i++){
                    int x = 0;
                    List<int> ints = new List<int>();
                    for (int j = 0; j < command[i].Length; j++){
                        if (command[i][j] == ';'){
                            for (int k = j; k < command[i].Length; k++){
                                x++;
                                if (command[i][k] != ';'){
                                    if (x > 1){
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
            localInts = ints1;
            return code.ToString(); 
        }
        public void convertToC(List<List<int>> ints){
            int[] jpoints = new int[255];
            compiledCode= "#include <stdio.h>\n" +
                "#include <stdlib.h>\n" +
                "int array[1000];\n";
            for(int i =0; i < 20; i++){
                compiledCode += "int func" + i + "();";
            }
            compiledCode += "\n";
            for (int i = 0; i < ints.Count; i++) {
                compiledCode += "int line" + i.ToString() + "();";
            }
            compiledCode += "\nint main(){\n\tline0();\n\treturn 0;\n}";
            for (int i = 0; i < ints.Count; i++) {
                 jumping = false ;
                inFunc=false;
                compiledCode += "\nint line" + i.ToString() + "(){\n\t";
                    if (ints[i].Count > 0){
                    ParseFunc(ints[i],i);
                    }
                if (inFunc){
                }
                else if (i < ints.Count - 1 && !jumping){
                    compiledCode += "\n\tline" + (i + 1).ToString() + "();\n\treturn 0;\n}";
                }
                else if(!jumping){
                    compiledCode += "exit(0);\n\treturn 0;\n}";
                }
                else{
                    compiledCode += "\treturn 0;\n}";
                }

            }

            File.WriteAllText(output, compiledCode);
           // Console.WriteLine(compiledCode);
        }
        public void ParseFunc(List<int> ints,int fnNext){
            switch (ints[0])
            {
                case 1:
                    compiledCode += "array[" + ints[2] + "] =array[" + ints[1] + "]";
                    break;
                case 2:
                    jumping = true;
                    compiledCode += "line" + ints[1] + "();\n";
                    break;
                case 3:
                    switch (ints[2])
                    {
                        case 1:
                            compiledCode += "if(array[" + ints[1] + "] == array[" + ints[3] + "]){line " + ints[2].ToString() + "();}";
                            break;
                        case 2:
                            compiledCode += "if(array[" + ints[1] + "] != array[" + ints[3] + "]){line" + ints[2].ToString() + "();}";
                            break;
                        case 3:
                            compiledCode += "if(array[" + ints[1] + "] > array[" + ints[3] + "]){line" + ints[2].ToString() + "();}";
                            break;
                        case 4:
                            compiledCode += "if(array[" + ints[1] + "] < array[" + ints[3] + "]){line" + ints[2].ToString() + "();}";
                            break;
                        case 5:
                            compiledCode += "if(array[" + ints[1] + "] >= array[" + ints[3] + "]){line" + ints[2].ToString() + "();}";
                            break;
                        case 6:
                            compiledCode += "if(" + ints[1] + " <= array[" + ints[3] + "]){line" + ints[2].ToString() + "();}";
                            break;
                    }

                    break;
                case 4:
                    switch (ints[2])
                    {
                        case 1:
                            compiledCode += "array[" + ints[4] + "] = array[" + ints[1] + "] + array[" + ints[3] + "];";
                            break;
                        case 2:
                            compiledCode += "array[" + ints[4] + "] = array[" + ints[1] + "] - array[" + ints[3] + "];";
                            break;
                        case 3:
                            compiledCode += "array[" + ints[4] + "] = array[" + ints[1] + "] * array[" + ints[3] + "];";
                            break;
                        case 4:
                            compiledCode += "array[" + ints[4] + "] = array[" + ints[1] + "] / array[" + ints[3] + "];";
                            break;
                        default:
                            compiledCode += "array[" + ints[4] + "] = array[" + ints[1] + "] + array[" + ints[3] + "];";
                            break;
                    }
                
                    //counting
                    break;
                case 5:
                    //printing char
                    compiledCode += "printf(\"%c\",(char)array[" + ints[1] + "]);";
                    break;
                case 6:
                    //exit
                    compiledCode += "exit(" + ints[1] + ");";
                    break;
                case 7:
                    //input - read character
                    compiledCode += "char tmp;\nscanf(\"%c\",tmp);\nint tmp2 = (int)tmp; array[ " + ints[1] + "] = tmp2;";
                    break;
                case 8:
                    //copy
                    compiledCode += "array[" + ints[2] + "] =" + ints[1] + ";";
                    break;
                case 9:
                    //numeric input
                    if (ints[2] == 2){
                        compiledCode += "int tmp;\nscanf(\"%d\",&tmp);\narray[" + ints[1] + "] = tmp;";
                    }
                    else if (ints[2] == 1){
                        compiledCode += "printf(\"%d\",array[" + ints[1] + "]); ";
                    }
                    break;
                //Function
                case 10:
                    inFunc = true;
                    compiledCode += "line" + (fnNext + 1).ToString() + "();\n\treturn 0;\n}";
                    compiledCode += "\nint func" + ints[1] +"(){\n";

                    List<List<int>> cmds = new List<List<int>>();
                    for(int i = 2; i < ints.Count; i++){
                        cmds.Add(new List<int>());
                            for(int j = i; j < ints.Count; j++){
                            if (ints[j] == 10){
                                i = j + 1;
                                j = Int16.MaxValue;
                                
                            }
                            else{
                                cmds[cmds.Count-1].Add(ints[j]);
                            }
                            }
                    }
                    cmds.RemoveAt(cmds.Count - 1);
                    for (int i =0; i < cmds.Count; i++){
                        compiledCode += "\t";
                        ParseFunc(cmds[i],-1);
                        compiledCode += "\n";
                        
                    }
                    
                    compiledCode += "\treturn 0;\n}";
                    break;
                case 11:
                    compiledCode += "func" + ints[1].ToString() + "();";
                    break;
                default:
                    compiledCode += "\n";
                    break;
            }
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
                string line = Console.ReadLine() + "";
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
                else{
                    line = line.Replace("\n", "");
                    line = line.Replace("\t", "");
                    line = line.Replace(" ", "");
                    int x = 0;
                    List<int> ints = new List<int>();
                    for (int j = 0; j < line.Length; j++){
                        if (line[j] == ';'){
                            for (int k = j; k < line.Length; k++){
                                x++;
                                if (line[k] != ';'){
                                    if (x > 1){
                                        ints.Add(x - 1);
                                    }
                                    j += x - 1;
                                    x = 0;
                                }
                            }
                        }
                    }
                    if (ints.Count > 0){
                        switch (ints[0]){
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
                                if (ints[2] == 1) { reg[ints[4]] = ints[3] + ints[1]; }
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
                            case 50:
                                Console.WriteLine("000:" + reg[0]);
                                Console.Write("    1 2 3 4 5 6 7 8 9 0");
                                for (int j = 0; j < reg.Length; j++){
                                    if (j > 0){
                                        Console.Write(reg[j] + " ");
                                    }
                                    if (j % 10 == 0){
                                        if (j < 100){
                                            Console.Write("\n0" + (j / 10).ToString() + "0:");
                                        }
                                        else{
                                            Console.Write("\n" + (j / 10).ToString() + "0:");
                                        }
                                    }
                                }
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
