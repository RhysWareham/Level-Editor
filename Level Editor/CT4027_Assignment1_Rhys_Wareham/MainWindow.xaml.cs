using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CT4027_Assignment1_Rhys_Wareham
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int inumCol = 100;
        private int inumRow = 30;
        private Image[,] images;
        bool bpenIsDown;
        const int imageSize = 32;

        public MainWindow()
        {
            // Create the application's main window
            InitializeComponent();

            // Create the Grid
            images = new Image[inumCol, inumRow];
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.ShowGridLines = true;
            

            //Define columns
            for (int x = 0; x < inumCol; x++)
            {
                ColumnDefinition colDef = new ColumnDefinition()
                {
                    //Set the width of columns to the pixel size of a sprite (32)
                    Width = new GridLength(imageSize, GridUnitType.Pixel)
                };
                grid.ColumnDefinitions.Add(colDef);
            }
            //Define rows
            for (int y = 0; y < inumRow; y++)
            {
                RowDefinition rowDef = new RowDefinition()
                {
                    //Set the height of the rows to the pixel size of a sprite (32)
                    Height = new GridLength(imageSize, GridUnitType.Pixel)
                };
                grid.RowDefinitions.Add(rowDef);
            }
            //Create a blank image on each square of the grid
            for (int x = 0; x < inumCol; x++)
            {
                for (int y = 0; y < inumRow; y++)
                {
                    Image image = new Image();
                    Grid.SetColumn(image, x);
                    Grid.SetRow(image, y);

                    grid.Children.Add(image);

                    images[x, y] = image;
                }
            }

            grid.MouseDown += OnMouseDown;
            grid.MouseUp += OnMouseUp;
            grid.MouseMove += OnMouseMove;
            swatch.SelectedItem += OnSelectedItem;
        }

        //When a sprite is clicked on, the chosen sprite will be displayed at the bottom of the window
        private void OnSelectedItem(int selectedIndex, ImageSource source)
        {
            label.Content = "Selected: " + selectedIndex;

            //Image scale
            image.Width = imageSize;
            image.Height = imageSize;

            image.Source = source;
        }

        //Function used to render the image and save as a png file
        private void Render(object sender, RoutedEventArgs e)
        {
            //Poorly grab width and height
            int width = inumCol * imageSize;
            int height = inumRow * imageSize;
            int dpi = 96; //One of the standard dpi (72/96) We're not using DPI. It would be for printing/viewing (dots per inch)
            byte[] pixels = new byte[height * width];

            grid.ShowGridLines = false;

            //Renders to a target bitmap
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(grid);

            //Encodes to a PNG
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            grid.ShowGridLines = true;

            //Writes PNG encoded image to file and saves in a file called "created.png"
            using (System.IO.FileStream fileStream = System.IO.File.Create("created.png"))
            {
                pngImage.Save(fileStream);
                fileStream.Close();
            }

        }

        //Function when mouse in moved
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            //Set x and y to the position of the cursor in relation to world, and then divided bt the width or height of each square.
            int x = (int)e.GetPosition(grid).X / imageSize;
            int y = (int)e.GetPosition(grid).Y / imageSize;
            //If the variable bpenIsDown is true, meaning the mouse button has been clicked whilst the pen tool is in use; call the UseTool function
            if (bpenIsDown == true)
            {
                UseTool(x, y);
            }

            //Displays the x and y coordinates of the cursor in regards to the grid
            axis.Content = "Coordinates: " + x + ", " + y;
        }

        //Function to change the variable bpenIsDown to false, if the mouse button is no longer clicked down
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            bpenIsDown = false;
        }

        //Function called when the mouse button is clicked down
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Set x and y to the position of the cursor in relation to world, and then divided bt the width or height of each square.
            int x = (int)e.GetPosition(grid).X / imageSize;
            int y = (int)e.GetPosition(grid).Y / imageSize;
            //Sets bpenIsDown to true and calls the UseTool function
            bpenIsDown = true;
            UseTool(x, y);
        }

        //The UseTool function changes the image which is on a grid square to whichever has been previously chosen; or erases the image.
        //Only called if the mouse button is clicked down. 
        //Checks if the coordinates of the cursor are inside the grid.
        private void UseTool(int igridX, int igridY)
        {
            if (igridX < inumCol && igridY < inumRow)
            {
                switch (selectedTool)
                {
                    case Tool.Pen:
                        images[igridX, igridY].Source = swatch.selectedImage;
                        break;
                    case Tool.Erase:
                        images[igridX, igridY].Source = null;
                        break;
                }
            }
        }

        //If the save button is clicked, the Render function is called
        private void HandleSave(object sender, RoutedEventArgs e)
        {
            Render(sender, e);
        }

        //If the load button is clicked, this function will be called
        private void HandleLoad(object sender, RoutedEventArgs e)
        {
            Uri location = null;

            //Creates a variable called fileDialogue and sets a filter so that only .png files can be imported
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = "Image Files (*.png) | *.png"
            };

            
            if (fileDialog.ShowDialog() == true)
            {
                //Sets the location variable to the file pathway
                location = new Uri(fileDialog.FileName);
            }

            //Sets the imageSource variable to the return value of BitmapFromUri function. This changes location into a bitmap
            var imageSource = BitmapFromUri(location);

            //Splits the .png image into an array in order to be displayed on the grid
            using (System.IO.StreamReader sr = System.IO.File.OpenText("created.png"))
            {
                for (int x = 0; x < (inumCol * imageSize); x += imageSize)
                {
                    for (int y = 0; y < (inumRow * imageSize); y += imageSize)
                    {
                        var croppedBitmap = new CroppedBitmap(imageSource, new Int32Rect(x, y, imageSize, imageSize));

                        images[x / imageSize, y / imageSize].Source = croppedBitmap;
                    }
                }
            }
        }

        //This function changes a Uri variable into a BitmapImage
        public static BitmapImage BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        //If the new button is clicked, set each square on the grid to a blank image
        private void HandleNew(object sender, RoutedEventArgs e)
        {
            for (int x = 0; x < inumCol; x++)
            {
                for (int y = 0; y < inumRow; y++)
                {
                    images[x, y].Source = null;
                }
            }
        }

        //If the close button is clicked, close the application
        private void HandleClose(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //If the pen button is clicked, set the selectedTool variable to pen
        private void HandlePen(object sender, RoutedEventArgs e)
        {
            selectedTool = Tool.Pen;
        }

        //If the erase button is clicked, set the selectedTool variable to erase
        private void HandleErase(object sender, RoutedEventArgs e)
        {
            selectedTool = Tool.Erase;
        }

        //This creates an enum called Tool, which is later used to swap betwwen each tool in a separate function
        public enum Tool
        {
            Pen,
            Erase
        }

        //Sets the selectedTool to pen when the application first opens
        private Tool selectedTool = Tool.Pen;

    }
}