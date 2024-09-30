using System.Diagnostics;

namespace iOSTranslationArrangeBug {
    public partial class App : Application {

        CAbsoluteLayout scrollWindow;
        CAbsoluteLayout scrollContent;
        
        public App() {

            ContentPage mainPage = new();
            MainPage = mainPage;

            CAbsoluteLayout dummyAbs = new();
            mainPage.Content = dummyAbs;

            scrollWindow = new();
            scrollWindow.StyleId = "Scroll Window";
            scrollWindow.BackgroundColor = Colors.DarkSlateBlue;
            dummyAbs.Add(scrollWindow);

            scrollContent = new();
            scrollContent.StyleId = "Scroll Content";
            scrollContent.BackgroundColor = Colors.AliceBlue;
            scrollWindow.Add(scrollContent);

            for (int i=0; i<3; i++) {
                
                CAbsoluteLayout absChild = new();
                absChild.StyleId = "Child Abs " + i.ToString();
                absChild.HeightRequest = absChild.WidthRequest = 100;
                absChild.BackgroundColor = Colors.Aqua;
                scrollContent.Add(absChild);
                
                Border border = new();
                border.StrokeThickness = 5;
                border.BackgroundColor = Colors.GreenYellow;
                border.HeightRequest = border.WidthRequest = 100;
                absChild.Add(border);

            }

            var timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1 / 60f);
            DateTime dateTime = DateTime.Now;
            timer.Tick += delegate {
                double oscPeriod = 2; //seconds for oscillatorperiod
                DateTime dateTimeNow = DateTime.Now;
                double seconds = (dateTimeNow - dateTime).TotalSeconds;
                double oscPos = Math.Sin(3.14159 * 2 * (seconds / oscPeriod));
                double translationRange = 200;
                double translationY = translationRange * oscPos;
                scrollContent.TranslationY = translationY;
                Debug.WriteLine("TRANSLATION CHANGED " + translationY);
            };
            timer.Start();


            mainPage.SizeChanged += delegate {
                if (mainPage.Width > 0) {
                    scrollWindow.WidthRequest = mainPage.Width;
                    scrollWindow.HeightRequest = mainPage.Height;
                    scrollContent.WidthRequest = mainPage.Width;
                    scrollContent.HeightRequest = mainPage.Height * 0.78;
                }
            };
        }
    }
}
