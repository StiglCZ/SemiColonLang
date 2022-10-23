using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Complier{
    class Program {
        public string compiledCode = "";
        public string hfile = "";
        public string allLines = "";
        public bool debug = false;
        List<List<int>> localInts;
        public bool inFunc = false;
        public string output = "compiled.c";
        public const int recursionLimit = 99;
        public int recursioncount = 1;
        public bool jumping = false;
        public static void Main(string[] args) {
            string pathToConfigs = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"/scl/";
            Program program = new Program();
            string complier = "";
            string complierArgs = "";
            string execution1 = "";
            bool delCCode = false; 
            bool createHfile = false;
            bool intsmode = false;
            string path = "";
            for (int i = 0; i < args.Length; i++) {
                if (args[i] == "-o" || args[i] == "--out"){
                    i++;
                    program.output = args[i];
                } else if (args[i] == "-i" || args[i] == "--input"){
                    i++;
                    path = args[i];
                } else if (args[i] == "-g" || args[i] == "--complier"){
                    i++;
                    complier = args[i];
                } else if (args[i] == "-gp"){
                    i++;
                    complierArgs = args[i].Replace("_", " ");
                } else if (args[i] == "-d" || args[i] == "-delete"){
                    delCCode = true;
                } else if (args[i] == "-h" || args[i] == "-header"){
                    createHfile = true;
                } else if (args[i] == "-x" || args[i] == "-exec") {
                    i++;
                    execution1 = args[i];
                } else if (args[i] == "-n" || args[i] == "-ints"){
                    intsmode = true;
                } else if (args[i] == "-c") {
                    if (args.Length - i - 1 > 0)
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
                            if (del == 'y') {
                                delCCode = true;
                            }
                            else if (del == 'n') {
                                Console.Write("\n");
                                delCCode = false;
                            }
                            else {
                                goto a;
                            }
                        b:
                            Console.Write("Create .h file[y/n]?");
                            char del1 = Console.ReadKey().KeyChar;
                            if (del1 == 'y'){
                                createHfile = true;
                            }
                            else if (del1 == 'n'){
                                Console.Write("\n");
                                createHfile = false;
                            }
                            else{
                                goto b;
                            }
                            string all = Input + "|" + Output + "|" + Complier + "|" + ComplierArgs + "|" + execution + "|" + delCCode.ToString() + "|" + createHfile.ToString();
                            if (!Directory.Exists(pathToConfigs)) {
                                Directory.CreateDirectory(pathToConfigs);
                            }

                            File.WriteAllText(pathToConfigs + ConfOut, all);
                        }
                        else if (args[i + 1] == "default") {
                            path = "code.sl";
                            program.output = "compiled.c";
                            program.allLines = file(path);
                            program.Compile();
                            program.CreateHFile();
                            var proc = Process.Start("gcc", "compiled.c -o compiled.exe");
                            proc.WaitForExit();
                            File.Delete("compiled.c");
                            proc.WaitForExit();
                            Process.Start("compiled.exe");
                        }
                        else if (File.Exists(pathToConfigs + args[i + 1])) {
                            string[] strings = File.ReadAllText(pathToConfigs + args[i + 1]).Split('|');
                            path = strings[0];
                            program.output = strings[1];
                            program.allLines = file(path);
                            program.Compile();
                            var proc = Process.Start(strings[2], strings[1] + " " + strings[3]);
                            if (strings[6].ToLower() == "true"){
                                createHfile = true;
                                program.CreateHFile();
                            }
                            if (strings[4] != "" || strings[5].ToLower() == "true")
                            {
                                proc.WaitForExit();
                            }
                            if (strings[5].ToLower() == "true") {
                                File.Delete(strings[1]);
                            }
                            
                            if (strings[4] != "") {
                                Process.Start(strings[4]);
                            }
                        }
                        else {
                            Console.WriteLine("Fatal error: No valid config file found");
                        }
                    }
                    else {
                        Console.Write("Fatal error: No config files");
                    }
                    Environment.Exit(0);
                }
                else if (File.Exists(args[i])) {
                    path = args[i];
                }
            }
            if (args.Length > 0 && path != "") {
                program.allLines = file(path);
                program.Compile();
                if (createHfile){
                    program.CreateHFile();
                }
                if (complier.Length > 0) {
                    var process = Process.Start(complier, program.output + " " + complierArgs);
                    if (delCCode || execution1 != "") {
                        process.WaitForExit();
                    }
                    if (intsmode){
                        for(int i =0; i < args.Length;i++ ) {
                            if (File.Exists(args[i])){
                                program.CompileOffInts(args[i]);
                            }
                        }
                    }
                    if (delCCode) {
                        File.Delete(program.output);
                    }
                    if (execution1 != "") {
                        Process.Start(execution1);
                    }
                }
            }
            else {
                program.Interactive();
            }
        }
        public void CreateHFile(){
            try{
                File.WriteAllText(output.Replace(".c", ".h"), hfile);
            }
            catch{
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Fatal Error: Failed to write header file: " + output.Replace(".c",".h"));
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(-1);
            }
        }
        public void Compile() {
            formatCode();
            allLines = PrepareString(allLines);
            convertToC(localInts);
        }
        public void formatCode() {
            string characters = "abcdefghijklmnopqrstuvwxyz";
            characters += characters.ToUpper() + "1234567890!@#$%^&*()[]':\",./§?<>";
            allLines = allLines.Replace("\n", "");
            allLines = allLines.Replace("\t", "");
            allLines = allLines.Replace(" ", "");
            for (int i = 0; i < characters.Length; i++) {
                allLines = allLines.Replace(characters[i].ToString(), "");
            }
        }
        public string PrepareString(string source) {
            StringBuilder code = new StringBuilder(source);
            int opened = 0;
            bool already1 = false;
            List<List<int>> ints1 = new List<List<int>>();

            for (int i = 0; i < code.Length; i++) {
                if (code[i] == '{') {
                    opened++;
                    already1 = true;
                }
                else if (code[i] == '}') {
                    opened--;
                }
                if (already1 && opened == 0) {
                    code[i + 1] = '|';
                    already1 = false;
                    opened = 0;
                }
            }
            if(opened > 1){
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Warning: 1 or more } characters are missing");
                Console.ForegroundColor = ConsoleColor.White;
            }


            List<string> command = code.ToString().Split('|').ToList();
            if (debug) { Console.Write("Values:"); }
            for (int i = 0; i < command.Count; i++) {
                int x = 0;
                List<int> ints = new List<int>();
                for (int j = 0; j < command[i].Length; j++) {
                    if (command[i][j] == ';') {
                        for (int k = j; k < command[i].Length; k++) {
                            x++;
                            if (command[i][k] != ';') {
                                if (x > 1) {
                                    ints.Add(x - 1);
                                }
                                j += x - 1;
                                x = 0;
                            }
                        }
                    }
                }
                ints1.Add(ints);
                if (debug) {
                    Console.Write("\n" /*+ i.ToString()*/);
                    for (int j1 = 0; j1 < ints.Count; j1++) {
                        Console.Write(ints[j1] + " ");
                    }
                }
            }
            localInts = ints1;
            return code.ToString();
        }
        public void CompileOffInts(string file){
            List<List<int>> ints = new List<List<int>>();
            List<string> lines = file.Split('\n').ToList();
            for(int i =0; i < lines.Count; i++){
                List<string> lines1 = lines[i].Split(' ').ToList();
                ints.Add(new List<int>());
                for (int j = 0; j < lines.Count; j++){
                    ints[i].Add(int.Parse(lines1[j]));
                }
            }
            convertToC(ints);


        }
        public void convertToC(List<List<int>> ints) {
            int[] jpoints = new int[255];
            compiledCode = "#include <stdio.h>\n" +
                "#include <conio.h>\n" +
                "#include <stdlib.h>\n" +
                "int array[1000];\n";
            for (int i = 0; i < 20; i++) {
                compiledCode += "int func" + i + "();";
            }
            compiledCode += "\n";
            for (int i = 0; i < ints.Count; i++) {
                compiledCode += "int line" + i.ToString() + "();";
            }
            compiledCode += "\nint main(){\n\tarray[5] = 0;\n\tline0();\n\treturn 0;\n}";
            for (int i = 0; i < ints.Count; i++) {
                jumping = false;
                inFunc = false;
                hfile += "int line" + i.ToString() + "();\n";
                compiledCode += "\nint line" + i.ToString() + "(){\n\t";
                if (ints[i].Count > 0) {
                    ParseFunc(ints[i], i);
                }
                if (inFunc) {
                }
                else if (i < ints.Count - 1 && !jumping) {
                    compiledCode += "\n\tline" + (i + 1).ToString() + "();\n\treturn 0;\n}";
                }
                else if (!jumping) {
                    compiledCode += "exit(0);\n\treturn 0;\n}";
                }
                else {
                    compiledCode += "\treturn 0;\n}";
                }

            }
            try{
                File.WriteAllText(output, compiledCode);
            }
            catch{
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Fatal Error: Cannot write to file: " + output);
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(-1);
            }
            // Console.WriteLine(compiledCode);
        }
        public void Warning(string reason){
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Warning: " + reason);
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }
        public void Warning(string reason, int line){
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Warning: " + reason + " at line:" + line);
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }
        public void ParseFunc(List<int> ints, int fnNext) {
            switch (ints[0]){
                case 1:
                    if (ints.Count < 3) {Warning("Too few arguments",(fnNext+1)); return; }
                    compiledCode += "array[" + ints[2] + "] =array[" + ints[1] + "]";
                    break;
                case 2:
                    if (ints.Count < 2) { Warning("Too few arguments", (fnNext + 1)); return; }
                    jumping = true;
                    compiledCode += "line" + ints[1] + "();\n";
                    break;
                case 3:
                    if (ints.Count < 5) { Warning("Too few arguments", (fnNext + 1)); return; }
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
                    if (ints.Count < 5) { Warning("Too few arguments", (fnNext + 1)); return; }
                    switch (ints[2]){
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
                    if (ints.Count < 2) { Warning("Too few arguments", (fnNext + 1)); return; }
                    //printing char
                    compiledCode += "printf(\"%c\",(char)array[" + ints[1] + "]);";
                    break;
                case 6:
                    if (ints.Count < 2) { Warning("Too few arguments", (fnNext + 1)); return; }
                    //exit
                    compiledCode += "exit(" + ints[1] + ");";
                    break;
                case 7:
                    if (ints.Count < 2) { Warning("Too few arguments", (fnNext + 1)); return; }
                    //input - read character
                    compiledCode += "char tmp;\nscanf(\"%c\",tmp);\nint tmp2 = (int)tmp; array[ " + ints[1] + "] = tmp2;";
                    break;
                case 8:
                    if (ints.Count < 3) { Warning("Too few arguments", (fnNext + 1)); return; }
                    //copy
                    compiledCode += "array[" + ints[2] + "] =" + ints[1] + ";";
                    break;
                case 9:
                    if (ints.Count < 2) { Warning("Too few arguments", (fnNext + 1)); return; }
                    //numeric input
                    if (ints[2] == 2) {
                        compiledCode += "int tmp;\nscanf(\"%d\",&tmp);\narray[" + ints[1] + "] = tmp;";
                    }
                    else if (ints[2] == 1) {
                        compiledCode += "printf(\"%d\",array[" + ints[1] + "]); ";
                    }
                    break;
                //Function
                case 10:
                    if (ints.Count < 1) { Warning("Too few arguments", (fnNext + 1)); return; }
                    inFunc = true;
                    compiledCode += "line" + (fnNext + 1).ToString() + "();\n\treturn 0;\n}";
                    compiledCode += "\nint func" + ints[1] + "(){\n";
                    hfile += "int func" + ints[1] + "();\n";
                    List<List<int>> cmds = new List<List<int>>();
                    for (int i = 2; i < ints.Count; i++) {
                        cmds.Add(new List<int>());
                        for (int j = i; j < ints.Count; j++) {
                            if (ints[j] == 10) {
                                i = j + 1;
                                j = Int16.MaxValue;
                            }
                            else {
                                cmds[cmds.Count - 1].Add(ints[j]);
                            }
                        }
                    }
                    cmds.RemoveAt(cmds.Count - 1);
                    cmds.RemoveAt(cmds.Count - 2);
                    cmds.RemoveAt(cmds.Count - 3);


                    for (int i = 0; i < cmds.Count; i++) {
                        compiledCode += "\t";
                        if (cmds[i].Count > 0) {
                            ParseFunc(cmds[i], -1);
                        }
                        compiledCode += "\n";

                    }

                    compiledCode += "\treturn 0;\n}";
                    break;
                case 11:
                    if (ints.Count < 2) { Warning("Too few arguments", (fnNext + 1)); return; }
                    compiledCode += "func" + ints[1].ToString() + "();";
                    break;
                case 12:
                    if (ints.Count < 1) { Warning("Too few arguments", (fnNext + 1)); return; }
                    //Custom C code
                    compiledCode += "\t";
                    for (int i = 1; i < ints.Count; i++){
                        compiledCode += Convert.ToChar(ints[i]);
                    }
                    compiledCode += "\n";
                    break;
                case 13:
                    if (ints.Count < 2) { Warning("Too few arguments", (fnNext + 1)); return; }
                    string name = "";
                    for (int i = 1; i < ints.Count; i++){
                        name += Convert.ToChar(ints[i]).ToString();
                    }
                    compiledCode = "#include \""+name+"\"" + compiledCode;
                    break;
                case 14:
                    compiledCode += "printf(\"\\e[1;1H\\e[2J\");";
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Fatal Error: Invalid instruction on line:" +  (fnNext +1));
                    Console.ForegroundColor = ConsoleColor.White;
                    Environment.Exit(-1);
                    break;
            }
        }
        public static string file(string address) {
            try{
                return File.ReadAllText(address);
            }
            catch{
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Fatal Error: Cannot read source file ");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(-1);
                return "";
            }
        }
        public void IntCmd(string line, int[] reg, string[] cmds){
            if (line == "" || line == null){
                return;
            }
            else if (line == "cls" || line == "clear"){
                Console.Clear();
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

                            for (int i = reg[ints[1]]; i < cmds.Length - 1; i++) {
                                if (recursioncount < recursionLimit) {
                                    line = cmds[i];
                                    IntCmd(line, reg, cmds);
                                }else{
                                    Console.WriteLine("Recursion limit reached(" + recursionLimit + ")");
                                }
                            }                            
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
                            if (ints[1] == 1){
                                Console.WriteLine(reg[ints[2]]);
                            }else if (ints[2] == 2){
                                reg[ints[2]] = int.Parse(Console.ReadLine());
                            }

                            break;
                        case 14:
                            Console.Clear();
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
        }
                    public void Interactive() {
                        int[] reg = new int[1000];
                    shellinit:
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine("Semicolon Language Interactive [" + DateTime.Now.ToString().Split(' ')[1].Split(':')[0] + ":" + DateTime.Now.ToString().Split(' ')[1].Split(':')[1] + "]");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                        List<string> cmds = new List<string>();
                        while (true) {
                            int commandNumber = 0;
                            Console.Write("$");
                            string line = Console.ReadLine() + "";
                            cmds.Add(line);

                            if (line == "" || line == null) {
                                goto end;
                            }
                            else if (line == "cls" || line == "clear") {
                                Console.Clear();
                                goto shellinit;
                            }
                            else if (line.Split(' ')[0] == "compile") {
                                allLines = file(line.Split(' ')[1]);
                                Compile();
                                Console.WriteLine("Done!");
                            }
                            else {
                                line = line.Replace("\n", "");
                                line = line.Replace("\t", "");
                                line = line.Replace(" ", "");
                                int x = 0;
                                List<int> ints = new List<int>();
                                for (int j = 0; j < line.Length; j++) {
                                    if (line[j] == ';') {
                                        for (int k = j; k < line.Length; k++) {
                                            x++;
                                            if (line[k] != ';') {
                                                if (x > 1) {
                                                    ints.Add(x - 1);
                                                }
                                                j += x - 1;
                                                x = 0;
                                            }
                                        }
                                    }
                                }
                                if (ints.Count > 0) {
                                    switch (ints[0]) {
                                        case 1:
                                            reg[ints[2]] = reg[ints[1]];
                                            break;
                                        case 2:
                                            if (recursioncount < recursionLimit){

                                                for (int i = reg[ints[1]]; i < cmds.Count - 1; i++){
                                                line = cmds[i];
                                                IntCmd(line, reg, cmds.ToArray());
                                                }
                                            }

                                            break;
                                        case 3:
                                            switch (ints[2]){
                                                case 1:
                                                    if (reg[ints[2]] == reg[ints[2]]){
                                                    for (int i = reg[ints[4]]; i < cmds.Count - 1; i++){
                                                        line = cmds[i];
                                                        IntCmd(line, reg, cmds.ToArray());
                                                    }
                                                    }
                                                    break;
                                    case 2:
                                        if (reg[ints[2]] != reg[ints[2]])
                                        {
                                            for (int i = reg[ints[4]]; i < cmds.Count - 1; i++)
                                            {
                                                line = cmds[i];
                                                IntCmd(line, reg, cmds.ToArray());
                                            }
                                        }
                                        break;
                                    case 3:
                                        if (reg[ints[2]] > reg[ints[2]])
                                        {
                                            for (int i = reg[ints[4]]; i < cmds.Count - 1; i++)
                                            {
                                                line = cmds[i];
                                                IntCmd(line, reg, cmds.ToArray());
                                            }
                                        }
                                        break;
                                    case 4:
                                        if (reg[ints[2]] < reg[ints[2]])
                                        {
                                            for (int i = reg[ints[4]]; i < cmds.Count - 1; i++)
                                            {
                                                line = cmds[i];
                                                IntCmd(line, reg, cmds.ToArray());
                                            }
                                        }
                                        break;
                                    case 5:
                                        if (reg[ints[2]] <= reg[ints[2]])
                                        {
                                            for (int i = reg[ints[4]]; i < cmds.Count - 1; i++)
                                            {
                                                line = cmds[i];
                                                IntCmd(line, reg, cmds.ToArray());
                                            }
                                        }
                                        break;
                                    case 6:
                                        if (reg[ints[2]] >= reg[ints[2]])
                                        {
                                            for (int i = reg[ints[4]]; i < cmds.Count - 1; i++)
                                            {
                                                line = cmds[i];
                                                IntCmd(line, reg, cmds.ToArray());
                                            }
                                        }
                                        break;
                                }   
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
                                            for (int j = 0; j < reg.Length; j++) {
                                                if (j > 0) {
                                                    Console.Write(reg[j] + " ");
                                                }
                                                if (j % 10 == 0) {
                                                    if (j < 100) {
                                                        Console.Write("\n0" + (j / 10).ToString() + "0:");
                                                    }
                                                    else {
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