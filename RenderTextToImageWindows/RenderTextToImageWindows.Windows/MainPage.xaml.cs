
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.IO;
using SharpDX.WIC;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.WIC.Bitmap;
using D2DPixelFormat = SharpDX.Direct2D1.PixelFormat;
using WicPixelFormat = SharpDX.WIC.PixelFormat;
using Windows.UI.Popups;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace RenderTextToImageWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Initialization

        public MainPage()
        {
            this.InitializeComponent();
        }

        #endregion
        
        #region Control Events

        private void btnCreateButton_Click(object sender, RoutedEventArgs e)
        {
            CreateImage();
        }

        #endregion

        #region Image Helpers

        /// <summary>
        /// Creates and Bind image to page for display.
        /// </summary>
        private async void CreateImage()
        {
            try
            {
                MemoryStream imageStream = RenderStaticTextToBitmap();
                IRandomAccessStream randomAccessStreamForImage = await ConvertToRandomAccessStream(imageStream);
                Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                bitmapImage.SetSource(randomAccessStreamForImage);
                imgCreated.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                MessageDialog msg = new MessageDialog(ex.Message, "Error");
                msg.ShowAsync();
            }
        }

        /// <summary>
        /// Read all of the bytes from a stream.s
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Create an image with text using SharpDx
        /// </summary>
        /// <returns></returns>
        private MemoryStream RenderStaticTextToBitmap()
        {
            var width = 1000;
            var height = 1000;
            var pixelFormat = WicPixelFormat.Format32bppBGR;

            var wicFactory = new ImagingFactory();
            var dddFactory = new SharpDX.Direct2D1.Factory();
            var dwFactory = new SharpDX.DirectWrite.Factory();
            var imageStream = new MemoryStream();

            try
            {
                var wicBitmap = new Bitmap(
                wicFactory,
                width,
                height,
                pixelFormat,
                BitmapCreateCacheOption.CacheOnLoad);

                var renderTargetProperties = new RenderTargetProperties(
                    RenderTargetType.Default,
                    new D2DPixelFormat(Format.Unknown, AlphaMode.Unknown),
                    0,
                    0,
                    RenderTargetUsage.None,
                    FeatureLevel.Level_DEFAULT);
                var renderTarget = new WicRenderTarget(
                    dddFactory,
                    wicBitmap,
                    renderTargetProperties)
                {
                    TextAntialiasMode = TextAntialiasMode.Cleartype
                };

                renderTarget.BeginDraw();

                var textFormat = new TextFormat(dwFactory, "Segoe UI", 48)
                {
                    TextAlignment = SharpDX.DirectWrite.TextAlignment.Justified,
                    ParagraphAlignment = ParagraphAlignment.Near
                };

                var textBrush = new SharpDX.Direct2D1.SolidColorBrush(
                    renderTarget,
                    SharpDX.Colors.White);

                renderTarget.Clear(Colors.Blue);
                renderTarget.DrawText(tbxImageText.Text,
                    textFormat,
                    new RectangleF(0, 0, width, height),
                    textBrush);

                renderTarget.EndDraw();

                var stream = new WICStream(
                    wicFactory,
                    imageStream);

                var encoder = new PngBitmapEncoder(wicFactory);
                encoder.Initialize(stream);

                var frameEncoder = new BitmapFrameEncode(encoder);
                frameEncoder.Initialize();
                frameEncoder.SetSize(width, height);
                frameEncoder.PixelFormat = WicPixelFormat.FormatDontCare;
                frameEncoder.WriteSource(wicBitmap);
                frameEncoder.Commit();

                encoder.Commit();

                frameEncoder.Dispose();
                encoder.Dispose();
                stream.Dispose();

                imageStream.Position = 0;
            }
            catch (Exception ex)
            {
                MessageDialog msg = new MessageDialog(ex.Message, "Error");
                msg.ShowAsync();
            }

            return imageStream;
        }

        /// <summary>
        /// Converts a memory stream to random access stream.  Have to use different stream when working with
        /// Sharp DX
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <returns></returns>
        public static async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = Task.Factory.StartNew(() => dw.WriteBytes(memoryStream.ToArray()));
            await task;
            await dw.StoreAsync();
            await outputStream.FlushAsync();
            return randomAccessStream;
        }

        #endregion
    }
}
