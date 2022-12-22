using System;

namespace DarkRift.Tests
{
    public class MockMessageBuffer : IMessageBuffer
    {
        public byte[] Buffer { get; set; }
        public int Count { get; set; }
        public int Offset { get; set; }

        public IMessageBuffer Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
