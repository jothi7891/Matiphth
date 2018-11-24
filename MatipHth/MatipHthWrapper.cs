using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Threading;  
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Configuration;


namespace MatipHth
{
    public class MatipHthWrapper
    {
        // The port number for the remote device.  
        private static Socket Client; 
        private static bool Error = false;


        //Receive Buffers
        private static byte[] ReceiveBuffer = new byte[8192];
        
        // Matip constant strings

        private static byte[] MatipOpenByte = new byte[] { 0x01, 0xFE, 0x00, 0x0C, 0x14, 0x20, 0x00, 0xA0, 0xFF, 0xFF, 0x00, 0x00 };
        private static byte[] MatipCloseByte = new byte[] { 0x01, 0xFC, 0x00, 0x05, 0x00 };
        private static byte[] MatipOpenConfirmByte = new byte[] { 0x01, 0xFD, 0x00, 0x05, 0x00 };
        private static string LayerStart = "V";
        private static string Layer5Request = "VHEG.WA";
        private static byte InLayerSep = 0x2F;
        private static byte EOD = 0x0D;
        private static byte EOT = 0x03;
        private static byte ACC = 0x2E;
        private static string Layer6 = "VGZ";
        private static string FlynasAirlineCode = "XY";
        private static int TPR;
        
        
        public MatipHthWrapper()
        {
            TPR = 1;
        }
        
        private void Connect() {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the   
                // remote device is "host.contoso.com".  
                //IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
                            
                var section = ConfigurationManager.GetSection("MatipSettings") as NameValueCollection;
                var ip = section["IpAddress"];
                var port = Convert.ToInt32(section["Port"]);
                
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket. 
                Client = new Socket(ipAddress.AddressFamily,
                                SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                Client.Connect(remoteEP);  
  
                Console.WriteLine("Socket connected to {0}",  
                    Client.RemoteEndPoint.ToString());  
                
              } catch (Exception e) {
                Error = true;  // Set up the error flag
                Console.WriteLine(e.ToString());  
            } 

         }
         
    
        private static void Receive() {  
            try {
                // Create the state object.  
                Client.Receive(ReceiveBuffer);
            } catch (Exception e) {  
                Error = true;  // Set up the error flag
                Console.WriteLine(e.ToString());  
            }  
        }  
     

        private static void Send(byte[] bytes) {
    // Convert the string data to byte data using ASCII encoding.  
    //byte[] byteData = Encoding.ASCII.GetBytes(data); 
                try {
                // Create the state object.  
                Client.Send(bytes);
            } catch (Exception e) {
                Error = true;  // Set up the error flag  
                Console.WriteLine(e.ToString());  
            }  
        }   
        
        public bool MatipOpen(){
            bool OpenConfirm = false;
            try{
                Connect();
                //Establish a MATIP - Send Matip Open
                var section = ConfigurationManager.GetSection("MatipSettings") as NameValueCollection;
                var H1H2 = section["MatipH1H2"];
                var H1H2Byte = BitConverter.GetBytes(Convert.ToInt16(H1H2));

                MatipOpenByte[8] = H1H2Byte[0];
                MatipOpenByte[9] = H1H2Byte[1];
                Send(MatipOpenByte);
                
                // Receive the Matip Open Response
                Receive();    
                
                //Check for Matip Open Confirmation
                byte[] ReceivedData = ReceiveBuffer.Skip(0).Take(5).ToArray();
                if(ReceivedData.SequenceEqual(MatipOpenConfirmByte))
                {
                    OpenConfirm = true;
                    Console.WriteLine("Open confirmed received : {0}", ReceiveBuffer);
                 }
                else
                {
                    Error = true;  // Set up the error flag
                    
                }
                
                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", ReceiveBuffer);

            } catch (Exception e) {  
                Error = true;  // Set up the error flag
                Console.WriteLine("Exception during Matip Open" + e.ToString());  
            }
            return OpenConfirm;
        }
        
