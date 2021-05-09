using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;

class Program
{
    public static string xorSource = @"
       public static byte[] xorEncDec(byte[] input, string theKeystring)
       {
            byte[] theKey = System.Text.Encoding.UTF8.GetBytes(theKeystring);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mixed[i] = (byte)(input[i] ^ theKey[i % theKey.Length]);
            }
            return mixed;
        }
";
    public class CSharpObfuscator : CSharpSyntaxRewriter
    {

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

        public List<(string before, string after)> _stringDict = new List<(string before, string after)>();
        public static string[] BadStrings = new string[] { "DllImport", "const" };

        public static SyntaxKind[] BadTypes = new SyntaxKind[] {
            SyntaxKind.Parameter,
            SyntaxKind.ParameterList,
            SyntaxKind.CaseKeyword,
            SyntaxKind.CasePatternSwitchLabel,
            SyntaxKind.CaseSwitchLabel
        };

        public override SyntaxNode Visit(SyntaxNode rawSyntaxNode)
        {
            if (rawSyntaxNode.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var castedSyntaxNode = (LiteralExpressionSyntax)rawSyntaxNode;

                if (castedSyntaxNode != null)
                {
                    //If this is a string node
                    if (castedSyntaxNode.Kind().Equals(SyntaxKind.StringLiteralExpression))
                    {
                        //Turn the node into it's raw string form 
                        var actualStringValue = ((LiteralExpressionSyntax)castedSyntaxNode).Token.ValueText;

                        var actualStringValueOuter = ((LiteralExpressionSyntax)castedSyntaxNode).Token.Text;

                        var declartionOfvalue = ((LiteralExpressionSyntax)castedSyntaxNode).Parent.Parent.ToFullString();

                        //Process the node
                        var randomkey = Guid.NewGuid().ToString();

                        var xorEncString = xorEncDec(Encoding.UTF8.GetBytes(actualStringValue), randomkey);

                        var xorEncStringB64 = Convert.ToBase64String(xorEncString);

                        var decrypted = xorEncDec(Convert.FromBase64String(xorEncStringB64), randomkey);

                        var decryptedLogic = $"System.Text.Encoding.UTF8.GetString(Program.xorEncDec(System.Convert.FromBase64String(\"{xorEncStringB64}\"), \"{randomkey}\"))";

                        var result = declartionOfvalue.Replace(actualStringValueOuter, decryptedLogic);
                        // Console.WriteLine($"[+] {actualStringValue} => {xorEncStringB64}");

                        try
                        {
                            if (BadStrings.Where(x => castedSyntaxNode.Parent.Parent.Parent.Parent.ToFullString().Contains(x)).Count() > 0)
                                return base.Visit(castedSyntaxNode);
                        }
                        catch (Exception)
                        {

                        }

                        if (castedSyntaxNode.Token.Value.ToString().Contains("\\Users\\"))
                        {
                            var value3 = (castedSyntaxNode.Parent.Parent.Parent.ToFullString());
                            var value2 = (castedSyntaxNode.Parent.Parent.ToFullString());
                            var value1 = (castedSyntaxNode.Parent.ToFullString());

                        }

                        if (!BadTypes.Contains(castedSyntaxNode.Parent.Parent.Parent.Kind()) && !BadTypes.Contains(castedSyntaxNode.Parent.Parent.Kind()) && !BadTypes.Contains(castedSyntaxNode.Parent.Kind()))
                            _stringDict.Add((declartionOfvalue, result));

                    }
                    return base.Visit(castedSyntaxNode);
                }
                return base.Visit(castedSyntaxNode);
            }
            return base.Visit(rawSyntaxNode);
        }

        public static async Task<Project> ObfuscateSyntaxNodes<T>(Project inputProject, DocumentId documentId) where T : SyntaxNode
        {

            Project project = inputProject;

            ProjectId projectId = project.Id;

            Document orgDoc = project.GetDocument(documentId);

            SyntaxNode orgRoot = await orgDoc.GetSyntaxRootAsync();

            int countOfTObjects = orgRoot.DescendantNodesAndSelf().OfType<T>().ToArray().Count();

            int maxSize = 0;
            for (int i = 0; i < countOfTObjects; i++)
            {
                Document currentDoc = project.GetDocument(documentId);

                //Get the correct root tree for doc we are modifing
                SyntaxNode currentRoot = await currentDoc.GetSyntaxRootAsync();

                T targetObject = currentRoot.DescendantNodesAndSelf().OfType<T>().ToArray()[i];

                Compilation targetComp = await project.GetCompilationAsync();

                SemanticModel targetModel = targetComp.GetSemanticModel(currentRoot.SyntaxTree);

                ISymbol targetSymbol = targetModel.GetDeclaredSymbol(targetObject);
               

                if (targetSymbol != null)
                {
                    if (!targetSymbol.Name.Equals("Main"))
                    {

                        //  if (!targetSymbol.IsOverride && !targetSymbol.IsImplicitlyDeclared && !targetSymbol.Name.ToLower().Contains("dispose"))
                        if (!targetSymbol.IsOverride  && !targetSymbol.Name.ToLower().Contains("dispose"))
                        {
                            //Generate random nale
                            string newName = "_" + Guid.NewGuid().ToString().Replace("-", "");

                            //Generate a new solution this this modification
                            Solution newSolution = await Renamer.RenameSymbolAsync(project.Solution, targetSymbol, newName, project.Solution.Workspace.Options);

                            var printString = $"[+] {targetSymbol.Name} => {newName}";

                            if (maxSize <= printString.Length)
                                maxSize = printString.Length;

                            Console.Write("\r" + printString.PadRight(maxSize + 10, ' '));

                            project = newSolution.GetProject(projectId);
                        }
                    }
                }

            }
            return project;
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }


        static async Task Main(string[] args)
        {

            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            try
            {
                //MSBuildLocator.RegisterInstance(instance);
                MSBuildLocator.RegisterDefaults();
            }
            catch (Exception) { }


            //Major credits to https://stackoverflow.com/questions/59069677/roslyn-save-edited-document-to-physical-solution

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
                                                    ░                                           
@Flangvik

#Obfuscate only strings and methods
Example: ./RosFuscator.exe /path/to/target/solution/SeatBelt.sln --strings --methods

#Obfuscate all the things!
Example: ./RosFuscator.exe /path/to/target/solution/SeatBelt.sln 
";
            Console.WriteLine(asciArt);
            var solutionPath = "";

            if (args.Count() == 0)
            {
                Console.WriteLine("[!] Missing solution path!");
                Environment.Exit(0);

            }


            bool obfuMethods = false;
            bool obfuStrings = false;
            bool obfuClasses = false;
            bool obfuVars = false;

            solutionPath = args[0];

            if (args.Select(x => x.ToLower()).Contains("--methods"))
                obfuMethods = true;

            if (args.Select(x => x.ToLower()).Contains("--strings"))
                obfuStrings = true;

            if (args.Select(x => x.ToLower()).Contains("--classes"))
                obfuClasses = true;

            if (args.Select(x => x.ToLower()).Contains("--vars"))
                obfuVars = true;

            if (!obfuVars && !obfuClasses && !obfuStrings && !obfuMethods)
            {
                obfuVars = true;
                obfuClasses = true;
                obfuStrings = true;
                obfuMethods = true;
            }



            using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.    
                //Uncomment to debug
                //workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                Console.WriteLine($"Loading solution '{solutionPath}'");

                // Attach progress reporter so we print projects as they are loaded.
                Solution solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
                Console.WriteLine($"Finished loading solution '{solutionPath}'");

                var mainTrigger = false;

                foreach (var projectId in solution.ProjectIds)
                {
                    Project project = solution.GetProject(projectId);

                    foreach (var documentId in project.DocumentIds)
                    {
                        Document orgDocument = project.GetDocument(documentId);

                        SyntaxNode orgRoot = await orgDocument.GetSyntaxRootAsync();


                        if (orgRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault() != null && obfuStrings)
                        {
                            if (!mainTrigger)
                            {
                                var rawDocText = (await orgDocument.GetTextAsync()).ToString().Split('\n');

                                var mainLine = rawDocText.Where(x => x.Contains("Main(")).FirstOrDefault();

                                if (mainLine != null)
                                {

                                    var NewmainLine = xorSource + "\n\n\n" + mainLine;

                                    var compiledraw = string.Join("\n", rawDocText);

                                    compiledraw = compiledraw.Replace(mainLine, NewmainLine);

                                    var xorNode = CSharpSyntaxTree.ParseText(compiledraw).GetRoot();

                                    var xorEditor = await DocumentEditor.CreateAsync(orgDocument);

                                    xorEditor.ReplaceNode(orgRoot, xorNode);

                                    var xorDocument = xorEditor.GetChangedDocument();

                                    project = xorDocument.Project;

                                    orgDocument = project.GetDocument(documentId);

                                    orgRoot = await orgDocument.GetSyntaxRootAsync();

                                    mainTrigger = true;
                                }
                            }

                            #region stringObfuscation 
                            var stringObfu = new CSharpObfuscator();

                            stringObfu.Visit(orgRoot);

                            var rootText = orgRoot.GetText().ToString();

                            foreach (var dictItem in stringObfu._stringDict)
                            {
                                rootText = rootText.Replace(dictItem.before, dictItem.after);
                            }

                            var newRoot = CSharpSyntaxTree.ParseText(rootText).GetRoot();

                            var editor = await DocumentEditor.CreateAsync(orgDocument);

                            editor.ReplaceNode(orgRoot, newRoot);

                            var newDocument = editor.GetChangedDocument();

                            project = newDocument.Project;

                            #endregion
                        }
                    }

                    //Persist the project changes to the current solution
                    solution = project.Solution;
                }


                foreach (var projectId in solution.ProjectIds)
                {
                    Project project = solution.GetProject(projectId);

                    foreach (var documentId in project.DocumentIds)
                    {


                        if (obfuVars)
                            project = await ObfuscateSyntaxNodes<VariableDeclaratorSyntax>(project, documentId);

                        if (obfuMethods)
                            project = await ObfuscateSyntaxNodes<MethodDeclarationSyntax>(project, documentId);

                        if (obfuClasses)
                            project = await ObfuscateSyntaxNodes<ClassDeclarationSyntax>(project, documentId);
                       

                    }


                    //Persist the project changes to the current solution
                    solution = project.Solution;
                }


                //Finally, apply all your changes to the workspace at once.
                var didItWork = workspace.TryApplyChanges(solution);



            }
        }
    }
}