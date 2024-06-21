using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using way.Views;

namespace way.ViewModels
{
    public partial class TimeRestPickerViewModel : ObservableObject
    {
        [RelayCommand]
        public async Task CloseAsync(TimeRestPicker view)
        {
            await view.CloseAsync();
        }

        public int[] Times
        {
            get
            {
                int[] res = new int[60];
                for (int i = 0; i < 60; i++)
                    res[i] = i;
                return res;
            }
        }
    }
}
