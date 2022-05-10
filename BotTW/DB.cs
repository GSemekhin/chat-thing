using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BotTW
{


    class DB
    {
        const string BD_PATH_LINUX = @"/var/shit/DB.txt";
        const string BD_PATH_WINDOWS = @"C:\Users\Gregory\Desktop\DB1.txt";

        string dbHost = string.Empty;
        string dbUser = string.Empty;
        string dbPassword = string.Empty;
        string dbName = string.Empty;

        MySqlConnection conn;

        Logs log;


        public delegate void MethodContainerReconnect();
        public event MethodContainerReconnect onDBConnect;

        public DB(Logs _log)
        {
            log = _log;

            StreamReader bdReader = null;
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    bdReader = new StreamReader(BD_PATH_WINDOWS);
                }
                else
                {
                    bdReader = new StreamReader(BD_PATH_LINUX);
                }
                //bdReader = new StreamReader(BD_PATH_LINUX);
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное] Не открылся поток на чтение информации из файла в БД");
            }

            //Чтение информации для логина в бд
            try
            {
                dbHost = bdReader.ReadLine();
                dbUser = bdReader.ReadLine();
                dbPassword = bdReader.ReadLine();
                dbName = bdReader.ReadLine();
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное] Не прочиталась информация из файла для логина в БД");
            }


            Login();
        }



        public void Login()
        {
            string connString = "server=" + dbHost + ";user=" + dbUser + ";password=" + dbPassword + ";database=" + dbName;
            try
            {
                conn = new MySqlConnection(connString);
                conn.Open();
                log.WriteLog("[Успех][Критичное] Удалось подключиться к БД");
                onDBConnect?.Invoke();
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное] Не удалось подключиться к БД");
            }
        }

        public void Reconnect()
        {
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Login();
                }
                log.WriteLog("[Ошибка][Исправлено] Удалось переподключиться к БД");
            }
            catch
            {
                log.WriteLog("[Ошибка][Критичное] Не удалось переподключиться к БД");
            }
        }


        //Запись в БД
        public void Write(string query)
        {
            try
            {
                MySqlCommand command = new MySqlCommand(query, conn);
                command.ExecuteNonQueryAsync();
            }
            catch
            {
                log.WriteLog("[Ошибка] Не удалось записать в БД: " + query);
                Reconnect();
            }
        }

        #region SimpleAnswer
        public List<SimpleAnswer> GetSimpleAnswers()
        {
            string query = "SELECT * FROM twitch.commands";
            MySqlCommand command;
            MySqlDataReader reader = null;
            try
            {
                command = new MySqlCommand(query, conn);
                reader = command.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Упали при получении списка зрителей - БД" + query);
                Reconnect();
            }


            List<SimpleAnswer> commands = new List<SimpleAnswer>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    if (Convert.ToBoolean(reader[2].ToString()))
                    {
                        commands.Add(new SimpleAnswer(reader[0].ToString(), reader[1].ToString(), Convert.ToBoolean(reader[2].ToString())));
                    }
                }
                reader.Close();
            }

            return commands;
        }

        public bool AddSimpleAnswer(string _name, string _answer, bool _isActive)
        {
            string queryExist = string.Format("SELECT EXISTS(SELECT commandName FROM twitch.commands WHERE commandName = '{0}')", _name);
            MySqlCommand commandExist;
            MySqlDataReader reader = null;
            try
            {
                commandExist = new MySqlCommand(queryExist, conn);
                reader = commandExist.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Отвалились при проверке простой команды на наличие " + queryExist);
            }
            if (reader != null)
            {
                reader.Read();
                if (Convert.ToBoolean(reader[0]))
                {
                    reader.Close();
                    string queryUpdate = string.Format("UPDATE twitch.commands SET commandText = '{0}', isActive = '1' WHERE commandName = '{1}'", _answer, _name);
                    MySqlCommand commandUpdate;
                    try
                    {
                        commandUpdate = new MySqlCommand(queryUpdate, conn);
                        commandUpdate.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при обновлении команды " + queryUpdate);
                        Reconnect();
                    }
                    return true;
                }
                else
                {
                    reader.Close();
                    string queryInsert = string.Format("INSERT INTO twitch.commands (commandName, commandText, isActive) " +
                        "VALUES ('{0}', '{1}', {2})", _name, _answer, Convert.ToInt32(_isActive));
                    MySqlCommand commandInsert;
                    try
                    {
                        commandInsert = new MySqlCommand(queryInsert, conn);
                        commandInsert.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при добавлении команды " + queryInsert);
                        Reconnect();
                    }
                    return false;
                }
            }
            return false;
        }

        public bool DeactivateSimpleAnswer(string _name)
        {
            string queryExist = string.Format("SELECT EXISTS(SELECT commandName FROM twitch.commands WHERE commandName = '{0}')", _name);
            MySqlCommand commandExist;
            MySqlDataReader reader = null;
            try
            {
                commandExist = new MySqlCommand(queryExist, conn);
                reader = commandExist.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Отвалились при отключении простой команды при проверке существовния " + queryExist);
                Reconnect();
            }
            if (reader != null)
            {
                reader.Read();
                if (Convert.ToBoolean(reader[0]))
                {
                    reader.Close();
                    string queryUpdate = string.Format("UPDATE twitch.commands SET isActive = '0' WHERE commandName = '{0}'", _name);
                    MySqlCommand commandUpdate;
                    try
                    {
                        commandUpdate = new MySqlCommand(queryUpdate, conn);
                        commandUpdate.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при отключении простой команды при смене статуса " + queryUpdate);
                        Reconnect();
                    }
                    return true;
                }
                else
                {
                    reader.Close();
                    return false;
                }
            }
            return false;
        }
        #endregion SimpleAnswer

        #region Party
        public List<string> GetPartyPhrase()
        {
            string query = "SELECT * FROM twitch.party";
            MySqlCommand command;
            MySqlDataReader reader = null;
            try
            {
                command = new MySqlCommand(query, conn);
                reader = command.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Упали при получении списка плохих фраз пати - БД" + query);
                Reconnect();
            }


            List<string> phrases = new List<string>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    //Если команда активна
                    if (Convert.ToBoolean(reader[1].ToString()))
                    {
                        phrases.Add(reader[0].ToString());
                    }
                }
                reader.Close();
            }

            return phrases;
        }

        public bool AddPartyPhrase(string _phrase, bool _isActive)
        {
            string queryExist = string.Format("SELECT EXISTS(SELECT phrase FROM twitch.party WHERE phrase = '{0}')", _phrase);
            MySqlCommand commandExist;
            MySqlDataReader reader = null;
            try
            {
                commandExist = new MySqlCommand(queryExist, conn);
                reader = commandExist.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Отвалились при проверке пати фразы на наличие " + queryExist);
                Reconnect();
            }
            if (reader != null)
            {
                reader.Read();
                if (Convert.ToBoolean(reader[0]))
                {
                    reader.Close();
                    string queryUpdate = string.Format("UPDATE twitch.party SET isActive = '1' WHERE phrase = '{0}'", _phrase);
                    MySqlCommand commandUpdate;
                    try
                    {
                        commandUpdate = new MySqlCommand(queryUpdate, conn);
                        commandUpdate.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при обновлении фразы " + queryUpdate);
                        Reconnect();
                    }
                    return true;
                }
                else
                {
                    reader.Close();
                    string queryInsert = string.Format("INSERT INTO twitch.party (phrase,  isActive) " +
                        "VALUES ('{0}', '{1}')", _phrase, Convert.ToInt32(_isActive));
                    MySqlCommand commandInsert;
                    try
                    {
                        commandInsert = new MySqlCommand(queryInsert, conn);
                        commandInsert.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при добавлении команды пати фразы " + queryInsert);
                        Reconnect();
                    }
                    return false;
                }
            }
            return false;
        }

        public bool DeactivatePartyPhrase(string _phrase)
        {
            string queryExist = string.Format("SELECT EXISTS(SELECT phrase FROM twitch.party WHERE phrase = '{0}')", _phrase);
            MySqlCommand commandExist;
            MySqlDataReader reader = null;
            try
            {
                commandExist = new MySqlCommand(queryExist, conn);
                reader = commandExist.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Отвалились при отключении пати фразы при проверке существовния " + queryExist);
                Reconnect();
            }
            if (reader != null)
            {
                reader.Read();
                if (Convert.ToBoolean(reader[0]))
                {
                    reader.Close();
                    string queryUpdate = string.Format("UPDATE twitch.party SET isActive = '0' WHERE phrase = '{0}'", _phrase);
                    MySqlCommand commandUpdate;
                    try
                    {
                        commandUpdate = new MySqlCommand(queryUpdate, conn);
                        commandUpdate.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при отключении пати фразы при смене статуса " + queryUpdate);
                        Reconnect();
                    }
                    return true;
                }
                else
                {
                    reader.Close();
                    return false;
                }
            }
            return false;
        }
        #endregion Party

        public void AddFollower(string followerId, string followerName, DateTime followerSince, DateTime followerCreationDate, bool notifications)
        {


            string queryInsert = string.Format("INSERT INTO twitch.followers (followerId,  followerName, followSince, followerCreationDate, notifications) " +
                "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", followerId, followerName, followerSince.ToString("yyyyMMddHHmmss"), followerCreationDate.ToString("yyyyMMddHHmmss"), Convert.ToInt32(notifications));
            MySqlCommand commandInsert;
            try
            {
                commandInsert = new MySqlCommand(queryInsert, conn);
                commandInsert.ExecuteNonQueryAsync();
            }
            catch
            {
                log.WriteLog("" + queryInsert);
                Reconnect();
            }

        }

        #region Bots
        public List<string> GetBots()
        {
            string query = "SELECT * FROM twitch.bots";
            MySqlCommand command;
            MySqlDataReader reader = null;
            try
            {
                command = new MySqlCommand(query, conn);
                reader = command.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Упали при получении списка ботов - БД" + query);
                Reconnect();
            }

            List<string> bots = new List<string>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    bots.Add(reader[0].ToString());
                }
                reader.Close();
            }

            return bots;
        }
        public bool AddBot(string _botName)
        {
            string queryExist = string.Format("SELECT EXISTS(SELECT botName FROM twitch.bots WHERE botName = '{0}')", _botName);
            MySqlCommand commandExist;
            MySqlDataReader reader = null;
            try
            {
                commandExist = new MySqlCommand(queryExist, conn);
                reader = commandExist.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Отвалились при проверке бота на наличие " + queryExist);
                Reconnect();
            }
            if (reader != null)
            {
                reader.Read();
                //если существует имя бота
                if (Convert.ToBoolean(reader[0]))
                {
                    reader.Close();
                    return true;
                }
                else
                {
                    reader.Close();
                    string queryInsert = string.Format("INSERT INTO twitch.bots (botName) " +
                        "VALUES ('{0}')", _botName);
                    MySqlCommand commandInsert;
                    try
                    {
                        commandInsert = new MySqlCommand(queryInsert, conn);
                        commandInsert.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при добавлении имени бота " + queryInsert);
                        Reconnect();
                    }
                    return false;
                }
            }
            return false;
        }

        public bool DeleteBot(string _botName)
        {
            string queryExist = string.Format("SELECT EXISTS(SELECT botName FROM twitch.bots WHERE botName = '{0}')", _botName);
            MySqlCommand commandExist;
            MySqlDataReader reader = null;
            try
            {
                commandExist = new MySqlCommand(queryExist, conn);
                reader = commandExist.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Отвалились при удалении бота при проверке существовния " + queryExist);
                Reconnect();
            }
            if (reader != null)
            {
                reader.Read();
                if (Convert.ToBoolean(reader[0]))
                {
                    reader.Close();
                    string queryUpdate = string.Format("DELETE FROM twitch.bots WHERE botName = '{0}'", _botName);
                    MySqlCommand commandUpdate;
                    try
                    {
                        commandUpdate = new MySqlCommand(queryUpdate, conn);
                        commandUpdate.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при удалении бота " + queryUpdate);
                        Reconnect();
                    }
                    return true;
                }
                else
                {
                    reader.Close();
                    return false;
                }
            }
            return false;
        }
        #endregion Bots

        #region Message
        public void WriteMessage(string messageId, string senderName, string senderId, string messageTime, string messageText, bool isBroadcaster,
                                 bool isModerator, bool isVip, bool isSubscriber, bool isHighlighted, string messageRaw)
        {
            string query = "INSERT INTO messages (messageId, senderName, senderId, messageTime, messageText, isBroadcaster," +
                "isModerator, isVip, isSubscriber, isHighlighted, messageRaw) " +
                "VALUES (@messageId, @senderName, @senderId, @messageTime, @messageText, @isBroadcaster," +
                "@isModerator, @isVip, @isSubscriber, @isHighlighted, @messageRaw)";
            try
            {
                MySqlCommand command = new MySqlCommand(query, conn);
                command.Parameters.Add("@messageId", MySqlDbType.VarChar, 45).Value = messageId;
                command.Parameters.Add("@senderName", MySqlDbType.VarChar, 25).Value = senderName;
                command.Parameters.Add("@senderId", MySqlDbType.VarChar, 45).Value = senderId;
                command.Parameters.Add("@messageTime", MySqlDbType.Timestamp).Value = messageTime;
                command.Parameters.Add("@messageText", MySqlDbType.Text).Value = messageText;
                command.Parameters.Add("@isBroadcaster", MySqlDbType.Byte).Value = isBroadcaster;
                command.Parameters.Add("@isModerator", MySqlDbType.Byte).Value = isModerator;
                command.Parameters.Add("@isVip", MySqlDbType.Byte).Value = isVip;
                command.Parameters.Add("@isSubscriber", MySqlDbType.Byte).Value = isSubscriber;
                command.Parameters.Add("@isHighlighted", MySqlDbType.Byte).Value = isHighlighted;
                command.Parameters.Add("@messageRaw", MySqlDbType.Text).Value = messageRaw;
                command.ExecuteNonQueryAsync();

            }
            catch
            {
                log.WriteLog("[Ошибка] Не удалось записать в БД: " + query);
                Reconnect();
            }
        }
        public void MarkMessageAsDeleted(string targetMessageId)
        {
            string queryUpdate = "UPDATE twitch.messages SET isDeleted ='1' WHERE messageId = '" + targetMessageId + "';";
            MySqlCommand commandUpdate;
            try
            {
                commandUpdate = new MySqlCommand(queryUpdate, conn);
                commandUpdate.ExecuteNonQueryAsync();

            }
            catch
            {
                log.WriteLog("[Ошибка] Не удалось записать в БД: " + queryUpdate);
                Reconnect();
            }
        }
        #endregion Message
        public void SetToxicity(string targetMessageId, float toxicityScore)
        {
            string querySetToxicity = "UPDATE twitch.messages SET toxicity ='" + toxicityScore.ToString().Replace(",", ".") + "' WHERE messageId = '" + targetMessageId + "';";
            MySqlCommand commandUpdate;
            try
            {
                commandUpdate = new MySqlCommand(querySetToxicity, conn);
                commandUpdate.ExecuteNonQueryAsync();

            }
            catch
            {
                log.WriteLog("[Ошибка] Не удалось записать в БД: " + querySetToxicity);
                Reconnect();
            }
        }

        #region Copypaste
        public List<string> GetCopypastes()
        {
            string query = "SELECT * FROM twitch.copypaste";
            MySqlCommand command;
            MySqlDataReader reader = null;
            try
            {
                command = new MySqlCommand(query, conn);
                reader = command.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Упали при получении списка копипаст - БД" + query);
                Reconnect();
            }


            List<string> copypastes = new List<string>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    //Если копипаста активна
                    if (Convert.ToBoolean(reader[1].ToString()))
                    {
                        copypastes.Add(reader[0].ToString());
                    }
                }
                reader.Close();
            }

            return copypastes;
        }

        public bool AddCopypaste(string _copypasteText, bool _isActive)
        {
            string queryExist = string.Format("SELECT EXISTS(SELECT copypasteText FROM twitch.copypaste WHERE copypasteText = '{0}')", _copypasteText);
            MySqlCommand copypasteExist;
            MySqlDataReader reader = null;
            try
            {
                copypasteExist = new MySqlCommand(queryExist, conn);
                reader = copypasteExist.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Отвалились при проверке копипасты на наличие " + queryExist);
                Reconnect();
            }
            if (reader != null)
            {
                reader.Read();
                if (Convert.ToBoolean(reader[0]))
                {
                    reader.Close();
                    string queryUpdate = string.Format("UPDATE twitch.copypaste SET isActive = '1' WHERE copypasteText = '{0}'", _copypasteText);
                    MySqlCommand copypasteUpdate;
                    try
                    {
                        copypasteUpdate = new MySqlCommand(queryUpdate, conn);
                        copypasteUpdate.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при обновлении копипасты " + queryUpdate);
                        Reconnect();
                    }
                    return true;
                }
                else
                {
                    reader.Close();
                    string queryInsert = string.Format("INSERT INTO twitch.copypaste (copypasteText,  isActive) " +
                        "VALUES ('{0}', '{1}')", _copypasteText, Convert.ToInt32(_isActive));
                    MySqlCommand commandInsert;
                    try
                    {
                        commandInsert = new MySqlCommand(queryInsert, conn);
                        commandInsert.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при добавлении копипасты " + queryInsert);
                        Reconnect();
                    }
                    return false;
                }
            }
            return false;
        }

        public bool DeactivateCopypaste(string _copypasteText)
        {
            string queryExist = string.Format("SELECT EXISTS(SELECT copypasteText FROM twitch.copypaste WHERE copypasteText = '{0}')", _copypasteText);
            MySqlCommand copypasteExist;
            MySqlDataReader reader = null;
            try
            {
                copypasteExist = new MySqlCommand(queryExist, conn);
                reader = copypasteExist.ExecuteReader();
            }
            catch
            {
                log.WriteLog("Отвалились при отключении копипасты при проверке существовния " + queryExist);
                Reconnect();
            }
            if (reader != null)
            {
                reader.Read();
                if (Convert.ToBoolean(reader[0]))
                {
                    reader.Close();
                    string queryUpdate = string.Format("UPDATE twitch.copypaste SET isActive = '0' WHERE copypasteText = '{0}'", _copypasteText);
                    MySqlCommand copypasteUpdate;
                    try
                    {
                        copypasteUpdate = new MySqlCommand(queryUpdate, conn);
                        copypasteUpdate.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        log.WriteLog("Отвалились при отключении копипасты при смене статуса " + queryUpdate);
                        Reconnect();
                    }
                    return true;
                }
                else
                {
                    reader.Close();
                    return false;
                }
            }
            return false;
        }

        #endregion Copypaste

    }
}
