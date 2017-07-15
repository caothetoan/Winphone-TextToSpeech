using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Hawaii.Services.Client;
using Hawaii.Services.Client.Speech;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;
using SpeechRecognitionTestClient;
using TextToSpeech.ServiceReference1;
using Microsoft.Xna.Framework;
using System.Net;
using Microsoft.Xna.Framework.Audio;

namespace TextToSpeech
{
    public partial class wpText2Speech : PhoneApplicationPage
    {
        // appId register with bing API at https://ssl.bing.com/webmaster/Developers/CreateAppId
        string AppId = "6977431160E0F05510746145006568DB40EFE78F";
           /// <summary>
        /// Field to store the available grammars.
        /// </summary>
        private List<string> availableGrammars;

        protected bool FirstFocusSpeechText = true; 
      
        public wpText2Speech()
        {
             InitializeComponent();
          
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //GetLanguageTextToSpeech();
                ProcessPageSpeechToText();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void ProcessPageSpeechToText()
        {
            if (this.VerifyHawaiiAppId())
            {
                this.AudioStream = new MemoryStream();
                this.MicroPhone = Microphone.Default;
                this.IsSoundPlaying = false;

                // Timer to simulate the XNA Framework game loop (Microphone is 
                // from the XNA Framework). We also use this timer to monitor the 
                // state of audio playback so we can update the UI appropriately.
                DispatcherTimer dispatchTimer = new DispatcherTimer();
                dispatchTimer.Interval = TimeSpan.FromMilliseconds(33);
                dispatchTimer.Tick += new EventHandler(this.DispatcherTimer_Tick);
                dispatchTimer.Start();

                // Event handler for getting audio data when the buffer is full
                this.MicroPhone.BufferReady += new EventHandler<EventArgs>(this.Microphone_BufferReady);
               
                SetSpeechGrammarListBoxDefault();
               
            }
        }
        private void GetGrammarSpeech()
        {
            this.SpeechDomainsList.Visibility = Visibility.Collapsed;
            SpeechService.GetGrammarsAsync(
                   HawaiiClient.HawaiiApplicationId,
                   (result) => this.Dispatcher.BeginInvoke(() => this.OnSpeechGrammarsReceived(result)));

            this.RetrievingGrammarsLabel.Visibility = Visibility.Visible;
            this.RecognizingProgress.Visibility = Visibility.Visible;
        }
        void GetLanguageTextToSpeech()
        {          
            FrameworkDispatcher.Update();

            var objTranslator = new ServiceReference1.LanguageServiceClient();
            objTranslator.GetLanguagesForSpeakCompleted += new EventHandler<GetLanguagesForSpeakCompletedEventArgs>(translator_GetLanguagesForSpeakCompleted);
            objTranslator.GetLanguagesForSpeakAsync(AppId, objTranslator);
            this.LanguageProgress.Visibility = Visibility.Visible;
        }

      
        void translator_GetLanguagesForSpeakCompleted(object sender, GetLanguagesForSpeakCompletedEventArgs e)
        {
            var objTranslator = e.UserState as ServiceReference1.LanguageServiceClient;
            objTranslator.GetLanguageNamesCompleted += new EventHandler<GetLanguageNamesCompletedEventArgs>(translator_GetLanguageNamesCompleted);
            objTranslator.GetLanguageNamesAsync(AppId, "en", e.Result, e.Result);
            this.LanguageProgress.Visibility = Visibility.Collapsed;
        }

        void translator_GetLanguageNamesCompleted(object sender, GetLanguageNamesCompletedEventArgs e)
        {
            var codes = e.UserState as ObservableCollection<string>;
            var names = e.Result;
            var languagesData = (from code in codes
                             let cindex = codes.IndexOf(code)
                             from name in names
                             let nindex = names.IndexOf(name)
                             where cindex == nindex
                             select new TranslatorLanguage()
                             {
                                 Name = name,
                                 Code = code
                             }).ToArray();
            this.Dispatcher.BeginInvoke(() =>
            {
                this.ListLanguages.ItemsSource = languagesData;
            });
            
        }
        // speak text to speech
        private void btnSpeak_Click(object sender, RoutedEventArgs e)
        {
                var languageCode = "en";
                var language = this.ListLanguages.SelectedItem as TranslatorLanguage;
                if (language != null)
                {
                    languageCode = language.Code;
                }
                this.SpeakProgress.Visibility = Visibility.Visible;
                var objTranslator = new ServiceReference1.LanguageServiceClient();
                objTranslator.SpeakCompleted += translator_SpeakCompleted;
                objTranslator.SpeakAsync(AppId, this.TextToSpeachText.Text, languageCode, "audio/wav");

              //  panoSpeech.DefaultItem = panoSpeech.Items[(int)2];           
        }
     
        void translator_SpeakCompleted(object sender, ServiceReference1.SpeakCompletedEventArgs e)
        {
            this.SpeakProgress.Visibility = Visibility.Collapsed;
            RecognizingProgress.Visibility = Visibility.Collapsed;
            var client = new WebClient();
            client.OpenReadCompleted += ((s, args) =>
            {
                SoundEffect se = SoundEffect.FromStream(args.Result);
                SoundEffect.MasterVolume = 0.8f;
                se.Play();
            });
            client.OpenReadAsync(new Uri(e.Result));
            
        }
      
        /// <summary>
        /// Delegate definition for SetRecognizedText.
        /// </summary>
        /// <param name="recognizedTexts">
        /// List of recognized results.
        /// </param>
        private delegate void SetRecognizedTextDelegate(List<string> recognizedTexts);

        /// <summary>
        /// Delegate definition for SetGrammars.
        /// </summary>
        private delegate void SetGrammarsDelegate();

        /// <summary>
        /// Gets or sets the object representing the physical microphone on the device.
        /// </summary>
        private Microphone MicroPhone { get; set; }

        /// <summary>
        /// Gets or sets dynamic buffer to retrieve audio data from the microphone.
        /// </summary>
        private byte[] AudioBuffer { get; set; }

        /// <summary>
        /// Gets or sets the audio data for later playback.
        /// </summary>
        private MemoryStream AudioStream { get; set; }

        /// <summary>
        /// Gets or sets the SoundEffect class we need to instantiate the SoundInstance.
        /// </summary>
        private SoundEffect SoundEffect { get; set; }

        /// <summary>
        /// Gets or sets the sound instance to play back audio.
        /// </summary>
        private SoundEffectInstance SoundInstance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sound is playing.
        /// </summary>
        private bool IsSoundPlaying { get; set; }

        private bool VerifyHawaiiAppId()
        {
            if (!String.IsNullOrEmpty(HawaiiClient.HawaiiApplicationId))
            {
                return true;
            }
            else
            {
                this.HawaiiAppIdErrorArea.Visibility = Visibility.Visible;
                this.ContentPanel.Visibility = Visibility.Collapsed;
                this.ApplicationBar.IsVisible = false;

                return false;
            }
        }

        /// <summary>
        /// Updates the XNA FrameworkDispatcher and checks to see if a sound is playing.
        /// If sound has stopped playing, it updates the UI.
        /// </summary>
        /// <param name="sender">
        /// Sender of this event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                FrameworkDispatcher.Update();
            }
            catch
            {
            }

