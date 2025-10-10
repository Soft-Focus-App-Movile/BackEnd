using System;

namespace SoftFocusBackend.Therapy.Domain.Model.ValueObjects
{
    public class ConnectionCode
    {
        public string Value { get; private set; }

        private ConnectionCode(string value)
        {
            Value = value;
        }

        public static ConnectionCode Create(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 8)
                throw new ArgumentException("Connection code must be exactly 8 characters long.");

            return new ConnectionCode(value.ToUpperInvariant());
        }

        public override bool Equals(object obj)
        {
            if (obj is not ConnectionCode other) return false;
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}