using System;
using System.Windows;

namespace TextToSpeech
{
    public class Verbot
    {
        #region private member
        //private static readonly string[,] KnowledgeBase = {
        //                                                      {
        //                                                          "WHAT IS YOUR NAME",
        //                                                          "I'm Voice Speech for Windows Phone 7."
        //                                                      },

        //                                                      {
        //                                                          "HI",
        //                                                          "Hi. Have a nice day!"
        //                                                      },

        //                                                      {
        //                                                          "HOW ARE YOU",
        //                                                          "I'm fine. Thank you!"
        //                                                      },

        //                                                      {
        //                                                          "WHO ARE YOU",
        //                                                          "I'M AN A.I PROGRAM."
        //                                                      },
        //                                                      {
        //                                                          "ARE YOU INTELLIGENT",
        //                                                          "YES,OFCORSE."
        //                                                      },
        //                                                      {
        //                                                          "ARE YOU REAL",
        //                                                          "DOES THAT QUESTION REALLY MATERS TO YOU?"
        //                                                      },
        //                                                      {
        //                                                          "I LIKE YOU",
        //                                                          "for love or for money"
        //                                                      },
        //                                                      {
        //                                                          "MAKE FRIEND WITH ME",
        //                                                          "we belong together"
        //                                                      },
        //                                                      {
        //                                                          "HOW ABOUT WEATHER", 
        //                                                          "open weather app"
        //                                                      }
        //                                                  };
        private static readonly string[] QuestionBase = new string[]{
                                    "WHAT IS YOUR NAME", "HI",
                                    "HOW ARE YOU",
                                    "WHO ARE YOU",
                                    "ARE YOU INTELLIGENT",
                                    "ARE YOU REAL",
                                    "I LIKE YOU",
                                    "MAKE FRIEND WITH ME",
                                    "HOW ABOUT WEATHER"
                                };

        private static readonly string[] KnowledgeBase = new string[]
                                  {
                                      "I'm Voice Speech for Windows Phone 7.",
                                      "Hi. Have a nice day!",
                                      "I'm fine. Thank you!",
                                      "I'M AN A.I PROGRAM.",
                                      "YES,OFCORSE.",
                                      "DOES THAT QUESTION REALLY MATERS TO YOU?",
                                      "For love or for money",
                                      "We belong together",
                                      "open weather app"
                                  };
        #endregion
        public static string GetReply(string question)
        {
            string result = "";
           
            //for (int i = 0; i < KnowledgeBase.GetUpperBound(0); ++i)
            //{
            //    string source = KnowledgeBase[i, 0].ToLower();
            //    if (source.IndexOf(question) != -1)
            //    {
            //        result = KnowledgeBase[i, 1];
            //        break;
            //    }
            //}
            int length = QuestionBase.Length;
            for (int i = 0; i < length; ++i)
            {
                if (question.IndexOf(QuestionBase[i].ToLower()) != -1)
                {
                    result = KnowledgeBase[i];
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(result))
            {
                const string strDonotknow = "I don't know ";
                return strDonotknow + question;
            }

            return result;
        }
      

        //private void parseEmbeddedOutputCommands(string text)
        //{
        //    string startCommand = "<";
        //    string endCommand = ">";

        //    int start = text.IndexOf(startCommand);
        //    int end = -1;

        //    while (start != -1)
        //    {
        //        end = text.IndexOf(endCommand, start);
        //        if (end != -1)
        //        {
        //            string command = text.Substring(start + 1, end - start - 1).Trim();
        //            if (command != "")
        //            {
        //                Response(command);
        //            }
        //        }
        //        start = text.IndexOf(startCommand, start + 1);
        //    }
        //}//parseEmbeddedOutputCommands(string text)
       
        //private void runAnotherApp(string command)
        //{
        //    try
        //    {
        //       if(!string.IsNullOrEmpty(command))
        //       {
        //          string[] pieces = this.splitOnFirstUnquotedSpace(command);

        //           OpenApp(pieces[0]);
                      
        //       }
               
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message);
        //    }
        //}//runProgram(string filename, string args)

        //public string[] splitOnFirstUnquotedSpace(string text)
        //{
        //    var pieces = new string[2];
        //    int index = -1;
        //    bool isQuoted = false;
        //    //find the first unquoted space
        //    for (int i = 0; i < text.Length; i++)
        //    {
        //        if (text[i] == '"')
        //            isQuoted = !isQuoted;
        //        else if (text[i] == ' ' && !isQuoted)
        //        {
        //            index = i;
        //            break;
        //        }
        //    }

        //    //break up the string
        //    if (index != -1)
        //    {
        //        pieces[0] = text.Substring(0, index);
        //        pieces[1] = text.Substring(index + 1);
        //    }
        //    else
        //    {
        //        pieces[0] = text;
        //        pieces[1] = "";
        //    }

        //    return pieces;
        //}//splitOnFirstUnquotedSpace(string text)
    }
}
