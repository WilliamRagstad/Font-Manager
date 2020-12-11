using System;
using System.Linq;
using System.Reflection;

namespace CSGO_Font_Manager
{
    internal static class LocateAssemblyLibrary
    {
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var requiredDLLName = $"{(new AssemblyName(args.Name)).Name}.dll";
            var resource = currentAssembly.GetManifestResourceNames().Where(s => s.EndsWith(requiredDLLName)).FirstOrDefault();

            if (resource != null)
            {
                using (var stream = currentAssembly.GetManifestResourceStream(resource))
                {
                    if (stream == null) return null;

                    var block = new byte[stream.Length];
                    stream.Read(block, 0, block.Length);
                    return Assembly.Load(block);
                }
            }
            else
            {
                return null;
            }
        }
    }
}