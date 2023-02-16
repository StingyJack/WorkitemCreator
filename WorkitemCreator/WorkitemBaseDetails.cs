namespace WorkitemCreator
{
    using System;

    public class WorkitemBaseDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string WorkitemType { get; set; }
        public Uri Uri { get; set; }
    }
}