# render-text-to-image-windows-app
Quick project that displays how to create an image and save to a bitmap image to save to picture library etc. through a Windows universal/8 application (XAML)

#Example

![Alt text](https://cloud.githubusercontent.com/assets/4294995/8870328/20710b08-31bb-11e5-9a17-0298239d3ba3.png "WindowsAppExample")

#Usage for Image Control (Windows 8.1 XAML)

```C#
MemoryStream imageStream = RenderStaticTextToBitmap();
IRandomAccessStream randomAccessStreamForImage = await ConvertToRandomAccessStream(imageStream);
Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
bitmapImage.SetSource(randomAccessStreamForImage);
imgCreated.Source = bitmapImage;
```

#Usage for Saving to Picture Library (Windows 8.1 XAML)

```C#
var imageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
MemoryStream pixelStream = RenderStaticTextToBitmap();
await FileIO.WriteBytesAsync(imageFile, ReadFully(pixelStream));
```

