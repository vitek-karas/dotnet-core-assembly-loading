using System;
using System.Reflection;
using System.Runtime.Loader;

public class ComponentContext : AssemblyLoadContext
{
    private string basePath;

    protected override Assembly Load(AssemblyName assemblyName)
    {
        try
        {
            Assembly result = Assembly.Load(assemblyName);
            if (result != null)
            {
                return result;
            }
        }
        catch (Exception)
        {
        }

        string candidatePath = System.IO.Path.Combine(basePath, assemblyName.Name + ".dll");
        if (System.IO.File.Exists(candidatePath))
        {
            return LoadFromAssemblyPath(candidatePath);
        }

        return null;
    }

    public static (ComponentContext, Assembly) CreateContext(string assemblyPath)
    {
        var host = new ComponentContext();
        host.basePath = System.IO.Path.GetDirectoryName(assemblyPath);
        var assembly = host.LoadFromAssemblyPath(assemblyPath);
        return (host,assembly);
    }

    public static (ComponentContext, Assembly, T) CreateContext<T>(string assemblyPath, string typeName)
    {
        var (ComponentContext, assembly) = CreateContext(assemblyPath);
        T obj =(T)assembly.CreateInstance(typeName);
        return (ComponentContext, assembly, obj);
    }
}