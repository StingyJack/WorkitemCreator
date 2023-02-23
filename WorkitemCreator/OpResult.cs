namespace WorkitemCreator
{
    public class OpResult<T>
    {
        public bool IsOk { get; set; }
        public string Errors { get; set; }
        public T Data { get; set; }
    }
}