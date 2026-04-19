using System.Reflection;

namespace RetailCoreEcommerce.Persistence;

/// <summary>
/// This class is used to reference the assembly of the repositories project 
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}