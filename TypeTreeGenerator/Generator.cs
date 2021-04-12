using AssetStudio;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace Generator
{
    public class Generator
    {
        private AssemblyLoader assemblyLoader = new AssemblyLoader();

        public void loadFolder(string assemblyFolder)
        {
            this.assemblyLoader.Load(assemblyFolder);
            this.assemblyLoader.Loaded = true;
        }

        public IEnumerable<TypeDefinition> getTypeDefs(string m_AssemblyName, String m_ClassName, String m_Namespace)
        {
            IEnumerable<TypeDefinition> typeDefs = (m_ClassName.Length > 0) ?
                new List<TypeDefinition>()
                {
                    assemblyLoader.GetTypeDefinition(m_AssemblyName, string.IsNullOrEmpty(m_Namespace) ? m_ClassName : $"{m_Namespace}.{m_ClassName}")
                } : assemblyLoader.GetTypeDefinitions(m_AssemblyName);
            return typeDefs;
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