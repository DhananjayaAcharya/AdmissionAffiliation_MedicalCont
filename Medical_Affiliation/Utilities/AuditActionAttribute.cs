using System;

namespace Medical_Affiliation.Utilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuditActionAttribute : Attribute
    {
        public string? Module { get; set; }
        public string? Description { get; set; }

        public AuditActionAttribute(string? module = null, string? description = null)
        {
            Module = module;
            Description = description;
        }
    }
}