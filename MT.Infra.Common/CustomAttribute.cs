using System;

namespace MT.Infra.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FixedLengthRecord:Attribute
    {
        public string Name { get; set; }

        public FixedLengthRecord()
        {
            Name = "FixedLengthRecord";
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyFixedLength:Attribute
    {
        public int Length { get; set; }
        public PropertyFixedLength(int length)
        {
            Length = length;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DelimitedRecord:Attribute
    {
        public char Delimiter { get; set; }
        public string Name { get; set; }
        public DelimitedRecord(char delimiter)
        {
            Delimiter = delimiter;
            Name = "DelimitedRecord";
        }
    }
}
