using System.Text;
class Program{
    //Command to run: dotnet run a; nasm -f elf program.asm;ld program.o -o o -m elf_i386;./o 
    public string output = "program.asm", inputStr = "";
    public static void Main(string[] args) => new Program().Init(args);
    public void Init(string[] args){
        string input = File.ReadAllText(args[0]);
        //Removes comments from the file, makes list of bytes
        List<byte> programBytes = new();
        foreach(char c in input){
            if(c == '{')
                programBytes.Add(0x01);
            else if(c == '}')
                programBytes.Add(0x02);
            else if(c == ';')
                programBytes.Add(0x00);
        }
        //Parser that processes each command
        int level = 0, argNumber = -1;
        int[] value = new int[5]{0,0,0,0,0};
        Stack<int[]> ints = new();
        foreach(byte d in programBytes){
            if(d == 0 && level > 0 && argNumber >= 0){
                value[argNumber]++;
            }else if(d == 1){
                level++;
                argNumber++;
            }else if(d == 2){
                level--;
                if(level == 0){
                    ints.Push(value);
                    value = new int[5]{0,0,0,0,0};
                    argNumber = -1;
                }
            }
        }
        //Reverses the stack of ints since its the opposite way
        Stack<int[]> ints_reversed = new();
        while(ints.Count > 0){
            int[] val = ints.Pop();
            ints_reversed.Push(val);
        }
        //while(ints_reversed.Count > 0){
        //    int[] val = ints_reversed.Pop();
        //    Console.WriteLine($"{val[0]} {val[1]} {val[2]}");
        //}
        
        //Parsing to x86 ASM
        string[] operands = {"","je","jne", "jg", "jl","jge","jle","jz","jnz"};
        string[] noperands = {"","add","sub","imul","idiv"};
        string result = "section .data\n\tregisters dd 256 dup (0)\nglobal _start\n_start:\n";
        int command_counter = 0;
        while(ints_reversed.Count > 0){
            int[] val = ints_reversed.Pop();
            result += $"cmd{command_counter++}:\n";
            switch(val[0]){
                case 1:
                    result += $"\tmov eax, dword [registers + {val[1]*4}]\n" +
                              $"\tmov dword [registers + {val[2]*4}], eax\n";
                    break;
                case 2:
                    result += $"\tjmp cmd{val[1]}\n";
                    break;
                case 3:
                    result += $"\tmov eax, dword [registers + {val[1]*4}]\n"+
                              $"\tmov ebx, dword [registers + {val[3]*4}]\n"+
                               "\tcmp eax, ebx\n"+
                              $"\t{operands[val[2]]} cmd{val[4]}\n";
                    break;
                case 4:
                    result += $"\tmov eax, dword [registers + {val[1]*4}]\n"+
                              $"\tmov ebx, dword [registers + {val[3]*4}]\n"+
                              $"\t{noperands[val[2]]} eax, ebx\n"+
                              $"\tmov dword [registers + {val[4]*4}], eax\n";
                    break;
                case 5:
                    result += "\tmov eax, 4\n" +
                              "\tmov ebx, 1\n" +
                              "\tmov ecx, registers\n" +
                              $"\tadd ecx, {val[1]*4}\n" +
                              "\tmov edx, 1\n" +
                              "\tint 0x80\n";
                    break;
                case 6:
                    result += "\tmov eax, 1\n" +
                              "\txor ebx, ebx\n" +
                              "\tint 0x80\n";
                    break;
                case 7:
                    result += "\tmov eax, 3\n" +
                              "\tmov ebx, 0\n" +
                              $"\tmov ecx, registers+{val[1]*4}\n" +
                              "\tmov edx, 1\n" +
                              "\tint 0x80\n";
                    break;
                case 8:
                    result += $"\tmov eax, registers\n" +
                              $"\tadd eax, {val[2]*4}\n" +
                              $"\tmov dword [eax], {val[1]}\n";
                    break;
                
            }
        }
        result += "mov eax, 1\n" +
                  "xor ebx, ebx\n" +
                  "int 0x80\n";
        File.WriteAllText(output,result);
    }
}