            if (true == this.IsSoundPlaying)
            {
                if (this.SoundInstance.State != SoundState.Playing)
                {
                    // Audio has finished playing
                    this.IsSoundPlaying = false;

                    // Update the UI to reflect that the 
                    // sound has stopped playing
                    this.SetButtonStates(true, false, true, true);
                }
                
            }

            //if ( this.MicroPhone.State == MicrophoneState.Started)
            //{
            //    // In RECORD mode, microphone started capture audio 
            //    // and sound instance is stopped
            //    this.MicroPhone.Stop();
            //    RecognizeSpeech();
            //}
        }

        /// <summary>
        /// Event handler for buffer ready.
        /// </summary>
        /// <param name="sender">
        /// Sender of this event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void Microphone_BufferReady(object sender, EventArgs e)
        {
            // Retrieve audio data
            this.MicroPhone.GetData(this.AudioBuffer);

            // Store the audio data in a stream
            this.AudioStream.Write(this.AudioBuffer, 0, this.AudioBuffer.Length);
        }

        /// <summary>
        /// Record button click event handler.
        /// </summary>
        /// <param name="sender">
        /// Sender of this event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void RecordButton_Click(object sender, EventArgs e)
        {
            RecordSpeech();
        }
        void RecordSpeech()
        {
            // Get audio data in 1/2 second chunks
            this.MicroPhone.BufferDuration = TimeSpan.FromMilliseconds(500);

            // Allocate memory to hold the audio data
            this.AudioBuffer = new byte[this.MicroPhone.GetSampleSizeInBytes(this.MicroPhone.BufferDuration)];

            // Set the stream back to zero in case there is already something in it
            this.AudioStream.SetLength(0);

            // Start recording
            this.MicroPhone.Start();

            this.SetButtonStates(false, true, false, false);
        }
        /// <summary>
        /// Stop button click event.
        /// </summary>
        /// <param name="sender">
        /// Sender of this event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void StopButton_Click(object sender, EventArgs e)
        {
            if (this.MicroPhone.State == MicrophoneState.Started)
            {
                // In RECORD mode, user clicked the 
                // stop button to end recording
                this.MicroPhone.Stop();
            }
            else if (this.SoundInstance.State == SoundState.Playing)
            {
                // In PLAY mode, user clicked the 
                // stop button to end playing back
                this.SoundInstance.Stop();
            }

            this.SetButtonStates(true, false, true, true);
            RecognizeSpeech();
        }
        void RecognizeSpeech()
        {
            //if (this.availableGrammars == null ||
            //   this.availableGrammars.Count == 0)
            //{
            //    return;
            //}

            this.RecognizingProgress.Visibility = Visibility.Visible;

            if (this.AudioStream != null && this.AudioStream.Length != 0)
            {
                SpeechService.RecognizeSpeechAsync(
                    HawaiiClient.HawaiiApplicationId,
                  SpeechService.DefaultGrammar,
                this.AudioStream.ToArray(),
                (result) => Dispatcher.BeginInvoke(() => this.OnSpeechRecognitionCompleted(result)));
            }
            else
            {
                MessageBox.Show("Invalid speech buffer found. Record speech and try again.", "Error", MessageBoxButton.OK);
            }
        }
        /// <summary>
        /// Play button click event.
        /// </summary>
        /// <param name="sender">
        /// Sender of this event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (this.AudioStream.Length > 0)
            {
                // Update the UI to reflect that sound is playing
                this.SetButtonStates(false, true, false, false);

                // Play the audio in a new thread so the UI can update.
                Thread soundThread = new Thread(new ThreadStart(this.PlaySound));
                soundThread.Start();
            }
        }

        /// <summary>
        /// Helper method to play sound.
        /// </summary>
        private void PlaySound()
        {
            if (this.SoundInstance != null)
            {
                this.SoundInstance.Dispose();
                this.SoundInstance = null;
            }
            
            if (this.SoundEffect != null)
            {
                this.SoundEffect.Dispose();
                this.SoundEffect = null;
            }

            // Play audio using SoundEffectInstance so we can monitor it's State 
            // and update the UI in the dt_Tick handler when it is done playing.
            this.SoundEffect = new SoundEffect(this.AudioStream.ToArray(), this.MicroPhone.SampleRate, AudioChannels.Mono);
            this.SoundInstance = this.SoundEffect.CreateInstance();
            this.SoundInstance.Play();
            this.IsSoundPlaying = this.SoundInstance.State == SoundState.Playing;
        }

        /// <summary>
        /// Enable or disable buttons in application bar.
        /// </summary>
        /// <param name="recordEnabled">
        /// Flag specifies whether record button is enable.
        /// </param>
        /// <param name="stopEnabled">
        /// Flag specifies whether stop button is enable.
        /// </param>
        /// <param name="playEnabled">
        /// Flag specifies whether play button is enable.
        /// </param>
        /// <param name="sendEnabled">
        /// Flag specifies whether send button is enable.
        /// </param>
        private void SetButtonStates(bool recordEnabled, bool stopEnabled, bool playEnabled, bool sendEnabled)
        {
            //(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = recordEnabled;
            //(ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = stopEnabled;
            //(ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = playEnabled;
            //(ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = sendEnabled;
            recordButton.IsEnabled = recordEnabled;
            stopButton.IsEnabled = stopEnabled;
            playButton.IsEnabled = playEnabled;
           // clearListButton.IsEnabled = sendEnabled;
        }

        /// <summary>
        /// Speech Grammars Received handler.
        /// </summary>
        /// <param name="result">
        /// Service Result.
        /// </param>
        private void OnSpeechGrammarsReceived(SpeechServiceResult result)
        {
            Debug.Assert(result != null, "result is null");

            this.RecognizingProgress.Visibility = Visibility.Collapsed;
            this.RetrievingGrammarsLabel.Visibility = Visibility.Collapsed;
            this.SetButtonStates(true, false, false, false);
            
            this.SpeechDomainsList.Visibility = Visibility.Visible;
            if (result.Status == Status.Success)
            {
                               
                this.availableGrammars = result.SpeechResult.Items;
                this.SetSpeechGrammarsListBox();
               
            }
            else
            {
                MessageBox.Show("Error receiving available speech grammars.", "Error", MessageBoxButton.OK);
               // this.NoGrammarsLabel.Visibility = Visibility.Visible;
               
            }
        }
        void SetSpeechGrammarListBoxDefault()
        {
            this.SpeechDomainsList.Items.Clear();
            this.SetButtonStates(true, false, false, false);
            this.SpeechDomainsList.Visibility = Visibility.Visible;

            this.SpeechDomainsList.Items.Add(SpeechService.DefaultGrammar);
        }
        /// <summary>
        /// Recognize button click event handler.
        /// </summary>
        /// <param name="sender">
        /// Sender of this event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void RecognizeButton_Click(object sender, EventArgs e)
        {
           RecognizeSpeech();
        }

        /// <summary>
        /// Speech recognition completed handler.
        /// </summary>
        /// <param name="speechResult">
        /// Service result.
        /// </param>
        private void OnSpeechRecognitionCompleted(SpeechServiceResult speechResult)
        {
            Debug.Assert(speechResult != null, "speechResult is null");

            this.RecognizingProgress.Visibility = Visibility.Collapsed;

            if (speechResult.Status == Status.Success)
            {
                this.SetRecognizedTextListBox(speechResult.SpeechResult.Items);
            }
            else
            {
                MessageBox.Show(
                    speechResult.Exception == null ? "Error recognizing the speech." : speechResult.Exception.Message,
                    "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Delegate method to populate recognition results in the list box.
        /// </summary>
        /// <param name="recognitionResultStrings">
        /// List of recognition results.
        /// </param>
        private void SetRecognizedTextListBox(List<string> recognitionResultStrings)
        {
            string result;
            if (recognitionResultStrings != null)
            {
                if (recognitionResultStrings.Count == 0)
                {
                    result = "Empty recognized text is received.";
                    
                    RecognizedStringListBox.Items.Add(result);
                    result = "I didn't heard your voice.";
                    SpeakFromText(result);
                }
                else
                {
                    if (recognitionResultStrings.Count == 1)
                    {
                        //recognitionResultStrings.ForEach((item) => this.RecognizedStringListBox.Items.Add(item));
                       
                        result = recognitionResultStrings[0];
                        this.RecognizedStringListBox.Items.Add(result);
                        AnswerFromText(result);
                    }
                    else
                    {
                        result = "Do you mean";
                        foreach (var item in recognitionResultStrings)
                        {
                            this.RecognizedStringListBox.Items.Add(item);
                            result += " or " + item;
                        }
                        SpeakFromText(result);
                    }
                       
                }
                
            }
            else
            {
                result = "Sorry, unable to recognize speech";
                this.RecognizedStringListBox.Items.Add(result);
            }
        }

        /// <summary>
        /// Delegate method to populate the grammars in the list box.
        /// </summary>
        private void SetSpeechGrammarsListBox()
        {
            if (this.availableGrammars == null)
            {
                return;
            }

            this.SpeechDomainsList.Items.Clear();

            this.availableGrammars.ForEach((item) => this.SpeechDomainsList.Items.Add(item));
           
        }

        /// <summary>
        /// Navigates to the Recognition details page when a recognition string 
        /// is selected from the list box of matches.
        /// </summary>
        /// <param name="sender">
        /// Sender object.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void RecognizedStringListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null)
            {
                return;
            }
            else if (e.AddedItems.Count == 0)
            {
                return;
            }
            else
            {
                IList selectedItems = e.AddedItems;
                App.RecognizedTextObject = selectedItems.OfType<string>().FirstOrDefault();

               // NavigationService.Navigate(new Uri("/RecognizedSpeechPage.xaml", UriKind.Relative));
               
               // SpeakFromText(App.RecognizedTextObject);
              //  AnswerFromText(App.RecognizedTextObject);
                RecognizedStringListBox.SelectedItem = null;
            }
        }
        // speak text to speech
        protected void SpeakFromText(string text)
        {
            RecognizingProgress.Visibility = Visibility.Visible;
            var languageCode = "en";
            var language = this.ListLanguages.SelectedItem as TranslatorLanguage;
            if (language != null)
            {
                languageCode = language.Code;
            }
            var objTranslator = new ServiceReference1.LanguageServiceClient();
            objTranslator.SpeakCompleted += translator_SpeakCompleted;
            objTranslator.SpeakAsync(AppId, text, languageCode, "audio/wav");
        }
        

        /// <summary>
        /// Clear all menu item menu click event handler.
        /// </summary>
        /// <param name="sender">
        /// Sender of this event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void ClearAllMenuItem_Click(object sender, EventArgs e)
        {
            if(RecognizedStringListBox.Items.Count() > 0) RecognizedStringListBox.Items.Clear();
        }
        private void ClearTextboxSpeech()
        {
            TextToSpeachText.Text = "";
        }
        private void TextToSpeachText_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (FirstFocusSpeechText)
            {
                ClearTextboxSpeech();
                FirstFocusSpeechText = false;
            }
        }
        private void TextToSpeech_Click(object sender, EventArgs e)
        {
            ClearTextboxSpeech();
        }
        private void btnGetLanguage_Click(object sender, RoutedEventArgs e)
        {
            GetLanguageTextToSpeech();
        }

        private void GrammarButton_Click(object sender, RoutedEventArgs e)
        {
            GetGrammarSpeech();
        }
        // speak answer text
        protected void AnswerFromText(string text)
        {
       
            Response(text);
            
        }
        public void Response(string stInput)
        {
            // stInput = stInput.Trim();
          
            if (!string.IsNullOrEmpty(stInput))
            {
                //parseEmbeddedOutputCommands(stInput);
                int spaceIndex = stInput.IndexOf(" ");

                string function;
                string args;
                if (spaceIndex == -1)
                {
                    function = stInput.ToLower();
                    args = "";
                }
                else
                {
                    function = stInput.Substring(0, spaceIndex).ToLower();
                    args = stInput.Substring(spaceIndex + 1);
                }
                CallPhoneTask(function, args);              
            }
           
        }

        public void CallPhoneTask(string cmd, string name)
        {
            string ret = cmd + " " + name;
            App.RecognizedTextObject = name;

            switch (cmd)
            {
                case Constant.Call:
                    NavigationService.Navigate(new Uri("/RecognizedSpeechPage.xaml", UriKind.Relative));
                    
                    PhoneTask.OnOpenCall(name);
                    break;
                case Constant.Sms:
                    PhoneTask.OnOpenSms(name);
                    break;
                case Constant.Quit:
                case Constant.Exit:
                    PhoneTask.OnQuitApplication();
                    break;
                case Constant.Email:
                    PhoneTask.OnOpenEmail(name);
                    break;
                case Constant.Browser:
                    PhoneTask.OnOpenBrowser(name);
                    break;
                case Constant.Run:
                case Constant.Open:
                    PhoneTask.OpenApp(name);
                    break;
                case Constant.Connection:
                    PhoneTask.OpenConnectionSettings();
                    break;
                case Constant.Camera:
                    PhoneTask.OpenConnectionSettings();
                    break;
                case Constant.Search:
                    PhoneTask.OnSearch(name);
                    break;
                case Constant.Map:
                    PhoneTask.OnOpenMap(name);
                    break;
                default:
                    ret = Verbot.GetReply(ret);
                    SpeakFromText(ret);
                    break;
            }
            
        }
       
    }

    public class TranslatorLanguage
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}