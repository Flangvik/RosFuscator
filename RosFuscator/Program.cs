using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RosFuscator
{
    /*
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * Add this code to the source code you are looking to obfuscate
     
    private static Random random = new Random();
	public static string RandomKey(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		return new string(Enumerable.Repeat(chars, length)
		  .Select(s => s[random.Next(s.Length)]).ToArray());
	}

	public static byte[] xorEncDec(byte[] input, string theKeystring)
	{
		byte[] theKey = Encoding.UTF8.GetBytes(theKeystring);
		byte[] mixed = new byte[input.Length];
		for (int i = 0; i < input.Length; i++)
		{
			mixed[i] = (byte)(input[i] ^ theKey[i % theKey.Length]);
		}
		return mixed;
	}
     * 
     * 
     * 
     * 
     * 
     * 
     */
    class StringsCollector : CSharpSyntaxWalker
    {
        public static string[] BadStrings = new string[] { "DllImport", "const" };

        public static SyntaxKind[] BadTypes = new SyntaxKind[] { SyntaxKind.Parameter, SyntaxKind.ParameterList, SyntaxKind.CaseKeyword, SyntaxKind.CasePatternSwitchLabel, SyntaxKind.CaseSwitchLabel };

        public List<(string varData, string fullString)> _stringDict = new List<(string varData, string fullString)>();

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                try
                {
                    if (BadStrings.Where(x => node.Parent.Parent.Parent.Parent.ToFullString().Contains(x)).Count() > 0)
                        return;
                }
                catch (Exception)
                {

                }
                if (((string)node.Token.Value).Contains("\\Users\\"))
                {
                    var value3 = (node.Parent.Parent.Parent.ToFullString());
                    var value2 = (node.Parent.Parent.ToFullString());
                    var value1 = (node.Parent.ToFullString());
                    var fpp = "nar";
                }

                if (!BadTypes.Contains(node.Parent.Parent.Parent.Kind()) && !BadTypes.Contains(node.Parent.Parent.Kind()) && !BadTypes.Contains(node.Parent.Kind()))
                    _stringDict.Add(((string)node.Token.Value, (string)node.Parent.Parent.ToFullString()));
                else
                    Console.WriteLine($"[+] Bad string value {(string)node.Token.Value}");

            }



        }

    }


    class Program
    {
        private static Random random = new Random();
        public static string RandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static byte[] xorEncDec(byte[] input, string theKeystring)
        {
            byte[] theKey = Encoding.UTF8.GetBytes(theKeystring);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mixed[i] = (byte)(input[i] ^ theKey[i % theKey.Length]);
            }
            return mixed;
        }

        static void Main(string[] args)
        {
            args = new string[] { @"C:\Users\FlangvikStream\source\repos\PoshBeacon\PoshBeacon\Program.cs" };
            string asciArt = @"
██▀███   ▒█████    ██████   █████▒█    ██   ██████  ▄████▄   ▄▄▄     ▄▄▄█████▓ ▒█████   ██▀███  
▓██ ▒ ██▒▒██▒  ██▒▒██    ▒ ▓██   ▒ ██  ▓██▒▒██    ▒ ▒██▀ ▀█  ▒████▄   ▓  ██▒ ▓▒▒██▒  ██▒▓██ ▒ ██▒
▓██ ░▄█ ▒▒██░  ██▒░ ▓██▄   ▒████ ░▓██  ▒██░░ ▓██▄   ▒▓█    ▄ ▒██  ▀█▄ ▒ ▓██░ ▒░▒██░  ██▒▓██ ░▄█ ▒
▒██▀▀█▄  ▒██   ██░  ▒   ██▒░▓█▒  ░▓▓█  ░██░  ▒   ██▒▒▓▓▄ ▄██▒░██▄▄▄▄██░ ▓██▓ ░ ▒██   ██░▒██▀▀█▄  
░██▓ ▒██▒░ ████▓▒░▒██████▒▒░▒█░   ▒▒█████▓ ▒██████▒▒▒ ▓███▀ ░ ▓█   ▓██▒ ▒██▒ ░ ░ ████▓▒░░██▓ ▒██▒
░ ▒▓ ░▒▓░░ ▒░▒░▒░ ▒ ▒▓▒ ▒ ░ ▒ ░   ░▒▓▒ ▒ ▒ ▒ ▒▓▒ ▒ ░░ ░▒ ▒  ░ ▒▒   ▓▒█░ ▒ ░░   ░ ▒░▒░▒░ ░ ▒▓ ░▒▓░
  ░▒ ░ ▒░  ░ ▒ ▒░ ░ ░▒  ░ ░ ░     ░░▒░ ░ ░ ░ ░▒  ░ ░  ░  ▒     ▒   ▒▒ ░   ░      ░ ▒ ▒░   ░▒ ░ ▒░
  ░░   ░ ░ ░ ░ ▒  ░  ░  ░   ░ ░    ░░░ ░ ░ ░  ░  ░  ░          ░   ▒    ░      ░ ░ ░ ▒    ░░   ░ 
   ░         ░ ░        ░            ░           ░  ░ ░            ░  ░            ░ ░     ░     
                                                    ░                                            ";

            // Read some input source code
            string orgSourceCode = "";
            string sourceCodeDirectory = @"C:\Users\FlangvikStream\Desktop\Seatbelt";

            if (args.Count() > 0)
                sourceCodeDirectory = args[0];



            foreach (var orgSourceCodePath in Directory.EnumerateFiles(sourceCodeDirectory, "*.cs", SearchOption.AllDirectories).Where(x => !x.Contains(@"\obj\") && !x.Contains("AssemblyInfo.cs")))
            {

                Console.WriteLine($"[+] Obfuscating {orgSourceCodePath}");

                orgSourceCode = File.ReadAllText(orgSourceCodePath);

                //If this is Program.cs
                if (orgSourceCodePath.EndsWith("Program.cs"))
                {
                    var orgCode = @"    public static class Program
    {";
                    //We need to add our function
                    orgSourceCode = orgSourceCode.Replace(orgCode,
                        @"    public static class Program
    {
        public static byte[] xorEncDec(byte[] input, string theKeystring)
        {
            byte[] theKey = System.Text.Encoding.UTF8.GetBytes(theKeystring);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mixed[i] = (byte)(input[i] ^ theKey[i % theKey.Length]);
            }
            return mixed;
        }"
                        );
                }

                // Pull strings and XOOOOOR them
                SyntaxTree tree = CSharpSyntaxTree.ParseText(orgSourceCode);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();



                // FOOBAR -> base64 
                var stringCollector = new StringsCollector();
                stringCollector.Visit(root);
                var newSourceCode = orgSourceCode;

                foreach (var stringItem in stringCollector._stringDict.Where(x => !string.IsNullOrEmpty(x.varData) && x.varData.Length > 3 && !x.varData.Equals("ABCDEFGHIJKLMNOPQRSTUVWXYZ")))
                {
                    //For each string

                    //Generate an random key 
                    var randomkey = Guid.NewGuid().ToString();

                    var xorEncString = xorEncDec(Encoding.UTF8.GetBytes(stringItem.varData), randomkey);

                    var xorEncStringB64 = Convert.ToBase64String(xorEncString);

                    var decrypted = xorEncDec(Convert.FromBase64String(xorEncStringB64), randomkey);

                    var decryptedLogic = $"System.Text.Encoding.UTF8.GetString(Program.xorEncDec(System.Convert.FromBase64String(\"{xorEncStringB64}\"), \"{randomkey}\"))";
                    //XOR the value

                    //Console.WriteLine($" {stringItem} <=> {decryptedLogic}");

                    //Replace the original value with decrypt logic
                    var newSnippet = stringItem.fullString.Replace("@\"" + stringItem.varData + "\"", decryptedLogic);
                    newSnippet = newSnippet.Replace("\"" + stringItem.varData + "\"", decryptedLogic);

                    newSourceCode = newSourceCode.Replace(stringItem.fullString, newSnippet);

                }

                //var orgFilenName = Path.GetFileNameWithoutExtension(sourceCodeDirectory);
                //var newPath = sourceCodeDirectory.Replace(orgFilenName, "Obf" + orgFilenName);
                //Console.WriteLine($"[+] Obfuscated source written {newPath}");
                File.WriteAllText(orgSourceCodePath, newSourceCode);
            }
            // Save and compile newly obfuscated source code



        }
    }
}
