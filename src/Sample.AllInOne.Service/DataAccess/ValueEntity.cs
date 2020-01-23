using System;

namespace Sample.AllInOne.Service.DataAccess
{
    public class ValueEntity
    {
        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        //public DateTimeOffset LastModified { get; set; }
        //public byte[] Rev { get; set; }
        public string Value { get; set; }
    }
}