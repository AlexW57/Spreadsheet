namespace Cpts321
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    internal class OperatorNode : ExpressionNode
    {
        // Class variables        
        private char operatorValue;
        private ExpressionNode left;
        private ExpressionNode right;

        // OperatorNode constructor
        public OperatorNode()
        {
            this.left = null;
            this.right = null;
        }

        public OperatorNode(char newOperatorValue)
        {
            this.operatorValue = newOperatorValue;
            this.left = null;
            this.right = null;
        }
        
        // Getters and setters
        public char Operator
        {
            get { return this.operatorValue;  }
            set { this.operatorValue = value;  }
        }

        public ExpressionNode Left
        {
            get { return this.left; }
            set { this.left = value; }
        }

        public ExpressionNode Right
        {
            get { return this.right; }
            set { this.right = value; }
        }

        public override double Evaluate()
        {
            return 0;
        }
    }
}
