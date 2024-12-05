using AssetStudio;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace Generator
{
    public class Generator
    {
        public AssemblyLoader assemblyLoader = new AssemblyLoader();

        public void loadFolder(string assemblyFolder)
        {
            this.assemblyLoader.Load(assemblyFolder);
            this.assemblyLoader.Loaded = true;
        }

        public IEnumerable<TypeDefinition> getTypeDefs(string m_AssemblyName, List<String> m_ClassNames, String m_Namespace)
        {
            if (m_ClassNames.Count > 0)
            {
                List<TypeDefinition> typeDefs = new List<TypeDefinition>();
                foreach (String m_ClassName in m_ClassNames)
                {
                    typeDefs.Add(assemblyLoader.GetTypeDefinition(m_AssemblyName, string.IsNullOrEmpty(m_Namespace) ? m_ClassName : $"{m_Namespace}.{m_ClassName}"));
                }
                return typeDefs;
            } else
            {
                return assemblyLoader.GetTypeDefinitions(m_AssemblyName);
            }
        }

        public List<TypeTreeNode> convertToTypeTreeNodes(TypeDefinition typeDef, int[] version)
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
    }
}
