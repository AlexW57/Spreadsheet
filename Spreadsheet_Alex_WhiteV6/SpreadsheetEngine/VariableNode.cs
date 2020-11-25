namespace Cpts321
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    internal class VariableNode : ExpressionNode
    {
        // Class variables        
        private string variableValue;
        private double variableDoubleValue;

        // VariableNode default constructor
        public VariableNode()
        {
        }

        // Getters and setters
        public string Variable
        {
            get { return this.variableValue; }
            set { this.variableValue = value; }
        }

        public double VariableDouble
        {
            get { return this.variableDoubleValue; }
            set { this.variableDoubleValue = value; }
        }

        public override double Evaluate()
        {
            return this.variableDoubleValue;
        }
    }
}
