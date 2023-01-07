using LedStripControls.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace LedStripControls.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}