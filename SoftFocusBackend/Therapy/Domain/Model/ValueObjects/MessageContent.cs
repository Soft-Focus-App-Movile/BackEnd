namespace SoftFocusBackend.Therapy.Domain.Model.ValueObjects
{
    public class MessageContent
    {
        public string Value { get; private set; }

        private MessageContent(string value)
        {
            Value = value;
        }

        public static MessageContent Create(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 2000)
                throw new ArgumentException("Message content must be between 1 and 2000 characters.");

            return new MessageContent(value);
        }
    }
}