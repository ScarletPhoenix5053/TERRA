using System;

namespace SCARLET.TERRA
{
    public class TERRAException : Exception
    {
        public TERRAException()
        {
        }
        public TERRAException(string message)
            : base(message)
        {
        }
        public TERRAException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class TER_NotImplimentedException : TERRAException
    {
        const string defaultMsg = "Feature planned and not yet implmented";

        public TER_NotImplimentedException() : this(defaultMsg) { }
        public TER_NotImplimentedException(string message)
            : base(message)
        {
        }
        public TER_NotImplimentedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}