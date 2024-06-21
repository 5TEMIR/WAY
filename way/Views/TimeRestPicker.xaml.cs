using CommunityToolkit.Maui.Views;
using way.ViewModels;

namespace way.Views;

public partial class TimeRestPicker : Popup
{
    public TimeRestPicker(TimeRestPickerViewModel viewmodel)
	{
		InitializeComponent();

        BindingContext = viewmodel;
    }
}