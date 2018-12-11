using System;
using System.Configuration;
using System.Text;

namespace MatipHth
{
    class Program
    {
        static void Main(string[] args)
        {
            var MatipHandler = new MatipHthWrapper();
            while(true)
            {
                Console.WriteLine("enter input \n");
                string input = Console.ReadLine();
                var response = MatipHandler.MatipDataSend(input);
                if (MatipHandler.MatipError || MatipHandler.HthError || MatipHandler.Timeout)
                {
                    Console.WriteLine(" Error reeived during Matip Oepration\n or timeout received\n");
                }
                else
                {
                    Console.WriteLine("the response receive is" + BitConverter.ToString(response));
                    Console.WriteLine("the response receive is" + Encoding.UTF8.GetString(response));
                }
            }

        }
    }
}
