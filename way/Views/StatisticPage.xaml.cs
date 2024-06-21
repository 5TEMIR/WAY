using way.ViewModels;

namespace way.Views;

public partial class StatisticPage : ContentPage
{
	public StatisticPage(StatisticViewModel viewmodel)
	{
		InitializeComponent();

		BindingContext = viewmodel;
	}
}