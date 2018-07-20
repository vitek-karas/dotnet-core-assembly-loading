using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using interfaces;

namespace guttenapp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var books = new Dictionary<string,string> {
                {"Pride and Prejudice", "http://www.gutenberg.org/files/1342/1342-0.txt"},
                {"The War That Will End War", "http://www.gutenberg.org/files/57481/57481-0.txt"},
                {"Alice’s Adventures in Wonderland","http://www.gutenberg.org/files/11/11-0.txt"},
                {"Dracula","http://www.gutenberg.org/cache/epub/345/pg345.txt"},
                {"The Iliad of Homer","http://www.gutenberg.org/cache/epub/6130/pg6130.txt"},
                {"Dubliners","http://www.gutenberg.org/files/2814/2814-0.txt"},
                {"Gulliver's Travels","http://www.gutenberg.org/files/829/829-0.txt"}
                };

            var assemblyResolver = new AssemblyFileResolver();
            var (wordcountFound, wordCountPath, wordCountCandidates ) = assemblyResolver.GetComponentLibrary("wordcount");

            var (mostcommonwordsFound, mostcommonwordsPath, mostcommonwordsCandidates) = assemblyResolver.GetComponentLibrary("mostcommonwords");

            if (!wordcountFound || !mostcommonwordsFound)
            {
                throw new Exception();
            }

            //Assembly.LoadFrom(wordCountPath);

            var (wordcountContext, wordCountAsm) = ComponentContext.CreateContext(wordCountPath);

            var (mostcommonwordsContext, mostcommonwordsAsm) = ComponentContext.CreateContext(mostcommonwordsPath);

            // var (taskTestFound, taskTestPath, taskTestCandidates) = assemblyResolver.GetComponentLibrary("tasktest");

            // var (taskTestContext, taskTestAsm) = ComponentContext.CreateContext(taskTestPath);

            var taskTestAsm = Assembly.LoadFrom(@"F:\AppModel\dotnet-core-assembly-loading\src\gutenapp\tasktest\bin\Debug\netstandard2.0\tasktest.dll");

            dynamic taskTest = taskTestAsm.CreateInstance("Lit.TaskTest");
            Console.WriteLine(taskTest.TestIt());

            // string wordCountV2Path = Path.GetFullPath(Path.Join(AppContext.BaseDirectory, "../../../../wordcount/bin/1.2/Debug/netstandard2.0/wordcount.dll"));

            // var wordcount2 = Assembly.LoadFile(wordCountV2Path);
            // Console.WriteLine($"{wordcount2.FullName} {wordcount2.Location} {AssemblyLoadContext.GetLoadContext(wordcount2) == AssemblyLoadContext.Default}");

            // wordcount2 = Assembly.LoadFrom(wordCountV2Path);
            // Console.WriteLine($"{wordcount2.FullName} {wordcount2.Location} {AssemblyLoadContext.GetLoadContext(wordcount2) == AssemblyLoadContext.Default}");

            // wordcount2 = Assembly.LoadFile(wordCountPath);
            // Console.WriteLine($"{wordcount2.FullName} {wordcount2.Location} {AssemblyLoadContext.GetLoadContext(wordcount2) == AssemblyLoadContext.Default}");

            ReportAssemblies(wordcountContext, mostcommonwordsContext);

            var client = new HttpClient();

            foreach(var book in books)
            {
                var url = book.Value;
                using(var stream = await client.GetStreamAsync(url))
                using(var reader = new StreamReader(stream))
                {
                    var wordCount = (IProvider)wordCountAsm.CreateInstance("Lit.WordCount");
                    var mostcommonwords = (IProvider)mostcommonwordsAsm.CreateInstance("Lit.MostCommonWords");
                    Task<string> line;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLineAsync();
                        try
                        {
                            var wordCountTask =  wordCount.ProcessTextAsync(line);
                            var mostcommonwordsTask = mostcommonwords.ProcessTextAsync(line);
                            await Task.WhenAll(wordCountTask, mostcommonwordsTask);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            throw e;
                        }
                    }

                    var wordcountReport = wordCount.GetReport();
                    Console.WriteLine($"Book: {book.Key}; Word Count: {wordcountReport["count"]}");
                    Console.WriteLine("Most common words, with count:");
                    var mostcommonwordsReport = mostcommonwords.GetReport();
                    var orderedMostcommonwords = (IOrderedEnumerable<KeyValuePair<string,int>>)mostcommonwordsReport["words"];
                    var mostcommonwordsCount = (int)mostcommonwordsReport["count"];

                    var index = 0;
                    foreach (var word in orderedMostcommonwords)
                    {
                        if (index++ >= 10)
                        {
                            break;
                        }
                        Console.WriteLine($"{word.Key}; {word.Value}");
                    }
                }
            }

            Console.WriteLine();
            
            ReportAssemblies(wordcountContext, mostcommonwordsContext);
        }

        private static void ReportAssemblies(AssemblyLoadContext wordcountContext, AssemblyLoadContext mostcommonwordsContext)
        {
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var context = AssemblyLoadContext.GetLoadContext(asm);
                var def = AssemblyLoadContext.Default;
                var isDefaultContext = context == def;
                var isWordcountContext = context == wordcountContext;
                var isMostcommonwordsContext = context == mostcommonwordsContext;

                if (asm.FullName.StartsWith("System") && isDefaultContext)
                {
                    continue;
                }

                Console.WriteLine($"{asm.FullName}  {(asm.IsDynamic ? "" : asm.Location)}");
                Console.WriteLine($"{context.GetType().Name} Default: {isDefaultContext}; WordCount: {isWordcountContext}; MostCommonWords: {isMostcommonwordsContext}");
            }
        }
    }
}
