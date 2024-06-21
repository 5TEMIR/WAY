using way.ViewModels;

namespace way.Views;

public partial class TrainingsPage : ContentPage
{
    public TrainingsPage(TrainingsViewModel viewmodel)
	{
		InitializeComponent();

        BindingContext = viewmodel;
	}
}