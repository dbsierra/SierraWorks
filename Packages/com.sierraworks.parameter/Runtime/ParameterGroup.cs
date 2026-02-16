using System;
using System.Collections.Generic;

namespace SierraWorks.Parameter
{
    [Serializable]
    public class ParameterGroup
    {
        public string groupName;
        public List<Parameter> parameters = new List<Parameter>();

        public ParameterGroup(string name)
        {
            groupName = name;
        }
    }
}
