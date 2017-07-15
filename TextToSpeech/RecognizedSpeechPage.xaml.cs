// -
// <copyright file="RecognizedSpeechPage.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.UserData;

namespace TextToSpeech
{
    public partial class RecognizedSpeechPage : PhoneApplicationPage
    {
        public RecognizedSpeechPage()
        {
            InitializeComponent();
        }
        private const FilterKind ContactFilterKind = FilterKind.None;
        private static readonly object ContactState = "Contacts Test #1";
        protected override void OnNavigatedTo(NavigationEventArgs e) 
        {
           // this.RecognitionDetailsText.Text = App.RecognizedTextObject;
            if (!string.IsNullOrEmpty(App.RecognizedTextObject))
                SearchContact(App.RecognizedTextObject);
        }
        public void SearchContact(string search)
        {
            //var contact = new SaveContactTask();
            //return contact.HomePhone;
            var contact = new Contacts();
            
            contact.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(Contacts_SearchCompleted);

            contact.SearchAsync(search, ContactFilterKind, ContactState);
            
        }

        void Contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            try
            {
                //Bind the results to the list box that displays them in the UI
                ContactResultsData.DataContext = e.Results;
            }
            catch (System.Exception)
            {
                //That's okay, no results
            }

            ContactResultsLabel.Text = ContactResultsData.Items.Count > 0 ? "Tap name for action...)" : "no results";
        }

        private void ContactResultsData_Tap(object sender, GestureEventArgs e)
        {
            App.Contact = ((sender as ListBox).SelectedValue as Contact);

            if (App.Contact != null)
            {
                NavigationService.GoBack();
                
            }
        }
    }
    public class ContactPictureConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var c = value as Contact;
            if (c == null) return null;

            System.IO.Stream imageStream = c.GetPicture();
            if (null != imageStream)
            {
                return Microsoft.Phone.PictureDecoder.DecodeJpeg(imageStream);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }//End converter class
}