namespace Lit
{
    public class TaskTest
    {
        public string TestIt()
        {
            WordCount wordCount = new WordCount();
            return wordCount.GetReport().Count.ToString();
        }
    }
}