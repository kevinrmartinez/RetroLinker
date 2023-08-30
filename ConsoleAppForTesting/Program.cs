// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// string file = "F:\\Zero Fox\\Anime Icon Matcher.exe";
// var icoStream = WinFunc.WinIconProc.ExtractIco(file);
// if (icoStream == null) { Console.WriteLine("Operacion Fallida"); }
// else { Console.WriteLine("Operación exitosa"); Console.Beep(); }
var temp = Path.GetTempPath();
Console.WriteLine();
Console.WriteLine(temp);