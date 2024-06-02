using System;

namespace RewindProject
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RewindStateBasedAttribute : System.Attribute { }
}

