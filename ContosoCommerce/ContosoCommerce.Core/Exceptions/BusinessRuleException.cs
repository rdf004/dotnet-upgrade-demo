using System;

namespace ContosoCommerce.Core.Exceptions
{
    /// <summary>
    /// Thrown when a business rule is violated.
    /// </summary>
    [Serializable]
    public class BusinessRuleException
        : Exception
    {
        public string RuleName { get; private set; }

        public BusinessRuleException(
            string ruleName, string message)
            : base(message)
        {
            RuleName = ruleName;
        }

        public BusinessRuleException(
            string message)
            : base(message)
        {
        }

        public BusinessRuleException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        protected BusinessRuleException(
            System.Runtime.Serialization
                .SerializationInfo info,
            System.Runtime.Serialization
                .StreamingContext context)
            : base(info, context)
        {
        }
    }
}
