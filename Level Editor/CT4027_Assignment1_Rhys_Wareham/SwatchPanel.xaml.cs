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
    /// Interaction logic for SwatchPanel.xaml
    /// </summary>
    public partial class SwatchPanel : UserControl
    {
        //Border Components
        protected int borderWidth = 2;
        protected SolidColorBrush selectedBrush = new SolidColorBrush(Colors.Black);
        protected SolidColorBrush unselectedBrush = new SolidColorBrush(Colors.White);

        //Selected item
        protected Border selectedSwatch = null;
        public ImageSource selectedImage = null;

        //Events to sire
        public delegate void SelectedItemDelegate(int selectedIndex, ImageSource source);
        public event SelectedItemDelegate SelectedItem;

        //Public Properties, but private setters
        public int SpriteWidth { get; private set; } = 32;
        public int SpriteHeight { get; private set; } = 32;

        public SwatchPanel()
        {
            InitializeComponent();

            //Check if we are in design mode, exit out if we are
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            //Call the AddTileSheet function
            AddTileSheet("Sprites/terrain_atlas.png", SpriteWidth, SpriteHeight);
        }

        //This function adds single sprites into the pack
        public void AddSprite(Image image)
        {
            var border = new Border();
            border.BorderThickness = new Thickness(borderWidth);
            border.BorderBrush = unselectedBrush;
            border.Child = image;
            border.MouseDown += OnMouseDown;

            stackPanel.Children.Add(border);
        }

        //The AddTileSheet function, adds a .png file which is then split up into smaller images to then be used in a scrollable menu of sprites
        public void AddTileSheet(string localTileSheetPath, int width, int height)
        {
            var imageSource = new BitmapImage(new Uri("pack://application:,,,/" + localTileSheetPath));
            for (int y = 0; y < imageSource.PixelHeight; y += height)
            {
                for (int x = 0; x < imageSource.PixelWidth; x += width)
                {
                    var croppedBitmap = new CroppedBitmap(imageSource, new Int32Rect(x, y, width, height));
                    AddSprite(new Image() { Source = croppedBitmap });
                }
            }
        }

        //When the mouse button is clicked down on the sprite menu, set the selectedImage to the sprite of which has been clicked on.
        //This will then allow the user to insert this sprite wherever they would like on the grid
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (selectedSwatch != null)
            {
                selectedSwatch.BorderBrush = unselectedBrush;
            }

            selectedSwatch = border;
            border.BorderBrush = selectedBrush;
            int selectedIndex = stackPanel.Children.IndexOf(border);

            selectedImage = (border.Child as Image).Source;

            if (selectedSwatch != null)
            {
                ImageSource source = (border.Child as Image).Source;
                SelectedItem?.Invoke(selectedIndex, source);
            }
        }


    }
}
