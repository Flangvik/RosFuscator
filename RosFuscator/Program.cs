using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public List<String> _strings = new List<string>();

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

                _strings.Add((string)node.Token.Value);


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
            string sourceCodePath = "";

            if (args.Count() > 0)
            {
                sourceCodePath = args[0];

                if (!File.Exists(sourceCodePath))
                {
                    Console.WriteLine("[+] Missing input source code path , eg rosfuscator.exe Program.cs");
                    Environment.Exit(0);
                }
            }

            orgSourceCode = File.ReadAllText(sourceCodePath);

            // Pull strings and XOOOOOR them
            SyntaxTree tree = CSharpSyntaxTree.ParseText(orgSourceCode);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();



            // FOOBAR -> base64 
            var stringCollector = new StringsCollector();
            stringCollector.Visit(root);
            var newSourceCode = orgSourceCode;

            foreach (var stringItem in stringCollector._strings.Where(x => !string.IsNullOrEmpty(x) && x.Length > 3 && !x.Equals("ABCDEFGHIJKLMNOPQRSTUVWXYZ")))
            {
                //For each string

                //Generate an random key 
                var randomkey = Guid.NewGuid().ToString();

                var xorEncString = xorEncDec(Encoding.UTF8.GetBytes(stringItem), randomkey);

                var xorEncStringB64 = Convert.ToBase64String(xorEncString);

                var decrypted = xorEncDec(Convert.FromBase64String(xorEncStringB64), randomkey);

                var decryptedLogic = $" Encoding.UTF8.GetString(xorEncDec(Convert.FromBase64String(\"{xorEncStringB64}\"), \"{randomkey}\"))";
                //XOR the value

                Console.WriteLine($" {stringItem} <=> {decryptedLogic}");

                //Replace the original value with decrypt logic
                newSourceCode = newSourceCode.Replace("@\"" + stringItem + "\"", decryptedLogic);
                newSourceCode = newSourceCode.Replace("\"" + stringItem + "\"", decryptedLogic);
            }

            var orgFilenName = Path.GetFileNameWithoutExtension(sourceCodePath);
            var newPath = sourceCodePath.Replace(orgFilenName, "Obf" + orgFilenName);
            Console.WriteLine($"[+] Obfuscated source written {newPath}");
            File.WriteAllText(newPath, newSourceCode);

            // Save and compile newly obfuscated source code



        }
    }
}
