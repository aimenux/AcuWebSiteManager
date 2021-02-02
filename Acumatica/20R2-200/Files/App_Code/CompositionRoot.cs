using System;

[Obsolete("This object is obsolete and will be removed. Rewrite your code without this object or contact your partner for assistance.")]
public static class CompositionRoot
{
    public static void CreateContainer()
    {
        PX.Data.DependencyInjection.CompositionRoot.CreateContainer();
    }
}