        public void MatipClose(){
            try{
                //Establish a MATIP - Send Matip Close
                Send(MatipCloseByte);
              
                // Release the socket.  
                Client.Shutdown(SocketShutdown.Both);  
                Client.Close(); 
                 
            } catch (Exception e) {  
                Error = true;  // Set up the error flag
                Console.WriteLine("Exception during Matip Close" + e.ToString());  
            } 
        }
        
        public void MatipDataSend(String Data){
            try{

                // Prepend Hth Header
               byte[] HthRequest = PrependHthRequest(Data);

                
                //Establish a data request
                Send(HthRequest);
                
                // Receive the data response
                Receive();


                // Extract the Response data and check if the hth headers have been received in tact

                byte[] response = ExtractResponse();
                
                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", ReceiveBuffer);
                
                // Write the response to the console.  
                Console.WriteLine("Extracted response is " +  response.ToString());
                 
            } catch (Exception e) {  
                Error = true;  // Set up the error flag
                Console.WriteLine("Exception during Matip data Send" + e.ToString());  
            } 
        }
        
        private byte[] PrependHthRequest(String Data){
            var section = ConfigurationManager.GetSection("MatipSettings") as NameValueCollection; 
            var SitaLayer5 = section["DestinationLayer5"];
            var Flynaslayer5 = section["SourceLayer5"];
            List<byte> DataRequest = new List<byte>();
            DataRequest.Add(0x01);
            DataRequest.AddRange(Encoding.ASCII.GetBytes(LayerStart));
            DataRequest.Add(ACC);
            DataRequest.Add(EOD);
            DataRequest.AddRange(Encoding.ASCII.GetBytes(Layer5Request));
            DataRequest.Add(InLayerSep);
            DataRequest.AddRange(Encoding.ASCII.GetBytes(SitaLayer5));
            DataRequest.Add(InLayerSep);
            DataRequest.AddRange(Encoding.ASCII.GetBytes(Flynaslayer5));
            DataRequest.Add(InLayerSep);
            DataRequest.Add((byte)'P');
            var TprString = TPR.ToString("D6");
            TPR = TPR + 1;
            if (TPR >= 1000)
            {
                TPR = 1;
            }
            DataRequest.AddRange(Encoding.ASCII.GetBytes(TprString));
            DataRequest.Add(EOD);
            DataRequest.AddRange(Encoding.ASCII.GetBytes(Layer6));
            DataRequest.Add(ACC);
            DataRequest.Add(EOD);            
            DataRequest.AddRange(Encoding.ASCII.GetBytes(LayerStart));
            DataRequest.AddRange(Encoding.ASCII.GetBytes(FlynasAirlineCode));
            DataRequest.AddRange(Encoding.ASCII.GetBytes("////"));
            DataRequest.Add(EOD);
            DataRequest.AddRange(Encoding.ASCII.GetBytes(Data));
            DataRequest.Add(EOT);
            byte[] length = BitConverter.GetBytes((short)DataRequest.ToArray().Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(length);
            }
            
            DataRequest.InsertRange(1, length);
            return DataRequest.ToArray();
        }
        

private byte[] ExtractResponse()
    {
            List<byte> response = new List<byte>();
            
        if (ReceiveBuffer[7] == 'H')   // check if host to host error occured
        {
                var Start = Array.LastIndexOf(ReceiveBuffer, (byte)0x0D) + 1;
                var Length = Array.LastIndexOf(ReceiveBuffer, (byte)0X03) - Array.LastIndexOf(ReceiveBuffer, (byte)0x0D);
                
                response.AddRange(ReceiveBuffer.Skip(Start).Take(Length).ToArray());      
        }
        else{
                Error = true;
                response.AddRange(Encoding.ASCII.GetBytes("Error Extracing Hth Headers"));
            }
            return response.ToArray();
    }
            
        }  
}
