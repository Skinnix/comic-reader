using System;
using System.Collections.Generic;
using System.Text;

namespace Skinnix.ComicReader.Client.MVVM
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ViewModelAttribute : Attribute
    {
        public Type ImplementationType { get; }
        public Type? DesignType { get; set; }
        public bool Singleton { get; set; }

        public ViewModelAttribute(Type implementationType)
        {
            this.ImplementationType = implementationType;
        }
    }
}
