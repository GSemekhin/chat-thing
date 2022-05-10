using System;
using System.Collections.Generic;
using System.Text;

namespace BotTW
{
    class TextCheck
    {
        List<string> copypastes;
        DB database;

        public TextCheck(DB _database)
        {
            database = _database;

        }

        public void CheckText(string text)
        {


        }

        private List<string> GetCopypateList()
        {
            return database.GetCopypastes();
        }
        private void AddCopypaste(string _copypasteText)
        {
            copypastes.Add(_copypasteText);
            database.AddCopypaste(_copypasteText, true);
        }

        private void DeleteCopypaste(string _copypasteText)
        {
            copypastes.Remove(_copypasteText);
            database.DeactivateCopypaste(_copypasteText);
        }
    }
}
