using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mondo.Common.Data
{
    public sealed class Base32 
    {
        private const   int    IN_BYTE_SIZE  = 8;
        private const   int    OUT_BYTE_SIZE = 5;
        private static  char[] alphabet      = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        /*********************************************************************/
        public static string Decode(string strEncoded)
        { 
            int    buffer   = 0;   
            int    bitsLeft = 0;   
            string strResult  = "";
            
            foreach(char ch2 in strEncoded) 
            {     
                buffer <<= 5;      
                char ch = ch2;
           
                // Look up one base32 digit     
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
                {       
                    ch = (char)((ch & 0x1F) - 1);     
                } 
                else if (ch >= '2' && ch <= '7') 
                {       
                    ch -= (char)(ch  - ('2' - 26));     
                } 
                else
                    return("");
                
                buffer |= ch;     
                bitsLeft += 5;   
  
                if (bitsLeft >= 8)
                {       
                    strResult += buffer >> (bitsLeft - 8);    
                    bitsLeft -= 8;
                }   
            }   
            
            return strResult; 
        }  

        /*************************************************************************/
        public static string Encode(byte[] data)
        {
            int i = 0;
            int index = 0;
            int digit = 0;
            int current_byte;
            int next_byte;
            StringBuilder result = new StringBuilder((data.Length + 7) * IN_BYTE_SIZE / OUT_BYTE_SIZE);

            while (i < data.Length) 
            {
                current_byte = (data [i] >= 0) ? data [i] : (data [i] + 256); // Unsign

                // Is the current digit going to span a byte boundary? 
                if(index > (IN_BYTE_SIZE - OUT_BYTE_SIZE)) 
                {
                    if((i + 1) < data.Length)
                        next_byte = (data [i + 1] >= 0) ? data [i + 1] : (data [i + 1] + 256);
                    else
                        next_byte = 0;

                    digit = current_byte & (0xFF >> index);
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    digit <<= index;
                    digit |= next_byte >> (IN_BYTE_SIZE - index);
                    i++;
                }
                else 
                {
                    digit = (current_byte >> (IN_BYTE_SIZE - (index + OUT_BYTE_SIZE))) & 0x1F;
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    if (index == 0)
                        i++;
                }
                result.Append(alphabet [digit]);
            }
            
            return(result.ToString());
        }
    }
}
