using System;
using System.Security.Cryptography;
using System.Text;

namespace keylog.modules
{
    public abstract class Module
    {
        protected String name;

        public Module(String name)
        {
            this.name = name;
        }

        public String GetName()
        {
            return name;
        }

        /**
        * Get a hash md5 of a string
        * @param input: A string to be hashed
        * @return: md5 of the string */
        public static String GetHash(String input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            //Convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        //Method to execute every module
        public abstract void Execute();
    }
}

