using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;

namespace TextToSpeech
{
    public class PhoneTask
    {
       
        public static void OnQuitApplication()
        {
          //  var phone = new PhoneTask();
           
        }
        public static void Vibrator()
        {
            
        }
       public static bool IsNumeric (System.Object Expression)
        {
            if(Expression == null || Expression is DateTime)
                return false;

            if(Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;
  
            try
            {
                if(Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                    return true;
                }
            catch {} // just dismiss errors but return false
                return false;
        }
        protected static string GetPhoneNumber(string name)
        {
            string number; 
            if (IsNumeric(name))
            {
                number = name;

            }
            else
            {

                if (App.Contact != null)
                {
                    number = App.Contact.PhoneNumbers.ToString();
                }
                else number = "900";
            }
            return number;
        }
        public static void OnOpenCall(string name)
        {
            string number = GetPhoneNumber(name);
            //App.Contact.DisplayName, App.Contact.PhoneNumbers.ToString()
            var phoneCallTask = new PhoneCallTask
            {
                DisplayName = name,

                PhoneNumber = number

            };
            phoneCallTask.Show();
        }

        public static void OnOpenSms(string name)
        {
            string number = GetPhoneNumber(name);
            var smsComposeTask = new SmsComposeTask

            {

                To = number,

                Body = ""

            };
            smsComposeTask.Show();

        }

        public static void OnOpenEmail(string name)
        {

            var emailComposeTask = new EmailComposeTask

            {

                To = "caothetoan@gmail.com",

                Subject = "Windows Phone 7 Email",

                Body = "Looking good!"

            };
            emailComposeTask.Show();
        }
        public static void OnSearch(string name)
        {
            
            var searchTask = new SearchTask
            {
                SearchQuery = name

            };
            searchTask.Show();
        }
        public static void OnOpenMap(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                name = "18 Tam trinh, Ha noi, Vietnam";
            }

           // var map = new BingMapsTask();
                        
            var searchTask = new SearchTask
            {
                SearchQuery = name

            };
            searchTask.Show();
        }

        public static void OnOpenBrowser(string name)
        {
            if (string.IsNullOrEmpty(name)) name = "go.vn";
            var webBrowserTask = new WebBrowserTask {Uri = new Uri(name)};
            webBrowserTask.Show();
        }
        public static void OpenApp(string name)
        {
           
        }
        public static void OpenConnectionSettings()
        {
            var connection = new ConnectionSettingsTask();
            connection.Show();
            
        }
        public static void OpenCamera()
        {
            var camera = new CameraCaptureTask();
            camera.Show();
        }
    }
}
