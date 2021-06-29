
namespace TwitterBot
{
    class Program
    {
        static int seconds = 0;
        static void Main(string[] args)
        {

            Twitter twitter = new Twitter();
            twitter.BuildAndSendTweet();


        }
    }
}
