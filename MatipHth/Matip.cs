using System;

namespace MatipHth
{
    public class Matip
    {
        public Matip()
        {
        }
        private const byte version = x'01';
        private const byte open = x'FE';
        private const byte openconfirm = x'FD';
        private const byte close = x'FC';
        private const byte ssq = x'FB';
        private const byte ssr = x'FA';
        private const byte coding = x'14';
        private const byte subtype = x'20';
        private const byte mpxhdr = x'A0';
        private const byte ocbyte2 = x'05';
        private const byte notrafficrefuse = x'01';
        private const byte soincoherent = x'02';
        private const byte normalclose = x'00';

        public byte[] matipopen()
        {
            byte[] openformat = new byte[12];
            openformat[0] = version;
            openformat[1] = open;
            openformat[2] = x'00';
            openformat[3] = x'0C';
            openformat[4] = coding;
            openformat[5] = subtype;
            openformat[6] = x'00';
            openformat[7] = mpxhdr;
            openformat[8] = x'A0';    // H1H2
            openformat[9] = x'A0';    //H1H2
            openformat[10] = x'00';
            openformat[11] = x'00';
            return openformat;
        }
        
        public byte[] maptipclose()
        {
            byte[] closeformat = new byte[5];
            closeformat[0] = version;
            closeformat[1] = close;
            closeformat[2] = x'00';
            closeformat[3] = ocbyte2;
            return closeformat;
        }
        

        public byte[] maptipopenconfirm()
        {
            byte[] openconfirmformat = new byte[5];
            closeformat[0] = version;
            closeformat[1] = openconfirm;
            closeformat[2] = x'00';
            closeformat[3] = ocbyte2;
            return openconfirmformat;
        }
        
    }
}
