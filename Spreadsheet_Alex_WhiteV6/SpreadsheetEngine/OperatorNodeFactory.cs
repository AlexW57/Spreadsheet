namespace Cpts321
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    internal class OperatorNodeFactory
    {
        public OperatorNodeFactory()
        {
        }

        public OperatorNode CreateOperatorNode(char character)
        {
            return CreateNode(character);
        }

        public OperatorNode CreateNode(char character)
        {
            return new OperatorNode(character);
        }
    }
}
