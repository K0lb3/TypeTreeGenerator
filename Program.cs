using AssetStudio;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace TypeTreeExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
"A simple typtree generator based in Perfare's AssetStudio." +
"Argumens:\n" +
"-p  ~ assembly folder path (required)\n" +
"-a  ~ assembly name (required)\n" +
"-c  ~ class name (if not set, all classes will be dumped)\n" +
"-n  ~ namespace (optional for set class name)\n" +
"-d  ~ dump style (simple, json, json_min, bin)\n" +
"-z  ~ if used, the data will be compressed via gzip\n" +
"-o  ~ output path (instead of printing the data will be stored in the given path)\n" +
"All binary formats (bin, compressed) are displayed as base64 string."
                );
                return;
            }
            string assemblyFolder = "";
            string m_AssemblyName = "";
            string m_ClassName = "";
            string m_Namespace = "";
            int[] version = { 0, 0, 0, 0 };
            string dump = "simple";
            bool zip = false;
            string output = "";
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-p":
                        assemblyFolder = args[i + 1];
                        i++;
                        break;
                    case "-a":
                        m_AssemblyName = args[i + 1];
                        i++;
                        break;
                    case "-c":
                        m_ClassName = args[i + 1];
                        i++;
                        break;
                    case "-n":
                        m_Namespace = args[i + 1];
                        i++;
                        break;
                    case "-v":
                        version = args[i + 1].Split(",").Select(int.Parse) as int[];
                        i++;
                        break;
                    case "-d":
                        dump = args[i + 1];
                        i++;
                        break;
                    case "-z":
                        zip = true;
                        break;
                    case "-o":
                        output = args[i + 1];
                        i++;
                        break;
                }
            }

            // load typedefs
            AssemblyLoader assemblyLoader = new AssemblyLoader();
            assemblyLoader.Load(assemblyFolder);

            IEnumerable<TypeDefinition> typeDefs = (m_ClassName.Length > 0) ?
                new List<TypeDefinition>() { GetType(m_AssemblyName, m_ClassName, m_Namespace, assemblyLoader) } :
                GetAllTypes(m_AssemblyName, assemblyLoader);

            byte[] data;
            switch (dump)
            {
                case "simple":
                    string str = simpleDump(typeDefs, version).ToString();
                    Console.WriteLine(zip ? Convert.ToBase64String(Zip(Encoding.UTF8.GetBytes(str))) : str);
                    break;
                case "json":
                    data = jsonDump(typeDefs, version, true);
                    Console.WriteLine(zip ? Convert.ToBase64String(Zip(data)) : Encoding.UTF8.GetString(data));
                    break;
                case "json_min":
                    data = jsonDump(typeDefs, version, false);
                    Console.WriteLine(zip ? Convert.ToBase64String(Zip(data)) : Encoding.UTF8.GetString(data));
                    break;
                case "bin":
                    data = binDump(typeDefs, version);
                    Console.WriteLine(Convert.ToBase64String(zip ? Zip(data) : data));
                    break;
            }
        }

        public static List<TypeTreeNode> ConvertToTypeTreeNodes(TypeDefinition typeDef, int[] version)
        {
            var nodes = new List<TypeTreeNode>();
            var helper = new SerializedTypeHelper(version);
            helper.AddMonoBehaviour(nodes, 0);
            if (typeDef != null)
            {
                var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, helper, 1);
                nodes.AddRange(typeDefinitionConverter.ConvertToTypeTreeNodes());
            }
            return nodes;
        }

        public static TypeDefinition GetType(string m_AssemblyName, string m_ClassName,
            string m_Namespace, AssemblyLoader assemblyLoader)
        {
            return assemblyLoader.GetTypeDefinition(m_AssemblyName, string.IsNullOrEmpty(m_Namespace) ? m_ClassName : $"{m_Namespace}.{m_ClassName}");
        }

        public static IEnumerable<TypeDefinition> GetAllTypes(string m_AssemblyName, AssemblyLoader assemblyLoader)
        {
            return assemblyLoader.GetTypeDefinitions(m_AssemblyName);
        }

        public static StringBuilder simpleDump(IEnumerable<TypeDefinition> typeDefs, int[] version)
        {
            StringBuilder sb = new StringBuilder();
            foreach (TypeDefinition typeDef in typeDefs)
            {
                try
                {
                    List<TypeTreeNode> nodes = Program.ConvertToTypeTreeNodes(typeDef, version);
                    sb.AppendLine(typeDef.Name);
                    foreach (TypeTreeNode node in nodes)
                    {
                        sb.AppendLine($"{node.m_Level}\t{node.m_Type}\t{node.m_Name}\t{node.m_MetaFlag}");
                    }

                    sb.AppendLine("");
                }
                catch
                {
                    continue;
                }
            }
            return sb;
        }

        public static byte[] jsonDump(IEnumerable<TypeDefinition> typeDefs, int[] version, bool indented)
        {
            SortedDictionary<string, List<TypeTreeNode>> nodeDict = new SortedDictionary<string, List<TypeTreeNode>>();
            foreach (TypeDefinition typeDef in typeDefs)
            {
                String key;
                List<TypeTreeNode> nodes;
                try
                {
                    key = typeDef.Namespace.Length > 0 ? $"{typeDef.Namespace}.{typeDef.Name}" : typeDef.Name;
                    nodes = Program.ConvertToTypeTreeNodes(typeDef, version);
                    if (nodes.Count > 0) nodeDict.Add(key, nodes);
                }
                catch { continue; }
            }
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented,

            };
            return JsonSerializer.SerializeToUtf8Bytes(nodeDict, options);
        }

        public static byte[] binDump(IEnumerable<TypeDefinition> typeDefs, int[] version)
        {
            MemoryStream s = new MemoryStream();
            foreach (TypeDefinition typeDef in typeDefs)
            {
                String key;
                List<TypeTreeNode> nodes;
                try
                {
                    key = typeDef.Namespace.Length > 0 ? $"{typeDef.Namespace}.{typeDef.Name}" : typeDef.Name;
                    nodes = Program.ConvertToTypeTreeNodes(typeDef, version);
                    if (nodes.Count == 0) continue;
                    s.Write(Encoding.UTF8.GetBytes(key));
                    s.Write(BitConverter.GetBytes((UInt32)nodes.Count));
                    foreach (var node in nodes)
                    {
                        s.Write(BitConverter.GetBytes(node.m_Level));
                        s.Write(Encoding.UTF8.GetBytes(node.m_Type));
                        s.Write(Encoding.UTF8.GetBytes(node.m_Name));
                        s.Write(BitConverter.GetBytes(node.m_MetaFlag));
                    }
                }
                catch { continue; }
            }
            return s.ToArray();
        }

        public static byte[] Zip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }
    }
}
