namespace Cpts321
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    internal class ConstantNode : ExpressionNode
    {
        // Class variables        
        private double constantValue;

        // ConstantNode default constructor
        public ConstantNode()
        {
        }

        // Getters and setters
        public double Constant
        {
            get { return this.constantValue; }
            set { this.constantValue = value; }
        }

        public override double Evaluate()
        {
            return this.constantValue;
        }
    }
}
