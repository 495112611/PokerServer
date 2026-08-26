using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using MySqlConnector;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Security.Cryptography;
#nullable disable
public class DbManager
{
    /// <summary>
    /// 数据库对象
    /// </summary>
    public static MySqlConnection mysql;
    /// <summary>
    /// 连接数据库
    /// </summary>
    /// <param name="db">数据表</param>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口号</param>
    /// <param name="user">用户名</param>
    /// <param name="pw">密码</param>
    /// <returns></returns>
    public static bool Connect(string db, string ip, int port, string user, string pw)
    {
        mysql = new MySqlConnection();
        string s = string.Format("Database={0};Data Source={1};port={2};User Id={3};Password={4}", db, ip, port, user, pw);
        mysql.ConnectionString = s;
        try
        {
            mysql.Open();
            Console.WriteLine("[数据库]启动成功");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库]启动失败" + e.Message);
            return false;
        }
    }
    /// <summary>
    /// 判断字符串是否安全
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static bool IsSafeString(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|\/|\[|\]|\{|\}|%|@|\*|!|\']");
    }
    /// <summary>
    /// 判断账号是否存在
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IsAccountExist(string id)
    {
        if (!IsSafeString(id))
            return true;
        //SQL语句
        //string s = $"select * from account where id={id}";
        string s = string.Format("select * from account where id='{0}'", id);
        try
        {
            //创建执行脚本对象
            MySqlCommand cmd = new MySqlCommand(s, mysql);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool result = dataReader.HasRows;
            dataReader.Close();
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] IsAccountExist fail " + e.Message);
            return true;
        }
    }
    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="pw">密码</param>
    /// <returns></returns>
    public static bool Register(string id, string pw)
    {
        //防止SQL注入
        if (!IsSafeString(id))
        {
            Console.WriteLine("[数据库]注册失败，id不安全");
            return false;
        }
        if (!IsSafeString(pw))
        {
            Console.WriteLine("[数据库]注册失败，密码不安全");
            return false;
        }
        if (IsAccountExist(id))
        {
            Console.WriteLine("[数据库]注册失败，账号存在");
            return false;
        }
        pw = GetMD5(pw);
        //SQL语句
        string s=string.Format("insert into account set id='{0}',pw='{1}';",id,pw);
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mysql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库]注册失败，"+e.Message);
            return false;
        }
    }
    /// <summary>
    /// 创建玩家
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public static bool CreadtePlayer(string id)
    {
        //防止SQL注入
        if (!IsSafeString(id))
        {
            Console.WriteLine("[数据库]创建角色失败，id不安全");
            return false;
        }
        PlayerData playerData = new PlayerData();
        string data=JsonConvert.SerializeObject(playerData);
        //SQL语句
        string s = string.Format("insert into player set id='{0}',data='{1}';", id, data);
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mysql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库]创建失败，" + e.Message);
            return false;
        }
    }
    /// <summary>
    /// 检查密码
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="pw">密码</param>
    /// <returns></returns>
    public static bool CheckPassword(string id,string pw)
    {
        //防止SQL注入
        if (!IsSafeString(id))
        {
            Console.WriteLine("[数据库]检查密码失败，id不安全");
            return false;
        }
        if (!IsSafeString(pw))
        {
            Console.WriteLine("[数据库]检查密码失败，密码不安全");
            return false;
        }
        pw = GetMD5(pw);
        //SQL语句
        string s=string.Format("select * from account where id='{0}' and pw='{1}';",id, pw);
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mysql);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool result = dataReader.HasRows;
            dataReader.Close();
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库]检查密码失败" + e.Message);
            return false;
        }
    }
    /// <summary>
    /// 获取玩家信息
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    public static PlayerData GetPlayerData(string id)
    {
        //防止SQL注入
        if (!IsSafeString(id))
        {
            Console.WriteLine("[数据库] 获取角色信息失败，id不安全");
            return null;
        }
        //SQL语句
        string s = string.Format("select * from player where id='{0}';", id);
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mysql);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool result = dataReader.HasRows;
            if (!result)
            {
                dataReader.Close();
                return null;
            }
            //读取信息
            dataReader.Read();
            string data = dataReader.GetString("data");

            //反序列化
            PlayerData playerData=JsonConvert.DeserializeObject<PlayerData>(data);
            dataReader.Close();
            return playerData;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] 获取玩家信息失败" + e.Message);
            return null;
        }
    }
    /// <summary>
    /// 更新玩家信息
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="playerData">玩家信息</param>
    /// <returns></returns>
    public static bool UpdatePlayerData(string id,PlayerData playerData)
    {
        string data=JsonConvert.SerializeObject(playerData);
        //SQL语句
        string s = string.Format("update player set data='{0}' where id='{1}';", data, id);
        try
        {
            MySqlCommand cmd = new MySqlCommand(s, mysql);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[数据库] 更新玩家数据失败" + e.Message);
            return false;
        }
    }
    public static string GetMD5(string input)
    {
        MD5 md5 = MD5.Create();
        byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}

