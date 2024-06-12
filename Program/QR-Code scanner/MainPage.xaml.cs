using Microsoft.Maui.Controls;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using System;
using System.Text.RegularExpressions;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

namespace QR_Code_scanner
{
    public partial class MainPage : ContentPage
    {
        private bool isPopupOpen = false;

        public MainPage()
        {
            InitializeComponent();
            Console.WriteLine("MainPage initialized.");
            ConfigureBarcodeReader();
        }

        private void ConfigureBarcodeReader()
        {
            cameraBarcodeReaderView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.All,
                AutoRotate = true,
                Multiple = false
            };
            Console.WriteLine("Barcode reader configured.");
        }

        private async void BarcodeDetected(object sender, BarcodeDetectionEventArgs e)
        {
            Console.WriteLine("Barcode detected.");
            if (isPopupOpen)
            {
                Console.WriteLine("Popup already open, returning.");
                return;
            }

            isPopupOpen = true;

            Device.BeginInvokeOnMainThread(async () =>
            {
                foreach (var barcode in e.Results)
                {
                    string barcodeValue = barcode.Value;
                    Console.WriteLine($"Detected barcode value: {barcodeValue}");

                    if (IsUrl(barcodeValue))
                    {
                        Console.WriteLine("Barcode is a URL.");
                        bool openLink = await DisplayCustomAlert("QR Code Detected", $"Möchtest du diese Seite öffnen?\n{barcodeValue}", "Ja", "Nein", "Kopieren", barcodeValue);
                        if (openLink)
                        {
                            Console.WriteLine("Opening URL.");
                            await Launcher.Default.OpenAsync(barcodeValue);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Barcode is not a URL.");
                        await DisplayCustomAlert("QR Code Detected", barcodeValue, "OK", null, "Kopieren", barcodeValue);
                    }
                }

                isPopupOpen = false;
            });
        }

        private bool IsUrl(string text)
        {
            bool isUrl = Uri.TryCreate(text, UriKind.Absolute, out Uri uriResult) &&
                         (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            Console.WriteLine($"Is URL check for '{text}': {isUrl}");
            return isUrl;
        }

        private TaskCompletionSource<bool> tcs;

        private async Task<bool> DisplayCustomAlert(string title, string message, string accept, string cancel, string copy, string textToCopy)
        {
            tcs = new TaskCompletionSource<bool>();

            var layout = new StackLayout
            {
                Padding = new Thickness(20),
                BackgroundColor = Color.FromArgb("#333333"),
                Spacing = 10,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 20,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = message,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Button
                    {
                        Text = copy,
                        BackgroundColor = Colors.White,
                        TextColor = Colors.Black,
                        Margin = new Thickness(0, 10, 0, 10),
                        Command = new Command(async () =>
                        {
                            await Clipboard.SetTextAsync(textToCopy);
                            Console.WriteLine("Text copied to clipboard: " + textToCopy);
                        })
                    },
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        Children =
                        {
                            new Button
                            {
                                Text = accept,
                                BackgroundColor = Colors.Green,
                                TextColor = Colors.White,
                                Margin = new Thickness(5, 0, 5, 0),
                                Command = new Command(() =>
                                {
                                    tcs.TrySetResult(true);
                                    Console.WriteLine("Accept button clicked.");
                                    Application.Current.MainPage.Navigation.PopModalAsync();
                                })
                            },
                            new Button
                            {
                                Text = cancel,
                                BackgroundColor = Colors.Red,
                                TextColor = Colors.White,
                                Margin = new Thickness(5, 0, 5, 0),
                                Command = new Command(() =>
                                {
                                    tcs.TrySetResult(false);
                                    Console.WriteLine("Cancel button clicked.");
                                    Application.Current.MainPage.Navigation.PopModalAsync();
                                })
                            }
                        }
                    }
                }
            };

            var page = new ContentPage
            {
                Content = layout
            };

            await Application.Current.MainPage.Navigation.PushModalAsync(page);

            return await tcs.Task;
        }
    }
}
