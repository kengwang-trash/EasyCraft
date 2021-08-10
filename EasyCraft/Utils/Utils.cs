using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace EasyCraft.Utils
{
    public static class Utils
    {
        public static IEnumerable<SqliteParameter> ToSqliteParameter(this Dictionary<string, object> dic)
        {
            return dic.Select(t => new SqliteParameter(t.Key, t.Value));
        }
        
        // 我觉得全大写好看点
        // ReSharper disable once InconsistentNaming
        public static string GetMD5(this string str)
        {
            if (str == null) str = "";
            var pwd = "";
            var md5 = MD5.Create(); //实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            var s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (var i = 0; i < s.Length; i++)
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符

                pwd = pwd + s[i].ToString("x2");
            return pwd;
        }
        
        /// <summary>
        ///     生成随机字符串
        /// </summary>
        /// <param name="length">目标字符串的长度</param>
        /// <param name="useNum">是否包含数字，1=包含，默认为包含</param>
        /// <param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        /// <param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        /// <param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        /// <param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        /// <returns>指定长度的随机字符串</returns>
        public static string CreateRandomString(int length = 15, bool useNum = true, bool useLow = true,
            bool useUpp = false, bool useSpe = false, string custom = "")
        {
            var b = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(b);
            var r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum) str += "0123456789";
            if (useLow) str += "abcdefghijklmnopqrstuvwxyz";
            if (useUpp) str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (useSpe) str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
            for (var i = 0; i < length; i++) s += str.Substring(r.Next(0, str.Length - 1), 1);
            return s;
        }
    }
}