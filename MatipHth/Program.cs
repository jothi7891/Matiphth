using System;
using System.Configuration;

namespace MatipHth
{
    class Program
    {
        static void Main(string[] args)
        {
            var MatipHandler = new MatipHthWrapper();
            MatipHandler.MatipOpen();
            MatipHandler.MatipDataSend("this is a test data<EOF>");
            MatipHandler.MatipClose();
        }
    }
}
